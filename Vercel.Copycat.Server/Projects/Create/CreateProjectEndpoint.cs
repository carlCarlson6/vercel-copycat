using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.Projects.Create;

public static class CreateProjectEndpoint
{
    private const string Route = "api/projects";
    
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: Route,
        handler: async ([FromServices] ISender sender, [FromBody] CreateProjectRequest body) =>
    {
        var response = await sender.Send(body);
        return response.Match(
            created     => Results.Created(created.ProjectId.ToString(), null),
            missingData => Results.BadRequest(),
            project     => Results.Problem());
    });
}
