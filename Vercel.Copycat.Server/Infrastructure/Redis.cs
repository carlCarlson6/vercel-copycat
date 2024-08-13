using System.Text;
using static System.String;
using StackExchange.Redis;

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
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration config)
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