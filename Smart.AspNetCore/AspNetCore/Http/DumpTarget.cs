namespace Smart.AspNetCore.Http;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Performance")]
internal sealed class DumpTarget
{
    public DumpType DumpType;

    public string ContentType = default!;
}
