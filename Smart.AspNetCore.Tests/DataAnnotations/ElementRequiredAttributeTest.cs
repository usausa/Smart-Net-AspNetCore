namespace Smart.AspNetCore.DataAnnotations;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class ElementRequiredAttributeTest
{
    private static ValidationContext MakeContext(string memberName = "Items") =>
        new(new object()) { MemberName = memberName };

    [Fact]
    public void WhenNullThenValidationFails()
    {
        var attribute = new ElementRequiredAttribute();
        Assert.NotEqual(ValidationResult.Success, attribute.GetValidationResult(null, MakeContext()));
    }

    [Fact]
    public void WhenEmptyCollectionThenValidationFails()
    {
        var attribute = new ElementRequiredAttribute();
        var result = attribute.GetValidationResult(new List<string>(), MakeContext());
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenCollectionHasNullElementThenValidationFails()
    {
        var attribute = new ElementRequiredAttribute();
        var result = attribute.GetValidationResult(new List<string?> { "a", null }, MakeContext());
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenCollectionHasWhitespaceElementThenValidationFails()
    {
        var attribute = new ElementRequiredAttribute();
        var result = attribute.GetValidationResult(new List<string> { "a", "  " }, MakeContext());
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenCollectionHasWhitespaceAndAllowEmptyStringsThenValidationSucceeds()
    {
        var attribute = new ElementRequiredAttribute { AllowEmptyStrings = true };
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(new List<string> { "a", "  " }, MakeContext()));
    }

    [Fact]
    public void WhenCollectionHasAllValidElementsThenValidationSucceeds()
    {
        var attribute = new ElementRequiredAttribute();
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(new List<string> { "a", "b", "c" }, MakeContext()));
    }

    [Fact]
    public void WhenNonCollectionValueThenValidationSucceeds()
    {
        var attribute = new ElementRequiredAttribute();
        // Non-IEnumerable value (e.g. int) always passes.
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(42, MakeContext()));
    }
}
