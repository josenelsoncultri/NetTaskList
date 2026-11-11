using System.Text.RegularExpressions;
using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class RecursionDetectionTests
{
    private readonly MockProcessExecutor _executor = new();
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockAnsiConsole _console = new();

    private string NormalizedOutput => NormalizeConsoleOutput(_console.Output);

    private static string NormalizeConsoleOutput(string input)
    {
        var withoutAnsi = Regex.Replace(input, @"\x1B\[[^@-~]*[@-~]", "");
        return Regex.Replace(withoutAnsi, @"\s+", " ");
    }

    [Fact]
    public void Execute_SelfReferencingTask_HaltsBeforeExecutionAndReportsRecursion()
    {
        var task = new ScriptTask("loop", new[] { TaskCommandStep.FromShortcut("loop") }, TaskType.Tool);
        var config = new TaskConfiguration("nxtl.json", new[] { task }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(task, config);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Referência circular detectada entre tarefas:", NormalizedOutput);
        Assert.Contains("loop -> loop", NormalizedOutput);
        Assert.Contains("loop infinito no nxtl", NormalizedOutput);
    }

    [Fact]
    public void Execute_MutualTwoTaskCycle_HaltsBeforeExecutionAndReportsCycleChain()
    {
        var taskClean = new ScriptTask("clean", new[] { TaskCommandStep.FromShortcut("build") }, TaskType.Tool);
        var taskBuild = new ScriptTask("build", new[] { TaskCommandStep.FromShortcut("clean") }, TaskType.Tool);
        var config = new TaskConfiguration("nxtl.json", new[] { taskClean, taskBuild }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(taskClean, config);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Referência circular detectada entre tarefas:", NormalizedOutput);
        Assert.Contains("clean -> build -> clean", NormalizedOutput);
        Assert.Contains("loop infinito no nxtl", NormalizedOutput);
    }

    [Fact]
    public void Execute_DeepTreeCycle_HaltsBeforeExecutionAndReportsFullChain()
    {
        var taskA = new ScriptTask("task-a", new[] { TaskCommandStep.FromShortcut("task-b") }, TaskType.Tool);
        var taskB = new ScriptTask("task-b", new[] { TaskCommandStep.FromShortcut("task-c") }, TaskType.Tool);
        var taskC = new ScriptTask("task-c", new[] { TaskCommandStep.FromShortcut("task-a") }, TaskType.Tool);
        var config = new TaskConfiguration("nxtl.json", new[] { taskA, taskB, taskC }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(taskA, config);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Referência circular detectada entre tarefas:", NormalizedOutput);
        Assert.Contains("task-a -> task-b -> task-c -> task-a", NormalizedOutput);
        Assert.Contains("loop infinito no nxtl", NormalizedOutput);
    }

    [Fact]
    public void Execute_CycleInSubtree_AbortsUpfrontWithoutExecutingPriorCommands()
    {
        var taskClean = new ScriptTask("clean", new[] { TaskCommandStep.FromShortcut("build") }, TaskType.Tool);
        var taskBuild = new ScriptTask("build", new[] { TaskCommandStep.FromShortcut("clean") }, TaskType.Tool);
        var taskPipeline = new ScriptTask("pipeline", new[]
        {
            TaskCommandStep.FromCommandLine("echo preparing-system"),
            TaskCommandStep.FromShortcut("clean")
        }, TaskType.Tool);

        var config = new TaskConfiguration("nxtl.json", new[] { taskClean, taskBuild, taskPipeline }, ConfigurationSourceType.NxtlJson);
        var coordinator = new TaskExecutionCoordinator(_executor, _fileSystem, _console);

        var exitCode = coordinator.Execute(taskPipeline, config);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("pipeline -> clean -> build -> clean", NormalizedOutput);
        Assert.Contains("loop infinito no nxtl", NormalizedOutput);
    }
}
