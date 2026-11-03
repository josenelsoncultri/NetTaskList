using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class ConfigurationFileResolverTests
{
    private readonly MockFileSystem _fileSystem = new();

    [Fact]
    public void Resolve_WhenOnlyPackageJsonExists_ReturnsPackageJson()
    {
        _fileSystem.AddFile("package.json", "{}");
        var resolver = new ConfigurationFileResolver(_fileSystem);

        var result = resolver.Resolve();

        Assert.True(result.Found);
        Assert.Equal("package.json", result.FilePath);
        Assert.Equal(ConfigurationSourceType.PackageJson, result.SourceType);
    }

    [Fact]
    public void Resolve_WhenOnlyNxtlJsonExists_ReturnsNxtlJson()
    {
        _fileSystem.AddFile("nxtl.json", "{}");
        var resolver = new ConfigurationFileResolver(_fileSystem);

        var result = resolver.Resolve();

        Assert.True(result.Found);
        Assert.Equal("nxtl.json", result.FilePath);
        Assert.Equal(ConfigurationSourceType.NxtlJson, result.SourceType);
    }

    [Fact]
    public void Resolve_WhenBothNxtlJsonAndPackageJsonExist_NxtlJsonPrevails()
    {
        _fileSystem.AddFile("nxtl.json", "{}");
        _fileSystem.AddFile("package.json", "{}");
        var resolver = new ConfigurationFileResolver(_fileSystem);

        var result = resolver.Resolve();

        Assert.True(result.Found);
        Assert.Equal("nxtl.json", result.FilePath);
        Assert.Equal(ConfigurationSourceType.NxtlJson, result.SourceType);
    }

    [Fact]
    public void Resolve_WhenNeitherExists_ReturnsNotFoundWithPortugueseMessage()
    {
        var resolver = new ConfigurationFileResolver(_fileSystem);

        var result = resolver.Resolve();

        Assert.False(result.Found);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("nxtl.json", result.ErrorMessage);
        Assert.Contains("package.json", result.ErrorMessage);
    }

    [Fact]
    public void Resolve_WhenExplicitPathProvidedAndExists_ReturnsExplicitPath()
    {
        _fileSystem.AddFile("custom-tasks.json", "{}");
        _fileSystem.AddFile("package.json", "{}");
        var resolver = new ConfigurationFileResolver(_fileSystem);

        var result = resolver.Resolve("custom-tasks.json");

        Assert.True(result.Found);
        Assert.Equal("custom-tasks.json", result.FilePath);
        Assert.Equal(ConfigurationSourceType.Custom, result.SourceType);
    }

    [Fact]
    public void Resolve_WhenExplicitPathDoesNotExist_ReturnsNotFoundWithPortugueseMessage()
    {
        var resolver = new ConfigurationFileResolver(_fileSystem);

        var result = resolver.Resolve("non-existent.json");

        Assert.False(result.Found);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("non-existent.json", result.ErrorMessage);
        Assert.Contains("não foi encontrado", result.ErrorMessage);
    }
}

