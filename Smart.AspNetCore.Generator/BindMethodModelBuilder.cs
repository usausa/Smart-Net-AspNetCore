namespace Smart.AspNetCore.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

internal static class BindMethodModelBuilder
{
    internal const string DefaultConverterTypeName = "global::Smart.AspNetCore.Binders.DefaultStringConverter";

    private const string ConverterAttributeName = "Smart.AspNetCore.Binders.BindConverterAttribute";
    private const string IgnoreAttributeName = "Smart.AspNetCore.Binders.BindIgnoreAttribute";
    private const string IgnoreMembersAttributeName = "Smart.AspNetCore.Binders.BindIgnoreMembersAttribute";
    private const string SetsRequiredMembersAttributeName = "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute";

    public static Result<MethodModel> GetMethodModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (MethodDeclarationSyntax)context.TargetNode;

        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not { } symbol)
        {
            return Results.Errors<MethodModel>();
        }

        if (!symbol.IsStatic || !symbol.IsPartialDefinition || (symbol.PartialImplementationPart is not null))
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.Identifier.GetLocation(), symbol.Name));
        }

        var containingType = symbol.ContainingType;
        if (!IsPartialType(containingType))
        {
            return Results.Error<MethodModel>(new DiagnosticInfo(Diagnostics.NotPartialContainingType, syntax.Identifier.GetLocation(), containingType.Name));
        }

        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var containingTypes = new EquatableArray<string>(containingType.GetContainingTypes()
            .Append(containingType)
            .Select(static x => x.GetPartialDeclaration())
            .ToArray());
        var hintName = HintNameBuilder.BuildFromType(containingType);
        var signature = symbol.GetImplementationSignature(syntax);
        var typeName = containingType.ToDisplayString();

        var canFallback = containingType.GetContainingTypes().All(IsPartialType);
        Result<MethodModel> Fallback(DiagnosticInfo error) =>
            !canFallback ? Results.Error<MethodModel>(error) : new(
                new MethodModel(
                    ns,
                    containingTypes,
                    hintName,
                    signature,
                    BindingPattern.Instance,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    EquatableArray<PropertyModel>.Empty,
                    false,
                    EquatableArray<DiagnosticInfo>.Empty,
                    IsFallback: true,
                    TypeName: typeName),
                new EquatableArray<DiagnosticInfo>([error]));

        if (symbol.IsGenericMethod)
        {
            return Fallback(new DiagnosticInfo(Diagnostics.GenericMethod, syntax.Identifier.GetLocation(), symbol.Name));
        }

        if (symbol.Parameters.Length is < 1 or > 2)
        {
            return Fallback(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.Identifier.GetLocation(), symbol.Name));
        }

        var sourceParam = symbol.Parameters[0];
        var sourceValueKind = GetSourceValueKind(sourceParam.Type);
        if (sourceValueKind is null)
        {
            return Fallback(new DiagnosticInfo(Diagnostics.InvalidMethodParameter, syntax.Identifier.GetLocation(), symbol.Name));
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
            return Fallback(new DiagnosticInfo(Diagnostics.InvalidMethodDefinition, syntax.Identifier.GetLocation(), symbol.Name));
        }

        if ((containingType.ContainingType is not null) || containingType.IsFileLocal)
        {
            return Fallback(new DiagnosticInfo(Diagnostics.NestedContainingType, syntax.Identifier.GetLocation(), containingType.Name));
        }

        // The factory pattern creates the instance in generated code, so it must be constructible.
        if (pattern == BindingPattern.Factory)
        {
            if (targetType.IsAbstract)
            {
                return Fallback(new DiagnosticInfo(Diagnostics.AbstractTargetType, syntax.Identifier.GetLocation(), targetType.Name));
            }

            if (!HasAccessibleParameterlessConstructor(targetType))
            {
                return Fallback(new DiagnosticInfo(Diagnostics.NoParameterlessConstructor, syntax.Identifier.GetLocation(), targetType.Name));
            }
        }

        var sourceParameterName = CSharpIdentifier.Escape(sourceParam.Name);
        var targetName = pattern != BindingPattern.Factory
            ? CSharpIdentifier.Escape(symbol.Parameters[1].Name)
            : "__target";
        var targetNullable = (pattern != BindingPattern.Factory) && (symbol.Parameters[1].NullableAnnotation == NullableAnnotation.Annotated);

        // Gather ignores
        var ignoredNames = new HashSet<string>(StringComparer.Ordinal);
        GetIgnoreMemberNames(ignoredNames, symbol);
        GetIgnoreMemberNames(ignoredNames, targetType);

        // Gather converters
        var methodConverter = GetConverterType(symbol);
        var containingConverter = GetConverterType(containingType);
        var targetConverter = GetConverterType(targetType);

        // Gather properties
        var diagnostics = new List<DiagnosticInfo>();
        var properties = GetProperties(context.SemanticModel, syntax.SpanStart, targetType, ignoredNames, targetConverter, methodConverter, containingConverter, diagnostics);

        var strict = GetStrictOption(context.Attributes);

        var targetTypeName = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return Results.Success(new MethodModel(
            ns,
            containingTypes,
            hintName,
            signature,
            pattern,
            targetTypeName,
            targetName,
            sourceValueKind,
            sourceParameterName,
            new EquatableArray<PropertyModel>(properties),
            strict,
            new EquatableArray<DiagnosticInfo>(diagnostics),
            TargetNullable: targetNullable,
            TypeName: typeName));
    }

    private static string? GetSourceValueKind(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return null;
        }

        // Non-generic ASP.NET Core source collections
        var display = named.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (display is "global::Microsoft.AspNetCore.Http.IQueryCollection"
                or "global::Microsoft.AspNetCore.Http.IHeaderDictionary"
                or "global::Microsoft.AspNetCore.Http.IFormCollection")
        {
            return "StringValues";
        }

        // Dictionary-like sources: inspect the actual generic type arguments instead of matching display strings
        if (IsDictionaryType(named) &&
            (named.TypeArguments.Length == 2) &&
            (named.TypeArguments[0].SpecialType == SpecialType.System_String))
        {
            var valueType = named.TypeArguments[1];
            if (valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Microsoft.Extensions.Primitives.StringValues")
            {
                return "StringValues";
            }

            if (valueType.SpecialType == SpecialType.System_String)
            {
                return "String";
            }
        }

        return null;
    }

    private static bool IsDictionaryType(INamedTypeSymbol type)
    {
        if (type.ContainingNamespace?.ToDisplayString() != "System.Collections.Generic")
        {
            return false;
        }

        return type.ConstructedFrom.MetadataName is "Dictionary`2" or "IDictionary`2" or "IReadOnlyDictionary`2";
    }

    private static List<PropertyModel> GetProperties(
        SemanticModel semanticModel,
        int position,
        ITypeSymbol targetType,
        HashSet<string> ignoredNames,
        ConverterTypeModel? targetConverter,
        ConverterTypeModel? methodConverter,
        ConverterTypeModel? containingConverter,
        List<DiagnosticInfo> diagnostics)
    {
        var properties = new List<PropertyModel>();

        foreach (var member in GetPropertiesWithBase(targetType))
        {
            if (member.IsStatic || member.IsIndexer)
            {
                continue;
            }

            if ((member.SetMethod is not { IsInitOnly: false } setter) ||
                (member.IsObsolete(out var isError) && isError) ||
                (setter.IsObsolete(out var isSetterError) && isSetterError) ||
                !semanticModel.IsAccessible(position, member) ||
                !semanticModel.IsAccessible(position, setter))
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
            var (typeName, methodName, returnsNullable) = ResolveConverterMethod(converterCandidates, assignmentType);

            if ((methodName is null) && (valueKind is PropertyValueKind.Scalar or PropertyValueKind.Array))
            {
                diagnostics.Add(new DiagnosticInfo(Diagnostics.UnconvertibleProperty, member.Locations.FirstOrDefault() ?? Location.None, member.Name));
            }

            properties.Add(new PropertyModel(
                member.Name,
                propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                assignmentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                valueKind,
                assignmentType.TypeKind == TypeKind.Enum,
                typeName,
                methodName,
                returnsNullable));
        }

        return properties;
    }

    private static List<IPropertySymbol> GetPropertiesWithBase(ITypeSymbol type)
    {
        var types = new List<ITypeSymbol>();
        for (var current = type; (current is not null) && (current.SpecialType != SpecialType.System_Object); current = current.BaseType)
        {
            types.Add(current);
        }

        var hidden = new HashSet<string>(StringComparer.Ordinal);
        var properties = new List<IPropertySymbol>[types.Count];
        for (var i = 0; i < types.Count; i++)
        {
            properties[i] = [];
            var declared = new List<string>();
            foreach (var member in types[i].GetMembers())
            {
                if (hidden.Contains(member.Name))
                {
                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    properties[i].Add(property);
                }

                if (member.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
                {
                    declared.Add(member.Name);
                }
            }

            hidden.UnionWith(declared);
        }

        var result = new List<IPropertySymbol>();
        for (var i = types.Count - 1; i >= 0; i--)
        {
            result.AddRange(properties[i]);
        }

        return result;
    }

    private static (string TypeName, string? MethodName, bool ReturnsNullable) ResolveConverterMethod(List<ConverterTypeModel> converterTypes, ITypeSymbol assignmentType)
    {
        var assignmentTypeName = assignmentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        foreach (var converterType in converterTypes)
        {
            var method = converterType.Methods.FirstOrDefault(x => x.ReturnTypeName == assignmentTypeName);
            if (method is not null)
            {
                return (converterType.TypeName, method.Name, method.ReturnsNullable);
            }
        }

        if (assignmentType.TypeKind == TypeKind.Enum)
        {
            return (DefaultConverterTypeName, "ToEnum", false);
        }

        if (TryGetDefaultConverterMethod(assignmentType, out var defaultMethod))
        {
            return (DefaultConverterTypeName, defaultMethod, false);
        }

        return (DefaultConverterTypeName, null, false);
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
        if ((type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named) &&
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
            if ((converter is null) || results.Any(x => x.TypeName == converter.TypeName))
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
                (attribute.ConstructorArguments[0].Value is ITypeSymbol type))
            {
                var methods = GetConverterMethods(type);
                return new ConverterTypeModel(
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    new EquatableArray<ConverterMethodModel>(methods));
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

            var returnType = UnwrapNullable(member.ReturnType);
            var returnsNullable = !SymbolEqualityComparer.Default.Equals(returnType, member.ReturnType) ||
                                  (member.ReturnType.IsReferenceType && (member.ReturnType.NullableAnnotation == NullableAnnotation.Annotated));
            methods.Add(new ConverterMethodModel(member.Name, returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), returnsNullable));
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
                if ((value.Value is string name) && !String.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }
    }

    private static bool GetStrictOption(ImmutableArray<AttributeData> attributes)
    {
        foreach (var attribute in attributes)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if ((argument.Key == "Strict") && (argument.Value.Value is bool value))
                {
                    return value;
                }
            }
        }

        return false;
    }

    private static bool IsPartialType(INamedTypeSymbol type)
    {
        foreach (var reference in type.DeclaringSyntaxReferences)
        {
            if ((reference.GetSyntax() is TypeDeclarationSyntax declaration) &&
                declaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAccessibleParameterlessConstructor(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        var hasRequiredMembers = HasRequiredMembers(named);
        if (type.IsValueType)
        {
            return !hasRequiredMembers;
        }

        foreach (var constructor in named.InstanceConstructors)
        {
            if ((constructor.Parameters.Length == 0) &&
                (constructor.DeclaredAccessibility != Accessibility.Private) &&
                (constructor.DeclaredAccessibility != Accessibility.Protected) &&
                (!hasRequiredMembers || HasAttribute(constructor, SetsRequiredMembersAttributeName)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasRequiredMembers(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.GetMembers().Any(static x => x is IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true }))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == metadataName);
}
