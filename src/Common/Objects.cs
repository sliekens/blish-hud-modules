using Microsoft.Extensions.DependencyInjection;

namespace SL.Common;

public static class Objects
{
    public static T Create<T>()
    {
        return ActivatorUtilities.CreateInstance<T>(ServiceLocator.ServiceProvider);
    }

    public static T Create<T>(params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<T>(ServiceLocator.ServiceProvider, parameters);
    }
}