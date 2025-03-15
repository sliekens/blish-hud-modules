using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace SL.ChatLinks;
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFactoryDelegate<TDelegate>(this IServiceCollection serviceCollecton)
        where TDelegate : Delegate
    {
        Type delegateType = typeof(TDelegate);

        // The invoke method is what will be called when we try to use the factory delegate
        MethodInfo invokeMethod = delegateType.GetMethod("Invoke");

        if (invokeMethod.ReturnType == typeof(void))
        {
            throw new ArgumentException("The delegate must have a return type.", nameof(TDelegate));
        }

        // Create the factory based on the type we want to create and the parameters of the delegate
        ObjectFactory factory = ActivatorUtilities.CreateFactory(invokeMethod.ReturnType, [.. invokeMethod.GetParameters().Select(p => p.ParameterType)]);
        ConstantExpression factoryExpression = Expression.Constant(factory);
        MethodInfo factoryMethod = typeof(ObjectFactory).GetMethod("Invoke");

        // The factory delegate takes an IServiceProvider and a parameters array (object[]), 
        // so we'll need to cast our parameters to object
        ParameterExpression[] parameterExpressions = [.. invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType))];
        IEnumerable<UnaryExpression> objectParameterExpressions = parameterExpressions.Select(p => Expression.TypeAs(p, typeof(object)));

        // Build our object[] array expression
        NewArrayExpression arrayExpression = Expression.NewArrayInit(typeof(object), objectParameterExpressions);

        // Create the factory method call, passing the service provider and parameters array
        ParameterExpression serviceProviderParameterExpression = Expression.Parameter(typeof(IServiceProvider));
        MethodCallExpression factoryCallExpression = Expression.Call(factoryExpression, factoryMethod, serviceProviderParameterExpression, arrayExpression);

        // The factory method returns object, so we need to cast that to the return type
        UnaryExpression resultConversionExpression = Expression.Convert(factoryCallExpression, invokeMethod.ReturnType);

        // Now we can construct our delegate that takes the parameters and returns the instantiated type
        Expression<TDelegate> delegateLambdaExpression = Expression.Lambda<TDelegate>(resultConversionExpression, parameterExpressions);

        // Finally we need to wrap that up in a lambda method
        // that takes an IServiceProvider and returns the delegate as an object:
        Expression<Func<IServiceProvider, object>> delegateConstructor = Expression.Lambda<Func<IServiceProvider, object>>(delegateLambdaExpression, serviceProviderParameterExpression);

        // Compile the delegate metod factory lambda
        Func<IServiceProvider, object> compiledDelegateFactory = delegateConstructor.Compile();

        // Register the factory against the container
        serviceCollecton.Add(new ServiceDescriptor(typeof(TDelegate), compiledDelegateFactory, ServiceLifetime.Singleton));

        return serviceCollecton;
    }
}
