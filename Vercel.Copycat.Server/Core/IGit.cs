namespace Vercel.Copycat.Server.Core;

[GenerateSerializer]
public record GitCommitInfo(string Hash, string Message);

public interface IGit
{
    Task<GitCommitInfo> Clone(Guid deploymentId, string repoUrl);
}

public class GitCli(Cli cli, DirectoriesManager directories) : IGit
{
    public async Task<GitCommitInfo> Clone(Guid deploymentId, string repoUrl)
    {
        await ExecuteClone(deploymentId, repoUrl);
        var hash = await ReadCurrentHash(deploymentId);
        var message = await ReadCurrentCommitMessage(deploymentId);
        return new GitCommitInfo(hash, message);
    }
    
    private async Task ExecuteClone(Guid deploymentId, string repoUrl) => await cli.Execute(
        $"git clone {repoUrl} .", 
        directories.BuildPath(deploymentId));

    private Task<string> ReadCurrentHash(Guid deploymentId) => cli.Execute(
        "git rev-parse HEAD", 
        directories.BuildPath(deploymentId));

    private Task<string> ReadCurrentCommitMessage(Guid deploymentId) => cli.Execute(
        "git log -1 --pretty=%B", 
        directories.BuildPath(deploymentId));
}