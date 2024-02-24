using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.Services.Upload;

public static class UploadRepoEndpoint
{
    private const string EndpointPath = "api/upload";

    public static void MapUploadRepoEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: EndpointPath, 
        handler: async ([FromBody] UploadRepoRequestBody requestBody) =>
        {
            return Results.Created();
        });
}

public record UploadRepoRequestBody(string Url);