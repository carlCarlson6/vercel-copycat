using Azure.Storage.Blobs;

namespace Vercel.Copycat.Server.Projects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddSingleton<IGit, GitCli>()
            .AddSingleton<IBuilder, Builder>()
            .AddSingleton(new BlobContainerClient(config.GetConnectionString("blob-storage"), "apps"))
            .AddSingleton<IDeploymentFilesStorage, DeploymentFileAzureBlobStorage>();
    }
}