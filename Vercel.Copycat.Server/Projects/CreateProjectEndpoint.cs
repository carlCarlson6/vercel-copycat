using Microsoft.AspNetCore.Mvc;
using Vercel.Copycat.Server.Core;

namespace Vercel.Copycat.Server.Projects;

public static class CreateProjectEndpoint
{
    private const string Route = "api/projects";
    
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: Route,
        handler: Handler);

    private static async Task<IResult> Handler([FromServices] IGrainFactory grains, [FromBody] CreateProjectRequest body)
    {
        var project = grains.GetGrain<IProject>(Guid.NewGuid());
        var response = await project.Create(body);
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
    }
}

[GenerateSerializer]
public record CreateProjectRequest(string RepoUrl, string Name, string BuildOutputPath = "build");

[GenerateSerializer]
public record CreateProjectResponse(CreateProjectResponseResult Result, ProjectCreated? Created);

[GenerateSerializer]
public enum CreateProjectResponseResult
{
    ProjectCreated,
    MissingData,
    Problem
}