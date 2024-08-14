using Mediator;
using Microsoft.AspNetCore.Mvc;
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
        return response.Result switch
        {
            CreateProjectResponseResult.ProjectCreated => Results.Created(response.Created!.ProjectId.ToString(), new
            {
                ProjectId = response.Created!.ProjectId.ToString()
            }),
            CreateProjectResponseResult.MissingData => Results.BadRequest(),
            CreateProjectResponseResult.Problem => Results.Problem(),
            _ => throw new ArgumentOutOfRangeException()
        };
    });
}

[GenerateSerializer]
public record CreateProjectRequest(string RepoUrl, string Name, string BuildOutputPath = "build") : IRequest<CreateProjectResponse>;

[GenerateSerializer]
public record CreateProjectResponse(CreateProjectResponseResult Result, ProjectCreated? Created);

public enum CreateProjectResponseResult
{
    ProjectCreated,
    MissingData,
    Problem
}