using NetTaskList.Domain.Models;
using NetTaskList.Tests.Mocks;
using Spectre.Console;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class CSharpTaskExecutionTests
{
    private const string ConfigWithCSharp = """
    {
      "scripts": [
        {
          "name": "seed",
          "command": "scripts/seed.cs",
          "type": "csharp",
          "description": "Script de carga inicial"
        },
        {
          "name": "custom-cs",
          "command": "dotnet run scripts/custom.cs --verbose",
          "type": "csharp",
          "description": "Script customizado"
        }
      ]
    }
    """;

    [Fact]
    public void CSharpTask_WhenCsFileExists_DispatchesDotnetRunThroughExecutor()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigWithCSharp);
        fileSystem.AddFile("scripts/seed.cs", "System.Console.WriteLine(\"Seed\");");

        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "seed"], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet run scripts/seed.cs", executor.ExecutedCommands[0]);
    }

    [Fact]
    public void CSharpTask_WhenCsFileDoesNotExist_AbortsWithErrorCodeOneAndDoesNotExecute()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigWithCSharp);
        // Note: scripts/seed.cs is NOT added to fileSystem

        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "seed"], fileSystem, executor, console);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Empty(executor.ExecutedCommands);
        Assert.Contains("O arquivo de script C#", console.Output);
        Assert.Contains("não foi encontrado", console.Output);
    }

    [Fact]
    public void CSharpTask_AlreadyPrefixedWithDotnetRun_ExecutesWithoutDuplication()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigWithCSharp);

        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "custom-cs"], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet run scripts/custom.cs --verbose", executor.ExecutedCommands[0]);
    }

    [Fact]
    public void CSharpTask_InteractiveSelection_WhenCsFileExists_ExecutesDotnetRun()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigWithCSharp);
        fileSystem.AddFile("scripts/seed.cs", "Console.WriteLine(1);");

        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();
        // First item in config is "seed"
        console.PushKey(ConsoleKey.Enter);

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet run scripts/seed.cs", executor.ExecutedCommands[0]);
    }
}
