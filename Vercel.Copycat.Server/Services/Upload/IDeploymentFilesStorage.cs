using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Services.Upload;

public interface IDeploymentFilesStorage
{
    Task Upload(DeploymentDocument deploymentDoc);
}

public class DeploymentFileAzureBlobStorage: IDeploymentFilesStorage
{
    private readonly BlobClient _blob;
    private readonly DirectoriesConfig _directories;
    
    public DeploymentFileAzureBlobStorage(BlobContainerClient containerClient, DirectoriesConfig directories)
    {
        _blob = containerClient.GetBlobClient("repo-files");
        _directories = directories;
    }
    
    public async Task Upload(DeploymentDocument deploymentDoc)
    {
        var filesPath = $"{_directories.GitDirectory}/{deploymentDoc.DeploymentId()}";
        await _blob.UploadAsync(path: filesPath, overwrite: true);
        
        throw new NotImplementedException();
    }
}