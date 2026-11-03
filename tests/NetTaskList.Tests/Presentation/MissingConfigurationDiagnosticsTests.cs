using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class MissingConfigurationDiagnosticsTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockProcessExecutor _executor = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void ProgramRun_WhenNeitherConfigFileExists_OutputsPortugueseDiagnosticsWithInitGuidance()
    {
        var exitCode = Program.Run([], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Nenhum arquivo de configuração", _console.Output);
        Assert.Contains("nxtl.json", _console.Output);
        Assert.Contains("package.json", _console.Output);
        Assert.Contains("nxtl --init", _console.Output);
    }

    [Fact]
    public void ProgramRun_WhenExplicitConfigFileMissing_OutputsMissingFileError()
    {
        var exitCode = Program.Run(["-c", "custom-missing.json"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("custom-missing.json", _console.Output);
        Assert.Contains("não foi encontrado", _console.Output);
    }
}

