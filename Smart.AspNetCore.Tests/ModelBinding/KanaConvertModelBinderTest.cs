namespace Smart.AspNetCore.ModelBinding;

using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Smart.Text.Japanese;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public sealed class KanaQueryRequest
{
    [KanaConvert(KanaOption.HankanaToKatakana)]
    public string? HankanaToKatakana { get; set; }

    [KanaConvert(KanaOption.KatakanaToHankana)]
    public string? KatakanaToHankana { get; set; }

    public string? NoConversion { get; set; }
}

[ApiController]
[Route("[controller]")]
public sealed class KanaController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromQuery] KanaQueryRequest request) => Ok(new
    {
        request.HankanaToKatakana,
        request.KatakanaToHankana,
        request.NoConversion
    });

    [HttpPost]
    public IActionResult Post([FromForm] KanaQueryRequest request) => Ok(new
    {
        request.HankanaToKatakana,
        request.KatakanaToHankana,
        request.NoConversion
    });
}

//--------------------------------------------------------------------------------
// Tests
//--------------------------------------------------------------------------------

public sealed class KanaConvertModelBinderTest
{
    private static readonly HttpClient Client = CreateClient();

    private static HttpClient CreateClient()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(KanaController).Assembly)
            .AddKanaConvertModelBinder();

        var app = builder.Build();
        app.MapControllers();
        app.StartAsync().GetAwaiter().GetResult();

        return app.GetTestClient();
    }

    //--------------------------------------------------------------------------------
    // Query
    //--------------------------------------------------------------------------------

    [Fact]
    public async Task WhenHankanaQueryIsReceivedThenConvertedToKatakana()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await Client.GetAsync(
            new Uri("/Kana?HankanaToKatakana=%EF%BD%B1%EF%BD%B2%EF%BD%B3", UriKind.Relative), ct); // ｱｲｳ
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal("アイウ", json.GetProperty("hankanaToKatakana").GetString());
    }

    [Fact]
    public async Task WhenKatakanaQueryIsReceivedThenConvertedToHankana()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await Client.GetAsync(
            new Uri("/Kana?KatakanaToHankana=%E3%82%A2%E3%82%A4%E3%82%A6", UriKind.Relative), ct); // アイウ
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal("ｱｲｳ", json.GetProperty("katakanaToHankana").GetString());
    }

    [Fact]
    public async Task WhenPropertyHasNoKanaAttributeThenValueIsNotConverted()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await Client.GetAsync(
            new Uri("/Kana?NoConversion=%EF%BD%B1%EF%BD%B2%EF%BD%B3", UriKind.Relative), ct); // ｱｲｳ (hankana)
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal("ｱｲｳ", json.GetProperty("noConversion").GetString());
    }

    [Fact]
    public async Task WhenMultiplePropertiesHaveDifferentOptionsThenEachIsConvertedIndependently()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await Client.GetAsync(
            new Uri("/Kana?HankanaToKatakana=%EF%BD%B1%EF%BD%B2%EF%BD%B3&KatakanaToHankana=%E3%82%A2%E3%82%A4%E3%82%A6", UriKind.Relative),
            ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal("アイウ", json.GetProperty("hankanaToKatakana").GetString());
        Assert.Equal("ｱｲｳ", json.GetProperty("katakanaToHankana").GetString());
    }

    [Fact]
    public async Task WhenNullQueryIsReceivedThenNullIsReturned()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await Client.GetAsync(new Uri("/Kana", UriKind.Relative), ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal(JsonValueKind.Null, json.GetProperty("hankanaToKatakana").ValueKind);
    }

    //--------------------------------------------------------------------------------
    // Form
    //--------------------------------------------------------------------------------

    [Fact]
    public async Task WhenHankanaFormIsPostedThenConvertedToKatakana()
    {
        var ct = TestContext.Current.CancellationToken;
        using var form = new FormUrlEncodedContent([new("HankanaToKatakana", "ｱｲｳ")]);

        var response = await Client.PostAsync(new Uri("/Kana", UriKind.Relative), form, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal("アイウ", json.GetProperty("hankanaToKatakana").GetString());
    }
}
