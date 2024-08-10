using System.Text.Json;
using Mediator;
using StackExchange.Redis;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Deployments;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Projects.Create;

public record ProjectCreated(Guid EventId, Guid ProjectId) : IEvent;

// ReSharper disable once UnusedType.Global
public class ProjectCreatedHandler(
    IDatabase db,
    IBus bus,
    IGit git,
    IBuilder builder,
    IDeploymentFilesStorage storage,
    DirectoriesConfig directoriesConfig,
    ILogger<ProjectCreatedHandler> logger
) 
    : IRequestHandler<ProjectCreated>
{
    public async ValueTask<Unit> Handle(ProjectCreated projectCreated, CancellationToken cancellationToken)
    {
        var projectId = projectCreated.ProjectId;
        var projectDocId = ProjectDocument.BuildDocId(projectId);
        var redisValue = await db.StringGetAsync(projectDocId);
        var strContent = redisValue.ToString();
        var projectDoc = JsonSerializer.Deserialize<ProjectDocument>(strContent);
        if (projectDoc is null)
            throw new Exception("project not found");        
        
        var deploymentFolder = $"{directoriesConfig.GitDirectory}/{projectId}";
        Directory.CreateDirectory(deploymentFolder);
        await git.Clone(projectDoc);
        
        await builder.BuildProject(projectDoc);
        
        await storage.Upload(projectDoc);
        Directory.Delete(deploymentFolder, true);

        var deploymentCodeUploaded = new DeploymentCodeUploaded(Guid.NewGuid(), projectId); 
        var transaction = db.CreateTransaction();
        _ = transaction.ListRightPushAsync(ProjectEventsStreamDocument.BuildDocId(projectDocId), RedisEvent.Serialize(deploymentCodeUploaded));
        _ = bus.Publish(deploymentCodeUploaded, transaction);
        var confirmedTransaction = await transaction.ExecuteAsync();
        if (!confirmedTransaction)
            throw new Exception();
        
        return Unit.Value;
    }
}