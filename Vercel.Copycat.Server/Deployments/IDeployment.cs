using Azure.Storage.Blobs;
using HeyRed.Mime;
using Orleans.Concurrency;
using Vercel.Copycat.Server.Deployments.Workers;
using Vercel.Copycat.Server.Projects;
using static Vercel.Copycat.Server.Infrastructure.ServiceCollectionExtensions;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeployment))]
public interface IDeployment : IGrainWithGuidKey
{
    [OneWay, Alias($"{nameof(Handle)}.{nameof(ExecuteNewDeploymentCommand)}")]
    Task Handle(ExecuteNewDeploymentCommand command);
    
    [Alias(nameof(GetFile))]
    Task<DeploymentFile?> GetFile(string fileName);
}

[Alias(nameof(DeploymentFile)), GenerateSerializer]
public record DeploymentFile(Uri BlobUri, string ContentType);

public class Deployment(
    [PersistentState(
        stateName: "deployment-files", 
        storageName: CacheStorageName)
    ] IPersistentState<List<string>> persistentStateFiles,
    IGrainFactory grains,
    IDeploymentFilesStorage deploymentFilesStorage,
    ILogger<Deployment> logger
) 
    : Grain, IDeployment
{
    public async Task Handle(ExecuteNewDeploymentCommand command)
    {
        var (projectId, repoInfo) = command;
        logger.LogInformation("handling execute new deployment command");
        
        
        logger.LogInformation("initiating deployment");
        var (gitCommitInfo, deploymentFiles) = await grains
            .GetGrain<IDeploymentWorker>(0)
            .Execute(new ExecuteDeploymentCommand(this.GetGrainId().GetGuidKey(), repoInfo));
        
        logger.LogInformation("updating deployment status");
        persistentStateFiles.State = deploymentFiles;
        await persistentStateFiles.WriteStateAsync();

        logger.LogInformation("dispatching event deployment completed");
        await grains.GetGrain<IProject>(projectId).Handle(DeploymentCompleted.Default with
        {
            ProjectId = projectId,
            DeploymentId = this.GetGrainId().GetGuidKey(),
            GitCommitInfo = gitCommitInfo
        });
        logger.LogInformation("deployment completed");
    }
    
    public Task<DeploymentFile?> GetFile(string fileName)
    {
        logger.LogInformation("requested file {FileName} for deployment", fileName);
        
        var startsWithSlash = fileName.StartsWith('/');
        var formatedFileName = startsWithSlash ? fileName[1..] : fileName;
        
        if (!persistentStateFiles.State.Contains(formatedFileName))
        {
            logger.LogWarning("no file for the deployment");
            return Task.FromResult<DeploymentFile?>(null);
        } 
        
        logger.LogInformation("downloading file");
        var blob = deploymentFilesStorage.GetBlob(this.GetGrainId().GetGuidKey(), formatedFileName);
        logger.LogInformation("returning stream to the client");
        return Task.FromResult<DeploymentFile?>(new DeploymentFile(blob.Uri, GetContentType(blob)));
    }

    public override Task OnActivateAsync(CancellationToken ct)
    {
        if (!persistentStateFiles.RecordExists)
            persistentStateFiles.State = [];
        return base.OnActivateAsync(ct);
    }
    
    private static string GetContentType(BlobClient blob)
    {
        var fileExtension = blob.Name.Split(".").LastOrDefault() ?? string.Empty;
        return MimeTypesMap.GetMimeType(fileExtension);
    }
}

[Alias(nameof(ExecuteNewDeploymentCommand)), GenerateSerializer]
public record ExecuteNewDeploymentCommand(Guid ProjectId, RepoInfo RepoInfo);