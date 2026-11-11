using NetTaskList.Domain.Models;

namespace NetTaskList.Tests.Domain;

public class TaskCommandStepTests
{
    [Fact]
    public void FromCommandLine_ShouldSetCommandLineAndIsShortcutFalse()
    {
        var step = TaskCommandStep.FromCommandLine("dotnet build");

        Assert.False(step.IsShortcut);
        Assert.Equal("dotnet build", step.CommandLine);
        Assert.Null(step.TargetTaskName);
        Assert.Equal("dotnet build", step.ToString());
    }

    [Fact]
    public void FromShortcut_ShouldSetTargetTaskNameAndIsShortcutTrue()
    {
        var step = TaskCommandStep.FromShortcut("clean");

        Assert.True(step.IsShortcut);
        Assert.Equal("clean", step.TargetTaskName);
        Assert.Null(step.CommandLine);
        Assert.Equal("task: clean", step.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromCommandLine_ShouldThrowOnNullOrWhitespace(string? invalid)
    {
        Assert.Throws<ArgumentException>(() => TaskCommandStep.FromCommandLine(invalid!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromShortcut_ShouldThrowOnNullOrWhitespace(string? invalid)
    {
        Assert.Throws<ArgumentException>(() => TaskCommandStep.FromShortcut(invalid!));
    }

    [Fact]
    public void Constructor_WithIsShortcutFalse_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new TaskCommandStep("clean", false));
    }
}
