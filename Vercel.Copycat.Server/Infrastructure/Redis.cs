using System.Text;
using Mediator;
using static System.String;
using static System.Text.Json.JsonSerializer;
using StackExchange.Redis;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Services.Build;
using static Vercel.Copycat.Server.Infrastructure.RedisMessaging;

namespace Vercel.Copycat.Server.Infrastructure;

public class RedisConfig
{
    public string UserName { get; set; } = Empty;
    public string HostName { get; set; } = Empty;
    public string Password { get; set; } = Empty;
    public string Port { get; set; } = Empty;
}

public static class Redis
{
    public static IServiceCollection AddRedisCore(this IServiceCollection services, IConfiguration config)
    {
        var redisConfig = new RedisConfig();
        config.GetSection(nameof(RedisConfig)).Bind(redisConfig);

        var connection = new StringBuilder()
            .AppendJoin(",", 
                $"{redisConfig.HostName}:{redisConfig.Port}",
                $"password={redisConfig.Password}",
                $"user={redisConfig.UserName}",
                "allowAdmin=true")
            .ToString();
        
        var multiplexer = ConnectionMultiplexer.Connect(connection);
        return services
            .AddSingleton<IDatabase>(_ => multiplexer.GetDatabase())
            .AddSingleton<ISubscriber>(_ => multiplexer.GetSubscriber());
    }
}

public static class RedisMessaging
{
    public const string Queue = "vercerl-copycat-queue";
    
    public static IServiceCollection AddRedisMessaging(this IServiceCollection services) => services
        .AddSingleton<IBus, RedisBus>()
        .AddHostedService<RedisConsumerBackgroundService>();
}

public class RedisBus(ISubscriber subscriber) : IBus
{
    public async Task<Unit> Publish(Event @event)
    {
        var redisEvent = new RedisEventMessage(
            @event.TypeName,
            @event.ToSerializedMessage());
        await subscriber.PublishAsync(Queue, Serialize(redisEvent));
        return Unit.Value;
    }
}

public record RedisEventMessage(string Type, string Content)
{
    public Event ToDomainEvent() => Type switch
    {
        nameof(ProjectCreated)           => new Event(Deserialize<ProjectCreated>(Content)),
        nameof(DeploymentCodeUploaded)   => new Event(Deserialize<DeploymentCodeUploaded>(Content)),
        _ => throw new ArgumentException(),
    };
}

// TODO
// - add logs
// - improve nullability
// - handle errors / exceptions
public class RedisConsumerBackgroundService(
    ISubscriber subscriber, 
    ISender sender, 
    ILogger<RedisConsumerBackgroundService> logger
) 
    : BackgroundService, IEventConsumer
{
    protected override async Task ExecuteAsync(CancellationToken ct) => await subscriber.SubscribeAsync(Queue, 
        async (_, message) =>
    {
        try
        {
            var redisEvent = Deserialize<RedisEventMessage>(message!);
            var @event = redisEvent?.ToDomainEvent() ?? throw new ArgumentException();
            await Consume(@event);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
    });

    public ValueTask<Unit> Consume(Event @event) => @event.Match(
            projectCreated => sender.Send(projectCreated),
            deploymentCodeUploaded => sender.Send(deploymentCodeUploaded)
        );
}