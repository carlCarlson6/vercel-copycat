using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Core;

public interface IEvent
{
    public Guid EventId { get; }
    public Guid ProjectId { get; }
    public DateTime AtUtc { get; }
    public string Type { get; }
}

[GenerateSerializer]
public record ProjectCreated(
    Guid EventId, 
    Guid ProjectId,
    DateTime AtUtc,
    RepoInfo RepoInfo, 
    string Type = nameof(ProjectCreated)
) : IEvent;

[GenerateSerializer]
public record DeploymentCompleted(
    Guid EventId, 
    Guid ProjectId,
    Guid DeploymentId,
    DateTime AtUtc,
    GitCommitInfo GitCommitInfo,
    string Type = nameof(DeploymentCompleted)
) : IEvent;