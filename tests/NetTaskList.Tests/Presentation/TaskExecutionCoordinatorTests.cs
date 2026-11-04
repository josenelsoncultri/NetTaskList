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
    public void Execute_MultipleCommands_ExecutesAllInOrderWithProgressionOutput()
    {
        var task = new ScriptTask("multi", new[] { "dotnet clean", "dotnet build", "dotnet test" }, TaskType.Tool);
        var config = new TaskConfiguration("nxtl.json", new[] { task }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(task, config);

        Assert.Equal(0, exitCode);
        Assert.Equal(3, _executor.ExecutedCommands.Count);
        Assert.Equal("dotnet clean", _executor.ExecutedCommands[0]);
        Assert.Equal("dotnet build", _executor.ExecutedCommands[1]);
        Assert.Equal("dotnet test", _executor.ExecutedCommands[2]);

        var output = _console.Output;
        Assert.Contains("Comando [1/3]:", output);
        Assert.Contains("dotnet clean", output);
        Assert.Contains("Comando [2/3]:", output);
        Assert.Contains("dotnet build", output);
        Assert.Contains("Comando [3/3]:", output);
        Assert.Contains("dotnet test", output);
    }

    [Fact]
    public void Execute_SingleCommand_RetainsSingleCommandOutputFormat()
    {
        var task = new ScriptTask("single", "dotnet build", TaskType.Tool);
        var config = new TaskConfiguration("nxtl.json", new[] { task }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(task, config);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("dotnet build", _executor.ExecutedCommands[0]);

        var output = _console.Output;
        Assert.Contains("Comando:", output);
        Assert.Contains("dotnet build", output);
        Assert.DoesNotContain("Comando [1/1]", output);
    }

    [Fact]
    public void Execute_MultipleCommands_SecondCommandFails_HaltsExecutionAndPropagatesExitCode()
    {
        var task = new ScriptTask("multi-fail", new[] { "step1", "step2-fail", "step3-never" }, TaskType.Tool);
        var config = new TaskConfiguration("nxtl.json", new[] { task }, ConfigurationSourceType.NxtlJson);
        _executor.EnqueueExitCode(0);
        _executor.EnqueueExitCode(42);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(task, config);

        Assert.Equal(42, exitCode);
        Assert.Equal(2, _executor.ExecutedCommands.Count);
        Assert.Equal("step1", _executor.ExecutedCommands[0]);
        Assert.Equal("step2-fail", _executor.ExecutedCommands[1]);

        var output = _console.Output;
        Assert.Contains("Execução interrompida:", output);
        Assert.Contains("O comando falhou com código de saída 42", output);
        Assert.Contains("posteriores não serão executados", output);
        Assert.DoesNotContain("step3-never", output);
    }

    [Fact]
    public void Execute_MultipleCSharpCommands_MissingScript_HaltsExecution()
    {
        _fileSystem.AddFile("scripts/step1.cs", "// valid");
        var task = new ScriptTask("multi-cs", new[] { "scripts/step1.cs", "scripts/missing.cs", "scripts/step3.cs" }, TaskType.CSharp);
        var config = new TaskConfiguration("nxtl.json", new[] { task }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(task, config);

        Assert.Equal(1, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("dotnet run scripts/step1.cs", _executor.ExecutedCommands[0]);

        var output = _console.Output;
        Assert.Contains("O arquivo de script C#", output);
        Assert.Contains("scripts/missing.cs", output);
        Assert.Contains("não foi encontrado", output);
        Assert.Contains("Execução interrompida:", output);
        Assert.Contains("Comandos posteriores não serão executados", output);
    }

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
