using Azure.Storage.Blobs;
using HeyRed.Mime;

namespace Vercel.Copycat.Server.VisitDeployment;

public class VisitDeploymentAssestsMiddleware(BlobContainerClient containerClient) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.Cookies.TryGetValue("visit-deployment", out var projectId);
        if (string.IsNullOrWhiteSpace(projectId))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.Value!.Contains("api")|| context.Request.Path.Value.Contains("project"))
        {
            await next(context);
            return;
        }
        
        var blob = containerClient.GetBlobClient($"{projectId}/{context.Request.Path}".Replace("//", "/"));
        var fileExtension = blob.Name.Split(".").LastOrDefault() ?? string.Empty;
        var contentType = MimeTypesMap.GetMimeType(fileExtension);
        
        var streaming =  await blob.DownloadStreamingAsync();
        var result = Results.File(streaming.Value.Content, contentType);
        await result.ExecuteAsync(context);
    }
}