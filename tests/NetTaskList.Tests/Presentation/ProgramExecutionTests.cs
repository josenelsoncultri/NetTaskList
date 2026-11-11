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

    [Fact]
    public void Run_WithDirectToolTask_WhenUpdateAvailable_DisplaysAlertAtTopOfConsole()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();
        var checker = new MockNugetVersionChecker(VersionCheckResult.UpdateAvailable("1.6.1", "1.7.0"));

        // Act
        var exitCode = Program.Run(["--run", "build"], fileSystem, executor, console, checker);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Nova versão disponível", console.Output);
        Assert.Contains("1.6.1", console.Output);
        Assert.Contains("1.7.0", console.Output);
        Assert.Contains("dotnet tool update -g nettasklist", console.Output);

        var alertIndex = console.Output.IndexOf("Nova versão disponível", StringComparison.Ordinal);
        var taskExecutionIndex = console.Output.IndexOf("build", StringComparison.Ordinal);
        Assert.True(alertIndex >= 0);
        Assert.True(alertIndex < taskExecutionIndex, "Alert panel must be rendered at the very top before task execution.");
    }

    [Fact]
    public void Run_InteractiveMode_WhenUpdateAvailable_DisplaysAlertAtTopOfConsoleBeforeHeader()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);
        var checker = new MockNugetVersionChecker(VersionCheckResult.UpdateAvailable("1.6.1", "1.7.0"));

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console, checker);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Nova versão disponível", console.Output);
        Assert.Contains("1.6.1", console.Output);
        Assert.Contains("1.7.0", console.Output);

        var alertIndex = console.Output.IndexOf("Nova versão disponível", StringComparison.Ordinal);
        var versionHeaderIndex = console.Output.IndexOf($"nxtl versão {AppVersion.Current}", StringComparison.Ordinal);
        Assert.True(alertIndex >= 0);
        Assert.True(alertIndex < versionHeaderIndex, "Alert panel must be rendered before the version header and task selector.");
    }

    [Fact]
    public void Run_WhenInstalledVersionIsUpToDate_DoesNotDisplayAlert()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);
        var checker = new MockNugetVersionChecker(VersionCheckResult.NoUpdate(AppVersion.Current));

        // Act
        var exitCode = Program.Run([], fileSystem, executor, console, checker);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("Nova versão disponível", console.Output);
        Assert.DoesNotContain("dotnet tool update -g nettasklist", console.Output);
    }

    [Fact]
    public void Run_WhenCheckerFails_DoesNotOutputErrorsAndExecutesNormally()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", SampleConfig);
        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();
        var checker = new MockNugetVersionChecker(VersionCheckResult.Failed(AppVersion.Current));

        // Act
        var exitCode = Program.Run(["--run", "build"], fileSystem, executor, console, checker);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("Nova versão disponível", console.Output);
        Assert.DoesNotContain("Erro", console.Output);
        Assert.Single(executor.ExecutedCommands);
    }

    [Fact]
    public void Run_ShowVersion_DoesNotInvokeVersionChecker()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        var checker = new MockNugetVersionChecker();

        // Act
        var exitCode = Program.Run(["--version"], fileSystem, executor, console, checker);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(0, checker.CallCount);
    }

    [Fact]
    public void Run_ShowHelp_DoesNotInvokeVersionChecker()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var executor = new MockProcessExecutor();
        var console = new MockAnsiConsole();
        var checker = new MockNugetVersionChecker();

        // Act
        var exitCode = Program.Run(["--help"], fileSystem, executor, console, checker);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(0, checker.CallCount);
    }
}
