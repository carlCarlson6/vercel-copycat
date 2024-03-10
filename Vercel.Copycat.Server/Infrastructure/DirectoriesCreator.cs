namespace Vercel.Copycat.Server.Infrastructure;

public class DirectoriesCreator : BackgroundService
{
    private readonly DirectoriesConfig _directories;

    public DirectoriesCreator(DirectoriesConfig directories)
    {
        _directories = directories;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Directory.Exists(_directories.GitDirectory))
            return Task.CompletedTask;
        
        Directory.CreateDirectory(_directories.GitDirectory);
        return Task.CompletedTask;
    }
}

public class DirectoriesConfig
{
    public string GitDirectory { get; init; } = "./git-working-directory";
}