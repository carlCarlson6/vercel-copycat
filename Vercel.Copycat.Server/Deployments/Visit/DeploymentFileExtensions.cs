using Azure.Storage.Blobs;

namespace Vercel.Copycat.Server.Deployments.Visit;

public static class DeploymentFileExtensions
{
    public static async Task<IResult> ToApiResponse(this DeploymentFile? maybeDeploymentFile)
    {
        if (maybeDeploymentFile is null) return Results.NotFound();
        var blobStream = await new BlobClient(maybeDeploymentFile.BlobUri)
            .DownloadStreamingAsync();
        return Results.File(blobStream.Value.Content, maybeDeploymentFile.ContentType);
    }
}