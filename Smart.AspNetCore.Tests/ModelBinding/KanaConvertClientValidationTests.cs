namespace Smart.AspNetCore.ModelBinding;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Smart.Text.Japanese;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public sealed class KanaClientValidationForm
{
    [Required]
    [StringLength(10)]
    [KanaConvert(KanaOption.Wide)]
    public string? Converted { get; set; }

    [Required]
    [StringLength(10)]
    [KanaConvert(KanaOption.Wide, ClientValidation = true)]
    public string? ClientValidated { get; set; }

    [Required]
    [StringLength(10)]
    public string? NoConversion { get; set; }
}

//--------------------------------------------------------------------------------
// Tests
//--------------------------------------------------------------------------------

public sealed class KanaConvertClientValidationTests
{
    private static readonly IServiceProvider Services = CreateServices();

    private static IServiceProvider CreateServices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services
            .AddControllersWithViews()
            .AddKanaConvertModelBinder();

        return builder.Build().Services;
    }

    [Fact]
    public void WhenPropertyIsConvertedThenOnlyRequiredIsValidatedOnClient()
    {
        Assert.Equal(["RequiredAttribute"], GetClientValidators(nameof(KanaClientValidationForm.Converted)));
    }

    [Fact]
    public void WhenClientValidationIsEnabledThenAllRulesAreValidatedOnClient()
    {
        Assert.Equal(["RequiredAttribute", "StringLengthAttribute"], GetClientValidators(nameof(KanaClientValidationForm.ClientValidated)));
    }

    [Fact]
    public void WhenPropertyIsNotConvertedThenAllRulesAreValidatedOnClient()
    {
        Assert.Equal(["RequiredAttribute", "StringLengthAttribute"], GetClientValidators(nameof(KanaClientValidationForm.NoConversion)));
    }

    private static List<string> GetClientValidators(string propertyName)
    {
        var metadata = Services.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(typeof(KanaClientValidationForm), propertyName);
        var items = metadata.ValidatorMetadata.Select(static x => new ClientValidatorItem(x)).ToList();
        var provider = new CompositeClientModelValidatorProvider(Services.GetRequiredService<IOptions<MvcViewOptions>>().Value.ClientModelValidatorProviders);
        provider.CreateValidators(new ClientValidatorProviderContext(metadata, items));

        return items.Where(static x => x.Validator is not null).Select(static x => x.ValidatorMetadata).OfType<ValidationAttribute>().Select(static x => x.GetType().Name).Order(StringComparer.Ordinal).ToList();
    }
}
