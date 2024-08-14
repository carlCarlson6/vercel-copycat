using System.Text;
using StackExchange.Redis;

namespace Vercel.Copycat.Server.Infrastructure.Redis;

public class RedisConfig
{
    public string UserName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
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
        return services.AddSingleton<IDatabase>(_ => multiplexer.GetDatabase());
    }
}