using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Vercel.Copycat.Server.Core;

namespace Vercel.Copycat.Server.Projects;

public static class CreateProjectEndpoint
{
    private const string Route = "api/projects";
    
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: Route,
        handler: async ([FromServices] IGrainFactory grains, [FromBody] CreateProjectRequest body) =>
    {
        var response = await grains.GetGrain<IProject>(Guid.NewGuid()).Create(body); 
        return response.Match(
            created     => Results.Created(created.ProjectId.ToString(), null),
            missingData => Results.BadRequest(),
            project     => Results.Problem());
    });
}

public record CreateProjectRequest(string RepoUrl, string Name, string BuildOutputPath = "build") : IRequest<CreateProjectResponse>;

[GenerateOneOf]
public partial class CreateProjectResponse : OneOfBase<
    ProjectCreated, 
    MissingData, 
    ProblemCreatingProject
>;

public readonly struct MissingData;
public readonly struct ProblemCreatingProject;