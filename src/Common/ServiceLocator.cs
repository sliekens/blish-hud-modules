using Microsoft.Extensions.DependencyInjection;

namespace SL.Common;

// Antipattern or not, I got tired of passing services around
public static class ServiceLocator
{
    public static IServiceProvider? ServiceProvider { get; set; }

    public static T Resolve<T>()
    {
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(ServiceProvider);
    }
}