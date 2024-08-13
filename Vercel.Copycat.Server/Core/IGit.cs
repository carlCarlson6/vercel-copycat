using System.Diagnostics;

namespace Vercel.Copycat.Server.Core;

public record GitCommitInfo(string Hash, string Message);

public interface IGit
{
    Task<GitCommitInfo> Clone(Guid projectId, string repoUrl);
}

public class GitCli(Cli cli, DirectoriesConfig directories) : IGit
{
    public async Task<GitCommitInfo> Clone(Guid projectId, string repoUrl)
    {
        await ExecuteClone(projectId, repoUrl);
        var hash = await ReadCurrentHash(projectId);
        var message = await ReadCurrentCommitMessage(projectId);
        return new GitCommitInfo(hash, message);
    }
    
    private async Task ExecuteClone(Guid projectId, string repoUrl) => await cli.Execute(
        $"git clone {repoUrl} .", 
        $"{directories.GitDirectory}/{projectId}");

    private Task<string> ReadCurrentHash(Guid projectId) => cli.Execute(
        "git rev-parse HEAD", 
        $"{directories.GitDirectory}/{projectId}");

    private Task<string> ReadCurrentCommitMessage(Guid projectId) => cli.Execute(
        "git log -1 --pretty=%B", 
        $"{directories.GitDirectory}/{projectId}");

    private static void ProcessExitedHandler(object? sender, EventArgs e)
    {
        if (sender is null) throw new Exception("Process was closed unexpectedly");
        if ((sender as Process)!.ExitCode != 0)
        {
            throw new Exception($"The git process fails with code {(sender as Process)!.ExitCode}");
        }
    }
}