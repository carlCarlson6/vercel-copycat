using Mediator;
using OneOf;
using Orleans.Concurrency;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Deployments;
using static System.String;

namespace Vercel.Copycat.Server.Projects;

[Alias(nameof(IProject))]
public interface IProject : IGrainWithGuidKey
{
    [Alias(nameof(Create))]
    Task<CreateProjectResponse> Create(CreateProjectRequest request);

    [OneWay, Alias(nameof(Handle))]
    Task Handle(DeploymentCompleted deploymentCompleted);
}

public class Project(
    [PersistentState(
        stateName: "project-status", 
        storageName: "projects")
    ] IPersistentState<ProjectStatus> persistentProjectState,
    IGrainFactory grains,
    ILogger<Project> logger
) 
    : Grain, IProject
{
    public async Task<CreateProjectResponse> Create(CreateProjectRequest request)
    {
        logger.LogInformation("handling create project request");
        
        var (repoUrl, projectName, buildOutputPath) = request;
        if (IsNullOrWhiteSpace(repoUrl) || IsNullOrWhiteSpace(projectName))
        {
            logger.LogWarning("missing data on the request");
            return new MissingData();
        }
        
        var projectCreated = new ProjectCreated(
            Guid.NewGuid(), 
            this.GetGrainId().GetGuidKey(),
            DateTime.UtcNow, 
            new RepoInfo(projectName, buildOutputPath));
        persistentProjectState.State = new ProjectStatus(
            repoUrl, 
            projectCreated.RepoInfo, 
            null, 
            [projectCreated]
        );
        
        await persistentProjectState.WriteStateAsync();
        
        await grains
            .GetGrain<IDeployment>(Guid.NewGuid())
            .Handle(projectCreated);
        
        return projectCreated;
    }

    public async Task Handle(DeploymentCompleted deploymentCompleted)
    {
        persistentProjectState.State = persistentProjectState.State with
        {
            CurrentDeploymentId = deploymentCompleted.DeploymentId,
            Events = persistentProjectState.State.Events.Append(deploymentCompleted)
        };
        await persistentProjectState.WriteStateAsync();
    }
}

public record ProjectStatus(string Name, RepoInfo RepoInfo, Guid? CurrentDeploymentId, IEnumerable<IEvent> Events);
public record RepoInfo(string RepoUrl, string BuildOutputPath);

public record CreateProjectRequest(string RepoUrl, string Name, string BuildOutputPath = "build") : IRequest<CreateProjectResponse>;

[GenerateOneOf]
public partial class CreateProjectResponse : OneOfBase<
    ProjectCreated, MissingData, ProblemCreatingProject
>;

public readonly struct MissingData;
public readonly struct ProblemCreatingProject;