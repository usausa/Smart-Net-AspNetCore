namespace Smart.AspNetCore.Generator.Models;

/// <summary>
/// Represents the binding metadata for a single property of the target type.
/// ターゲット型の単一プロパティのバインドメタデータを表す。
/// </summary>
internal sealed record PropertyModel(
    /// <summary>Name of the property.
    /// プロパティ名。</summary>
    string Name,
    /// <summary>Fully-qualified type name of the property (may be array or nullable).
    /// プロパティの完全修飾型名（配列型または nullable 型の場合あり）。</summary>
    string TypeName,
    /// <summary>Fully-qualified type name used for conversion; for arrays this is the element type, and nullable wrappers are unwrapped.
    /// 変換に使用する完全修飾型名。配列の場合は要素型、nullable は unwrap された値型。</summary>
    string AssignmentTypeName,
    /// <summary>True when the property type is <c>string</c> or <c>string?</c>.
    /// プロパティ型が <c>string</c> または <c>string?</c> の場合 true。</summary>
    bool IsString,
    /// <summary>True when the property type is <c>string[]</c> or <c>string?[]</c>.
    /// プロパティ型が <c>string[]</c> または <c>string?[]</c> の場合 true。</summary>
    bool IsStringArray,
    /// <summary>True when the property is a non-string array type.
    /// プロパティが string 以外の配列型の場合 true。</summary>
    bool IsArray,
    /// <summary>True when the assignment type is an enum.
    /// 代入型が enum の場合 true。</summary>
    bool IsEnum,
    /// <summary>True when the property is excluded from binding (by attribute or by method-level ignore list).
    /// プロパティがバインドから除外されている場合 true（属性またはメソッドレベルの無視リストによる）。</summary>
    bool IsIgnored,
    /// <summary>True when the property has an accessible setter.
    /// プロパティにアクセス可能なセッターがある場合 true。</summary>
    bool HasSetter,
    /// <summary>Fully-qualified type name of the converter class that provides the conversion method.
    /// 変換メソッドを提供するコンバータークラスの完全修飾型名。</summary>
    string ConverterMethodTypeName,
    /// <summary>Name of the static conversion method on <see cref="ConverterMethodTypeName"/>; null when no suitable method is found.
    /// <see cref="ConverterMethodTypeName"/> 上の静的変換メソッド名。適切なメソッドが見つからない場合は null。</summary>
    string? ConverterMethodName);
