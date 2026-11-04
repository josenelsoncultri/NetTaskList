using System.Text.Json;
using NetTaskList.Infrastructure.Dtos;
using NetTaskList.Infrastructure.Serialization;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class CommandPropertyJsonConverterTests
{
    [Fact]
    public void Deserialize_SingleStringCommand_ReturnsSingleItemList()
    {
        var json = """
        {
          "name": "build",
          "command": "dotnet build"
        }
        """;

        var dto = JsonSerializer.Deserialize(json, TaskJsonContext.Default.ScriptTaskDto);

        Assert.NotNull(dto);
        Assert.NotNull(dto.Commands);
        Assert.Single(dto.Commands);
        Assert.Equal("dotnet build", dto.Commands[0]);
        Assert.Equal("dotnet build", dto.Command);
    }

    [Fact]
    public void Deserialize_ArrayOfCommands_ReturnsAllItemsInOrder()
    {
        var json = """
        {
          "name": "pipeline",
          "command": [
            "dotnet clean",
            "dotnet build",
            "dotnet test"
          ]
        }
        """;

        var dto = JsonSerializer.Deserialize(json, TaskJsonContext.Default.ScriptTaskDto);

        Assert.NotNull(dto);
        Assert.NotNull(dto.Commands);
        Assert.Equal(3, dto.Commands.Count);
        Assert.Equal("dotnet clean", dto.Commands[0]);
        Assert.Equal("dotnet build", dto.Commands[1]);
        Assert.Equal("dotnet test", dto.Commands[2]);
        Assert.Equal("dotnet clean", dto.Command);
    }

    [Fact]
    public void Deserialize_EmptyArrayOfCommands_ReturnsEmptyList()
    {
        var json = """
        {
          "name": "empty",
          "command": []
        }
        """;

        var dto = JsonSerializer.Deserialize(json, TaskJsonContext.Default.ScriptTaskDto);

        Assert.NotNull(dto);
        Assert.NotNull(dto.Commands);
        Assert.Empty(dto.Commands);
        Assert.Null(dto.Command);
    }

    [Fact]
    public void Deserialize_NullCommand_ReturnsNullCommands()
    {
        var json = """
        {
          "name": "null-cmd",
          "command": null
        }
        """;

        var dto = JsonSerializer.Deserialize(json, TaskJsonContext.Default.ScriptTaskDto);

        Assert.NotNull(dto);
        Assert.Null(dto.Commands);
        Assert.Null(dto.Command);
    }

    [Fact]
    public void Deserialize_NonStringElementInArray_ThrowsJsonException()
    {
        var json = """
        {
          "name": "invalid",
          "command": [ 123 ]
        }
        """;

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize(json, TaskJsonContext.Default.ScriptTaskDto));
    }

    [Fact]
    public void Serialize_SingleCommand_WritesString()
    {
        var dto = new ScriptTaskDto
        {
            Name = "test",
            Commands = new List<string> { "dotnet test" }
        };

        var json = JsonSerializer.Serialize(dto, TaskJsonContext.Default.ScriptTaskDto);

        Assert.Contains("\"command\": \"dotnet test\"", json);
    }

    [Fact]
    public void Serialize_MultipleCommands_WritesArray()
    {
        var dto = new ScriptTaskDto
        {
            Name = "test",
            Commands = new List<string> { "dotnet clean", "dotnet test" }
        };

        var json = JsonSerializer.Serialize(dto, TaskJsonContext.Default.ScriptTaskDto);

        Assert.Contains("\"command\": [", json);
        Assert.Contains("\"dotnet clean\"", json);
        Assert.Contains("\"dotnet test\"", json);
    }
}
