namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class Ms932LengthAttributeTest
{
    private static ValidationContext MakeContext(string memberName = "Value") =>
        new(new object()) { MemberName = memberName };

    [Fact]
    public void WhenNullThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(10);
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(null, MakeContext()));
    }

    [Fact]
    public void WhenAsciiStringIsWithinMaxLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(5);
        // "Hello" = 5 bytes (ASCII = 1 byte each)
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult("Hello", MakeContext()));
    }

    [Fact]
    public void WhenStringIsWithinMaxLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(10);
        // "アイウ" = 3 chars × 2 bytes (Shift_JIS) = 6 bytes
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult("アイウ", MakeContext()));
    }

    [Fact]
    public void WhenStringIsExactlyMaxLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(6);
        // "アイウ" = exactly 6 bytes
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult("アイウ", MakeContext()));
    }

    [Fact]
    public void WhenStringExceedsMaxLengthThenValidationFails()
    {
        var attribute = new Ms932LengthAttribute(4);
        // "アイウ" = 6 bytes > 4
        var result = attribute.GetValidationResult("アイウ", MakeContext());
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenStringMeetsMinLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(4, 10);
        // "アイウ" = 6 bytes >= 4
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult("アイウ", MakeContext()));
    }

    [Fact]
    public void WhenStringIsBelowMinLengthThenValidationFails()
    {
        var attribute = new Ms932LengthAttribute(4, 10);
        // "ア" = 2 bytes < 4
        var result = attribute.GetValidationResult("ア", MakeContext());
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenStringIsExactlyMinLengthThenValidationSucceeds()
    {
        var attribute = new Ms932LengthAttribute(2, 10);
        // "ア" = exactly 2 bytes
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult("ア", MakeContext()));
    }
}
