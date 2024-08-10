using Mediator;
using Vercel.Copycat.Server.Core;

namespace Vercel.Copycat.Server.Services.Build;

public record DeploymentCodeUploaded(Guid EventId, Guid ProjectId) : IEvent;

public class DeploymentCodeUploadedHandler : IRequestHandler<DeploymentCodeUploaded>
{
    public ValueTask<Unit> Handle(DeploymentCodeUploaded deploymentCodeUploaded, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}