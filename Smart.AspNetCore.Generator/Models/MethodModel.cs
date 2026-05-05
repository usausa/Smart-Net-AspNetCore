namespace Smart.AspNetCore.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

/// <summary>
/// Represents the analysis result of a single <c>[Bind]</c>-annotated partial method.
/// This model is passed from the analysis phase to the code-generation phase.
/// <c>[Bind]</c> が付与された partial メソッド1つの解析結果を表す。
/// 解析フェーズからコード生成フェーズへ渡されるモデル。
/// </summary>
internal sealed record MethodModel(
    /// <summary>Namespace of the containing type; empty string for global namespace.
    /// 含有型の名前空間。グローバル名前空間の場合は空文字列。</summary>
    string Namespace,
    /// <summary>Simple (or generic) class name of the containing type.
    /// 含有型のシンプルな（またはジェネリックの）クラス名。</summary>
    string ClassName,
    /// <summary>True when the containing type is a value type (struct).
    /// 含有型が値型（struct）の場合 true。</summary>
    bool IsValueType,
    /// <summary>True when the containing type is declared static.
    /// 含有型が static として宣言されている場合 true。</summary>
    bool IsStatic,
    /// <summary>Declared accessibility of the partial method (e.g. Public, Internal).
    /// partial メソッドの宣言アクセシビリティ（例: Public、Internal）。</summary>
    Accessibility MethodAccessibility,
    /// <summary>Name of the partial method to be implemented.
    /// 実装対象の partial メソッド名。</summary>
    string MethodName,
    /// <summary>Fully-qualified return type name; "void" when the method has no return value.
    /// 完全修飾の戻り値型名。戻り値がない場合は "void"。</summary>
    string ReturnTypeName,
    /// <summary>Binding pattern that determines how the target is created or received and whether it is returned.
    /// ターゲットの生成・受け取り方法と返却有無を決定するバインドパターン。</summary>
    BindingPattern Pattern,
    /// <summary>Fully-qualified type name of the object that properties are bound into.
    /// プロパティのバインド先オブジェクトの完全修飾型名。</summary>
    string TargetTypeName,
    /// <summary>Fully-qualified type name of the source collection parameter (e.g. IQueryCollection).
    /// ソースコレクションパラメーターの完全修飾型名（例: IQueryCollection）。</summary>
    string SourceTypeName,
    /// <summary>Kind of the source collection: "StringValues" or "String".
    /// ソースコレクションの種別: "StringValues" または "String"。</summary>
    string SourceValueKind,
    /// <summary>Name of the source parameter as declared in the partial method signature.
    /// partial メソッドシグネチャで宣言されたソースパラメーター名。</summary>
    string SourceParameterName,
    /// <summary>True when the method is an extension method (first parameter has <c>this</c> modifier).
    /// メソッドが拡張メソッドの場合 true（第1パラメーターに <c>this</c> 修飾子がある）。</summary>
    bool IsExtensionMethod,
    /// <summary>Per-property binding metadata for every bindable property of the target type.
    /// ターゲット型のバインド対象プロパティのバインドメタデータ。</summary>
    EquatableArray<PropertyModel> Properties);
