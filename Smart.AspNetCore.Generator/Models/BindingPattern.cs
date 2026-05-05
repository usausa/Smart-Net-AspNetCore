namespace Smart.AspNetCore.Generator.Models;

/// <summary>
/// Identifies the binding pattern of a generated binder method.
/// 生成されるバインダーメソッドのバインドパターンを識別する。
/// </summary>
internal enum BindingPattern
{
    /// <summary>
    /// Factory pattern — creates and returns a new target instance.
    /// <c>TargetType Method(SourceCollection source)</c>
    /// ファクトリーパターン — 新しいターゲットインスタンスを生成して返す。
    /// </summary>
    Factory,

    /// <summary>
    /// Instance pattern — populates an existing target instance and returns void.
    /// <c>void Method(SourceCollection source, TargetType target)</c>
    /// インスタンスパターン — 既存のターゲットインスタンスにバインドして void を返す。
    /// </summary>
    Instance,

    /// <summary>
    /// Return-instance pattern — populates an existing target instance and returns it.
    /// <c>TargetType Method(SourceCollection source, TargetType target)</c>
    /// インスタンス返却パターン — 既存のターゲットインスタンスにバインドしてそれを返す。
    /// </summary>
    ReturnInstance,
}
