namespace Smart.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Smart.AspNetCore.ModelBinding;

public static class MvcBuilderExtensions
{
    //--------------------------------------------------------------------------------
    // Kana
    //--------------------------------------------------------------------------------

    public static IMvcBuilder AddKanaConvertModelBinder(this IMvcBuilder builder)
    {
        builder.Services.TryAddSingleton<KanaConvertModelBinderProvider>();
        builder.Services.TryAddSingleton<KanaConvertClientModelValidatorProvider>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, KanaConvertMvcOptionsSetup>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<MvcViewOptions>, KanaConvertMvcViewOptionsSetup>());
        return builder;
    }

    private sealed class KanaConvertMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly KanaConvertModelBinderProvider provider;

        public KanaConvertMvcOptionsSetup(KanaConvertModelBinderProvider provider)
        {
            this.provider = provider;
        }

        public void Configure(MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, provider);
        }
    }

    private sealed class KanaConvertMvcViewOptionsSetup : IPostConfigureOptions<MvcViewOptions>
    {
        private readonly KanaConvertClientModelValidatorProvider provider;

        public KanaConvertMvcViewOptionsSetup(KanaConvertClientModelValidatorProvider provider)
        {
            this.provider = provider;
        }

        public void PostConfigure(string? name, MvcViewOptions options)
        {
            options.ClientModelValidatorProviders.Add(provider);
        }
    }
}
