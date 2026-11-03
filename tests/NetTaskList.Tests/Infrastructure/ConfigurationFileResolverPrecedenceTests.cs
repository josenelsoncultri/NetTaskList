using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class ConfigurationFileResolverPrecedenceTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockProcessExecutor _executor = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void ProgramRun_WhenBothNxtlJsonAndPackageJsonExist_ExecutesOnlyFromNxtlJson()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "build", "command": "dotnet build" }
          ]
        }
        """;
        var packageJson = """
        {
          "scripts": {
            "build": "npm run build-web",
            "web": "vite"
          }
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("dotnet build", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void ProgramRun_WhenBothExistAndTaskOnlyInPackageJson_ReturnsErrorTaskNotFoundInNxtlJson()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "build", "command": "dotnet build" }
          ]
        }
        """;
        var packageJson = """
        {
          "scripts": {
            "web": "vite"
          }
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--run", "web"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("não encontrada", _console.Output);
        Assert.Contains("nxtl.json", _console.Output);
    }

    [Fact]
    public void ProgramRun_WhenBothExistAndNxtlJsonIsCorrupted_FailsWithoutFallingBackToPackageJson()
    {
        var corruptedNxtlJson = "{ scripts: [ unquoted ] }";
        var validPackageJson = """
        {
          "scripts": {
            "build": "npm run build"
          }
        }
        """;
        _fileSystem.AddFile("nxtl.json", corruptedNxtlJson);
        _fileSystem.AddFile("package.json", validPackageJson);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Erro de Configuração", _console.Output);
        Assert.Contains("nxtl.json", _console.Output);
    }
}
