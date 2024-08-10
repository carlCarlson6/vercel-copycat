using System.Text.Json;
using Mediator;
using OneOf;
using StackExchange.Redis;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;
using static System.String;

namespace Vercel.Copycat.Server.Projects.Create;

public record CreateProjectRequest(string RepoUrl, string Name) : IRequest<CreateProjectResponse>;

[GenerateOneOf]
public partial class CreateProjectResponse : OneOfBase<
    ProjectCreated, MissingData, ProblemCreatingProject
>;
public readonly struct MissingData;
public readonly struct ProblemCreatingProject;

public class CreateDeploymentHandler(IBus bus, IDatabase db, ILogger<CreateDeploymentHandler> logger)
    : IRequestHandler<CreateProjectRequest, CreateProjectResponse>
{
    public async ValueTask<CreateProjectResponse> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("handling create project request");
        
        var (repoUrl, projectName) = request;
        if (IsNullOrWhiteSpace(repoUrl) || IsNullOrWhiteSpace(projectName))
        {
            logger.LogWarning("missing data on the request");
            return new MissingData();
        }
        
        var deploymentDocId = ProjectDocument.BuildDocId(Guid.NewGuid());
        
        logger.LogInformation("storing new project doc");
        
        var project = new ProjectDocument(deploymentDocId, projectName, repoUrl);

        var projectCreated = new ProjectCreated(Guid.NewGuid(), project.ProjectId());
        var transaction = db.CreateTransaction();
        
        _ = transaction.StringSetAsync(project.Id, JsonSerializer.Serialize(project));
        _ = transaction.ListRightPushAsync(ProjectEventsStreamDocument.BuildDocId(project.Id), RedisEvent.Serialize(projectCreated));
        _ = bus.Publish(projectCreated, transaction);
        var confirmedTransaction = await transaction.ExecuteAsync();
        if (!confirmedTransaction)
        {
            logger.LogError("error while storing project");
            return new ProblemCreatingProject();
        }

        return projectCreated;
    }
    
    
}