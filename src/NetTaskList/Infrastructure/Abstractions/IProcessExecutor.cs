using NetTaskList.Domain.Models;

namespace NetTaskList.Infrastructure.Abstractions;

/// <summary>
/// Abstraction for operating system process execution to enable testability and decoupling.
/// </summary>
public interface IProcessExecutor
{
    /// <summary>
    /// Executes a command line in the host environment, attaching terminal streams and waiting for completion.
    /// </summary>
    /// <param name="commandLine">The command string to execute.</param>
    /// <param name="workingDirectory">Optional working directory; defaults to current directory if null.</param>
    /// <returns>A result containing the exit code and potential error diagnostics.</returns>
    ProcessExecutionResult Execute(string commandLine, string? workingDirectory = null);
}
