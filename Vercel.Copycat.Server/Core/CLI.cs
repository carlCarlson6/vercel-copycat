using System.Diagnostics;

namespace Vercel.Copycat.Server.Core;

public class Cli
{
    public async Task<string> Execute(string command, string pathWhereToExecute)
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
            WorkingDirectory = pathWhereToExecute
        };
        
        process.EnableRaisingEvents = false;
        process.StartInfo = startInfo;
            
        process.Start();
        await process.StandardInput.WriteLineAsync(command.Replace("^?", ""));
        await process.StandardInput.WriteLineAsync("exit");
            
        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var errorOutput = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return standardOutput;
    }
}