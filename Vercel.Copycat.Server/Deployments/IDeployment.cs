using Azure.Storage.Blobs;
using HeyRed.Mime;
using Orleans.Concurrency;
using Orleans.Runtime;
using Vercel.Copycat.Server.Deployments.Workers;
using Vercel.Copycat.Server.Projects;
using static Vercel.Copycat.Server.Infrastructure.ServiceCollectionExtensions;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeployment))]
public interface IDeployment : IGrainWithGuidKey
{
    [OneWay, Alias($"{nameof(Handle)}.{nameof(ExecuteNewDeployment)}")]
    Task Handle(ExecuteNewDeployment command);
    
    [OneWay, Alias($"{nameof(Handle)}.{nameof(ProjectCreated)}")]
    Task Handle(ProjectCreated projectCreated);
    
    [Alias(nameof(GetFile))]
    Task<DeploymentFile?> GetFile(string fileName);
}

[Alias(nameof(DeploymentFile)), GenerateSerializer]
public record DeploymentFile(Uri BlobUri, string ContentType);

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

[Alias(nameof(ExecuteNewDeployment)), GenerateSerializer]
public record ExecuteNewDeployment;