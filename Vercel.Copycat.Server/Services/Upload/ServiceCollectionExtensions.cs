namespace Vercel.Copycat.Server.Services.Upload;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUploadServices(this IServiceCollection services) => services
        .AddSingleton<IGit, GitCli>()
        .AddSingleton<IDeploymentFilesStorage, DeploymentFileAzureBlobStorage>();
}