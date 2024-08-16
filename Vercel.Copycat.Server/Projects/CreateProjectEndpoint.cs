using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.Projects;

public static class CreateProjectEndpoint
{
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: "api/projects",
        handler: Handler);

    private static async Task<IResult> Handler([FromServices] IGrainFactory grains, [FromBody] CreateProjectRequest body)
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
    }
}

[Alias(nameof(CreateProjectRequest)), GenerateSerializer]
public record CreateProjectRequest(string RepoUrl, string Name, string BuildOutputPath = "build");

[Alias(nameof(CreateProjectResponse)), GenerateSerializer]
public record CreateProjectResponse(CreateProjectResponseResult Result, ProjectCreated Created);

[GenerateSerializer]
public enum CreateProjectResponseResult
{
    ProjectCreated,
    MissingData,
    Problem
}