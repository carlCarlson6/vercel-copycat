using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Core;

public class DirectoriesManager(DirectoriesConfig config) : BackgroundService
{
    public void Create(Guid projectId) => Directory.CreateDirectory(BuildPath(projectId));
    public void Delete(Guid projectId) => Directory.Delete(BuildPath(projectId), true);

    private string BuildPath(Guid projectId) => $"{config.GitDirectory}/{projectId}";
    
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