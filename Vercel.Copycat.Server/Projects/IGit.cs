using System.Diagnostics;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Projects;

public interface IGit
{
    Task<string> Clone(ProjectDocument projectDoc);
}

public class GitCli(DirectoriesConfig directories) : IGit
{
    public async Task<string> Clone(ProjectDocument projectDoc)
    {
        using var process = new Process();
        await ExecuteClone(projectDoc, process);
        return await ReadCurrentHash(projectDoc, process);
    }
    
    private async Task ExecuteClone(ProjectDocument projectDoc, Process process)    
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = $"{directories.GitDirectory}/{projectDoc.ProjectId()}"
        };
        
        process.EnableRaisingEvents = false; // if this is true, when throwing the error breaks and ends the execution of the server
        //process.Exited += ProcessExitedHandler;
        process.StartInfo = startInfo;
            
        process.Start();
        var command = $"git clone {projectDoc.RepoUrl} .";
        await process.StandardInput.WriteLineAsync(command.Replace("^?", ""));
        await process.StandardInput.WriteLineAsync("exit");
            
        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
    }
    
    private Task<string> ReadCurrentHash(ProjectDocument projectDoc, Process process) => Task.FromResult(string.Empty);

    private static void ProcessExitedHandler(object? sender, EventArgs e)
    {
        if (sender is null) throw new Exception("Process was closed unexpectedly");
        if ((sender as Process)!.ExitCode != 0)
        {
            throw new Exception($"The git process fails with code {(sender as Process)!.ExitCode}");
        }
    }
}