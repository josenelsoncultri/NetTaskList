using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using NetTaskList.Tests.Mocks;
using Spectre.Console;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class ProgramExecutionTests
{
    private const string SampleConfig = """
    {
      "scripts": [
        {
          "name": "build",
          "command": "dotnet build -c Release",
          "type": "tool",
          "description": "Compila o projeto"
        },
        {
          "name": "test",
          "command": "dotnet test",
          "type": "tool",
          "description": "Executa testes"
        }
      ]
    }
    """;

    [Fact]
    public void Run_ShowVersion_OutputsDynamicAppVersion()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--version"], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("nxtl", console.Output);
        Assert.Contains("versão", console.Output);
        Assert.Contains(AppVersion.Current, console.Output);
    }

    [Fact]
    public void Run_WithDirectToolTask_ExecutesCommandAndReturnsExitCodeZero()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "build"], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet build -c Release", executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Run_WithDirectToolTask_WhenCommandFails_PropagatesNonZeroExitCode()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(42);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["-r", "test"], fileSystem, executor, console);

        // Assert
        Assert.Equal(42, exitCode);
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet test", executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Run_WhenTaskNotFound_ReturnsOneAndDoesNotExecute()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "nonexistent"], fileSystem, executor, console);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Empty(executor.ExecutedCommands);
    }

    [Fact]
    public void Run_InteractiveMode_WhenFirstTaskSelected_ExecutesTaskAndReturnsExitCode()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();
        // First choice is build
        console.PushKey(ConsoleKey.Enter);

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Single(executor.ExecutedCommands);
        Assert.Equal("dotnet build -c Release", executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Run_InteractiveMode_WhenExitSelected_ReturnsZeroWithoutExecuting()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        // 2 tasks: Down -> test, Down -> [Sair], Enter -> select
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Empty(executor.ExecutedCommands);
    }

    [Fact]
    public void Run_InteractiveMode_DoesNotRenderStaticTableOrAvailableTasksText()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("Tarefas Disponíveis (nxtl)", console.Output);
        Assert.DoesNotContain("Tarefas Disponíveis", console.Output);
    }

    [Fact]
    public void Run_InteractiveMode_RendersVersionHeaderInGreenAtTop()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("nxtl", console.Output);
        Assert.Contains("versão", console.Output);
        Assert.Contains(AppVersion.Current, console.Output);

        var versionIndex = console.Output.IndexOf("nxtl", StringComparison.Ordinal);
        var promptIndex = console.Output.IndexOf("Selecionar uma tarefa para executar", StringComparison.Ordinal);
        Assert.True(versionIndex >= 0, "Version header should be rendered.");
        Assert.True(promptIndex > versionIndex, "Version header should appear before the selection prompt.");
    }
}
