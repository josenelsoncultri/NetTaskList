using NetTaskList.Infrastructure;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class PackageJsonValidationTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockProcessExecutor _executor = new();
    private readonly MockAnsiConsole _console = new();

    [Fact]
    public void Read_MalformedPackageJsonSyntax_ThrowsJsonConfigurationException()
    {
        var brokenJson = "{ \"scripts\": { unquoted } }";
        _fileSystem.AddFile("package.json", brokenJson);
        var reader = new JsonConfigurationReader(_fileSystem);

        var ex = Assert.Throws<JsonConfigurationException>(() => reader.Read("package.json"));
        Assert.Contains("não é um JSON válido", ex.Message);
        Assert.Contains("package.json", ex.Message);
    }

    [Fact]
    public void Read_NonStringScriptValue_ThrowsConfigurationValidationException()
    {
        var jsonWithNumber = """
        {
          "scripts": {
            "build": 123
          }
        }
        """;
        _fileSystem.AddFile("package.json", jsonWithNumber);
        var reader = new JsonConfigurationReader(_fileSystem);

        var ex = Assert.Throws<ConfigurationValidationException>(() => reader.Read("package.json"));
        Assert.False(ex.ValidationResult.IsValid);
        Assert.Contains(ex.ValidationResult.Errors, e => e.Message.Contains("deve ser uma string de texto válida"));
    }

    [Fact]
    public void Read_EmptyOrWhitespaceCommand_ThrowsConfigurationValidationException()
    {
        var jsonWithEmptyCommand = """
        {
          "scripts": {
            "build": "   "
          }
        }
        """;
        _fileSystem.AddFile("package.json", jsonWithEmptyCommand);
        var reader = new JsonConfigurationReader(_fileSystem);

        var ex = Assert.Throws<ConfigurationValidationException>(() => reader.Read("package.json"));
        Assert.False(ex.ValidationResult.IsValid);
        Assert.Contains(ex.ValidationResult.Errors, e => e.Message.Contains("não pode ser vazio"));
    }

    [Fact]
    public void ProgramRun_WhenPackageJsonHasNoScripts_OutputsPoliteNoticeAndReturnsZero()
    {
        var jsonWithoutScripts = """
        {
          "name": "metadata-only"
        }
        """;
        _fileSystem.AddFile("package.json", jsonWithoutScripts);

        var exitCode = Program.Run([], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Nenhuma tarefa configurada no arquivo", _console.Output);
    }
}

