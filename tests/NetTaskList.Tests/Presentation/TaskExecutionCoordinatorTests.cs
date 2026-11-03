using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class TaskExecutionCoordinatorTests
{
    private readonly MockProcessExecutor _executor = new();
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void Execute_PackageJsonWithPreAndPost_ExecutesPreThenMainThenPost()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "echo pre", TaskType.Tool),
            new("build", "tsc", TaskType.Tool),
            new("postbuild", "echo post", TaskType.Tool)
        };
        var config = new TaskConfiguration("package.json", tasks, ConfigurationSourceType.PackageJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var targetTask = tasks[1]; // build
        var exitCode = coordinator.Execute(targetTask, config);

        Assert.Equal(0, exitCode);
        Assert.Equal(3, _executor.ExecutedCommands.Count);
        Assert.Equal("echo pre", _executor.ExecutedCommands[0]);
        Assert.Equal("tsc", _executor.ExecutedCommands[1]);
        Assert.Equal("echo post", _executor.ExecutedCommands[2]);
    }

    [Fact]
    public void Execute_PackageJsonWithFailingPreHook_AbortsAndDoesNotRunMainOrPost()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "exit 2", TaskType.Tool),
            new("build", "tsc", TaskType.Tool),
            new("postbuild", "echo post", TaskType.Tool)
        };
        var config = new TaskConfiguration("package.json", tasks, ConfigurationSourceType.PackageJson);
        _executor.EnqueueExitCode(2);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var targetTask = tasks[1]; // build
        var exitCode = coordinator.Execute(targetTask, config);

        Assert.Equal(2, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("exit 2", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Execute_PackageJsonWithFailingMainTask_AbortsAndDoesNotRunPost()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "echo pre", TaskType.Tool),
            new("build", "exit 1", TaskType.Tool),
            new("postbuild", "echo post", TaskType.Tool)
        };
        var config = new TaskConfiguration("package.json", tasks, ConfigurationSourceType.PackageJson);
        _executor.EnqueueExitCode(0); // prebuild
        _executor.EnqueueExitCode(1); // build
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var targetTask = tasks[1]; // build
        var exitCode = coordinator.Execute(targetTask, config);

        Assert.Equal(1, exitCode);
        Assert.Equal(2, _executor.ExecutedCommands.Count);
        Assert.Equal("echo pre", _executor.ExecutedCommands[0]);
        Assert.Equal("exit 1", _executor.ExecutedCommands[1]);
    }

    [Fact]
    public void Execute_PackageJsonDirectRunOfHook_ExecutesOnlyThatHook()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "echo pre", TaskType.Tool),
            new("build", "tsc", TaskType.Tool)
        };
        var config = new TaskConfiguration("package.json", tasks, ConfigurationSourceType.PackageJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var preTask = tasks[0]; // prebuild directly
        var exitCode = coordinator.Execute(preTask, config);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo pre", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Execute_NxtlJson_DoesNotTriggerLifecycleHooks()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "echo pre", TaskType.Tool),
            new("build", "dotnet build", TaskType.Tool),
            new("postbuild", "echo post", TaskType.Tool)
        };
        var config = new TaskConfiguration("nxtl.json", tasks, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var targetTask = tasks[1]; // build
        var exitCode = coordinator.Execute(targetTask, config);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("dotnet build", _executor.ExecutedCommands[0]);
    }
}
