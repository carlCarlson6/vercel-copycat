using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.Services.Upload;

public static class CreateDeploymentEndpoint
{
    private const string Route = "api/deploy";
    
    public static void MapCreateDeploymentEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: Route,
        handler: async ([FromServices] ISender sender, [FromBody] CreateDeploymentRequest body) =>
    {
        var response = await sender.Send(body);
        return response.MatchToEndpointResult();
    });

    private static IResult MatchToEndpointResult(this CreateDeploymentResponse response) => response.Match(
        created       => Results.Created("", null),
        missingData   => Results.BadRequest(),
        alreadyExists => Results.Problem());
}
