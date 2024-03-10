using Marten;
using Mediator;
using OneOf;
using Rebus.Bus;
using Vercel.Copycat.Server.Core;

namespace Vercel.Copycat.Server.Services.Upload;

public record CreateDeploymentRequest(string RepoUrl, string DeploymentId) : IRequest<CreateDeploymentResponse>;

[GenerateOneOf]
public partial class CreateDeploymentResponse : OneOfBase<DeploymentRequestCreated, MissingData, DeploymentIdAlreadyExists> { }

public readonly struct MissingData { }

public readonly struct DeploymentIdAlreadyExists { }

public class CreateDeploymentHandler
    : IRequestHandler<CreateDeploymentRequest, CreateDeploymentResponse>
{
    private readonly IDocumentStore _store;
    private readonly IBus _bus;
    private readonly ILogger<CreateDeploymentHandler> _logger;

    public CreateDeploymentHandler(IDocumentStore store, IBus bus, ILogger<CreateDeploymentHandler> logger)
    {
        _store = store;
        _bus = bus;
        _logger = logger;
    }

    public async ValueTask<CreateDeploymentResponse> Handle(CreateDeploymentRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("handling create deployment request");
        
        var (repoUrl, deploymentId) = request;
        if (string.IsNullOrWhiteSpace(repoUrl) || string.IsNullOrWhiteSpace(deploymentId))
        {
            _logger.LogWarning("missing data on the request");
            return new MissingData();
        }
        
        _logger.LogInformation("checking if deployment already exist");
        
        var deploymentDocId = DeploymentDocument.BuildDocId(deploymentId);
        await using var session = _store.LightweightSession();
        var maybeDoc = await session.LoadAsync<DeploymentDocument>(deploymentDocId, cancellationToken);
        if (maybeDoc is not null)
        {
            _logger.LogWarning("deployment id already in use");
            return new DeploymentIdAlreadyExists();
        }
        
        _logger.LogInformation("storing new deployment doc");
        
        session.Store(new DeploymentDocument(deploymentDocId, repoUrl));
        await session.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("deployment doc initiated");
        _logger.LogInformation("sending DeploymentRequestCreated");
        
        var deploymentRequestCreated = new DeploymentRequestCreated(deploymentId);
        await _bus.SendLocal(deploymentRequestCreated);

        return deploymentRequestCreated;
    }
}