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

public sealed class CompareToWhenMismatchModel
{
    public bool IsEnabled { get; set; }

    public int Value { get; set; }

    public string Other { get; set; } = "x";
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

    [Fact]
    public void WhenConditionIsTrueAndComparedTypesDifferThenThrowsInvalidOperation()
    {
        var model = new CompareToWhenMismatchModel { IsEnabled = true, Value = 5, Other = "x" };
        var context = ValidationContextHelper.Create(model, nameof(CompareToWhenMismatchModel.Value));
        var attribute = new CompareToWhenAttribute(nameof(CompareToWhenMismatchModel.IsEnabled), CompareToOperation.GreaterThan, nameof(CompareToWhenMismatchModel.Other)) { ErrorMessage = "{0} vs {1}" };

        Assert.Throws<InvalidOperationException>(() => attribute.GetValidationResult(model.Value, context));
    }
}
