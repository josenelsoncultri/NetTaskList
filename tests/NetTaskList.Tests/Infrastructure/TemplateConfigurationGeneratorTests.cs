using NetTaskList.Domain.Models;
using NetTaskList.Domain.Validation;
using NetTaskList.Infrastructure;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class TemplateConfigurationGeneratorTests
{
    [Fact]
    public void GenerateDefaultJson_GeneratesValidParsableJson()
    {
        var template = TemplateConfigurationGenerator.GenerateDefaultJson();
        Assert.NotNull(template);
        Assert.DoesNotContain("$schema", template);
        Assert.DoesNotContain("nxtl-v1.schema.json", template);
        Assert.Contains("build", template);
        Assert.Contains("test", template);
        Assert.Contains("seed", template);
        Assert.Contains("update-nettasklist", template);
        Assert.Contains("dotnet tool update -g nettasklist", template);

        // Verify it parses cleanly with JsonConfigurationReader and passes validation
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("nxtl.json", template);
        var reader = new JsonConfigurationReader(fileSystem, new ConfigurationValidator());

        var config = reader.Read("nxtl.json");
        Assert.True(config.HasTasks);
        Assert.Equal(4, config.Count);
        Assert.Contains(config.Tasks, t => t.Name == "build" && t.Type == TaskType.Tool);
        Assert.Contains(config.Tasks, t => t.Name == "test" && t.Type == TaskType.Tool);
        Assert.Contains(config.Tasks, t => t.Name == "seed" && t.Type == TaskType.CSharp);
        Assert.Contains(config.Tasks, t => t.Name == "update-nettasklist" && t.Type == TaskType.Tool && t.Command == "dotnet tool update -g nettasklist" && t.Description == "Atualiza a ferramenta nxtl de modo global");
    }
}
