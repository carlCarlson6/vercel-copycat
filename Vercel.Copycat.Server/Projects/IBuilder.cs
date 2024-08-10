using System.Diagnostics;
using Vercel.Copycat.Server.Core;
using Vercel.Copycat.Server.Infrastructure;

namespace Vercel.Copycat.Server.Projects;

public interface IBuilder
{
    Task BuildProject(ProjectDocument projectDoc);
}

public class Builder(DirectoriesConfig directories) : IBuilder
{
    public async Task BuildProject(ProjectDocument projectDoc)
    {
        await InstallDependencies(projectDoc);
        await ExecuteBuild(projectDoc);
    }

    private async Task InstallDependencies(ProjectDocument projectDoc)
    {
        using var process = new Process();
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
        var command = "npm i";
        await process.StandardInput.WriteLineAsync(command.Replace("^?", ""));
        await process.StandardInput.WriteLineAsync("exit");
            
        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
    }
    
    private async Task ExecuteBuild(ProjectDocument projectDoc)
    {
        using var process = new Process();
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
        var command = "npm run build";
        await process.StandardInput.WriteLineAsync(command.Replace("^?", ""));
        await process.StandardInput.WriteLineAsync("exit");
            
        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
    }
}