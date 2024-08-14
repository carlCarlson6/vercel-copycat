namespace Vercel.Copycat.Server.Projects;

public record ProjectStatus(string Name, RepoInfo RepoInfo, Guid? CurrentDeploymentId, IEnumerable<IEvent> Events);

[Alias(nameof(RepoInfo)), GenerateSerializer]
public record RepoInfo(string RepoUrl, string BuildOutputPath);