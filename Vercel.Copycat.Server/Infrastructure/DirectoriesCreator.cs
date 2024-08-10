namespace Vercel.Copycat.Server.Infrastructure;

public class DirectoriesCreator(DirectoriesConfig directories) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Directory.Exists(directories.GitDirectory))
            return Task.CompletedTask;
        
        Directory.CreateDirectory(directories.GitDirectory);
        return Task.CompletedTask;
    }
}

public class DirectoriesConfig
{
    public string GitDirectory { get; init; } = "./git-working-directory";
}