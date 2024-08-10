using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env) =>
        services
            .AddDirectoriesCreator(config)
            .AddUploadServices(config)
            .AddRedisCore(config)
            .AddRedisMessaging()
            .AddMediator(x => x.ServiceLifetime = ServiceLifetime.Singleton);

    private static IServiceCollection AddDirectoriesCreator(this IServiceCollection services, IConfiguration config)
    {
        var directoriesConfig = new DirectoriesConfig();
        config.GetSection(nameof(DirectoriesConfig)).Bind(directoriesConfig);
        return services
            .AddSingleton(directoriesConfig)
            .AddHostedService<DirectoriesCreator>();
    }
}