using Azure.Storage.Blobs;
using Vercel.Copycat.Server.Deployments.Workers;

namespace Vercel.Copycat.Server.Deployments;

public interface IDeploymentFilesStorage
{
    Task<Dictionary<string, BlobClient>> Upload(Guid deploymentId, string buildOutputPath);
    BlobClient GetBlob(Guid deploymentId, string fileName);
}

public class DeploymentFileAzureBlobStorage(
    BlobContainerClient containerClient, 
    DirectoriesConfig directories
) 
    : IDeploymentFilesStorage
{
    public async Task<Dictionary<string, BlobClient>> Upload(Guid deploymentId, string buildOutputPath)
    {
        var uploadedFiles = new Dictionary<string, BlobClient>();
        var path = $"{directories.GitDirectory}/{deploymentId}/{buildOutputPath}";
        var filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var filePath in filesPath)
        {
            var fileName = filePath.Replace($"{path}/", "").Replace("\\", "/");
            var blobName = $"{deploymentId}/{fileName}";
            
            var blob = containerClient.GetBlobClient(blobName);
            await using var fs = File.Open(filePath, FileMode.Open);
            await blob.UploadAsync(fs, overwrite: true);
            
            uploadedFiles.Add(fileName, blob);
        }
        
        return uploadedFiles;
    }

    public BlobClient GetBlob(Guid deploymentId, string fileName) => containerClient
        .GetBlobClient($"{deploymentId}/{fileName}");
}