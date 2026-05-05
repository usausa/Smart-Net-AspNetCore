namespace Smart.AspNetCore.Binders;

using System.Collections.Generic;

using Microsoft.Extensions.Primitives;

// -----------------------------------------------------------------------
// Binder
// -----------------------------------------------------------------------

[BindConverter(typeof(SearchRequestConverter))]
internal static partial class SearchRequestBinder
{
    [Bind]
    [BindIgnoreMembers(nameof(SearchRequest.IgnoredByMethod))]
    public static partial SearchRequest BindQuery(Dictionary<string, StringValues> values);

    [Bind]
    public static partial void BindQueryToInstance(Dictionary<string, StringValues> values, SearchRequest target);

    [Bind]
    public static partial SearchRequest BindQueryReturnInstance(Dictionary<string, StringValues> values, SearchRequest target);
}

internal static partial class SimpleBinder
{
    [Bind]
    public static partial StringPropertyTarget BindStringProperty(Dictionary<string, StringValues> values);

    [Bind]
    public static partial ConvertPropertyTarget BindConvertProperty(Dictionary<string, StringValues> values);

    [Bind]
    public static partial ConvertNullablePropertyTarget BindConvertNullableProperty(Dictionary<string, StringValues> values);

    [Bind]
    public static partial StringArrayPropertyTarget BindStringArrayProperty(Dictionary<string, StringValues> values);

    [Bind]
    public static partial ConvertArrayPropertyTarget BindConvertArrayProperty(Dictionary<string, StringValues> values);

    [Bind]
    public static partial ConvertNullableArrayPropertyTarget BindConvertNullableArrayProperty(Dictionary<string, StringValues> values);
}

internal static partial class DictionaryExtensionBinder
{
    [Bind]
    public static partial SearchRequest BindToSearchRequest(this Dictionary<string, StringValues> values);

    [Bind]
    public static partial void BindToSearchRequestInstance(this Dictionary<string, StringValues> values, SearchRequest target);
}

// -----------------------------------------------------------------------
// Target
// -----------------------------------------------------------------------

internal sealed class SearchRequest
{
    public int Page { get; set; }

    public string? Keyword { get; set; }

    public int[] Tags { get; set; } = [];

    public PostStatus Status { get; set; }

    public string? IgnoredByMethod { get; set; }

    [BindIgnore]
    public string? IgnoredByProperty { get; set; }
}

internal enum PostStatus
{
    Draft,
    Published
}

internal static class SearchRequestConverter
{
    public static PostStatus ToPostStatus(ReadOnlySpan<char> value) =>
        value.Equals("published", StringComparison.OrdinalIgnoreCase) ? PostStatus.Published : PostStatus.Draft;
}

internal sealed class StringPropertyTarget
{
    public string? Value { get; set; }
}

internal sealed class ConvertPropertyTarget
{
    public int Value { get; set; }
}

internal sealed class ConvertNullablePropertyTarget
{
    public int? Value { get; set; }
}

internal sealed class StringArrayPropertyTarget
{
    public string?[]? Value { get; set; }
}

internal sealed class ConvertArrayPropertyTarget
{
    public int[]? Value { get; set; }
}

internal sealed class ConvertNullableArrayPropertyTarget
{
    public int?[]? Value { get; set; }
}

// -----------------------------------------------------------------------
// Test
// -----------------------------------------------------------------------

public sealed class BinderDictionaryTest
{
    [Fact]
    public void WhenBindingQueryThenAllPropertiesAreMapped()
    {
        // Arrange
        var values = new Dictionary<string, StringValues>
        {
            ["Page"] = "3",
            ["Keyword"] = "source-generator",
            ["Tags"] = new(["1", "2"]),
            ["Status"] = "published",
            ["IgnoredByMethod"] = "method",
            ["IgnoredByProperty"] = "property"
        };

        // Act
        var actual = SearchRequestBinder.BindQuery(values);

        // Assert
        Assert.Equal(3, actual.Page);
        Assert.Equal("source-generator", actual.Keyword);
        Assert.Equal([1, 2], actual.Tags);
        Assert.Equal(PostStatus.Published, actual.Status);
        Assert.Null(actual.IgnoredByMethod);
        Assert.Null(actual.IgnoredByProperty);
    }

    [Fact]
    public void WhenBindingToInstanceThenPropertiesAreMapped()
    {
        // Arrange
        var values = new Dictionary<string, StringValues>
        {
            ["Page"] = "5",
            ["Keyword"] = "instance-pattern"
        };
        var target = new SearchRequest();

        // Act
        SearchRequestBinder.BindQueryToInstance(values, target);

        // Assert
        Assert.Equal(5, target.Page);
        Assert.Equal("instance-pattern", target.Keyword);
    }

