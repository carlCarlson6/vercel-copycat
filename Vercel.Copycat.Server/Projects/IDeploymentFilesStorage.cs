using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Projects;

public interface IDeploymentFilesStorage
{
    Task Upload(ProjectDocument projectDoc);
}

public class DeploymentFileAzureBlobStorage(BlobContainerClient containerClient, DirectoriesConfig directories)
    : IDeploymentFilesStorage
{
    private readonly BlobClient _blob = containerClient.GetBlobClient("repo-files");
    private readonly BlobContainerClient _blobContainerClient = containerClient;

    public async Task Upload(ProjectDocument projectDoc)
    {
        
        var filesPath = $"{directories.GitDirectory}/{projectDoc.ProjectId()}";
        await _blob.UploadAsync(path: filesPath, overwrite: true);
        
        throw new NotImplementedException();
    }
}