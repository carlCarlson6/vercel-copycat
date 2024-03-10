using Vercel.Copycat.Server.Services.Upload;

namespace Vercel.Copycat.Server.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env) =>
        services
            .AddDirectoriesCreator(config)
            .AddUploadServices()
            .AddRebusMessaging(config)
            .AddMartenDb(config, env)
            .AddMediator();

    private static IServiceCollection AddDirectoriesCreator(this IServiceCollection services, IConfiguration config)
    {
        var directoriesConfig = new DirectoriesConfig();
        config.GetSection(nameof(DirectoriesConfig)).Bind(directoriesConfig);
        return services
            .AddSingleton(directoriesConfig)
            .AddHostedService<DirectoriesCreator>();
    }
}