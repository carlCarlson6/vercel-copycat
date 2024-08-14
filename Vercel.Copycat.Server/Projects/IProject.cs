using Orleans.Concurrency;
using Orleans.Runtime;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Deployments;
using static System.String;
using static Vercel.Copycat.Server.Infrastructure.ServiceCollectionExtensions;

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
        storageName: DbStorageName)
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
            return new CreateProjectResponse(CreateProjectResponseResult.MissingData, null);
        }
        
        var projectCreated = new ProjectCreated(
            Guid.NewGuid(), 
            this.GetGrainId().GetGuidKey(),
            DateTime.UtcNow, 
            new RepoInfo(repoUrl, buildOutputPath));
        persistentProjectState.State = new ProjectStatus(
            projectName, 
            projectCreated.RepoInfo, 
            null, 
            [projectCreated]
        );
        await persistentProjectState.WriteStateAsync();
        
        logger.LogInformation("project created dispatching event");
        await grains
            .GetGrain<IDeployment>(Guid.NewGuid())
            .Handle(projectCreated);
        
        logger.LogInformation("project creation completed");
        return new CreateProjectResponse(CreateProjectResponseResult.ProjectCreated, projectCreated);
    }

    public async Task Handle(DeploymentCompleted deploymentCompleted)
    {
        logger.LogInformation("handling deployment completed event");
        persistentProjectState.State = persistentProjectState.State with
        {
            CurrentDeploymentId = deploymentCompleted.DeploymentId,
            Events = persistentProjectState.State.Events.Append(deploymentCompleted)
        };
        await persistentProjectState.WriteStateAsync();
        await grains
            .GetGrain<ICurrentDeployment>(this.GetGrainId().GetGuidKey())
            .SetDeployment(deploymentCompleted.DeploymentId);
        logger.LogInformation("state updated with last deployment status");
    }
}

public record ProjectStatus(string Name, RepoInfo RepoInfo, Guid? CurrentDeploymentId, IEnumerable<IEvent> Events);

[GenerateSerializer]
public record RepoInfo(string RepoUrl, string BuildOutputPath);