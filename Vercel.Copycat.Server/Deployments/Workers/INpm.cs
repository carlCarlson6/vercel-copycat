namespace Vercel.Copycat.Server.Deployments.Workers;

public interface INpm
{
    Task InstallDependencies(Guid deploymentId);
    Task BuildProject(Guid deploymentId);
}

public class Npm(DirectoriesManager directories) : INpm
{
    public Task InstallDependencies(Guid deploymentId) => Cli.Execute(
        "npm i", 
        directories.BuildPath(deploymentId));
    
    public Task BuildProject(Guid deploymentId) => Cli.Execute(
        "npm run build",
        directories.BuildPath(deploymentId));
}