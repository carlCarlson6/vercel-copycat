using Marten;
using Rebus.Bus;
using Rebus.Handlers;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Services.Build;

namespace Vercel.Copycat.Server.Services.Upload;

public record DeploymentRequestCreated(string DeploymentId);

// ReSharper disable once UnusedType.Global
public class DeploymentRequestCreatedHandler: IHandleMessages<DeploymentRequestCreated>
{
    private readonly IQuerySession _query;
    private readonly IGit _git;
    private readonly IDeploymentFilesStorage _deploymentFilesStorage;
    private readonly IBus _bus;
    private readonly DirectoriesConfig _directories;
    private readonly ILogger<DeploymentRequestCreatedHandler> _logger;

    public DeploymentRequestCreatedHandler(
        IQuerySession query, IGit git, IDeploymentFilesStorage deploymentFilesStorage, IBus bus, 
        DirectoriesConfig directories, ILogger<DeploymentRequestCreatedHandler> logger)
    {
        _query = query;
        _git = git;
        _deploymentFilesStorage = deploymentFilesStorage;
        _bus = bus;
        _directories = directories;
        _logger = logger;
    }

    public async Task Handle(DeploymentRequestCreated message)
    {
        try
        {
            await UploadDeploymentFiles(message.DeploymentId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error while processing deployment request created for {DeploymentId}", message.DeploymentId);
            Directory.Delete($"{_directories.GitDirectory}/{message.DeploymentId}", true);
            throw;
        }
    }

    private async Task UploadDeploymentFiles(string deploymentId)
    {
        var deploymentDoc = await _query.LoadAsync<DeploymentDocument>(DeploymentDocument.BuildDocId(deploymentId));
        if (deploymentDoc is null) 
            return;

        var deploymentFolder = $"{_directories.GitDirectory}/{deploymentDoc.DeploymentId()}";
        
        Directory.CreateDirectory(deploymentFolder);
        
        await _git.Clone(deploymentDoc);

        Directory.Delete($"{deploymentFolder}/.git", true);
        
        await _deploymentFilesStorage.Upload(deploymentDoc);
        
        Directory.Delete(deploymentFolder, true);

        await _bus.SendLocal(new DeploymentCodeUploaded(deploymentId));
    }
}