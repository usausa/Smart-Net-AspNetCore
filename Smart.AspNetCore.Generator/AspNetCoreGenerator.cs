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
public sealed class AspNetCoreGenerator : IIncrementalGenerator
{
    private const string BindAttributeName = "Smart.AspNetCore.Binders.BindAttribute";
    private const string ConverterAttributeName = "Smart.AspNetCore.Binders.BindConverterTypeAttribute";
    private const string IgnoreAttributeName = "Smart.AspNetCore.Binders.BindIgnoreAttribute";
    private const string IgnoreMembersAttributeName = "Smart.AspNetCore.Binders.BindIgnoreMembersAttribute";
    private const string DefaultConverterTypeName = "global::Smart.AspNetCore.Binders.DefaultStringConverter";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BindAttributeName,
                static (syntax, _) => syntax is MethodDeclarationSyntax,
                static (context, _) => GetMethodModel(context))
            .Collect();

        context.RegisterImplementationSourceOutput(
            methodProvider,
            static (context, provider) => Execute(context, provider));
    }

    private static Result<MethodModel> GetMethodModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (MethodDeclarationSyntax)context.TargetNode;
        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IMethodSymbol symbol)
        {
            return Results.Error<MethodModel>(null);
        }

        if (!symbol.IsStatic || !symbol.IsPartialDefinition)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.GetLocation(), symbol.Name));
        }

        // Must have 1 or 2 parameters
        // Pattern A (return):   TargetType Bind(IQueryCollection query)
        // Pattern B (instance): void Bind(IQueryCollection query, TargetType target)
        //                    or TargetType Bind(IQueryCollection query, TargetType target)
        if (symbol.Parameters.Length < 1 || symbol.Parameters.Length > 2)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.GetLocation(), symbol.Name));
        }

        var sourceParam = symbol.Parameters[0];
        var sourceValueKind = GetSourceValueKind(sourceParam.Type);
        if (sourceValueKind is null)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.GetLocation(), symbol.Name));
        }

        bool hasTargetParameter;
        ITypeSymbol targetType;

        if (symbol.Parameters.Length == 2)
        {
            hasTargetParameter = true;
            targetType = symbol.Parameters[1].Type;
        }
        else if (!symbol.ReturnsVoid)
        {
            hasTargetParameter = false;
            targetType = symbol.ReturnType;
        }
        else
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.GetLocation(), symbol.Name));
        }

        var containingType = symbol.ContainingType;
        var ns = string.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();

        var methodIgnoredNames = GetIgnoreMemberNames(symbol);
        var targetIgnoredNames = GetIgnoreMemberNames(targetType);
        var ignoredNames = methodIgnoredNames.Concat(targetIgnoredNames).Distinct(StringComparer.Ordinal).ToArray();
        var methodConverter = GetConverterType(symbol);
        var containingConverter = GetConverterType(containingType);
        var targetConverter = GetConverterType(targetType);
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
            methodConverter?.TypeName,
            containingConverter?.TypeName,
            new EquatableArray<string>(ignoredNames),
            new EquatableArray<PropertyModel>(properties)));
    }

    private static string? GetSourceValueKind(ITypeSymbol type)
    {
        var display = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (display == "global::Microsoft.AspNetCore.Http.IQueryCollection" ||
            display == "global::Microsoft.AspNetCore.Http.IHeaderDictionary" ||
            display == "global::Microsoft.AspNetCore.Http.IFormCollection" ||
            (display.StartsWith("global::System.Collections.Generic.Dictionary<string,", StringComparison.Ordinal) &&
             display.Contains("Microsoft.Extensions.Primitives.StringValues")))
        {
            return "StringValues";
        }

        if (display == "global::System.Collections.Generic.Dictionary<string, string>" ||
            display == "global::System.Collections.Generic.IReadOnlyDictionary<string, string>" ||
            display == "global::System.Collections.Generic.IDictionary<string, string>")
        {
            return "String";
        }

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
            if (member.IsStatic)
            {
                continue;
            }

            var isIgnored = ignoredNames.Contains(member.Name, StringComparer.Ordinal) || HasAttribute(member, IgnoreAttributeName);
            var hasSetter = member.SetMethod is not null;
            var propertyType = member.Type;
            var isString = propertyType.SpecialType == SpecialType.System_String;
            var isStringArray = propertyType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String };
            var arrayType = propertyType as IArrayTypeSymbol;
            var isArray = arrayType is not null && !isStringArray;
            var assignmentType = isArray ? UnwrapNullable(arrayType!.ElementType) : UnwrapNullable(propertyType);
            var propertyConverter = GetConverterType(member);
            var converterType = propertyConverter?.TypeName ?? targetConverter?.TypeName ?? methodConverter?.TypeName ?? containingConverter?.TypeName;
            var converterCandidates = DistinctConverterTypes(new[] { propertyConverter, targetConverter, methodConverter, containingConverter });
            var converterMethod = ResolveConverterMethod(converterCandidates, assignmentType);

            yield return new PropertyModel(
                member.Name,
                propertyType.ToGlobalTypeName(),
                assignmentType.ToGlobalTypeName(),
                converterType,
                isString,
                isStringArray,
                isArray,
                assignmentType.TypeKind == TypeKind.Enum,
                isIgnored,
                hasSetter,
                converterMethod.TypeName,
                converterMethod.MethodName);
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
        foreach (var info in methods.SelectError())
        {
            context.ReportDiagnostic(info);
        }

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

        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();

        if (!string.IsNullOrEmpty(ns))
        {
            builder.Append("namespace ").Append(ns).AppendLine(";");
            builder.AppendLine();
        }

        builder.Append(isStatic ? "static " : string.Empty).Append("partial ").Append(isValueType ? "struct " : "class ").AppendLine(className);
        builder.AppendLine("{");

        foreach (var method in methods)
        {
            BuildMethod(builder, method);
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void BuildMethod(StringBuilder builder, MethodModel method)
    {
        // Build signature
        builder.Append("    ").Append(method.MethodAccessibility.ToText()).Append(" static partial ");
        builder.Append(method.ReturnTypeName).Append(' ').Append(method.MethodName).Append('(');
        builder.Append(method.SourceTypeName).Append(' ').Append(method.SourceParameterName);

        if (method.HasTargetParameter)
        {
            builder.Append(", ").Append(method.TargetTypeName).Append(" target");
        }

        builder.AppendLine(")");
        builder.AppendLine("    {");

        if (!method.HasTargetParameter)
        {
            builder.Append("        var target = new ").Append(method.ReturnTypeName).AppendLine("();");
            builder.AppendLine();
        }

        foreach (var property in method.Properties.ToArray())
        {
            if (property.IsIgnored || !property.HasSetter)
            {
                continue;
            }

            BuildProperty(builder, method, property);
            builder.AppendLine();
        }

        if (method.ReturnTypeName != "void")
        {
            builder.AppendLine("        return target;");
        }

        builder.AppendLine("    }");
        builder.AppendLine();
    }

    private static void BuildProperty(StringBuilder builder, MethodModel method, PropertyModel property)
    {
        var valueName = "_v_" + property.Name;
        builder.Append("        if (").Append(method.SourceParameterName).Append(".TryGetValue(\"").Append(property.Name).Append("\", out var ").Append(valueName).AppendLine(") && ")
            .Append("            ").Append(GetHasValueExpression(method.SourceValueKind, valueName)).AppendLine(")");
        builder.AppendLine("        {");

        if (property.IsString)
        {
            builder.Append("            target.").Append(property.Name).Append(" = ").Append(GetSingleValueExpression(method.SourceValueKind, valueName)).AppendLine(";");
        }
        else if (property.IsStringArray)
        {
            builder.Append("            target.").Append(property.Name).Append(" = ").Append(GetArrayValueExpression(method.SourceValueKind, valueName)).AppendLine(";");
        }
        else if (property.IsArray)
        {
            BuildArrayProperty(builder, method, property, valueName);
        }
        else
        {
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
