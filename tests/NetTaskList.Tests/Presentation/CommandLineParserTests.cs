using NetTaskList.Presentation;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_EmptyArgs_ReturnsDefaultOptions()
    {
        var options = CommandLineParser.Parse([]);

        Assert.True(options.IsValid);
        Assert.Null(options.ConfigPath);
        Assert.Null(options.RunTaskName);
        Assert.False(options.Init);
        Assert.False(options.ShowHelp);
        Assert.False(options.ShowVersion);
    }

    [Theory]
    [InlineData("--run", "build")]
    [InlineData("-r", "build")]
    public void Parse_RunOption_SetsRunTaskName(string flag, string taskName)
    {
        var options = CommandLineParser.Parse([flag, taskName]);

        Assert.True(options.IsValid);
        Assert.Equal("build", options.RunTaskName);
    }

    [Fact]
    public void Parse_RunOptionWithoutValue_ReturnsInvalidWithError()
    {
        var options = CommandLineParser.Parse(["--run"]);

        Assert.False(options.IsValid);
        Assert.NotNull(options.ErrorMessage);
    }

    [Theory]
    [InlineData("--config", "custom.json")]
    [InlineData("-c", "custom.json")]
    [InlineData("-c", "configs/my-tasks.json")]
    public void Parse_ConfigOption_SetsConfigPath(string flag, string path)
    {
        var options = CommandLineParser.Parse([flag, path]);

        Assert.True(options.IsValid);
        Assert.Equal(path, options.ConfigPath);
    }

    [Fact]
    public void Parse_ConfigOptionWithoutValue_ReturnsInvalidWithError()
    {
        var options = CommandLineParser.Parse(["--config"]);

        Assert.False(options.IsValid);
        Assert.NotNull(options.ErrorMessage);
    }

    [Theory]
    [InlineData("--init")]
    [InlineData("-i")]
    public void Parse_InitOption_SetsInitTrue(string flag)
    {
        var options = CommandLineParser.Parse([flag]);

        Assert.True(options.IsValid);
        Assert.True(options.Init);
    }

    [Fact]
    public void Parse_InitWithCustomConfigPath_SetsBothOptions()
    {
        var options = CommandLineParser.Parse(["--init", "--config", "custom-tasks.json"]);

        Assert.True(options.IsValid);
        Assert.True(options.Init);
        Assert.Equal("custom-tasks.json", options.ConfigPath);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    [InlineData("-?")]
    public void Parse_HelpOption_SetsShowHelpTrue(string flag)
    {
        var options = CommandLineParser.Parse([flag]);

        Assert.True(options.IsValid);
        Assert.True(options.ShowHelp);
    }

    [Theory]
    [InlineData("--version")]
    [InlineData("-v")]
    public void Parse_VersionOption_SetsShowVersionTrue(string flag)
    {
        var options = CommandLineParser.Parse([flag]);

        Assert.True(options.IsValid);
        Assert.True(options.ShowVersion);
    }

    [Fact]
    public void Parse_UnknownOption_ReturnsInvalidWithError()
    {
        var options = CommandLineParser.Parse(["--unknown-flag"]);

        Assert.False(options.IsValid);
        Assert.NotNull(options.ErrorMessage);
    }
}
