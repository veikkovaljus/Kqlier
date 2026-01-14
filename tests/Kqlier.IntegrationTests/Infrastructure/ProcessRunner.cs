using System.Diagnostics;
using System.Text;

namespace Kqlier.IntegrationTests.Infrastructure;

public record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

public static class ProcessRunner
{
    public static async Task<ProcessResult> RunKqlierAsync(
        string workingDirectory,
        params string[] args
    )
    {
        var projectDir = GetProjectDirectory();
        var dllPath = Path.Combine(projectDir, "bin", "Debug", "net10.0", "Kqlier.dll");

        var arguments =
            args.Length > 0 ? $"\"{dllPath}\" {string.Join(" ", args)}" : $"\"{dllPath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                stdout.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                stderr.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString());
    }

    private static string GetProjectDirectory()
    {
        // Navigate from test assembly location to main project
        var testAssemblyDir = Path.GetDirectoryName(typeof(ProcessRunner).Assembly.Location)!;
        // From bin/Debug/net10.0 -> tests/Kqlier.IntegrationTests -> tests -> Kqlier root
        return Path.GetFullPath(Path.Combine(testAssemblyDir, "..", "..", "..", "..", ".."));
    }
}
