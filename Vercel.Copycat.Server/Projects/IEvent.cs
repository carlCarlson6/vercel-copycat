using Vercel.Copycat.Server.Deployments.Workers;

namespace Vercel.Copycat.Server.Projects;

public interface IEvent
{
    public Guid EventId { get; }
    public Guid ProjectId { get; }
    public DateTime AtUtc { get; }
    public string Type { get; }
}

[Alias(nameof(ProjectCreated)), GenerateSerializer]
public record ProjectCreated(
    Guid EventId, 
    Guid ProjectId,
    DateTime AtUtc,
    RepoInfo RepoInfo, 
    string Type = nameof(ProjectCreated)
) : IEvent
{
    public static ProjectCreated Default =>
        new(Guid.NewGuid(), new Guid(), DateTime.UtcNow, new RepoInfo(string.Empty, string.Empty));
}

[Alias(nameof(DeploymentCompleted)), GenerateSerializer]
public record DeploymentCompleted(
    Guid EventId, 
    Guid ProjectId,
    Guid DeploymentId,
    DateTime AtUtc,
    GitCommitInfo GitCommitInfo,
    string Type = nameof(DeploymentCompleted)
) : IEvent;