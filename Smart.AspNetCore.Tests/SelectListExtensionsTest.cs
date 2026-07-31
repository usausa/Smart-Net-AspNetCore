namespace Smart.AspNetCore;

using System.Collections;

using Microsoft.AspNetCore.Mvc.Rendering;

using static Smart.AspNetCore.SelectListAssert;

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class SelectListExtensionsTest
{
    private sealed record Entity(string Code, string Name);

    private sealed class CountingEnumerable(IEnumerable<string> source) : IEnumerable<string>
    {
        public int EnumerateCount { get; private set; }

        public IEnumerator<string> GetEnumerator()
        {
            EnumerateCount++;
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private static readonly Entity[] Entities =
    [
        new("001", "Tokyo"),
        new("002", "Osaka")
    ];

    private static readonly string[] Strings = ["A", "B"];

    //--------------------------------------------------------------------------------
    // Projection
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenToSelectListThenSelectorsAreApplied()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name);

        Assert.Equal("001,002", Values(list));
        Assert.Equal("Tokyo,Osaka", Texts(list));
    }

    [Fact]
    public void WhenToSelectListWithoutSelectedThenNothingIsSelected()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name);

        Assert.DoesNotContain(list, static x => x.Selected);
    }

    [Fact]
    public void WhenToSelectListWithSelectedValueThenMatchedItemIsSelected()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name, "002");

        var selected = Assert.Single(list, static x => x.Selected);
        Assert.Equal("002", selected.Value);
    }

    [Fact]
    public void WhenToSelectListWithUnmatchedSelectedValueThenNothingIsSelected()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name, "999");

        Assert.DoesNotContain(list, static x => x.Selected);
    }

    [Fact]
    public void WhenToSelectListWithSelectedValueThenComparisonIsOrdinal()
    {
        var source = new Entity[] { new("abc", "Text") };

        var list = source.ToSelectList(static x => x.Code, static x => x.Name, "ABC");

        Assert.DoesNotContain(list, static x => x.Selected);
    }

    [Fact]
    public void WhenToSelectListWithEmptySelectedValueThenEmptyValuedItemIsSelected()
    {
        var source = new Entity[] { new(string.Empty, "Unselected"), new("001", "Tokyo") };

        var list = source.ToSelectList(static x => x.Code, static x => x.Name, string.Empty);

        var selected = Assert.Single(list, static x => x.Selected);
        Assert.Equal("Unselected", selected.Text);
    }

    [Fact]
    public void WhenToSelectListSourceIsNullThenResultIsEmpty()
    {
        Assert.Empty(((IEnumerable<Entity>?)null).ToSelectList(static x => x.Code, static x => x.Name));
    }

    [Fact]
    public void WhenToSelectListSourceIsLazyThenEnumeratedOnce()
    {
        var source = new CountingEnumerable(Strings);

        var list = source.ToSelectList(static x => x, static x => x);

        Assert.Equal(1, source.EnumerateCount);
        Assert.Equal("A,B", Values(list));
    }

    //--------------------------------------------------------------------------------
    // String
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenToSelectListOfStringThenValueAndTextAreSame()
    {
        var list = Strings.ToSelectList();

        Assert.Equal("A,B", Values(list));
        Assert.Equal("A,B", Texts(list));
    }

    [Fact]
    public void WhenToSelectListOfStringWithSelectedValueThenMatchedItemIsSelected()
    {
        var list = Strings.ToSelectList("B");

        var selected = Assert.Single(list, static x => x.Selected);
        Assert.Equal("B", selected.Value);
    }

    [Fact]
    public void WhenToSelectListOfStringSourceIsNullThenResultIsEmpty()
    {
        Assert.Empty(((IEnumerable<string>?)null).ToSelectList());
    }

    //--------------------------------------------------------------------------------
    // Empty
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenWithEmptyThenBlankItemIsPrepended()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name).WithEmpty();

        Assert.Equal(",001,002", Values(list));
        Assert.Equal(",Tokyo,Osaka", Texts(list));
    }

    [Fact]
    public void WhenWithEmptyWithTextThenTextIsUsedAndValueIsBlank()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name).WithEmpty("-- Select --");

        Assert.Equal("-- Select --", list[0].Text);
        Assert.Equal(string.Empty, list[0].Value);
    }

    [Fact]
    public void WhenWithEmptyWithTextAndValueThenBothAreUsed()
    {
        var list = Entities.ToSelectList(static x => x.Code, static x => x.Name).WithEmpty("All", "ALL");

        Assert.Equal("All", list[0].Text);
        Assert.Equal("ALL", list[0].Value);
    }

    [Fact]
    public void WhenWithEmptyThenSourceIsNotModified()
    {
        var source = Entities.ToSelectList(static x => x.Code, static x => x.Name);

        var list = source.WithEmpty();

        Assert.Equal(2, source.Count);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void WhenWithEmptyOnEmptyListThenOnlyBlankItemRemains()
    {
        var item = Assert.Single(((IEnumerable<string>?)null).ToSelectList().WithEmpty("-- Select --"));

        Assert.Equal("-- Select --", item.Text);
    }
}
