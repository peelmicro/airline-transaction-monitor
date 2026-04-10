using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Hosting;

namespace Common.Messaging;

/// <summary>
/// Extension methods to register NATS client and messaging services.
/// </summary>
public static class NatsServiceExtensions
{
    public static IServiceCollection AddNatsMessaging(this IServiceCollection services, string natsUrl = "nats://localhost:4222")
    {
        // Register the NATS client connection
        services.AddNats(configureOpts: opts => opts with { Url = natsUrl });

        // Register publisher and subscriber as the port interfaces
        services.AddSingleton<IEventPublisher, NatsEventPublisher>();
        services.AddSingleton<IEventSubscriber, NatsEventSubscriber>();

        return services;
    }
}
