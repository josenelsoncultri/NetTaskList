using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class PackageJsonReaderTests
{
    private readonly MockFileSystem _fileSystem = new();

    [Fact]
    public void Read_ValidPackageJsonWithScripts_ReturnsTasksWithToolType()
    {
        var json = """
        {
          "name": "sample-project",
          "scripts": {
            "start": "node index.js",
            "build": "tsc",
            "test:watch": "vitest --watch"
          }
        }
        """;
        _fileSystem.AddFile("package.json", json);
        var reader = new JsonConfigurationReader(_fileSystem);

        var config = reader.Read("package.json");

        Assert.Equal(ConfigurationSourceType.PackageJson, config.SourceType);
        Assert.Equal(3, config.Count);

        var startTask = config.Tasks[0];
        Assert.Equal("start", startTask.Name);
        Assert.Equal("node index.js", startTask.Command);
        Assert.Equal(TaskType.Tool, startTask.Type);
        Assert.Null(startTask.Description);

        var buildTask = config.Tasks[1];
        Assert.Equal("build", buildTask.Name);
        Assert.Equal("tsc", buildTask.Command);
        Assert.Equal(TaskType.Tool, buildTask.Type);

        var testWatchTask = config.Tasks[2];
        Assert.Equal("test:watch", testWatchTask.Name);
        Assert.Equal("vitest --watch", testWatchTask.Command);
        Assert.Equal(TaskType.Tool, testWatchTask.Type);
    }

    [Fact]
    public void Read_PackageJsonWithoutScripts_ReturnsEmptyConfiguration()
    {
        var json = """
        {
          "name": "sample-project"
        }
        """;
        _fileSystem.AddFile("package.json", json);
        var reader = new JsonConfigurationReader(_fileSystem);

        var config = reader.Read("package.json");

        Assert.Equal(ConfigurationSourceType.PackageJson, config.SourceType);
        Assert.False(config.HasTasks);
        Assert.Equal(0, config.Count);
    }

    [Fact]
    public void Read_PackageJsonWithEmptyScripts_ReturnsEmptyConfiguration()
    {
        var json = """
        {
          "name": "sample-project",
          "scripts": {}
        }
        """;
        _fileSystem.AddFile("package.json", json);
        var reader = new JsonConfigurationReader(_fileSystem);

        var config = reader.Read("package.json");

        Assert.Equal(ConfigurationSourceType.PackageJson, config.SourceType);
        Assert.False(config.HasTasks);
    }
}

