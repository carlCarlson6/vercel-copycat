using Azure.Storage.Blobs;
using Orleans.Runtime;
using Orleans.Storage;
using Vercel.Copycat.Server.Deployments;
using Vercel.Copycat.Server.Deployments.Visit;
using Vercel.Copycat.Server.Deployments.Workers;
using Vercel.Copycat.Server.Infrastructure.RavenDb;
using Vercel.Copycat.Server.Infrastructure.Redis;

namespace Vercel.Copycat.Server.Infrastructure;

public static class ServiceCollectionExtensions
{
    public const string DbStorageName = "raven";
    public const string CacheStorageName = "cache";
    
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env) =>
        services
            .AddSingleton<DirectoriesManager>()
            .AddSingleton<VisitDeploymentAssestsMiddleware>()
            .AddDirectoriesManager(config)
            .AddSingleton<IGit, GitCli>()
            .AddSingleton<INpm, Npm>()
            .AddSingleton(new BlobContainerClient(config.GetConnectionString("blob-storage"), "apps"))
            .AddSingleton<IDeploymentFilesStorage, DeploymentFileAzureBlobStorage>()
            .AddRavenDb(config)
            .AddRedis(config)
            .AddGrainStorages();

    private static IServiceCollection AddDirectoriesManager(this IServiceCollection services, IConfiguration config)
    {
        var directoriesConfig = new DirectoriesConfig();
        config.GetSection(nameof(DirectoriesConfig)).Bind(directoriesConfig);
        return services
            .AddSingleton(directoriesConfig)
            .AddHostedService<DirectoriesManager>();
    }

    private static IServiceCollection AddGrainStorages(this IServiceCollection services) => services
        .AddKeyedSingleton<IGrainStorage, RavenDbGrainStateStorage>(DbStorageName)
        .AddKeyedSingleton<IGrainStorage, RedisGrainStateStorage>(CacheStorageName);
}