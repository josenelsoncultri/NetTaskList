namespace NetTaskList.Domain.Models;

/// <summary>
/// Represents the outcome of an operating system process execution.
/// </summary>
public sealed class ProcessExecutionResult
{
    public ProcessExecutionResult(int exitCode, string? errorMessage = null)
    {
        ExitCode = exitCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// The numeric exit code returned by the executed process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// True if the process completed with exit code 0 and no launch error occurred.
    /// </summary>
    public bool Success => ExitCode == 0 && ErrorMessage == null;

    /// <summary>
    /// Diagnostic message if the process failed to start or threw an OS exception.
    /// </summary>
    public string? ErrorMessage { get; }

    public static ProcessExecutionResult FromExitCode(int exitCode) => new(exitCode);

    public static ProcessExecutionResult Failure(string errorMessage, int exitCode = 1) =>
        new(exitCode, errorMessage);
}
