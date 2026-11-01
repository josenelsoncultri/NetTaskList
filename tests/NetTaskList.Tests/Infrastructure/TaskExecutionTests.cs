using NetTaskList.Domain.Models;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class TaskExecutionTests
{
    [Fact]
    public void ToolTask_WhenExecuted_DispatchesResolvedCommandLineThroughExecutor()
    {
        // Arrange
        var task = new ScriptTask("build", "dotnet build -c Release", TaskType.Tool, "Builds project");
        var executor = new MockProcessExecutor();

        // Act
        var command = task.ResolveExecutionCommand();
        var result = executor.Execute(command);

        // Assert
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet build -c Release", executor.ExecutedCommands[0]);
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void ToolTask_WithCustomWorkingDir_PassesWorkingDirectoryToExecutor()
    {
        // Arrange
        var task = new ScriptTask("test", "npm test", TaskType.Tool);
        var executor = new MockProcessExecutor();
        var targetDir = "D:\\Projects\\SubModule";

        // Act
        var result = executor.Execute(task.ResolveExecutionCommand(), targetDir);

        // Assert
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("npm test", executor.ExecutedCommands[0]);
        Assert.Equal(targetDir, executor.WorkingDirectories[0]);
    }

    [Fact]
    public void ToolTask_WhenExitCodeIsNonZero_ReturnsExitCodeFromExecutor()
    {
        // Arrange
        var task = new ScriptTask("failing", "exit 5", TaskType.Tool);
        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(5);

        // Act
        var result = executor.Execute(task.ResolveExecutionCommand());

        // Assert
        Assert.False(result.Success);
        Assert.Equal(5, result.ExitCode);
    }
}
