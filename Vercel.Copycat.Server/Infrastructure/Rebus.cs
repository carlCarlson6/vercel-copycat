using Rebus.Config;
using Rebus.Retry.Simple;

namespace Vercel.Copycat.Server.Infrastructure;

public static class Rebus
{
    public static IServiceCollection AddRebusMessaging(this IServiceCollection services, IConfiguration config) => services
        .AutoRegisterHandlersFromAssemblyOf<Program>()
        .AddRebus(c => c
            .Transport(t =>t.UseAzureStorageQueues(config.GetConnectionString("azure-queues")!, "vercelcopycat", new AzureStorageQueuesTransportOptions
            {
                AutomaticallyCreateQueues = true
            }))
            .Options(o => o.RetryStrategy(maxDeliveryAttempts: 1))
        );
}