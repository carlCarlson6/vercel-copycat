using Mediator;

namespace Vercel.Copycat.Server.Deployments;

public record CreateDeploymentCommand : IRequest<CreateDeploymentResult>;

public record CreateDeploymentResult;

public class CreateDeployment : IRequestHandler<CreateDeploymentCommand, CreateDeploymentResult>
{
    public ValueTask<CreateDeploymentResult> Handle(CreateDeploymentCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}