using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure.Abstractions;

namespace NetTaskList.Tests.Mocks;

public sealed class MockProcessExecutor : IProcessExecutor
{
    private readonly List<string> _executedCommands = new();
    private readonly List<string?> _workingDirectories = new();
    private readonly Queue<ProcessExecutionResult> _queuedResults = new();
    private Func<string, string?, ProcessExecutionResult>? _executionHandler;

    public IReadOnlyList<string> ExecutedCommands => _executedCommands;
    public IReadOnlyList<string?> WorkingDirectories => _workingDirectories;

    public ProcessExecutionResult DefaultResult { get; set; } = ProcessExecutionResult.FromExitCode(0);

    public void EnqueueResult(ProcessExecutionResult result)
    {
        _queuedResults.Enqueue(result);
    }

    public void EnqueueExitCode(int exitCode)
    {
        _queuedResults.Enqueue(ProcessExecutionResult.FromExitCode(exitCode));
    }

    public void EnqueueFailure(string errorMessage, int exitCode = 1)
    {
        _queuedResults.Enqueue(ProcessExecutionResult.Failure(errorMessage, exitCode));
    }

    public void SetHandler(Func<string, string?, ProcessExecutionResult> handler)
    {
        _executionHandler = handler;
    }

    public ProcessExecutionResult Execute(string commandLine, string? workingDirectory = null)
    {
        _executedCommands.Add(commandLine);
        _workingDirectories.Add(workingDirectory);

        if (_executionHandler != null)
        {
            return _executionHandler(commandLine, workingDirectory);
        }

        if (_queuedResults.Count > 0)
        {
            return _queuedResults.Dequeue();
        }

        return DefaultResult;
    }
}
