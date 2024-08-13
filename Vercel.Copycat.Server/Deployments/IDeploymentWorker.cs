using Orleans.Concurrency;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeploymentWorker))]
public interface IDeploymentWorker : IGrainWithIntegerKey
{
    [Alias(nameof(Execute))]
    Task<GitCommitInfo> Execute(ExecuteDeploymentCommand command);
}

[StatelessWorker(maxLocalWorkers: 3)]
public class DeploymentWorker(
    IGit git,
    IBuilder builder,
    IDeploymentFilesStorage storage,
    DirectoriesManager directoriesManager
) 
    : Grain, IDeploymentWorker
{
    public async Task<GitCommitInfo> Execute(ExecuteDeploymentCommand command)
    {
        var (projectId, (repoUrl, buildOutputPath)) = command;
        
        directoriesManager.Create(projectId);
        var gitCommitInfo = await git.Clone(projectId, repoUrl);
        await builder.BuildProject(projectId);
        await storage.Upload(projectId, buildOutputPath);
        directoriesManager.Delete(projectId);
        return gitCommitInfo;
    }
}

public record ExecuteDeploymentCommand(Guid ProjectId, RepoInfo RepoInfo);