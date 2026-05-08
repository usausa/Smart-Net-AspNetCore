namespace Smart.AspNetCore.ModelBinding;

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Smart.AspNetCore.DataAnnotations;
using Smart.Text.Japanese;

// -----------------------------------------------------------------------
// Models
// -----------------------------------------------------------------------

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

public sealed class KanaValidationRequest
{
    [KanaConvert(KanaOption.HankanaToKatakana)]
    [Ms932Length(10)]
    public string? Name { get; set; }
}

[ApiController]
[Route("[controller]")]
public sealed class KanaValidationController : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromForm] KanaValidationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        return Ok(new { request.Name });
    }
}

// -----------------------------------------------------------------------
// Helper
// -----------------------------------------------------------------------

internal static class KanaTestWebAppFactory
{
    public static HttpClient CreateClient()
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
}

// -----------------------------------------------------------------------
// Test
// -----------------------------------------------------------------------

public sealed class KanaModelBinderTest
{
    private static readonly HttpClient Client = KanaTestWebAppFactory.CreateClient();

    // -- Query --

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

        // No conversion should occur.
        Assert.Equal("ｱｲｳ", json.GetProperty("noConversion").GetString());
    }

    [Fact]
    public async Task WhenMultiplePropertiesHaveDifferentOptionsThenEachIsConvertedIndependently()
    {
        var ct = TestContext.Current.CancellationToken;

        // ｱｲｳ (hankana) for HankanaToKatakana, アイウ (katakana) for KatakanaToHankana
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

    // -- Form --

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

// -----------------------------------------------------------------------
// DataAnnotations Tests
// -----------------------------------------------------------------------

public sealed class KanaDataAnnotationsTest
{
    [Fact]
    public void WhenStringIsWithinMs932LengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(10);
        var context = new ValidationContext(new object()) { MemberName = "Name" };

        // "アイウ" = 3 chars × 2 bytes (Shift_JIS) = 6 bytes → within limit
        var result = attribute.GetValidationResult("アイウ", context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenStringExceedsMs932MaxLengthThenValidationFails()
    {
        var attribute = new Ms932LengthAttribute(4);
        var context = new ValidationContext(new object()) { MemberName = "Name" };

        // "アイウ" = 6 bytes > 4 → should fail
        var result = attribute.GetValidationResult("アイウ", context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenStringIsExactlyMs932MaxLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(6);
        var context = new ValidationContext(new object()) { MemberName = "Name" };

        // "アイウ" = exactly 6 bytes
        var result = attribute.GetValidationResult("アイウ", context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenStringIsBelowMs932MinLengthThenValidationFails()
    {
        var attribute = new Ms932LengthAttribute(4, 10);
        var context = new ValidationContext(new object()) { MemberName = "Name" };

        // "ア" = 2 bytes < 4 → should fail
        var result = attribute.GetValidationResult("ア", context);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenValueIsNullThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(2);
        var context = new ValidationContext(new object()) { MemberName = "Name" };

        var result = attribute.GetValidationResult(null, context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenAsciiStringIsWithinLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(5);
        var context = new ValidationContext(new object()) { MemberName = "Name" };

        // "Hello" = 5 bytes (ASCII = 1 byte each)
        var result = attribute.GetValidationResult("Hello", context);

        Assert.Equal(ValidationResult.Success, result);
    }
}
