namespace Smart.AspNetCore.Generator.Models;

/// <summary>
/// Classifies how a bindable property is read from the source and assigned to the target.
/// バインド対象プロパティのソースからの読み取りおよびターゲットへの代入方法を分類する。
/// </summary>
internal enum PropertyValueKind
{
    /// <summary>
    /// <c>string</c> or <c>string?</c> — assigned directly from the source value.
    /// <c>string</c> または <c>string?</c> — ソース値から直接代入する。
    /// </summary>
    String,

    /// <summary>
    /// <c>string[]</c> or <c>string?[]</c> — assigned directly from all source values.
    /// <c>string[]</c> または <c>string?[]</c> — すべてのソース値から直接代入する。
    /// </summary>
    StringArray,

    /// <summary>
    /// Non-string array — each element is individually converted from a source value.
    /// string 以外の配列 — 各要素をソース値から個別に変換する。
    /// </summary>
    Array,

    /// <summary>
    /// Scalar non-string value — converted from a single source value via a converter method.
    /// string 以外のスカラー値 — コンバーターメソッドを通じて単一のソース値から変換する。
    /// </summary>
    Scalar,
}
