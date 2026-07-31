namespace Smart.AspNetCore;

using System.Globalization;

using Microsoft.AspNetCore.Mvc.Rendering;

using static Smart.AspNetCore.SelectListAssert;

public sealed class SelectListBuilderTest
{
    //--------------------------------------------------------------------------------
    // Enum
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenFromEnumThenMemberNameIsValueAndDisplayNameIsText()
    {
        var list = SelectListBuilder.FromEnum<SampleStatus>();

        Assert.Equal("HasDisplay,HasResource,HasDescriptionOnly,NoAttribute", Values(list));
        Assert.Equal("DisplayText,ResourceText,HasDescriptionOnly,NoAttribute", Texts(list));
    }

    [Fact]
    public void WhenFromEnumWithoutSelectedThenNothingIsSelected()
    {
        var list = SelectListBuilder.FromEnum<SampleStatus>();

        Assert.DoesNotContain(list, static x => x.Selected);
    }

    [Fact]
    public void WhenFromEnumWithSelectedThenMatchedItemIsSelected()
    {
        var list = SelectListBuilder.FromEnum(SampleStatus.NoAttribute);

        var selected = Assert.Single(list, static x => x.Selected);
        Assert.Equal(nameof(SampleStatus.NoAttribute), selected.Value);
    }

    [Fact]
    public void WhenFromEnumWithPredicateThenItemsAreFiltered()
    {
        var list = SelectListBuilder.FromEnum<SampleStatus>(static x => x is SampleStatus.HasDisplay or SampleStatus.NoAttribute);

        Assert.Equal("HasDisplay,NoAttribute", Values(list));
    }

    [Fact]
    public void WhenFromEnumWithPredicateAndSelectedThenMatchedItemIsSelected()
    {
        var list = SelectListBuilder.FromEnum(
            static x => x is SampleStatus.HasDisplay or SampleStatus.NoAttribute,
            SampleStatus.NoAttribute);

        Assert.Equal("HasDisplay,NoAttribute", Values(list));
        var selected = Assert.Single(list, static x => x.Selected);
        Assert.Equal(nameof(SampleStatus.NoAttribute), selected.Value);
    }

    [Fact]
    public void WhenFromEnumWithPredicateExcludingAllThenResultIsEmpty()
    {
        Assert.Empty(SelectListBuilder.FromEnum<SampleStatus>(static _ => false));
    }

    [Fact]
    public void WhenFromEnumCalledTwiceThenItemsAreNotShared()
    {
        var first = SelectListBuilder.FromEnum<SampleStatus>();
        first[0].Selected = true;

        var second = SelectListBuilder.FromEnum<SampleStatus>();

        Assert.DoesNotContain(second, static x => x.Selected);
    }

    [Fact]
    public void WhenFromEnumWithAliasedMemberThenAllMembersAreListed()
    {
        var list = SelectListBuilder.FromEnum<SampleAlias>();

        Assert.Equal(4, list.Count);
    }

    //--------------------------------------------------------------------------------
    // Range
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenFromRangeThenValueAndTextAreSame()
    {
        var list = SelectListBuilder.FromRange(1, 3);

        Assert.Equal("1,2,3", Values(list));
        Assert.Equal("1,2,3", Texts(list));
    }

    [Fact]
    public void WhenFromRangeWithStepThenValuesAreSkipped()
    {
        Assert.Equal("1,4,7,10", Values(SelectListBuilder.FromRange(1, 10, 3)));
    }

    [Fact]
    public void WhenFromRangeStepOvershootsEndThenLastValueIsExcluded()
    {
        Assert.Equal("1,5,9", Values(SelectListBuilder.FromRange(1, 10, 4)));
    }

    [Fact]
    public void WhenFromRangeWithFormatThenValuesAreFormatted()
    {
        var list = SelectListBuilder.FromRange(0, 23, 1, "D2");

        Assert.Equal(24, list.Count);
        Assert.Equal("00", list[0].Value);
        Assert.Equal("23", list[^1].Value);
    }

    [Fact]
    public void WhenFromRangeWithFormatAndStepThenValuesAreFormatted()
    {
        Assert.Equal("05,10,15,20,25,30", Values(SelectListBuilder.FromRange(5, 30, 5, "D2")));
    }

    [Fact]
    public void WhenFromRangeWithNegativeStartThenValuesAreSigned()
    {
        Assert.Equal("-2,-1,0,1,2", Values(SelectListBuilder.FromRange(-2, 2)));
    }

    [Fact]
    public void WhenFromRangeWithSelectedThenMatchedItemIsSelected()
    {
        var list = SelectListBuilder.FromRange(0, 55, 5, "D2", 10);

        var selected = Assert.Single(list, static x => x.Selected);
        Assert.Equal("10", selected.Value);
    }

    [Fact]
    public void WhenFromRangeWithUnmatchedSelectedThenNothingIsSelected()
    {
        var list = SelectListBuilder.FromRange(0, 55, 5, "D2", 11);

        Assert.DoesNotContain(list, static x => x.Selected);
    }

    [Fact]
    public void WhenFromRangeStartIsGreaterThanEndThenResultIsEmpty()
    {
        Assert.Empty(SelectListBuilder.FromRange(3, 1));
    }

    [Fact]
    public void WhenFromRangeStartEqualsEndThenSingleItemIsReturned()
    {
        var item = Assert.Single(SelectListBuilder.FromRange(7, 7));

        Assert.Equal("7", item.Value);
    }

    [Fact]
    public void WhenFromRangeCoversIntBoundaryThenDoesNotOverflow()
    {
        var list = SelectListBuilder.FromRange(Int32.MaxValue - 2, Int32.MaxValue, 2);

        var expected = String.Join(
            ',',
            (Int32.MaxValue - 2).ToString(CultureInfo.InvariantCulture),
            Int32.MaxValue.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(expected, Values(list));
    }

    //--------------------------------------------------------------------------------
    // Rendering
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenResultIsAssignedToTagHelperItemsThenTypeIsCompatible()
    {
        IEnumerable<SelectListItem> items = SelectListBuilder.FromEnum<SampleStatus>();

        Assert.NotEmpty(items);
    }
}
