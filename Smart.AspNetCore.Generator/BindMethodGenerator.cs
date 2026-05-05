namespace Smart.AspNetCore.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

[Generator]
public sealed class BindMethodGenerator : IIncrementalGenerator
{
    private const string BindAttributeName = "Smart.AspNetCore.Binders.BindAttribute";
    private const string ConverterAttributeName = "Smart.AspNetCore.Binders.BindConverterAttribute";
    private const string IgnoreAttributeName = "Smart.AspNetCore.Binders.BindIgnoreAttribute";
    private const string IgnoreMembersAttributeName = "Smart.AspNetCore.Binders.BindIgnoreMembersAttribute";
    private const string DefaultConverterTypeName = "global::Smart.AspNetCore.Binders.DefaultStringConverter";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect all methods decorated with [Bind] and transform each to a MethodModel (or an error).
        // [Bind] が付いたメソッドをすべて収集し、それぞれを MethodModel（またはエラー）に変換する。
        var methodProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BindAttributeName,
                static (syntax, _) => syntax is MethodDeclarationSyntax,
                static (context, _) => GetMethodModel(context))
            .Collect();

        // Register the source-output step; runs only when the collected models change.
        // ソース出力ステップを登録する。収集したモデルが変化したときのみ実行される。
        context.RegisterImplementationSourceOutput(
            methodProvider,
            static (context, provider) => Execute(context, provider));
    }

    private static Result<MethodModel> GetMethodModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (MethodDeclarationSyntax)context.TargetNode;

        // Resolve the method symbol from the semantic model.
        // セマンティックモデルからメソッドシンボルを解決する。
        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IMethodSymbol symbol)
        {
            return Results.Error<MethodModel>(null);
        }

        // The method must be static and a partial definition (not an implementation).
        // メソッドは static かつ partial 定義（実装側ではない）でなければならない。
        if (!symbol.IsStatic || !symbol.IsPartialDefinition)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.GetLocation(), symbol.Name));
        }

        // Validate parameter count.
        // パラメーター数を検証する。
        // Pattern A (factory):  TargetType Method(SourceCollection source)
        // Pattern B (instance): void       Method(SourceCollection source, TargetType target)
        //                    or TargetType Method(SourceCollection source, TargetType target)
        if (symbol.Parameters.Length < 1 || symbol.Parameters.Length > 2)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.GetLocation(), symbol.Name));
        }

        // Validate that the first parameter is a supported source collection.
        // 第1パラメーターがサポートされているソースコレクションであることを検証する。
        var sourceParam = symbol.Parameters[0];
        var sourceValueKind = GetSourceValueKind(sourceParam.Type);
        if (sourceValueKind is null)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.GetLocation(), symbol.Name));
        }

        // Determine binding pattern and the target type.
        // バインドパターンとターゲット型を決定する。
        bool hasTargetParameter;
        ITypeSymbol targetType;

        if (symbol.Parameters.Length == 2)
        {
            // Pattern B: second parameter is the target instance.
            // パターン B: 第2パラメーターがターゲットインスタンス。
            hasTargetParameter = true;
            targetType = symbol.Parameters[1].Type;
        }
        else if (!symbol.ReturnsVoid)
        {
            // Pattern A: return type is the target.
            // パターン A: 戻り値型がターゲット。
            hasTargetParameter = false;
            targetType = symbol.ReturnType;
        }
        else
        {
            // Single-parameter void method is not a valid binder signature.
            // パラメーターが1つで void を返すメソッドは有効なバインダーシグネチャではない。
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.GetLocation(), symbol.Name));
        }

        // Collect namespace and containing-type metadata.
        // 名前空間と含有型のメタデータを収集する。
        var containingType = symbol.ContainingType;
        var ns = string.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();

        // Build the combined list of member names to ignore (from method-level and target-type-level attributes).
        // 無視するメンバー名のリストを構築する（メソッドレベルおよびターゲット型レベルの属性から収集）。
        var methodIgnoredNames = GetIgnoreMemberNames(symbol);
        var targetIgnoredNames = GetIgnoreMemberNames(targetType);
        var ignoredNames = methodIgnoredNames.Concat(targetIgnoredNames).Distinct(StringComparer.Ordinal).ToArray();

        // Resolve converter types at each scope (property < target < method < containing type).
        // 各スコープのコンバーター型を解決する（優先順位: プロパティ < ターゲット型 < メソッド < 含有型）。
        var methodConverter = GetConverterType(symbol);
        var containingConverter = GetConverterType(containingType);
        var targetConverter = GetConverterType(targetType);

        // Build per-property binding metadata for the target type.
        // ターゲット型の各プロパティのバインドメタデータを構築する。
        var properties = GetProperties(targetType, ignoredNames, targetConverter, methodConverter, containingConverter).ToArray();

        var returnTypeName = symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToGlobalTypeName();
        var targetTypeName = hasTargetParameter ? symbol.Parameters[1].Type.ToGlobalTypeName() : returnTypeName;

        return Results.Success(new MethodModel(
            ns,
            containingType.GetClassName(),
            containingType.IsValueType,
            containingType.IsStatic,
            symbol.DeclaredAccessibility,
            symbol.Name,
            returnTypeName,
            hasTargetParameter,
            targetTypeName,
            sourceParam.Type.ToGlobalTypeName(),
            sourceValueKind,
            sourceParam.Name,
            symbol.IsExtensionMethod,
            new EquatableArray<PropertyModel>(properties)));
    }

    private static string? GetSourceValueKind(ITypeSymbol type)
    {
        // IQueryCollection, IFormCollection, IHeaderDictionary and Dictionary<string, StringValues>
        // are all indexed by StringValues.
        // IQueryCollection、IFormCollection、IHeaderDictionary および Dictionary<string, StringValues> は
        // すべて StringValues でインデックスアクセスされる。
        var display = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (display == "global::Microsoft.AspNetCore.Http.IQueryCollection" ||
            display == "global::Microsoft.AspNetCore.Http.IHeaderDictionary" ||
            display == "global::Microsoft.AspNetCore.Http.IFormCollection" ||
            (display.StartsWith("global::System.Collections.Generic.Dictionary<string,", StringComparison.Ordinal) &&
             display.Contains("Microsoft.Extensions.Primitives.StringValues")))
        {
            return "StringValues";
        }

        // Plain string dictionaries are indexed by a single string value.
        // 純粋な文字列ディクショナリーは単一の string 値でインデックスアクセスされる。
        if (display == "global::System.Collections.Generic.Dictionary<string, string>" ||
            display == "global::System.Collections.Generic.IReadOnlyDictionary<string, string>" ||
            display == "global::System.Collections.Generic.IDictionary<string, string>")
        {
            return "String";
        }

        // Unsupported source type — the caller will report a diagnostic.
        // サポートされていないソース型 — 呼び出し元がダイアグノスティックを報告する。
        return null;
    }

    private static IEnumerable<PropertyModel> GetProperties(
        ITypeSymbol targetType,
        string[] ignoredNames,
        ConverterTypeModel? targetConverter,
        ConverterTypeModel? methodConverter,
        ConverterTypeModel? containingConverter)
    {
        foreach (var member in targetType.GetMembers().OfType<IPropertySymbol>())
        {
            // Skip static properties; only instance properties are bound.
            // 静的プロパティはスキップする。バインド対象はインスタンスプロパティのみ。
            if (member.IsStatic)
            {
                continue;
            }

            var isIgnored = ignoredNames.Contains(member.Name, StringComparer.Ordinal) || HasAttribute(member, IgnoreAttributeName);
            var hasSetter = member.SetMethod is not null;
            var propertyType = member.Type;

            // Classify the property kind so the code-emitter can choose the right generation branch.
            // コード生成時に適切な分岐を選択できるよう、プロパティの種別を分類する。
            var isString = propertyType.SpecialType == SpecialType.System_String;
            var isStringArray = propertyType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String };
            var arrayType = propertyType as IArrayTypeSymbol;
            var isArray = arrayType is not null && !isStringArray;

            // For arrays and nullable types, unwrap to the underlying element/value type for conversion.
            // 配列型および nullable 型は、変換に使う基底の要素型・値型に unwrap する。
            var assignmentType = isArray ? UnwrapNullable(arrayType!.ElementType) : UnwrapNullable(propertyType);

            // Resolve the converter in priority order: property → target-type → method → containing-type.
            // コンバーターを優先順位に従って解決する: プロパティ → ターゲット型 → メソッド → 含有型。
            var propertyConverter = GetConverterType(member);
            var converterCandidates = DistinctConverterTypes(new[] { propertyConverter, targetConverter, methodConverter, containingConverter });
            var (typeName, methodName) = ResolveConverterMethod(converterCandidates, assignmentType);

            yield return new PropertyModel(
                member.Name,
                propertyType.ToGlobalTypeName(),
                assignmentType.ToGlobalTypeName(),
                isString,
                isStringArray,
                isArray,
                assignmentType.TypeKind == TypeKind.Enum,
                isIgnored,
                hasSetter,
                typeName,
                methodName);
        }
    }

    private static (string TypeName, string? MethodName) ResolveConverterMethod(IEnumerable<ConverterTypeModel> converterTypes, ITypeSymbol assignmentType)
    {
        foreach (var converterType in converterTypes)
        {
            var method = converterType.Methods.ToArray().FirstOrDefault(x => x.ReturnTypeName == assignmentType.ToGlobalTypeName());
            if (method is not null)
            {
                return (converterType.TypeName, method.Name);
            }
        }

        if (assignmentType.TypeKind == TypeKind.Enum)
        {
            return (DefaultConverterTypeName, "ToEnum");
        }

        if (TryGetDefaultConverterMethod(assignmentType, out var defaultMethod))
        {
            return (DefaultConverterTypeName, defaultMethod);
        }

        return (DefaultConverterTypeName, null);
    }

    private static bool TryGetDefaultConverterMethod(ITypeSymbol type, out string methodName)
    {
        methodName = type.SpecialType switch
        {
            SpecialType.System_Boolean => "ToBoolean",
            SpecialType.System_Byte => "ToByte",
            SpecialType.System_SByte => "ToSByte",
            SpecialType.System_Int16 => "ToInt16",
            SpecialType.System_UInt16 => "ToUInt16",
            SpecialType.System_Int32 => "ToInt32",
            SpecialType.System_UInt32 => "ToUInt32",
            SpecialType.System_Int64 => "ToInt64",
            SpecialType.System_UInt64 => "ToUInt64",
            SpecialType.System_Single => "ToSingle",
            SpecialType.System_Double => "ToDouble",
            SpecialType.System_Decimal => "ToDecimal",
            SpecialType.System_Char => "ToChar",
            SpecialType.System_DateTime => "ToDateTime",
            _ => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) switch
            {
                "global::System.DateTimeOffset" => "ToDateTimeOffset",
                "global::System.DateOnly" => "ToDateOnly",
                "global::System.TimeOnly" => "ToTimeOnly",
                "global::System.TimeSpan" => "ToTimeSpan",
                "global::System.Guid" => "ToGuid",
                _ => string.Empty
            }
        };

        return methodName.Length > 0;
    }

    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named && named.TypeArguments.Length == 1)
        {
            return named.TypeArguments[0];
        }

        return type;
    }

    private static ConverterTypeModel[] DistinctConverterTypes(IEnumerable<ConverterTypeModel?> converters)
    {
        var results = new List<ConverterTypeModel>();
        foreach (var converter in converters)
        {
            if (converter is null || results.Any(x => x.TypeName == converter.TypeName))
            {
                continue;
            }

            results.Add(converter);
        }

        return results.ToArray();
    }

    private static ConverterTypeModel? GetConverterType(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == ConverterAttributeName &&
                attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is ITypeSymbol type)
            {
                return new ConverterTypeModel(type.ToGlobalTypeName(), new EquatableArray<ConverterMethodModel>(GetConverterMethods(type).ToArray()));
            }
        }

        return null;
    }

    private static IEnumerable<ConverterMethodModel> GetConverterMethods(ITypeSymbol type)
    {
        foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (!member.IsStatic || member.Parameters.Length != 1 || member.ReturnsVoid)
            {
                continue;
            }

            if (member.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "global::System.ReadOnlySpan<char>")
            {
                continue;
            }

            yield return new ConverterMethodModel(member.Name, member.ReturnType.ToGlobalTypeName());
        }
    }

    private static string[] GetIgnoreMemberNames(ISymbol symbol)
    {
        var names = new List<string>();
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != IgnoreMembersAttributeName || attribute.ConstructorArguments.Length != 1)
            {
                continue;
            }

            foreach (var value in attribute.ConstructorArguments[0].Values)
            {
                if (value.Value is string name && !string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }

        return names.ToArray();
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == metadataName);

    private static void Execute(SourceProductionContext context, ImmutableArray<Result<MethodModel>> methods)
    {
        // Report any diagnostics collected during the analysis phase.
        // 解析フェーズで収集したダイアグノスティックをすべて報告する。
        foreach (var info in methods.SelectError())
        {
            context.ReportDiagnostic(info);
        }

        // Group valid models by containing type and emit one source file per type.
        // 有効なモデルを含有型ごとにグループ化し、型ごとに1つのソースファイルを出力する。
        foreach (var group in methods.SelectValue().GroupBy(static x => new { x.Namespace, x.ClassName }))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var source = BuildSource(group.ToList());
            var filename = MakeFilename(group.Key.Namespace, group.Key.ClassName);
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string BuildSource(List<MethodModel> methods)
    {
        var builder = new StringBuilder();
        var ns = methods[0].Namespace;
        var className = methods[0].ClassName;
        var isValueType = methods[0].IsValueType;
        var isStatic = methods[0].IsStatic;

        // Emit the file header.
        // ファイルヘッダーを出力する。
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();

        // Emit namespace declaration if present.
        // 名前空間宣言が存在する場合は出力する。
        if (!string.IsNullOrEmpty(ns))
        {
            builder.Append("namespace ").Append(ns).AppendLine(";");
            builder.AppendLine();
        }

        // Open the partial type declaration.
        // partial 型宣言を開く。
        builder.Append(isStatic ? "static " : string.Empty).Append("partial ").Append(isValueType ? "struct " : "class ").AppendLine(className);
        builder.AppendLine("{");

        // Emit each binder method implementation.
        // バインダーメソッドの実装をそれぞれ出力する。
        foreach (var method in methods)
        {
            BuildMethod(builder, method);
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void BuildMethod(StringBuilder builder, MethodModel method)
    {
        // Emit the method signature, matching the partial declaration exactly.
        // partial 宣言と完全に一致するメソッドシグネチャを出力する。
        builder.Append("    ").Append(method.MethodAccessibility.ToText()).Append(" static partial ");
        builder.Append(method.ReturnTypeName).Append(' ').Append(method.MethodName).Append('(');
        if (method.IsExtensionMethod)
        {
            builder.Append("this ");
        }

        builder.Append(method.SourceTypeName).Append(' ').Append(method.SourceParameterName);

        if (method.HasTargetParameter)
        {
            builder.Append(", ").Append(method.TargetTypeName).Append(" target");
        }

        builder.AppendLine(")");
        builder.AppendLine("    {");

        // Factory pattern: create a new target instance.
        // ファクトリーパターン: 新しいターゲットインスタンスを生成する。
        if (!method.HasTargetParameter)
        {
            builder.Append("        var target = new ").Append(method.ReturnTypeName).AppendLine("();");
            builder.AppendLine();
        }

        // Emit per-property binding statements, skipping ignored or read-only properties.
        // プロパティごとのバインド文を出力する。無視指定または読み取り専用のプロパティはスキップする。
        foreach (var property in method.Properties.ToArray())
        {
            if (property.IsIgnored || !property.HasSetter)
            {
                continue;
            }

            BuildProperty(builder, method, property);
            builder.AppendLine();
        }

        // Return the target for factory and return-instance patterns.
        // ファクトリーパターンおよびインスタンス返却パターンではターゲットを返す。
        if (method.ReturnTypeName != "void")
        {
            builder.AppendLine("        return target;");
        }

        builder.AppendLine("    }");
        builder.AppendLine();
    }

    private static void BuildProperty(StringBuilder builder, MethodModel method, PropertyModel property)
    {
        // Guard: only bind when the key exists in the source and has a non-empty value.
        // ガード: ソースにキーが存在し、かつ値が空でない場合にのみバインドする。
        var valueName = "_v_" + property.Name;
        builder.Append("        if (").Append(method.SourceParameterName).Append(".TryGetValue(\"").Append(property.Name).Append("\", out var ").Append(valueName).AppendLine(") && ")
            .Append("            ").Append(GetHasValueExpression(method.SourceValueKind, valueName)).AppendLine(")");
        builder.AppendLine("        {");

        // Dispatch to the appropriate binding strategy based on the property kind.
        // プロパティの種別に応じて適切なバインド戦略へディスパッチする。
        if (property.IsString)
        {
            // String properties are assigned directly from the source value.
            // string プロパティはソース値から直接代入する。
            builder.Append("            target.").Append(property.Name).Append(" = ").Append(GetSingleValueExpression(method.SourceValueKind, valueName)).AppendLine(";");
        }
        else if (property.IsStringArray)
        {
            // String array properties are assigned directly from all source values.
            // string 配列プロパティはすべてのソース値から直接代入する。
            builder.Append("            target.").Append(property.Name).Append(" = ").Append(GetArrayValueExpression(method.SourceValueKind, valueName)).AppendLine(";");
        }
        else if (property.IsArray)
        {
            // Non-string array properties require per-element conversion.
            // string 以外の配列プロパティは要素ごとに変換が必要。
            BuildArrayProperty(builder, method, property, valueName);
        }
        else
        {
            // Scalar non-string properties require a single conversion call.
            // string 以外のスカラープロパティは単一の変換呼び出しが必要。
            BuildSingleConvertedProperty(builder, method, property, valueName);
        }

        builder.AppendLine("        }");
    }

    private static void BuildSingleConvertedProperty(StringBuilder builder, MethodModel method, PropertyModel property, string valueName)
    {
        if (property.ConverterMethodName is null)
        {
            return;
        }

        builder.Append("            target.").Append(property.Name).Append(" = ");
        AppendConvertCall(builder, property, GetSingleSpanExpression(method.SourceValueKind, valueName));
        builder.AppendLine(";");
    }

    private static void BuildArrayProperty(StringBuilder builder, MethodModel method, PropertyModel property, string valueName)
    {
        if (property.ConverterMethodName is null)
        {
            return;
        }

        builder.Append("            var _arr_").Append(property.Name).Append(" = new ").Append(GetArrayElementCreationType(property.TypeName, property.AssignmentTypeName)).Append('[');
        builder.Append(GetCountExpression(method.SourceValueKind, valueName)).AppendLine("];");
        builder.Append("            for (var _i_").Append(property.Name).Append(" = 0; _i_").Append(property.Name).Append(" < ");
        builder.Append(GetCountExpression(method.SourceValueKind, valueName)).Append("; _i_").Append(property.Name).AppendLine("++)");
        builder.AppendLine("            {");
        builder.Append("                _arr_").Append(property.Name).Append("[_i_").Append(property.Name).Append("] = ");
        AppendConvertCall(builder, property, GetIndexedSpanExpression(method.SourceValueKind, valueName, "_i_" + property.Name));
        builder.AppendLine(";");
        builder.AppendLine("            }");
        builder.Append("            target.").Append(property.Name).Append(" = _arr_").Append(property.Name).AppendLine(";");
    }

    private static void AppendConvertCall(StringBuilder builder, PropertyModel property, string valueExpression)
    {
        builder.Append(property.ConverterMethodTypeName).Append('.').Append(property.ConverterMethodName);
        if (property.IsEnum && property.ConverterMethodTypeName == DefaultConverterTypeName)
        {
            builder.Append('<').Append(property.AssignmentTypeName).Append('>');
        }

        builder.Append('(').Append(valueExpression).Append(')');
    }

    private static string GetArrayElementCreationType(string propertyTypeName, string assignmentTypeName)
    {
        var suffix = "[]";
        return propertyTypeName.EndsWith(suffix, StringComparison.Ordinal)
            ? propertyTypeName.Substring(0, propertyTypeName.Length - suffix.Length)
            : assignmentTypeName;
    }

    private static string GetHasValueExpression(string sourceValueKind, string valueName) => sourceValueKind switch
    {
        "StringValues" => $"{valueName}.Count > 0",
        _ => $"{valueName} is not null"
    };

    private static string GetSingleValueExpression(string sourceValueKind, string valueName) => sourceValueKind switch
    {
        "StringValues" => $"{valueName}[0]",
        _ => valueName
    };

    private static string GetSingleSpanExpression(string sourceValueKind, string valueName) => sourceValueKind switch
    {
        "StringValues" => $"{valueName}[0].AsSpan()",
        _ => $"{valueName}.AsSpan()"
    };

    private static string GetArrayValueExpression(string sourceValueKind, string valueName) => sourceValueKind switch
    {
        "StringValues" => $"{valueName}.ToArray()",
        _ => $"new[] {{ {valueName} }}"
    };

    private static string GetCountExpression(string sourceValueKind, string valueName) => sourceValueKind switch
    {
        "StringValues" => $"{valueName}.Count",
        _ => "1"
    };

    private static string GetIndexedSpanExpression(string sourceValueKind, string valueName, string indexName) => sourceValueKind switch
    {
        "StringValues" => $"{valueName}[{indexName}].AsSpan()",
        _ => $"{valueName}.AsSpan()"
    };

    private static string MakeFilename(string ns, string className)
    {
        var buffer = new StringBuilder();
        if (!string.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className.Replace('<', '[').Replace('>', ']'));
        buffer.Append(".g.cs");
        return buffer.ToString();
    }
}

internal static class SymbolExtensions
{
    public static string ToGlobalTypeName(this ITypeSymbol symbol) =>
        symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
