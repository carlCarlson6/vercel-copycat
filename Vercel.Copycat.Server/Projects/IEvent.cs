using Vercel.Copycat.Server.Deployments.Workers;
using static System.String;

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
        new(Guid.NewGuid(), new Guid(), DateTime.UtcNow, new RepoInfo(Empty, Empty));
}

public record NewDeploymentRequested(
    Guid EventId, 
    Guid ProjectId,
    DateTime AtUtc,
    RepoInfo RepoInfo, 
    string Type = nameof(ProjectCreated)
) : IEvent
{
    public static NewDeploymentRequested Default =>
        new(Guid.NewGuid(), new Guid(), DateTime.UtcNow, new RepoInfo(Empty, Empty));
}

[Alias(nameof(DeploymentCompleted)), GenerateSerializer]
public record DeploymentCompleted(
    Guid EventId,
    Guid ProjectId,
    Guid DeploymentId,
    DateTime AtUtc,
    GitCommitInfo GitCommitInfo,
    string Type = nameof(DeploymentCompleted)
) : IEvent
{
    public static DeploymentCompleted Default =>
        new(Guid.NewGuid(), new Guid(), new Guid(), DateTime.UtcNow, new GitCommitInfo(Empty, Empty));   
}