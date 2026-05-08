namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Model
//--------------------------------------------------------------------------------

[AnyRequired(nameof(Email), nameof(Phone), ErrorMessage = "Either {0} is required.")]
public sealed class AnyRequiredModel
{
    public string? Email { get; set; }

    public string? Phone { get; set; }
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class AnyRequiredAttributeTest
{
    [Fact]
    public void WhenAtLeastOnePropertyIsSetThenValidationSucceeds()
    {
        var model = new AnyRequiredModel { Email = "a@b.com", Phone = null };
        var context = ValidationContextHelper.Create(model);
        var attribute = new AnyRequiredAttribute(nameof(AnyRequiredModel.Email), nameof(AnyRequiredModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model, context));
    }

    [Fact]
    public void WhenAllPropertiesAreSetThenValidationSucceeds()
    {
        var model = new AnyRequiredModel { Email = "a@b.com", Phone = "123" };
        var context = ValidationContextHelper.Create(model);
        var attribute = new AnyRequiredAttribute(nameof(AnyRequiredModel.Email), nameof(AnyRequiredModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model, context));
    }

    [Fact]
    public void WhenAllPropertiesAreNullThenValidationFails()
    {
        var model = new AnyRequiredModel { Email = null, Phone = null };
        var context = ValidationContextHelper.Create(model);
        var attribute = new AnyRequiredAttribute(nameof(AnyRequiredModel.Email), nameof(AnyRequiredModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        var result = attribute.GetValidationResult(model, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenValueIsNullThenValidationSucceeds()
    {
        var context = ValidationContextHelper.Create(new AnyRequiredModel());
        var attribute = new AnyRequiredAttribute(nameof(AnyRequiredModel.Email), nameof(AnyRequiredModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(null, context));
    }
}
