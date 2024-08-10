using System.Text.Json;
using Mediator;
using StackExchange.Redis;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Services.Build;

namespace Vercel.Copycat.Server.Projects;

// ReSharper disable once UnusedType.Global
public class ProjectCreatedHandler(
    IDatabase db,
    IBus bus,
    IGit git,
    IDeploymentFilesStorage storage,
    DirectoriesConfig directoriesConfig,
    ILogger<ProjectCreatedHandler> logger
) 
    : IRequestHandler<ProjectCreated>
{
    public async ValueTask<Unit> Handle(ProjectCreated projectCreated, CancellationToken cancellationToken)
    {
        var projectDocId = ProjectDocument.BuildDocId(projectCreated.Id);
        var redisValue = await db.StringGetAsync(projectDocId);
        var strContent = redisValue.ToString();
        var projectDoc = JsonSerializer.Deserialize<ProjectDocument>(strContent);
        if (projectDoc is null)
            throw new Exception("project not found");        
        
        var deploymentFolder = $"{directoriesConfig.GitDirectory}/{projectDoc.ProjectId()}";
        
        Directory.CreateDirectory(deploymentFolder);
        
        await git.Clone(projectDoc);

        Directory.Delete($"{deploymentFolder}/.git", true);
        
        await storage.Upload(projectDoc);
        
        Directory.Delete(deploymentFolder, true);

        await bus.Publish(new DeploymentCodeUploaded(Guid.NewGuid(), projectCreated.Id));
        
        return Unit.Value;
    }
}