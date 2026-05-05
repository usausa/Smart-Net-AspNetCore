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

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

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

        if (symbol.Parameters.Length is < 1 or > 2)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.GetLocation(), symbol.Name));
        }

        var sourceParam = symbol.Parameters[0];
        var sourceValueKind = GetSourceValueKind(sourceParam.Type);
        if (sourceValueKind is null)
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.GetLocation(), symbol.Name));
        }

        BindingPattern pattern;
        ITypeSymbol targetType;
        if (symbol.Parameters.Length == 2)
        {
            targetType = symbol.Parameters[1].Type;
            pattern = symbol.ReturnsVoid ? BindingPattern.Instance : BindingPattern.ReturnInstance;
        }
        else if (!symbol.ReturnsVoid)
        {
            pattern = BindingPattern.Factory;
            targetType = symbol.ReturnType;
        }
        else
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.GetLocation(), symbol.Name));
        }

        var containingType = symbol.ContainingType;
        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();

        // Gather ignores
        var ignoredNames = new HashSet<string>(StringComparer.Ordinal);
        GetIgnoreMemberNames(ignoredNames, symbol);
        GetIgnoreMemberNames(ignoredNames, targetType);

        // Gather converters
        var methodConverter = GetConverterType(symbol);
        var containingConverter = GetConverterType(containingType);
        var targetConverter = GetConverterType(targetType);

        // Gather properties
        var properties = GetProperties(targetType, ignoredNames, targetConverter, methodConverter, containingConverter);

        var returnTypeName = symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var targetTypeName = pattern != BindingPattern.Factory ? symbol.Parameters[1].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : returnTypeName;

        return Results.Success(new MethodModel(
            ns,
            containingType.GetClassName(),
            containingType.IsStatic,
            containingType.IsValueType,
            symbol.DeclaredAccessibility,
            symbol.Name,
            returnTypeName,
            pattern,
            targetTypeName,
            sourceParam.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            sourceValueKind,
            sourceParam.Name,
            symbol.IsExtensionMethod,
            new EquatableArray<PropertyModel>(properties.ToArray())));
    }

    private static string? GetSourceValueKind(ITypeSymbol type)
    {
        var display = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // StringValues types
        if ((display == "global::Microsoft.AspNetCore.Http.IQueryCollection") ||
            (display == "global::Microsoft.AspNetCore.Http.IHeaderDictionary") ||
            (display == "global::Microsoft.AspNetCore.Http.IFormCollection") ||
            (display.StartsWith("global::System.Collections.Generic.Dictionary<string,", StringComparison.Ordinal) &&
             display.Contains("Microsoft.Extensions.Primitives.StringValues")))
        {
            return "StringValues";
        }

        // Basic dictionary types
        if ((display == "global::System.Collections.Generic.Dictionary<string, string>") ||
            (display == "global::System.Collections.Generic.IReadOnlyDictionary<string, string>") ||
            (display == "global::System.Collections.Generic.IDictionary<string, string>"))
        {
            return "String";
        }

        return null;
    }

    private static List<PropertyModel> GetProperties(
        ITypeSymbol targetType,
        HashSet<string> ignoredNames,
        ConverterTypeModel? targetConverter,
        ConverterTypeModel? methodConverter,
        ConverterTypeModel? containingConverter)
    {
        var properties = new List<PropertyModel>();

        foreach (var member in targetType.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsStatic)
            {
                continue;
            }

            if (member.SetMethod is null)
            {
                continue;
            }

            if (ignoredNames.Contains(member.Name) || HasAttribute(member, IgnoreAttributeName))
            {
                continue;
            }

            var propertyType = member.Type;

            // Property kind
            PropertyValueKind valueKind;
            IArrayTypeSymbol? arrayType;
            if (propertyType.SpecialType == SpecialType.System_String)
            {
                valueKind = PropertyValueKind.String;
                arrayType = null;
            }
            else if (propertyType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String } strArr)
            {
                valueKind = PropertyValueKind.StringArray;
                arrayType = strArr;
            }
            else if (propertyType is IArrayTypeSymbol arr)
            {
                valueKind = PropertyValueKind.Array;
                arrayType = arr;
            }
            else
            {
                valueKind = PropertyValueKind.Scalar;
                arrayType = null;
            }

            // Unwrap arrays and nullable types
            var assignmentType = arrayType is not null ? UnwrapNullable(arrayType.ElementType) : UnwrapNullable(propertyType);

            // Resolve converter
            var propertyConverter = GetConverterType(member);
            var converterCandidates = DistinctConverterTypes(new[] { propertyConverter, targetConverter, methodConverter, containingConverter });
            var (typeName, methodName) = ResolveConverterMethod(converterCandidates, assignmentType);

            properties.Add(new PropertyModel(
                member.Name,
                propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                assignmentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                valueKind,
                assignmentType.TypeKind == TypeKind.Enum,
                typeName,
                methodName));
        }

        return properties;
    }

    private static (string TypeName, string? MethodName) ResolveConverterMethod(List<ConverterTypeModel> converterTypes, ITypeSymbol assignmentType)
    {
        var assignmentTypeName = assignmentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        foreach (var converterType in converterTypes)
        {
            var method = converterType.Methods.ToArray().FirstOrDefault(x => x.ReturnTypeName == assignmentTypeName);
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
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named &&
            (named.TypeArguments.Length == 1))
        {
            return named.TypeArguments[0];
        }

        return type;
    }

    private static List<ConverterTypeModel> DistinctConverterTypes(IEnumerable<ConverterTypeModel?> converters)
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

        return results;
    }

    private static ConverterTypeModel? GetConverterType(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if ((attribute.AttributeClass?.ToDisplayString() == ConverterAttributeName) &&
                (attribute.ConstructorArguments.Length == 1) &&
                attribute.ConstructorArguments[0].Value is ITypeSymbol type)
            {
                var methods = GetConverterMethods(type);
                return new ConverterTypeModel(
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    new EquatableArray<ConverterMethodModel>(methods.ToArray()));
            }
        }

        return null;
    }

    private static List<ConverterMethodModel> GetConverterMethods(ITypeSymbol type)
    {
        var methods = new List<ConverterMethodModel>();
        foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (!member.IsStatic || (member.Parameters.Length != 1) || member.ReturnsVoid)
            {
                continue;
            }

            if (member.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "global::System.ReadOnlySpan<char>")
            {
                continue;
            }

            methods.Add(new ConverterMethodModel(member.Name, member.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        }

        return methods;
    }

    private static void GetIgnoreMemberNames(HashSet<string> names, ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if ((attribute.AttributeClass?.ToDisplayString() != IgnoreMembersAttributeName) ||
                (attribute.ConstructorArguments.Length != 1))
            {
                continue;
            }

            foreach (var value in attribute.ConstructorArguments[0].Values)
            {
                if (value.Value is string name && !String.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == metadataName);

    // ------------------------------------------------------------
    // Builder
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, ImmutableArray<Result<MethodModel>> methods)
    {
        foreach (var info in methods.SelectError())
        {
            context.ReportDiagnostic(info);
        }

        var builder = new SourceBuilder();
        foreach (var group in methods.SelectValue().GroupBy(static x => new { x.Namespace, x.ClassName }))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            builder.Clear();
            BuildSource(builder, group.ToList());
            var filename = MakeFilename(group.Key.Namespace, group.Key.ClassName);
            context.AddSource(filename, SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    private static void BuildSource(SourceBuilder builder, List<MethodModel> methods)
    {
        var ns = methods[0].Namespace;
        var isValueType = methods[0].IsValueType;
        var isStatic = methods[0].IsStatic;
        var className = methods[0].ClassName;

        builder.AutoGenerated();
        builder.EnableNullable();
        builder.NewLine();

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            builder.Namespace(ns);
            builder.NewLine();
        }

        // class
        builder
            .Indent()
            .Append(isStatic ? "static " : string.Empty)
            .Append("partial ")
            .Append(isValueType ? "struct " : "class ")
            .Append(className)
            .NewLine();
        builder.BeginScope();

        // method
        foreach (var method in methods)
        {
            BuildMethod(builder, method);
        }

        builder.EndScope();
    }

    private static void BuildMethod(SourceBuilder builder, MethodModel method)
    {
        builder
            .Indent()
            .Append(method.MethodAccessibility.ToText())
            .Append(" static partial ")
            .Append(method.ReturnTypeName)
            .Append(" ")
            .Append(method.MethodName)
            .Append("(");

        if (method.IsExtensionMethod)
        {
            builder.Append("this ");
        }

        builder
            .Append(method.SourceTypeName)
            .Append(" ")
            .Append(method.SourceParameterName);

        if (method.Pattern != BindingPattern.Factory)
        {
            builder
                .Append(", ")
                .Append(method.TargetTypeName)
                .Append(" target");
        }

        builder.Append(")").NewLine();
        builder.BeginScope();

        // Create instance if factory pattern
        if (method.Pattern == BindingPattern.Factory)
        {
            builder
                .Indent()
                .Append("var target = new ")
                .Append(method.ReturnTypeName)
                .Append("();")
                .NewLine();
            builder.NewLine();
        }

        // Property bindings
        var properties = method.Properties.ToArray();
        for (var index = 0; index < properties.Length; index++)
        {
            BuildProperty(builder, method, properties[index], index);
            builder.NewLine();
        }

        // Return target if factory or return-instance
        if (method.Pattern != BindingPattern.Instance)
        {
            builder
                .Indent()
                .Append("return target;")
                .NewLine();
        }

        builder.EndScope();
        builder.NewLine();
    }

    private static void BuildProperty(SourceBuilder builder, MethodModel method, PropertyModel property, int index)
    {
        var valueName = "p" + index;

        // Bind if value exists in the source and is not empty
        builder
            .Indent()
            .Append("if (")
            .Append(method.SourceParameterName)
            .Append(".TryGetValue(\"")
            .Append(property.Name)
            .Append("\", out var ")
            .Append(valueName)
            .Append(") &&")
            .NewLine()
            .Indent()
            .Append("    ")
            .Append(GetHasValueExpression(method.SourceValueKind, valueName))
            .Append(")")
            .NewLine();
        builder.BeginScope();

        // Bind by property kind
        switch (property.ValueKind)
        {
            case PropertyValueKind.String:
                // Directly assign from source
                builder
                    .Indent()
                    .Append("target.")
                    .Append(property.Name)
                    .Append(" = ")
                    .Append(GetSingleValueExpression(method.SourceValueKind, valueName))
                    .Append(";")
                    .NewLine();
                break;
            case PropertyValueKind.StringArray:
                // Directly assign from all source values
                builder
                    .Indent()
                    .Append("target.")
                    .Append(property.Name)
                    .Append(" = ")
                    .Append(GetArrayValueExpression(method.SourceValueKind, valueName))
                    .Append(";")
                    .NewLine();
                break;
            case PropertyValueKind.Array:
                // Convert each element for non-string arrays
                BuildArrayProperty(builder, method, property, valueName);
                break;
            default:
                // Convert from single source
                BuildSingleConvertedProperty(builder, method, property, valueName);
                break;
        }

        builder.EndScope();
    }

    private static void BuildArrayProperty(SourceBuilder builder, MethodModel method, PropertyModel property, string valueName)
    {
        if (property.ConverterMethodName is null)
        {
            return;
        }

        var arrName = valueName + "arr";
        var countExpr = GetCountExpression(method.SourceValueKind, valueName);
        var elementType = GetArrayElementCreationType(property.TypeName, property.AssignmentTypeName);

        // Allocate result array
        builder
            .Indent()
            .Append("var ")
            .Append(arrName)
            .Append(" = new ")
            .Append(elementType)
            .Append("[")
            .Append(countExpr)
            .Append("];")
            .NewLine();

        // Loop over source values and convert each element
        builder
            .Indent()
            .Append("for (var i = 0; i < ")
            .Append(countExpr)
            .Append("; i++)")
            .NewLine();
        builder.BeginScope();

        builder
            .Indent()
            .Append(arrName)
            .Append("[i] = ");
        AppendConvertCall(builder, property, GetIndexedSpanExpression(method.SourceValueKind, valueName, "i"));
        builder.Append(";").NewLine();

        builder.EndScope();

        // Assign to property
        builder
            .Indent()
            .Append("target.")
            .Append(property.Name)
            .Append(" = ")
            .Append(arrName)
            .Append(";")
            .NewLine();
    }

    private static void BuildSingleConvertedProperty(SourceBuilder builder, MethodModel method, PropertyModel property, string valueName)
    {
        if (property.ConverterMethodName is null)
        {
            return;
        }

        builder
            .Indent()
            .Append("target.")
            .Append(property.Name)
            .Append(" = ");
        AppendConvertCall(builder, property, GetSingleSpanExpression(method.SourceValueKind, valueName));
        builder.Append(";").NewLine();
    }

    private static void AppendConvertCall(SourceBuilder builder, PropertyModel property, string valueExpression)
    {
        builder
            .Append(property.ConverterMethodTypeName)
            .Append(".")
            .Append(property.ConverterMethodName!);

        if (property.IsEnum &&
            (property.ConverterMethodTypeName == DefaultConverterTypeName) &&
            (property.ValueKind == PropertyValueKind.Scalar))
        {
            builder
                .Append("<")
                .Append(property.AssignmentTypeName)
                .Append(">");
        }

        builder
            .Append("(")
            .Append(valueExpression)
            .Append(")");
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
        "StringValues" => $"({valueName}.Count > 0)",
        _ => $"({valueName} is not null)"
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

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static string MakeFilename(string ns, string className)
    {
        var buffer = new StringBuilder();
        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className.Replace('<', '[').Replace('>', ']'));
        buffer.Append(".g.cs");
        return buffer.ToString();
    }
}
