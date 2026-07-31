namespace Smart.AspNetCore;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

public static class DisplayResources
{
    public static string Localized => "ResourceText";
}

internal enum SampleStatus
{
    [Display(Name = "DisplayText")]
    HasDisplay,

    [Display(Name = nameof(DisplayResources.Localized), ResourceType = typeof(DisplayResources))]
    HasResource,

    [Description("DescriptionText")]
    HasDescriptionOnly,

    NoAttribute
}

[Flags]
internal enum SampleModes
{
    None = 0,
    First = 1,
    Second = 2
}

internal enum SampleAlias
{
    Zero,
    Primary,
    Alias = Primary,
    Other
}

internal static class SelectListAssert
{
    public static string Values(IEnumerable<SelectListItem> items) =>
        String.Join(',', items.Select(static x => x.Value));

    public static string Texts(IEnumerable<SelectListItem> items) =>
        String.Join(',', items.Select(static x => x.Text));
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class EnumDisplayExtensionsTest
{
    [Fact]
    public void WhenDisplayAttributeExistsThenNameIsUsed()
    {
        Assert.Equal("DisplayText", SampleStatus.HasDisplay.GetDisplayName());
    }

    [Fact]
    public void WhenDisplayAttributeHasResourceTypeThenResourceValueIsUsed()
    {
        Assert.Equal("ResourceText", SampleStatus.HasResource.GetDisplayName());
    }

    [Fact]
    public void WhenOnlyDescriptionAttributeExistsThenMemberNameIsUsed()
    {
        Assert.Equal(nameof(SampleStatus.HasDescriptionOnly), SampleStatus.HasDescriptionOnly.GetDisplayName());
    }

    [Fact]
    public void WhenNoAttributeExistsThenMemberNameIsUsed()
    {
        Assert.Equal(nameof(SampleStatus.NoAttribute), SampleStatus.NoAttribute.GetDisplayName());
    }

    [Fact]
    public void WhenValueIsUndefinedThenToStringIsUsed()
    {
        Assert.Equal("999", ((SampleStatus)999).GetDisplayName());
    }

    [Fact]
    public void WhenValueIsCombinedFlagsThenToStringIsUsed()
    {
        Assert.Equal("First, Second", (SampleModes.First | SampleModes.Second).GetDisplayName());
    }

    [Fact]
    public void WhenValueIsSingleFlagThenMemberNameIsUsed()
    {
        Assert.Equal(nameof(SampleModes.First), SampleModes.First.GetDisplayName());
    }

    [Fact]
    public void WhenEnumHasAliasedMemberThenResolutionDoesNotThrow()
    {
        Assert.True(SampleAlias.Primary.GetDisplayName() is nameof(SampleAlias.Primary) or nameof(SampleAlias.Alias));
        Assert.Equal(nameof(SampleAlias.Other), SampleAlias.Other.GetDisplayName());
    }
}
