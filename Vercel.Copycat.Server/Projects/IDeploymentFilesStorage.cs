using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Projects;

public interface IDeploymentFilesStorage
{
    Task Upload(ProjectDocument projectDoc);
}

public class DeploymentFileAzureBlobStorage(
    BlobContainerClient containerClient, 
    DirectoriesConfig directories
) 
    : IDeploymentFilesStorage
{
    public async Task Upload(ProjectDocument projectDoc)
    {
        var path = $"{directories.GitDirectory}/{projectDoc.ProjectId()}/dist";
        var filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var filePath in filesPath)
        {
            var blobName = filePath.Replace($"{path}/", "").Replace("\\", "/");
            await using var fs = File.Open(filePath, FileMode.Open);
            var blob = containerClient.GetBlobClient($"{projectDoc.ProjectId()}/{blobName}");
            await blob.UploadAsync(fs);
        }
    }
}