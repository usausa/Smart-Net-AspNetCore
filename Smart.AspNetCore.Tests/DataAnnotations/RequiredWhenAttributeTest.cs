namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Model
//--------------------------------------------------------------------------------

public sealed class RequiredWhenModel
{
    public bool IsActive { get; set; }

    [RequiredWhen(nameof(IsActive))]
    public string? Name { get; set; }
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class RequiredWhenAttributeTest
{
    [Fact]
    public void WhenConditionIsFalseAndValueIsNullThenValidationSucceeds()
    {
        var model = new RequiredWhenModel { IsActive = false, Name = null };
        var context = ValidationContextHelper.Create(model, nameof(RequiredWhenModel.Name));
        var attribute = new RequiredWhenAttribute(nameof(RequiredWhenModel.IsActive)) { ErrorMessage = "{0} is required." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Name, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndValueIsNullThenValidationFails()
    {
        var model = new RequiredWhenModel { IsActive = true, Name = null };
        var context = ValidationContextHelper.Create(model, nameof(RequiredWhenModel.Name));
        var attribute = new RequiredWhenAttribute(nameof(RequiredWhenModel.IsActive)) { ErrorMessage = "{0} is required." };

        var result = attribute.GetValidationResult(model.Name, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenConditionIsTrueAndValueIsWhitespaceThenValidationFails()
    {
        var model = new RequiredWhenModel { IsActive = true, Name = "  " };
        var context = ValidationContextHelper.Create(model, nameof(RequiredWhenModel.Name));
        var attribute = new RequiredWhenAttribute(nameof(RequiredWhenModel.IsActive)) { ErrorMessage = "{0} is required." };

        var result = attribute.GetValidationResult(model.Name, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenConditionIsTrueAndValueIsWhitespaceAndAllowEmptyStringsThenValidationSucceeds()
    {
        var model = new RequiredWhenModel { IsActive = true, Name = "  " };
        var context = ValidationContextHelper.Create(model, nameof(RequiredWhenModel.Name));
        var attribute = new RequiredWhenAttribute(nameof(RequiredWhenModel.IsActive)) { AllowEmptyStrings = true, ErrorMessage = "{0} is required." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Name, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndValueIsSetThenValidationSucceeds()
    {
        var model = new RequiredWhenModel { IsActive = true, Name = "Alice" };
        var context = ValidationContextHelper.Create(model, nameof(RequiredWhenModel.Name));
        var attribute = new RequiredWhenAttribute(nameof(RequiredWhenModel.IsActive)) { ErrorMessage = "{0} is required." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Name, context));
    }
}
