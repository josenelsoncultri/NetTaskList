using System.Diagnostics;
using System.Runtime.InteropServices;
using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure.Abstractions;

namespace NetTaskList.Infrastructure;

/// <summary>
/// Executes operating system commands through the native platform shell with direct console stream attachment.
/// </summary>
public class ProcessExecutor : IProcessExecutor
{
    public virtual ProcessExecutionResult Execute(string commandLine, string? workingDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return ProcessExecutionResult.Failure("Nenhum comando fornecido para execução.", 1);
        }

        var (fileName, arguments) = GetShellCommand(commandLine);

        var startInfo = CreateStartInfo(fileName, arguments, workingDirectory);

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return ProcessExecutionResult.Failure("Não foi possível iniciar o processo no sistema operacional.", 1);
            }

            process.WaitForExit();
            return ProcessExecutionResult.FromExitCode(process.ExitCode);
        }
        catch (Exception ex)
        {
            return ProcessExecutionResult.Failure(ex.Message, 1);
        }
    }

    /// <summary>
    /// Creates ProcessStartInfo configured with direct console handles for high fidelity and zero stream buffering.
    /// </summary>
    public static ProcessStartInfo CreateStartInfo(string fileName, string arguments, string? workingDirectory = null)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? Directory.GetCurrentDirectory() : workingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false
        };
    }

    /// <summary>
    /// Resolves the shell executable and arguments for the current operating system.
    /// </summary>
    public static (string FileName, string Arguments) GetShellCommand(string commandLine)
    {
        return GetShellCommand(commandLine, RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    }

    /// <summary>
    /// Resolves the shell executable and arguments for a specified operating system platform.
    /// </summary>
    public static (string FileName, string Arguments) GetShellCommand(string commandLine, bool isWindows)
    {
        if (isWindows)
        {
            var comSpec = Environment.GetEnvironmentVariable("COMSPEC");
            var shell = string.IsNullOrWhiteSpace(comSpec) ? "cmd.exe" : comSpec;
            return (shell, $"/c {commandLine}");
        }

        return ("/bin/sh", $"-c \"{commandLine.Replace("\"", "\\\"")}\"");
    }
}
