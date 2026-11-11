using NetTaskList.Domain.Models;
using NetTaskList.Domain.Validation;
using NetTaskList.Infrastructure;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class JsonConfigurationReaderTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly ConfigurationValidator _validator = new();

    [Fact]
    public void Read_MalformedJsonSyntax_ThrowsJsonConfigurationExceptionWithPortugueseMessage()
    {
        _fileSystem.AddFile("nxtl.json", "{ \"scripts\": [ { \"name\": \"broken\", ");
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var ex = Assert.Throws<JsonConfigurationException>(() => reader.Read("nxtl.json"));
        Assert.Contains("não é um JSON válido", ex.Message);
    }

    [Fact]
    public void Read_FileNotFound_ThrowsFileNotFoundException()
    {
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        Assert.Throws<FileNotFoundException>(() => reader.Read("missing.json"));
    }

    [Fact]
    public void Read_DuplicateTaskNames_ThrowsConfigurationValidationException()
    {
        var json = """
        {
          "scripts": [
            { "name": "build", "command": "dotnet build" },
            { "name": "BUILD", "command": "dotnet build -c Release" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var ex = Assert.Throws<ConfigurationValidationException>(() => reader.Read("nxtl.json"));
        Assert.False(ex.ValidationResult.IsValid);
        Assert.Contains(ex.ValidationResult.Errors, e => e.PropertyName == "Name" && e.Message.Contains("duplicado"));
    }

    [Fact]
    public void Read_ValidScriptsObject_ReturnsTaskConfiguration()
    {
        var json = """
        {
          "scripts": [
            { "name": "build", "command": "dotnet build", "type": "tool", "description": "Compilação" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var config = reader.Read("nxtl.json");

        Assert.True(config.HasTasks);
        Assert.Equal(1, config.Count);
        Assert.Equal("build", config.Tasks[0].Name);
        Assert.Equal("dotnet build", config.Tasks[0].Command);
        Assert.Equal(TaskType.Tool, config.Tasks[0].Type);
        Assert.Equal("Compilação", config.Tasks[0].Description);
    }

    [Fact]
    public void Read_ValidTasksObjectAlias_ReturnsTaskConfiguration()
    {
        var json = """
        {
          "tasks": [
            { "name": "test", "command": "dotnet test" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var config = reader.Read("nxtl.json");

        Assert.True(config.HasTasks);
        Assert.Equal(1, config.Count);
        Assert.Equal("test", config.Tasks[0].Name);
    }

    [Fact]
    public void Read_ValidDirectArray_ReturnsTaskConfiguration()
    {
        var json = """
        [
          { "name": "seed", "command": "scripts/seed.cs", "type": "csharp" }
        ]
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var config = reader.Read("nxtl.json");

        Assert.True(config.HasTasks);
        Assert.Equal(1, config.Count);
        Assert.Equal("seed", config.Tasks[0].Name);
        Assert.Equal(TaskType.CSharp, config.Tasks[0].Type);
    }

    [Fact]
    public void Read_LegacyConfigurationWithSchema_ReturnsTaskConfigurationSuccessfully()
    {
        var json = """
        {
          "$schema": "https://raw.githubusercontent.com/nettasklist/nxtl/main/schemas/nxtl-v1.schema.json",
          "scripts": [
            { "name": "legacy-task", "command": "dotnet run", "type": "tool", "description": "Legacy task" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var config = reader.Read("nxtl.json");

        Assert.True(config.HasTasks);
        Assert.Equal(1, config.Count);
        Assert.Equal("legacy-task", config.Tasks[0].Name);
        Assert.Equal("dotnet run", config.Tasks[0].Command);
        Assert.Equal(TaskType.Tool, config.Tasks[0].Type);
        Assert.Equal("Legacy task", config.Tasks[0].Description);
    }

    [Fact]
    public void Read_TaskWithShortcutObject_MapsToTaskCommandStepWithIsShortcutTrue()
    {
        var json = """
        {
          "scripts": [
            { "name": "compilar", "command": { "task": "build" } }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var config = reader.Read("nxtl.json");

        Assert.True(config.HasTasks);
        Assert.Equal(1, config.Count);
        var task = config.Tasks[0];
        Assert.Equal("compilar", task.Name);
        Assert.Single(task.CommandSteps);
        Assert.True(task.CommandSteps[0].IsShortcut);
        Assert.Equal("build", task.CommandSteps[0].TargetTaskName);
    }

    [Fact]
    public void Read_TaskWithMixedArray_MapsBothCommandLinesAndShortcuts()
    {
        var json = """
        {
          "scripts": [
            {
              "name": "pipeline",
              "command": [
                "echo start",
                { "task": "build" },
                "echo finish"
              ]
            }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", json);
        var reader = new JsonConfigurationReader(_fileSystem, _validator);

        var config = reader.Read("nxtl.json");

        Assert.True(config.HasTasks);
        var task = config.Tasks[0];
        Assert.Equal(3, task.CommandSteps.Count);
        Assert.False(task.CommandSteps[0].IsShortcut);
        Assert.Equal("echo start", task.CommandSteps[0].CommandLine);
        Assert.True(task.CommandSteps[1].IsShortcut);
        Assert.Equal("build", task.CommandSteps[1].TargetTaskName);
        Assert.False(task.CommandSteps[2].IsShortcut);
        Assert.Equal("echo finish", task.CommandSteps[2].CommandLine);
    }
}
