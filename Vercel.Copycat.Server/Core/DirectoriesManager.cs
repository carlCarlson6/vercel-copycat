namespace Vercel.Copycat.Server.Core;

public class DirectoriesManager(DirectoriesConfig config) : BackgroundService
{
    public void Create(Guid id) => Directory.CreateDirectory(BuildPath(id));
    public void Delete(Guid id) => Directory.Delete(BuildPath(id), true);

    public string BuildPath(Guid projectId) => $"{config.GitDirectory}/{projectId}";
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Directory.Exists(config.GitDirectory))
            return Task.CompletedTask;
        
        Directory.CreateDirectory(config.GitDirectory);
        return Task.CompletedTask;
    }
}

public class DirectoriesConfig
{
    public string GitDirectory { get; init; } = "./working-directory";
}