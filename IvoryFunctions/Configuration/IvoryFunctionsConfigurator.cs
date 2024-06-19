using MassTransit;

namespace IvoryFunctions.Configuration;

public delegate void ConfigureMassTransitAction(IBusRegistrationConfigurator configurator,
    Action<IBusRegistrationContext, IBusFactoryConfigurator<IReceiveEndpointConfigurator>> configure);

public interface IIvoryFunctionsConfigurator
{
    IIvoryFunctionsConfigurator ConfigureMassTransit(ConfigureMassTransitAction configure);
    internal void AddInternalConfiguration<T>(T configuration);
    internal T? GetInternalConfiguration<T>();
}

internal class IvoryFunctionsConfigurator : IIvoryFunctionsConfigurator
{
    internal ConfigureMassTransitAction? MassTransit;
    private readonly Dictionary<string, object> _configurations = new();

    public IIvoryFunctionsConfigurator ConfigureMassTransit(ConfigureMassTransitAction configure)
    {
        MassTransit = configure;
        return this;
    }

    public void AddInternalConfiguration<T>(T configuration)
    {
        _configurations.Add(typeof(T).Name, configuration);
    }

    public T? GetInternalConfiguration<T>()
    {
        return (T?)_configurations.GetValueOrDefault(typeof(T).Name);
    }
}