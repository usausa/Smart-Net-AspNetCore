namespace Smart.AspNetCore.Routing;

public sealed class RouteValuesTest
{
    public sealed class SampleModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void FromCreatesDictionaryWithPropertyNamesAndValues()
    {
        var values = RouteValues.From(new SampleModel { Id = 5, Name = "abc" });

        Assert.Equal(5, values["Id"]);
        Assert.Equal("abc", values["Name"]);
    }

    [Fact]
    public void FromWithPathPrefixesKeys()
    {
        var values = RouteValues.From("model", new SampleModel { Id = 5, Name = "abc" });

        Assert.Equal(5, values["model.Id"]);
        Assert.Equal("abc", values["model.Name"]);
    }
}
