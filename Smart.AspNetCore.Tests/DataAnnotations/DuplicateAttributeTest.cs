namespace Smart.AspNetCore.DataAnnotations;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//--------------------------------------------------------------------------------
// Model
//--------------------------------------------------------------------------------

#pragma warning disable CA1002
#pragma warning disable CA2227
public sealed class TagItem
{
    public int Id { get; set; }

    public string? Name { get; set; }
}

public sealed class DuplicateModel
{
    [Duplicate<TagItem, int>(nameof(TagItem.Id), ErrorMessage = "Duplicate {0}.")]
    public List<TagItem>? Tags { get; set; }
}

public sealed class DuplicateCompositeModel
{
    [Duplicate<TagItem, int, string>(nameof(TagItem.Id), nameof(TagItem.Name), ErrorMessage = "Duplicate {0} {1}.")]
    public List<TagItem>? Tags { get; set; }
}
#pragma warning restore CA2227
#pragma warning restore CA1002

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class DuplicateAttributeTest
{
    //--------------------------------------------------------------------------------
    // Single key
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenAllIdsAreUniqueThenValidationSucceeds()
    {
        var model = new DuplicateModel
        {
            Tags = [new TagItem { Id = 1 }, new TagItem { Id = 2 }, new TagItem { Id = 3 }]
        };
        var context = ValidationContextHelper.Create(model, nameof(DuplicateModel.Tags));
        var attribute = new DuplicateAttribute<TagItem, int>(nameof(TagItem.Id))
        {
            ErrorMessage = "Duplicate {0}."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Tags, context));
    }

    [Fact]
    public void WhenDuplicateIdExistsThenValidationFails()
    {
        var model = new DuplicateModel
        {
            Tags = [new TagItem { Id = 1 }, new TagItem { Id = 2 }, new TagItem { Id = 1 }]
        };
        var context = ValidationContextHelper.Create(model, nameof(DuplicateModel.Tags));
        var attribute = new DuplicateAttribute<TagItem, int>(nameof(TagItem.Id))
        {
            ErrorMessage = "Duplicate {0}."
        };

        var result = attribute.GetValidationResult(model.Tags, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void WhenCollectionIsEmptyThenValidationSucceeds()
    {
        var model = new DuplicateModel { Tags = [] };
        var context = ValidationContextHelper.Create(model, nameof(DuplicateModel.Tags));
        var attribute = new DuplicateAttribute<TagItem, int>(nameof(TagItem.Id))
        {
            ErrorMessage = "Duplicate {0}."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Tags, context));
    }

    [Fact]
    public void WhenCollectionIsNullThenValidationSucceeds()
    {
        var model = new DuplicateModel { Tags = null };
        var context = ValidationContextHelper.Create(model, nameof(DuplicateModel.Tags));
        var attribute = new DuplicateAttribute<TagItem, int>(nameof(TagItem.Id))
        {
            ErrorMessage = "Duplicate {0}."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(null, context));
    }

    //--------------------------------------------------------------------------------
    // Composite key
    //--------------------------------------------------------------------------------

    [Fact]
    public void WhenCompositeKeysAreUniqueThenValidationSucceeds()
    {
        var model = new DuplicateCompositeModel
        {
            Tags =
            [
                new TagItem { Id = 1, Name = "A" },
                new TagItem { Id = 1, Name = "B" },
                new TagItem { Id = 2, Name = "A" }
            ]
        };
        var context = ValidationContextHelper.Create(model, nameof(DuplicateCompositeModel.Tags));
        var attribute = new DuplicateAttribute<TagItem, int, string>(nameof(TagItem.Id), nameof(TagItem.Name))
        {
            ErrorMessage = "Duplicate {0} {1}."
        };

        Assert.Equal(ValidationResult.Success, attribute.GetValidationResult(model.Tags, context));
    }

    [Fact]
    public void WhenCompositeKeyIsDuplicateThenValidationFails()
    {
        var model = new DuplicateCompositeModel
        {
            Tags =
            [
                new TagItem { Id = 1, Name = "A" },
                new TagItem { Id = 2, Name = "B" },
                new TagItem { Id = 1, Name = "A" }
            ]
        };
        var context = ValidationContextHelper.Create(model, nameof(DuplicateCompositeModel.Tags));
        var attribute = new DuplicateAttribute<TagItem, int, string>(nameof(TagItem.Id), nameof(TagItem.Name))
        {
            ErrorMessage = "Duplicate {0} {1}."
        };

        var result = attribute.GetValidationResult(model.Tags, context);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}
