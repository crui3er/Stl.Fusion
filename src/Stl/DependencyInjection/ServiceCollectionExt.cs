using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Stl.DependencyInjection.Internal;

namespace Stl.DependencyInjection;

public static class ServiceCollectionExt
{
    // HasService

    public static bool HasService<TService>(this IServiceCollection services)
        => services.HasService(typeof(TService));
    public static bool HasService(this IServiceCollection services, Type serviceType)
        => services.Any(d => d.ServiceType == serviceType);

    // Options

    public static IServiceCollection Configure<TOptions>(
        this IServiceCollection services,
        Action<IServiceProvider, string?, TOptions> configureOptions)
        where TOptions : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));
        services.AddOptions();
        services.TryAddSingleton<IConfigureOptions<TOptions>>(
            c => new ConfigureAllNamedOptions<TOptions>(c, configureOptions));
        return services;
    }

    // Settings

    public static IServiceCollection AddSettings<TSettings>(
        this IServiceCollection services,
        string? sectionName = null)
        => services.AddSettings(typeof(TSettings), sectionName);
    public static IServiceCollection AddSettings(
        this IServiceCollection services,
        Type settingsType,
        string? sectionName = null)
    {
        var altSectionName = (string?) null;
        if (sectionName == null) {
            sectionName = settingsType.Name;
            var plusIndex = sectionName.IndexOf('+');
            if (plusIndex >= 0)
                sectionName = sectionName[(plusIndex + 1)..];
            altSectionName = sectionName.TrimSuffix("Settings", "Cfg", "Config", "Configuration");
        }
        services.TryAddSingleton(settingsType, c => {
            var settings = c.Activate(settingsType);
            var cfg = c.GetRequiredService<IConfiguration>();
            var section = cfg.GetSection(sectionName);
            if (!section.Exists() && altSectionName != null)
                section = cfg.GetSection(altSectionName);
            section.Bind(settings);
            var validationContext = new ValidationContext(settings, c, null);
            Validator.ValidateObject(settings, validationContext);
            return settings;
        });
        return services;
    }
}
