using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class InteractiveTaskSelectorFilteringTests
{
    [Fact]
    public void FilterDisplayTasks_WhenPackageJsonSource_HidesPreAndPostWhenMainTaskExists()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "echo pre", TaskType.Tool),
            new("build", "tsc", TaskType.Tool),
            new("postbuild", "echo post", TaskType.Tool),
            new("prepare", "husky install", TaskType.Tool),
            new("pretest", "echo pretest", TaskType.Tool),
            new("test", "jest", TaskType.Tool)
        };

        var filtered = InteractiveTaskSelector.FilterDisplayTasks(tasks, ConfigurationSourceType.PackageJson);

        Assert.Equal(3, filtered.Count);
        Assert.Contains(filtered, t => t.Name == "build");
        Assert.Contains(filtered, t => t.Name == "prepare");
        Assert.Contains(filtered, t => t.Name == "test");
        Assert.DoesNotContain(filtered, t => t.Name == "prebuild");
        Assert.DoesNotContain(filtered, t => t.Name == "postbuild");
        Assert.DoesNotContain(filtered, t => t.Name == "pretest");
    }

    [Fact]
    public void FilterDisplayTasks_WhenNxtlJsonSource_DoesNotFilterAnyTasks()
    {
        var tasks = new List<ScriptTask>
        {
            new("prebuild", "echo pre", TaskType.Tool),
            new("build", "dotnet build", TaskType.Tool),
            new("postbuild", "echo post", TaskType.Tool)
        };

        var filtered = InteractiveTaskSelector.FilterDisplayTasks(tasks, ConfigurationSourceType.NxtlJson);

        Assert.Equal(3, filtered.Count);
    }

    [Fact]
    public void FilterDisplayTasks_WhenHookHasNoMatchingMainTask_KeepsHookInMenu()
    {
        var tasks = new List<ScriptTask>
        {
            new("precommit", "lint-staged", TaskType.Tool),
            new("build", "dotnet build", TaskType.Tool)
        };

        var filtered = InteractiveTaskSelector.FilterDisplayTasks(tasks, ConfigurationSourceType.PackageJson);

        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, t => t.Name == "precommit");
        Assert.Contains(filtered, t => t.Name == "build");
    }
}

