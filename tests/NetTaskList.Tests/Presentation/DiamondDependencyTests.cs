using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class DiamondDependencyTests
{
    private readonly MockProcessExecutor _executor = new();
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void Execute_DiamondDependencyGraph_ExecutesWithoutFalseCycleDetection()
    {
        // DAG structure:
        //          all
        //         /   \
        //     moduleA moduleB
        //         \   /
        //        shared-prep
        var sharedPrep = new ScriptTask("shared-prep", "echo prep", TaskType.Tool);
        var moduleA = new ScriptTask("module-a", new[]
        {
            TaskCommandStep.FromShortcut("shared-prep"),
            TaskCommandStep.FromCommandLine("echo modA")
        }, TaskType.Tool);
        var moduleB = new ScriptTask("module-b", new[]
        {
            TaskCommandStep.FromShortcut("shared-prep"),
            TaskCommandStep.FromCommandLine("echo modB")
        }, TaskType.Tool);
        var all = new ScriptTask("all", new[]
        {
            TaskCommandStep.FromShortcut("module-a"),
            TaskCommandStep.FromShortcut("module-b")
        }, TaskType.Tool);

        var config = new TaskConfiguration("nxtl.json", new[] { sharedPrep, moduleA, moduleB, all }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(all, config);

        Assert.Equal(0, exitCode);
        Assert.Equal(4, _executor.ExecutedCommands.Count);
        Assert.Equal("echo prep", _executor.ExecutedCommands[0]);
        Assert.Equal("echo modA", _executor.ExecutedCommands[1]);
        Assert.Equal("echo prep", _executor.ExecutedCommands[2]);
        Assert.Equal("echo modB", _executor.ExecutedCommands[3]);

        Assert.DoesNotContain("Referência circular detectada", _console.Output);
    }
}
