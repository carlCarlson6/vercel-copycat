using Orleans.Concurrency;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeployment))]
public interface IDeployment : IGrainWithGuidKey
{
    [OneWay, Alias(nameof(Handle))]
    Task Handle(ProjectCreated projectCreated);
}

public class Deployment(
    IGrainFactory grains
) 
    : Grain, IDeployment
{
    public async Task Handle(ProjectCreated projectCreated)
    {
        var deploymentWorker = grains.GetGrain<IDeploymentWorker>(0);
        var project = grains.GetGrain<IProject>(projectCreated.ProjectId);
        
        var gitCommitInfo = await deploymentWorker.Execute(new ExecuteDeploymentCommand(projectCreated.ProjectId, projectCreated.RepoInfo));
        await project.Handle(new DeploymentCompleted(
            Guid.NewGuid(),
            projectCreated.ProjectId,
            this.GetGrainId().GetGuidKey(),
            DateTime.UtcNow, 
            gitCommitInfo));
    }
}

