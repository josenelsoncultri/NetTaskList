using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class QuickstartValidationTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockProcessExecutor _executor = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void Scenario1_ProjectWithOnlyPackageJson_RunsTestTask()
    {
        var packageJson = """
        {
          "name": "sample-node-project",
          "scripts": {
            "start": "echo Starting Application",
            "test": "echo Running Tests"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--run", "test"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo Running Tests", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Scenario2_ProjectWithBothNxtlJsonAndPackageJson_NxtlJsonPrevails()
    {
        var packageJson = """
        {
          "name": "sample-node-project",
          "scripts": {
            "test": "echo Running from package.json"
          }
        }
        """;
        var nxtlJson = """
        {
          "scripts": [
            {
              "name": "test",
              "command": "echo Running from nxtl.json"
            }
          ]
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "test"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo Running from nxtl.json", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Scenario3_NpmLifecycleHooks_RunsPreMainPostInOrder()
    {
        var packageJson = """
        {
          "scripts": {
            "prebuild": "echo [PRE-BUILD]",
            "build": "echo [MAIN-BUILD]",
            "postbuild": "echo [POST-BUILD]"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Equal(3, _executor.ExecutedCommands.Count);
        Assert.Equal("echo [PRE-BUILD]", _executor.ExecutedCommands[0]);
        Assert.Equal("echo [MAIN-BUILD]", _executor.ExecutedCommands[1]);
        Assert.Equal("echo [POST-BUILD]", _executor.ExecutedCommands[2]);
    }

    [Fact]
    public void Scenario4_FailingPreHook_AbortsAndReturnsExitCode()
    {
        var packageJson = """
        {
          "scripts": {
            "prebuild": "exit 1",
            "build": "echo [MAIN-BUILD]",
            "postbuild": "echo [POST-BUILD]"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);
        _executor.EnqueueExitCode(1);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("exit 1", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Scenario5_NeitherConfigFileFound_ReturnsExitCode1WithPortugueseDiagnostics()
    {
        var exitCode = Program.Run([], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Contains("Nenhum arquivo de configuração", _console.Output);
        Assert.Contains("nxtl.json", _console.Output);
        Assert.Contains("package.json", _console.Output);
        Assert.Contains("nxtl --init", _console.Output);
    }
}
