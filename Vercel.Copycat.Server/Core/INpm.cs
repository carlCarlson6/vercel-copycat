namespace Vercel.Copycat.Server.Core;

public interface INpm
{
    Task InstallDependencies(Guid deploymentId);
    Task BuildProject(Guid deploymentId);
}

public class Npm(Cli cli, DirectoriesManager directories) : INpm
{
    public Task InstallDependencies(Guid deploymentId) => cli.Execute(
        "npm i", 
        directories.BuildPath(deploymentId));
    
    public Task BuildProject(Guid deploymentId) => cli.Execute(
        "npm run build",
        directories.BuildPath(deploymentId));
}