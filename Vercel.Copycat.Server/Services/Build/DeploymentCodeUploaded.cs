using Vercel.Copycat.Server.Core;

namespace Vercel.Copycat.Server.Services.Build;

public record DeploymentCodeUploaded(Guid Id, Guid ProjectId) : IEvent;
