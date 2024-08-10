using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.VisitDeployment;

public static class VisitDeploymentEndpoint
{
    public static void MapVisitDeploymentEndpoint(this IEndpointRouteBuilder builder) => builder.MapGet(
        pattern: "project/{projectId}",
        handler: async (
            [FromServices] BlobContainerClient containerClient, 
            [FromRoute] string projectId, 
            HttpContext ctx) =>
        {
            var blob = containerClient.GetBlobClient($"{projectId}/index.html");
            var streaming =  await blob.DownloadStreamingAsync();
            
            ctx.Response.Cookies.Append("visit-deployment", projectId);
            return Results.File(streaming.Value.Content, contentType: "text/html");
        });
}