    [Fact]
    public void WhenBindingToInstanceWithReturnThenSameInstanceIsReturned()
    {
        // Arrange
        var values = new Dictionary<string, StringValues>
        {
            ["Page"] = "7"
        };
        var target = new SearchRequest();

        // Act
        var result = SearchRequestBinder.BindQueryReturnInstance(values, target);

        // Assert
        Assert.Same(target, result);
        Assert.Equal(7, result.Page);
    }

    [Fact]
    public void WhenBindingStringPropertyThenValueIsMapped()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues> { ["Value"] = "test" };

        // Act
        var target = SimpleBinder.BindStringProperty(dictionary);

        // Assert
        Assert.Equal("test", target.Value);
    }

    [Fact]
    public void WhenBindingConvertPropertyThenValueIsMapped()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues> { ["Value"] = "123" };

        // Act
        var target = SimpleBinder.BindConvertProperty(dictionary);

        // Assert
        Assert.Equal(123, target.Value);
    }

    [Fact]
    public void WhenBindingConvertNullablePropertyThenValueIsMapped()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues> { ["Value"] = "123" };

        // Act
        var target = SimpleBinder.BindConvertNullableProperty(dictionary);

        // Assert
        Assert.Equal(123, target.Value);
    }

    [Fact]
    public void WhenBindingStringArrayPropertyThenAllValuesAreMapped()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues>
        {
            ["Value"] = new(["abc", "cde"])
        };

        // Act
        var target = SimpleBinder.BindStringArrayProperty(dictionary);

        // Assert
        Assert.NotNull(target.Value);
        Assert.Equal(2, target.Value!.Length);
        Assert.Equal("abc", target.Value[0]);
        Assert.Equal("cde", target.Value[1]);
    }

    [Fact]
    public void WhenBindingConvertArrayPropertyThenAllValuesAreMapped()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues>
        {
            ["Value"] = new(["123", "456"])
        };

        // Act
        var target = SimpleBinder.BindConvertArrayProperty(dictionary);

        // Assert
        Assert.NotNull(target.Value);
        Assert.Equal([123, 456], target.Value!);
    }

    [Fact]
    public void WhenBindingConvertNullableArrayPropertyThenAllValuesAreMapped()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues>
        {
            ["Value"] = new(["123", "456"])
        };

        // Act
        var target = SimpleBinder.BindConvertNullableArrayProperty(dictionary);

        // Assert
        Assert.NotNull(target.Value);
        Assert.Equal<int?>([123, 456], target.Value!);
    }

    [Fact]
    public void WhenConvertPropertyCannotBeParsedThenDefaultValueIsReturned()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues> { ["Value"] = "abc" };

        // Act
        var target = SimpleBinder.BindConvertProperty(dictionary);

        // Assert
        Assert.Equal(default, target.Value);
    }

    [Fact]
    public void WhenConvertArrayPropertyContainsUnparseableValueThenDefaultElementIsReturned()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues>
        {
            ["Value"] = new(["123", "abc"])
        };

        // Act
        var target = SimpleBinder.BindConvertArrayProperty(dictionary);

        // Assert
        Assert.NotNull(target.Value);
        Assert.Equal(2, target.Value!.Length);
        Assert.Equal(123, target.Value[0]);
        Assert.Equal(default, target.Value[1]);
    }

    [Fact]
    public void WhenUsingExtensionMethodPatternThenPropertiesAreMapped()
    {
        // Arrange
        var values = new Dictionary<string, StringValues>
        {
            ["Page"] = "3",
            ["Keyword"] = "extension-method"
        };

        // Act
        var actual = values.BindToSearchRequest();

        // Assert
        Assert.Equal(3, actual.Page);
        Assert.Equal("extension-method", actual.Keyword);
    }

    [Fact]
    public void WhenUsingExtensionMethodInstancePatternThenTargetIsPopulated()
    {
        // Arrange
        var values = new Dictionary<string, StringValues>
        {
            ["Page"] = "9",
            ["Keyword"] = "extension-instance"
        };
        var target = new SearchRequest();

        // Act
        values.BindToSearchRequestInstance(target);

        // Assert
        Assert.Equal(9, target.Page);
        Assert.Equal("extension-instance", target.Keyword);
    }
}
