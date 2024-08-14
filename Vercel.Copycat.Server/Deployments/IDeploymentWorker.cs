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
    INpm npm,
    IDeploymentFilesStorage storage,
    DirectoriesManager directoriesManager,
    ILogger<DeploymentWorker> logger
) 
    : Grain, IDeploymentWorker
{
    public async Task<ExecutedDeploymentInfo> Execute(ExecuteDeploymentCommand command)
    {
        var (deploymentId, (repoUrl, buildOutputPath)) = command;
        
        logger.LogInformation("creating deployment folder on working directory");
        directoriesManager.Create(deploymentId);
        
        logger.LogInformation("cloning repo");
        var gitCommitInfo = await git.Clone(deploymentId, repoUrl);
        
        logger.LogInformation("installing dependencies");
        await npm.InstallDependencies(deploymentId);
        
        logger.LogInformation("building code");
        await npm.BuildProject(deploymentId);
        
        logger.LogInformation("uploading files");
        var uploadedFiles = await storage.Upload(deploymentId, buildOutputPath);
        
        logger.LogInformation("cleaning up local resources");
        directoriesManager.Delete(deploymentId);
        
        return new ExecutedDeploymentInfo(gitCommitInfo, uploadedFiles.Keys.ToList());
    }
}

[GenerateSerializer]
public record ExecutedDeploymentInfo(GitCommitInfo GitCommitInfo, List<string> FileNames);

[GenerateSerializer]
public record ExecuteDeploymentCommand(Guid DeploymentId, RepoInfo RepoInfo);