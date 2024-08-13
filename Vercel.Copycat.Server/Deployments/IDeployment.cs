using Azure.Storage.Blobs;
using HeyRed.Mime;
using Orleans.Concurrency;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(IDeployment))]
public interface IDeployment : IGrainWithGuidKey
{
    [OneWay, Alias(nameof(Handle))]
    Task Handle(ProjectCreated projectCreated);

    [Alias(nameof(GetFile))]
    Task<DeploymentFile?> GetFile(string fileName);
}

public record DeploymentFile(Stream FileStream, string ContentType);

public class Deployment(
    [PersistentState(
        stateName: "deployment-files", 
        storageName: "deployments" )
    ] IPersistentState<List<string>> persistentStateFiles,
    IGrainFactory grains,
    IDeploymentFilesStorage deploymentFilesStorage
) 
    : Grain, IDeployment
{
    private Dictionary<string, BlobClient> _deploymentFiles = new();
    
    public async Task Handle(ProjectCreated projectCreated)
    {
        var deploymentWorker = grains.GetGrain<IDeploymentWorker>(0);
        var project = grains.GetGrain<IProject>(projectCreated.ProjectId);
        
        var (gitCommitInfo, deploymentFiles) = await deploymentWorker.Execute(new ExecuteDeploymentCommand(projectCreated.ProjectId, projectCreated.RepoInfo));

        _deploymentFiles = deploymentFiles;
        
        persistentStateFiles.State = deploymentFiles.Keys.ToList();
        await persistentStateFiles.WriteStateAsync();
        
        await project.Handle(new DeploymentCompleted(
            Guid.NewGuid(),
            projectCreated.ProjectId,
            this.GetGrainId().GetGuidKey(),
            DateTime.UtcNow, 
            gitCommitInfo));
    }

    public async Task<DeploymentFile?> GetFile(string fileName)
    {
        _deploymentFiles.TryGetValue(fileName, out var blob);
        if (blob is null) return null;

        var blobStream = await blob.DownloadStreamingAsync();
        return new DeploymentFile(blobStream.Value.Content, GetContentType(blob));
    }

    public override Task OnActivateAsync(CancellationToken ct)
    {
        if (!persistentStateFiles.RecordExists)
            persistentStateFiles.State = [];

        foreach (var fileName in persistentStateFiles.State)
        {
            var blob = deploymentFilesStorage.GetBlob(this.GetGrainId().GetGuidKey(), fileName);
            _deploymentFiles.Add(fileName, blob);    
        }
        
        return base.OnActivateAsync(ct);
    }
    
    private static string GetContentType(BlobClient blob)
    {
        var fileExtension = blob.Name.Split(".").LastOrDefault() ?? string.Empty;
        return MimeTypesMap.GetMimeType(fileExtension);
    }
}

