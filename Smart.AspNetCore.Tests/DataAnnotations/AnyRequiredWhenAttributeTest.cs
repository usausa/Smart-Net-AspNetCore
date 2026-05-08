namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Model
//--------------------------------------------------------------------------------

[AnyRequiredWhen(nameof(IsRequired), nameof(Email), nameof(Phone), ErrorMessage = "Either {0} is required.")]
public sealed class AnyRequiredWhenModel
{
    public bool IsRequired { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class AnyRequiredWhenAttributeTest
{
    [Fact]
    public void WhenConditionIsFalseAndBothNullThenValidationSucceeds()
    {
        var model = new AnyRequiredWhenModel { IsRequired = false, Email = null, Phone = null };
        var context = ValidationContextHelper.Create(model);
        var attribute = new AnyRequiredWhenAttribute(nameof(AnyRequiredWhenModel.IsRequired), nameof(AnyRequiredWhenModel.Email), nameof(AnyRequiredWhenModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndAtLeastOneIsSetThenValidationSucceeds()
    {
        var model = new AnyRequiredWhenModel { IsRequired = true, Email = "a@b.com", Phone = null };
        var context = ValidationContextHelper.Create(model);
        var attribute = new AnyRequiredWhenAttribute(nameof(AnyRequiredWhenModel.IsRequired), nameof(AnyRequiredWhenModel.Email), nameof(AnyRequiredWhenModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndBothNullThenValidationFails()
    {
        var model = new AnyRequiredWhenModel { IsRequired = true, Email = null, Phone = null };
        var context = ValidationContextHelper.Create(model);
        var attribute = new AnyRequiredWhenAttribute(nameof(AnyRequiredWhenModel.IsRequired), nameof(AnyRequiredWhenModel.Email), nameof(AnyRequiredWhenModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        var result = attribute.GetValidationResult(model, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenValueIsNullThenValidationSucceeds()
    {
        var context = ValidationContextHelper.Create(new AnyRequiredWhenModel());
        var attribute = new AnyRequiredWhenAttribute(nameof(AnyRequiredWhenModel.IsRequired), nameof(AnyRequiredWhenModel.Email), nameof(AnyRequiredWhenModel.Phone))
        {
            ErrorMessage = "Either {0} is required."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(null, context));
    }
}
