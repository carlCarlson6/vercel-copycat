using Azure.Storage.Blobs;

namespace Vercel.Copycat.Server.Core;

public interface IDeploymentFilesStorage
{
    Task Upload(Guid projectId, string buildOutputPath);
}

public class DeploymentFileAzureBlobStorage(
    BlobContainerClient containerClient, 
    DirectoriesConfig directories
) 
    : IDeploymentFilesStorage
{
    public async Task Upload(Guid projectId, string buildOutputPath)
    {
        var path = $"{directories.GitDirectory}/{projectId}/{buildOutputPath}";
        var filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var filePath in filesPath)
        {
            var blobName = filePath.Replace($"{path}/", "").Replace("\\", "/");
            var blob = containerClient.GetBlobClient($"{projectId}/{blobName}");
            await using var fs = File.Open(filePath, FileMode.Open);
            await blob.UploadAsync(fs, overwrite: true);
        }
    }
}