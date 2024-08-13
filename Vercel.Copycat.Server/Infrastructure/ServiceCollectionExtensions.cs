using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.VisitDeployment;

namespace Vercel.Copycat.Server.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env) =>
        services
            .AddSingleton<VisitDeploymentAssestsMiddleware>()
            .AddDirectoriesCreator(config)
            .AddSingleton<Cli>()
            .AddSingleton<IGit, GitCli>()
            .AddSingleton<IBuilder, Builder>()
            .AddSingleton(new BlobContainerClient(config.GetConnectionString("blob-storage"), "apps"))
            .AddSingleton<IDeploymentFilesStorage, DeploymentFileAzureBlobStorage>()
            .AddRedis(config)
            .AddMediator(x => x.ServiceLifetime = ServiceLifetime.Singleton);

    private static IServiceCollection AddDirectoriesCreator(this IServiceCollection services, IConfiguration config)
    {
        var directoriesConfig = new DirectoriesConfig();
        config.GetSection(nameof(DirectoriesConfig)).Bind(directoriesConfig);
        return services
            .AddSingleton(directoriesConfig)
            .AddHostedService<DirectoriesManager>();
    }
}