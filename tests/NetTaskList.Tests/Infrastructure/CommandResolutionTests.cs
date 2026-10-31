using NetTaskList.Domain.Models;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class CommandResolutionTests
{
    [Fact]
    public void ResolveExecutionCommand_ToolType_ReturnsUnchanged()
    {
        // Arrange
        var task = new ScriptTask("build", "dotnet build -c Release", TaskType.Tool, "Builds project");

        // Act
        var resolved = task.ResolveExecutionCommand();

        // Assert
        Assert.Equal("dotnet build -c Release", resolved);
    }

    [Fact]
    public void ResolveExecutionCommand_ToolTypeWithCsExtension_ReturnsUnchanged()
    {
        // Arrange - A tool that happens to reference a .cs file should not get 'dotnet run' prefixed if type is Tool
        var task = new ScriptTask("format", "dotnet format --include Program.cs", TaskType.Tool);

        // Act
        var resolved = task.ResolveExecutionCommand();

        // Assert
        Assert.Equal("dotnet format --include Program.cs", resolved);
    }

    [Fact]
    public void ResolveExecutionCommand_CSharpTypeEndingWithCs_InfersDotnetRun()
    {
        // Arrange
        var task = new ScriptTask("seed", "scripts/seed.cs", TaskType.CSharp, "Seeds database");

        // Act
        var resolved = task.ResolveExecutionCommand();

        // Assert
        Assert.Equal("dotnet run scripts/seed.cs", resolved);
    }

    [Theory]
    [InlineData("scripts\\seed.cs", "dotnet run scripts\\seed.cs")]
    [InlineData("clean.CS", "dotnet run clean.CS")]
    [InlineData("tasks/migration.cs", "dotnet run tasks/migration.cs")]
    public void ResolveExecutionCommand_CSharpTypeWithDifferentPathStyles_InfersDotnetRun(string command, string expected)
    {
        var task = new ScriptTask("task", command, TaskType.CSharp);

        Assert.Equal(expected, task.ResolveExecutionCommand());
    }

    [Fact]
    public void ResolveExecutionCommand_CSharpTypeAlreadyStartingWithDotnetRun_DoesNotDuplicate()
    {
        // Arrange
        var task = new ScriptTask("custom", "dotnet run scripts/custom.cs --verbose", TaskType.CSharp);

        // Act
        var resolved = task.ResolveExecutionCommand();

        // Assert
        Assert.Equal("dotnet run scripts/custom.cs --verbose", resolved);
    }

    [Fact]
    public void ResolveExecutionCommand_CSharpTypeWithoutCsExtension_ReturnsUnchanged()
    {
        // Arrange
        var task = new ScriptTask("csharp-cmd", "dotnet-script main.csx", TaskType.CSharp);

        // Act
        var resolved = task.ResolveExecutionCommand();

        // Assert
        Assert.Equal("dotnet-script main.csx", resolved);
    }

    [Fact]
    public void ResolveExecutionCommand_LargeCollectionOfMixedTasks_ResolvesCorrectly()
    {
        var tasks = new List<ScriptTask>();
        for (int i = 0; i < 50; i++)
        {
            var isCSharp = i % 2 == 0;
            var type = isCSharp ? TaskType.CSharp : TaskType.Tool;
            var command = isCSharp ? $"scripts/task_{i}.cs" : $"echo task_{i}";
            tasks.Add(new ScriptTask($"task_{i}", command, type, $"Task {i} description"));
        }

        foreach (var task in tasks)
        {
            var resolved = task.ResolveExecutionCommand();
            if (task.Type == TaskType.CSharp)
            {
                Assert.StartsWith("dotnet run ", resolved);
            }
            else
            {
                Assert.StartsWith("echo ", resolved);
            }
        }
    }
}
