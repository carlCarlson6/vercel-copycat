using Azure.Storage.Blobs;
using Orleans.Runtime;
using Orleans.Storage;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Deployments.Visit;
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
            .AddDirectoriesCreator(config)
            .AddSingleton<Cli>()
            .AddSingleton<IGit, GitCli>()
            .AddSingleton<INpm, Npm>()
            .AddSingleton(new BlobContainerClient(config.GetConnectionString("blob-storage"), "apps"))
            .AddSingleton<IDeploymentFilesStorage, DeploymentFileAzureBlobStorage>()
            .AddRavenDb(config)
            .AddSingletonNamedService<IGrainStorage, RavenDbGrainStateStorage>(DbStorageName) // TODO - check if work for singleton keyed service on alst version of orleans
            .AddRedis(config)
            .AddSingletonNamedService<IGrainStorage, RedisGrainStateStorage>(CacheStorageName)
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