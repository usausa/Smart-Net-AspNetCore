namespace Smart.AspNetCore.ModelBinding;

using System;

using Smart.Text.Japanese;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class KanaConvertAttribute : Attribute
{
    public KanaOption Option { get; }

    public bool ClientValidation { get; set; }

    public KanaConvertAttribute(KanaOption option)
    {
        Option = option;
    }
}
