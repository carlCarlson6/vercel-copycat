namespace Vercel.Copycat.Server.Infrastructure;

public class DirectoriesCreator(RepositoryConfig repoConfig) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Directory.Exists(repoConfig.WorkingDirectory))
            return Task.CompletedTask;
        
        Directory.CreateDirectory(repoConfig.WorkingDirectory);
        return Task.CompletedTask;
    }
}

public class RepositoryConfig
{
    public string WorkingDirectory { get; init; }
} 