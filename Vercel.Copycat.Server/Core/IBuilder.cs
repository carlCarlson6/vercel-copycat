namespace Vercel.Copycat.Server.Core;

public interface IBuilder
{
    Task BuildProject(Guid projectId);
}

public class Builder(Cli cli, DirectoriesConfig directories) : IBuilder
{
    public async Task BuildProject(Guid projectId)
    {
        await InstallDependencies(projectId);
        await ExecuteBuild(projectId);
    }

    private Task InstallDependencies(Guid projectId) => cli.Execute(
        "npm i", 
        $"{directories.GitDirectory}/{projectId}");

    private Task ExecuteBuild(Guid projectId) => cli.Execute(
        "npm run build",
        $"{directories.GitDirectory}/{projectId}");
}