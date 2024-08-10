using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Projects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUploadServices(this IServiceCollection services, IConfiguration config) => services
        .AddSingleton<IGit, GitCli>()
        .AddSingleton<IBuilder, Builder>()
        .AddSingleton<IDeploymentFilesStorage>(s => new DeploymentFileAzureBlobStorage(
            containerClient: new BlobContainerClient(config.GetConnectionString("blob-storage"), "apps"), 
            directories:     s.GetRequiredService<DirectoriesConfig>()));
}