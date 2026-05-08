namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Model
//--------------------------------------------------------------------------------

public sealed class CompareToModel
{
    public int Min { get; set; }

    [CompareTo(CompareToOperation.GreaterEqualThan, nameof(Min))]
    public int Max { get; set; }

    public DateTime StartDate { get; set; }

    [CompareTo(CompareToOperation.GreaterThan, nameof(StartDate))]
    public DateTime EndDate { get; set; }
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class CompareToAttributeTest
{
    [Fact]
    public void WhenMaxIsGreaterThanMinThenValidationSucceeds()
    {
        var model = new CompareToModel { Min = 1, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.Max));
        var attribute = new CompareToAttribute(CompareToOperation.GreaterEqualThan, nameof(CompareToModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Max, context));
    }

    [Fact]
    public void WhenMaxEqualsMinThenGreaterEqualValidationSucceeds()
    {
        var model = new CompareToModel { Min = 5, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.Max));
        var attribute = new CompareToAttribute(CompareToOperation.GreaterEqualThan, nameof(CompareToModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Max, context));
    }

    [Fact]
    public void WhenMaxIsLessThanMinThenValidationFails()
    {
        var model = new CompareToModel { Min = 10, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.Max));
        var attribute = new CompareToAttribute(CompareToOperation.GreaterEqualThan, nameof(CompareToModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        var result = attribute.GetValidationResult(model.Max, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenEndDateIsAfterStartDateThenGreaterThanValidationSucceeds()
    {
        var model = new CompareToModel
        {
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 2)
        };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.EndDate));
        var attribute = new CompareToAttribute(CompareToOperation.GreaterThan, nameof(CompareToModel.StartDate)) { ErrorMessage = "{0} must be > {1}." };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.EndDate, context));
    }

    [Fact]
    public void WhenEndDateEqualsStartDateThenGreaterThanValidationFails()
    {
        var date = new DateTime(2024, 1, 1);
        var model = new CompareToModel { StartDate = date, EndDate = date };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.EndDate));
        var attribute = new CompareToAttribute(CompareToOperation.GreaterThan, nameof(CompareToModel.StartDate)) { ErrorMessage = "{0} must be > {1}." };

        var result = attribute.GetValidationResult(model.EndDate, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenValueIsLessThanOtherAndLessThanOperationThenValidationSucceeds()
    {
        var model = new CompareToModel { Min = 3, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.Min));
        var attribute = new CompareToAttribute(CompareToOperation.LessThan, nameof(CompareToModel.Max)) { ErrorMessage = "{0} must be < {1}." };

        // Min(3) < Max(5) → succeeds
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Min, context));
    }

    [Fact]
    public void WhenValueIsNullThenValidationSucceeds()
    {
        var model = new CompareToModel { Min = 1, Max = 5 };
        var context = ValidationContextHelper.Create(model, nameof(CompareToModel.Max));
        var attribute = new CompareToAttribute(CompareToOperation.GreaterEqualThan, nameof(CompareToModel.Min)) { ErrorMessage = "{0} must be >= {1}." };

        // null value is not IComparable → succeeds
        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(null, context));
    }
}
