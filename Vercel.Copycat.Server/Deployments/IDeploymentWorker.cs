using Azure.Storage.Blobs;
using Orleans.Concurrency;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeploymentWorker))]
public interface IDeploymentWorker : IGrainWithIntegerKey
{
    [Alias(nameof(Execute))]
    Task<ExecutedDeploymentInfo> Execute(ExecuteDeploymentCommand command);
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
    public async Task<ExecutedDeploymentInfo> Execute(ExecuteDeploymentCommand command)
    {
        var (deploymentId, (repoUrl, buildOutputPath)) = command;
        
        directoriesManager.Create(deploymentId);
        var gitCommitInfo = await git.Clone(deploymentId, repoUrl);
        await builder.BuildProject(deploymentId);
        var uploadedFiles = await storage.Upload(deploymentId, buildOutputPath);
        directoriesManager.Delete(deploymentId);
        return new ExecutedDeploymentInfo(gitCommitInfo, uploadedFiles);
    }
}

public record ExecutedDeploymentInfo(GitCommitInfo GitCommitInfo, Dictionary<string, BlobClient> DeploymentFiles);

public record ExecuteDeploymentCommand(Guid DeploymentId, RepoInfo RepoInfo);