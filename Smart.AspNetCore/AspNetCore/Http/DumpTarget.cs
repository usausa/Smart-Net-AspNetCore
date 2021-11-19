namespace Smart.AspNetCore.Http;

using System.Diagnostics.CodeAnalysis;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Performance")]
internal class DumpTarget
{
    public DumpType DumpType;

    [AllowNull]
    public string ContentType;
}
