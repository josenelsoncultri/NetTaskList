using NetTaskList.Domain.Validation;
using NetTaskList.Infrastructure.Dtos;
using Xunit;

namespace NetTaskList.Tests.Domain;

public class ConfigurationValidatorTests
{
    private readonly ConfigurationValidator _validator = new();

    [Fact]
    public void Validate_UniqueValidTasks_ReturnsSuccess()
    {
        var tasks = new List<ScriptTaskDto>
        {
            new() { Name = "build", Command = "dotnet build", Type = "tool" },
            new() { Name = "test", Command = "dotnet test", Type = "tool" },
            new() { Name = "seed", Command = "scripts/seed.cs", Type = "csharp" }
        };

        var result = _validator.Validate(tasks);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("build", "build")]
    [InlineData("build", "BUILD")]
    [InlineData("Seed", "seed")]
    public void Validate_DuplicateNamesCaseInsensitive_ReturnsError(string name1, string name2)
    {
        var tasks = new List<ScriptTaskDto>
        {
            new() { Name = name1, Command = "dotnet build" },
            new() { Name = name2, Command = "dotnet test" }
        };

        var result = _validator.Validate(tasks);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("duplicado"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingOrWhitespaceName_ReturnsError(string? invalidName)
    {
        var tasks = new List<ScriptTaskDto>
        {
            new() { Name = invalidName, Command = "dotnet build" }
        };

        var result = _validator.Validate(tasks);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("sem nome"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingOrWhitespaceCommand_ReturnsError(string? invalidCommand)
    {
        var tasks = new List<ScriptTaskDto>
        {
            new() { Name = "build", Command = invalidCommand }
        };

        var result = _validator.Validate(tasks);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Command" && e.Message.Contains("não define um comando"));
    }

    [Fact]
    public void Validate_InvalidType_ReturnsError()
    {
        var tasks = new List<ScriptTaskDto>
        {
            new() { Name = "custom", Command = "echo hi", Type = "invalid-type" }
        };

        var result = _validator.Validate(tasks);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Type" && e.Message.Contains("não reconhecido"));
    }
}
