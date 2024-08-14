using Orleans.Concurrency;
using Orleans.Runtime;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Projects;
using static Vercel.Copycat.Server.Infrastructure.ServiceCollectionExtensions;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeployment))]
public interface IDeployment : IGrainWithGuidKey
{
    [OneWay, Alias(nameof(Handle))]
    Task Handle(ExecuteNewDeployment command);
    
    [OneWay, Alias(nameof(Handle))]
    Task Handle(ProjectCreated projectCreated);

    [Alias(nameof(GetFile))]
    Task<DeploymentFile?> GetFile(string fileName);
}

[GenerateSerializer]
public record DeploymentFile(Uri FileUri);

public class Deployment(
    [PersistentState(
        stateName: "deployment-files", 
        storageName: CacheStorageName )
    ] IPersistentState<List<string>> persistentStateFiles,
    IGrainFactory grains,
    IDeploymentFilesStorage deploymentFilesStorage,
    ILogger<Deployment> logger
) 
    : Grain, IDeployment
{
    public Task Handle(ExecuteNewDeployment command)
    {
        throw new NotImplementedException();
    }

    public async Task Handle(ProjectCreated projectCreated)
    {
        logger.LogInformation("handling project created");
        
        var deploymentWorker = grains.GetGrain<IDeploymentWorker>(0);
        var project = grains.GetGrain<IProject>(projectCreated.ProjectId);
        
        logger.LogInformation("initiating first deployment");
        var (gitCommitInfo, deploymentFiles) = await deploymentWorker
            .Execute(new ExecuteDeploymentCommand(this.GetGrainId().GetGuidKey(), projectCreated.RepoInfo));
        
        logger.LogInformation("updating deployment status");
        persistentStateFiles.State = deploymentFiles;
        await persistentStateFiles.WriteStateAsync();
        
        logger.LogInformation("dispatching event deployment completed");
        await project.Handle(new DeploymentCompleted(
            Guid.NewGuid(),
            projectCreated.ProjectId,
            this.GetGrainId().GetGuidKey(),
            DateTime.UtcNow, 
            gitCommitInfo));
        logger.LogInformation("deployment completed");
    }

    public async Task<DeploymentFile?> GetFile(string fileName)
    {
        logger.LogInformation("requested file {FileName} for deployment", fileName);
        if (!persistentStateFiles.State.Contains(fileName))
        {
            logger.LogWarning("no file for the deployment");
            return null;
        } 
        
        logger.LogInformation("downloading file");
        var blob = deploymentFilesStorage.GetBlob(this.GetGrainId().GetGuidKey(), fileName);
        // TODO not download just redirect
        var blobStream = await blob.DownloadStreamingAsync(); 
        logger.LogInformation("returning stream to the client");
        return new DeploymentFile(blob.Uri);
    }

    public override Task OnActivateAsync(CancellationToken ct)
    {
        if (!persistentStateFiles.RecordExists)
            persistentStateFiles.State = [];
        return base.OnActivateAsync(ct);
    }
}

public record ExecuteNewDeployment;