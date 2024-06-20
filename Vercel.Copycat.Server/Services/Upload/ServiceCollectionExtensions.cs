using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Services.Upload;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUploadServices(this IServiceCollection services, IConfiguration config) => services
        .AddSingleton<IGit, GitCli>()
        .AddSingleton<IDeploymentFilesStorage>(s => new DeploymentFileAzureBlobStorage(
            containerClient: new BlobContainerClient(config.GetConnectionString("blob-storage"), "repo-files"), 
            directories:     s.GetRequiredService<DirectoriesConfig>()));
}