using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class ExplicitConfigResolutionTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockProcessExecutor _executor = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void ProgramRun_WhenExplicitConfigSpecified_BypassesAutoDiscoveryOfPackageJson()
    {
        var packageJson = """
        {
          "scripts": {
            "build": "npm run build-pkg"
          }
        }
        """;
        var customJson = """
        {
          "scripts": [
            { "name": "build", "command": "echo custom-build" }
          ]
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);
        _fileSystem.AddFile("my-custom-scripts.json", customJson);

        var exitCode = Program.Run(["--config", "my-custom-scripts.json", "--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo custom-build", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void ProgramRun_InitCommandWithExistingPackageJson_CreatesStandardNxtlJsonWithoutModifyingPackageJson()
    {
        var packageJson = """
        {
          "name": "untouched",
          "scripts": {
            "test": "vitest"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--init"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.True(_fileSystem.FileExists("nxtl.json"));
        Assert.Equal(packageJson, _fileSystem.ReadAllText("package.json"));

        var nxtlContent = _fileSystem.ReadAllText("nxtl.json");
        Assert.Contains("build", nxtlContent);
        Assert.Contains("test", nxtlContent);
        Assert.Contains("dotnet build", nxtlContent);
    }
}

