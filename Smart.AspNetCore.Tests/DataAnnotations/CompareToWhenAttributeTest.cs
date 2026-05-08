namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Model
//--------------------------------------------------------------------------------

public sealed class CompareToWhenModel
{
    public bool IsEnabled { get; set; }

    public int Min { get; set; }

    [CompareToWhen(nameof(IsEnabled), CompareToOperation.GreaterEqualThan, nameof(Min))]
    public int Max { get; set; }
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class CompareToWhenAttributeTest
{
    [Fact]
    public void WhenConditionIsFalseAndMaxIsLessThanMinThenValidationSucceeds()
    {
        var model = new CompareToWhenModel { IsEnabled = false, Min = 10, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToWhenModel.Max));
        var attribute = new CompareToWhenAttribute(nameof(CompareToWhenModel.IsEnabled), CompareToOperation.GreaterEqualThan, nameof(CompareToWhenModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Max, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndMaxIsGreaterThanMinThenValidationSucceeds()
    {
        var model = new CompareToWhenModel { IsEnabled = true, Min = 1, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToWhenModel.Max));
        var attribute = new CompareToWhenAttribute(nameof(CompareToWhenModel.IsEnabled), CompareToOperation.GreaterEqualThan, nameof(CompareToWhenModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Max, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndMaxEqualsMinThenGreaterEqualValidationSucceeds()
    {
        var model = new CompareToWhenModel { IsEnabled = true, Min = 5, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToWhenModel.Max));
        var attribute = new CompareToWhenAttribute(nameof(CompareToWhenModel.IsEnabled), CompareToOperation.GreaterEqualThan, nameof(CompareToWhenModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Max, context));
    }

    [Fact]
    public void WhenConditionIsTrueAndMaxIsLessThanMinThenValidationFails()
    {
        var model = new CompareToWhenModel { IsEnabled = true, Min = 10, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToWhenModel.Max));
        var attribute = new CompareToWhenAttribute(nameof(CompareToWhenModel.IsEnabled), CompareToOperation.GreaterEqualThan, nameof(CompareToWhenModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        var result = attribute.GetValidationResult(model.Max, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}
