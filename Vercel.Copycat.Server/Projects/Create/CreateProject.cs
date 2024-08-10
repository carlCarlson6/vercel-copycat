using System.Text.Json;
using Mediator;
using OneOf;
using StackExchange.Redis;
using Vercel.Copycat.Server.Core;
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

        var transaction = db.CreateTransaction();
        
        var success = await transaction.StringSetAsync(project.Id, JsonSerializer.Serialize(project));
        if (!success)
        {
            logger.LogError("error while storing project");
            return new ProblemCreatingProject();
        }

        var confirmedTransaction = await transaction.ExecuteAsync();
        if (!confirmedTransaction)
        {
            logger.LogError("error while storing project");
            return new ProblemCreatingProject();
        }

        var @event = new ProjectCreated(Guid.NewGuid(), project.ProjectId());
        await bus.Publish(@event);

        return @event;
    }
    
    
}