using NetTaskList.Domain.Models;
using Xunit;

namespace NetTaskList.Tests.Domain;

public class ScriptTaskTests
{
    [Fact]
    public void Constructor_SingleCommand_InitializesCommandsList()
    {
        var task = new ScriptTask("build", "dotnet build", TaskType.Tool, "Build task");

        Assert.Equal("build", task.Name);
        Assert.Equal("dotnet build", task.Command);
        Assert.Single(task.Commands);
        Assert.Equal("dotnet build", task.Commands[0]);
        Assert.Equal(TaskType.Tool, task.Type);
        Assert.Equal("Build task", task.Description);
    }

    [Fact]
    public void Constructor_MultipleCommands_InitializesCommandsList()
    {
        var commands = new[] { "dotnet clean", "dotnet build", "dotnet test" };
        var task = new ScriptTask("pipeline", commands, TaskType.Tool, "Pipeline task");

        Assert.Equal("pipeline", task.Name);
        Assert.Equal("dotnet clean", task.Command);
        Assert.Equal(3, task.Commands.Count);
        Assert.Equal(commands, task.Commands);
    }

    [Fact]
    public void ResolveExecutionCommands_ToolType_ReturnsAllUnchanged()
    {
        var commands = new[] { "dotnet clean", "echo done", "scripts/test.cs" };
        var task = new ScriptTask("run", commands, TaskType.Tool);

        var resolved = task.ResolveExecutionCommands();

        Assert.Equal(commands, resolved);
    }

    [Fact]
    public void ResolveExecutionCommands_CSharpType_InfersDotnetRunForCsFiles()
    {
        var commands = new[]
        {
            "scripts/seed.cs",
            "dotnet run scripts/migrate.cs",
            "scripts/verify.cs"
        };
        var task = new ScriptTask("csharp-tasks", commands, TaskType.CSharp);

        var resolved = task.ResolveExecutionCommands();

        Assert.Equal(3, resolved.Count);
        Assert.Equal("dotnet run scripts/seed.cs", resolved[0]);
        Assert.Equal("dotnet run scripts/migrate.cs", resolved[1]);
        Assert.Equal("dotnet run scripts/verify.cs", resolved[2]);
    }

    [Fact]
    public void ResolveExecutionCommand_MultipleCommands_ResolvesFirstCommand()
    {
        var commands = new[] { "scripts/seed.cs", "scripts/verify.cs" };
        var task = new ScriptTask("csharp-tasks", commands, TaskType.CSharp);

        var resolved = task.ResolveExecutionCommand();

        Assert.Equal("dotnet run scripts/seed.cs", resolved);
    }

    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ScriptTask(null!, "cmd"));
    }

    [Fact]
    public void Constructor_NullCommand_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ScriptTask("test", (string)null!));
        Assert.Throws<ArgumentNullException>(() => new ScriptTask("test", (IReadOnlyList<string>)null!));
    }
}

