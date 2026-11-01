using NetTaskList.Domain.Models;
using NetTaskList.Tests.Mocks;
using Spectre.Console;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class ExitCodePropagationTests
{
    private const string ConfigJson = """
    {
      "scripts": [
        {
          "name": "sample",
          "command": "custom-tool run",
          "type": "tool",
          "description": "Tarefa demonstrativa"
        }
      ]
    }
    """;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(127)]
    public void ProgramRun_PropagatesExitCodeFaithfully(int expectedExitCode)
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigJson);

        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(expectedExitCode);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "sample"], fileSystem, executor, console);

        // Assert
        Assert.Equal(expectedExitCode, exitCode);
        Assert.Single(executor.ExecutedCommands);
    }

    [Fact]
    public void ProgramRun_WhenProcessLaunchFails_DisplaysFriendlyErrorMessageAndReturnsExitCode()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigJson);

        var executor = new MockProcessExecutor();
        executor.EnqueueFailure("O sistema não pôde encontrar o arquivo especificado.", exitCode: 1);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "sample"], fileSystem, executor, console);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("Erro ao executar tarefa:", console.Output);
        Assert.Contains("O sistema não pôde encontrar o arquivo especificado.", console.Output);
    }

    [Fact]
    public void ProgramRun_WhenProcessSucceeds_DoesNotDisplayErrorMessage()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", ConfigJson);

        var executor = new MockProcessExecutor();
        executor.EnqueueExitCode(0);
        var console = new MockAnsiConsole();

        // Act
        var exitCode = Program.Run(["--run", "sample"], fileSystem, executor, console);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("Erro ao executar tarefa:", console.Output);
    }
}
