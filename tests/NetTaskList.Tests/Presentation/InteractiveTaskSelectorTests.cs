using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class InteractiveTaskSelectorTests
{
    private static IReadOnlyList<ScriptTask> CreateSampleTasks()
    {
        return
        [
            new ScriptTask("build", "dotnet build", TaskType.Tool, "Compila a solução do projeto"),
            new ScriptTask("test", "dotnet test", TaskType.Tool, "Executa todos os testes automatizados"),
            new ScriptTask("seed", "scripts/seed.cs", TaskType.CSharp, "Executa o script de carga inicial em C#")
        ];
    }

    [Fact]
    public void SelectTask_WhenExitSelected_ReturnsNull()
    {
        // Arrange
        var tasks = CreateSampleTasks();
        var console = new MockAnsiConsole();

        // 3 tasks -> cursor starts on task 0 (build).
        // Down -> task 1 (test), Down -> task 2 (seed), Down -> [Sair], Enter -> select.
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);

        // Act
        var result = InteractiveTaskSelector.SelectTask(tasks, console);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectTask_WhenFirstTaskSelected_ReturnsFirstTask()
    {
        // Arrange
        var tasks = CreateSampleTasks();
        var console = new MockAnsiConsole();

        // Cursor starts on first task, press Enter to select it
        console.PushKey(ConsoleKey.Enter);

        // Act
        var result = InteractiveTaskSelector.SelectTask(tasks, console);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("build", result.Name);
    }

    [Fact]
    public void SelectTask_WhenNavigatingToSecondTask_ReturnsSecondTask()
    {
        // Arrange
        var tasks = CreateSampleTasks();
        var console = new MockAnsiConsole();

        // Down -> task 1 (test), Enter -> select
        console.PushKey(ConsoleKey.DownArrow);
        console.PushKey(ConsoleKey.Enter);

        // Act
        var result = InteractiveTaskSelector.SelectTask(tasks, console);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result.Name);
    }

    [Fact]
    public void SelectTask_WhenTasksContainBracketsInNameOrDescription_RendersWithoutMarkupException()
    {
        // Arrange
        var tasks = new List<ScriptTask>
        {
            new("[build:prod]", "dotnet build -c Release", TaskType.Tool, "Builds [production] release artifacts"),
            new("test:unit", "dotnet test", TaskType.Tool, "Executes [unit [nested]] tests with [tags]"),
            new("custom [csharp]", "scripts/run.cs", TaskType.CSharp, "Executa script com tag de cor [green] e [bold]")
        };

        var console = new MockAnsiConsole();
        console.PushKey(ConsoleKey.Enter);

        // Act & Assert (Should not throw InvalidOperationException: Encountered malformed markup tag)
        var result = InteractiveTaskSelector.SelectTask(tasks, console);

        Assert.NotNull(result);
        Assert.Equal("[build:prod]", result.Name);
    }

    [Fact]
    public void SelectTask_RendersCyanTitleAndRespectsPageSizeEight()
    {
        // Arrange: 10 tasks + 1 exit option = 11 choices
        var tasks = Enumerable.Range(1, 10)
            .Select(i => new ScriptTask($"task-{i:D2}", $"echo {i}", TaskType.Tool, $"Description {i}"))
            .ToList();

        var console = new MockAnsiConsole();
        console.PushKey(ConsoleKey.Enter);

        // Act
        var result = InteractiveTaskSelector.SelectTask(tasks, console);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("task-01", result.Name);

        // Title text in Portuguese without trailing colon
        Assert.Contains("Selecionar uma tarefa para executar", console.Output);
        Assert.DoesNotContain("Selecione uma tarefa para executar:", console.Output);

        // When PageSize is 8, tasks beyond the 8 visible viewport choices should not appear initially,
        // and the MoreChoicesText message should be rendered.
        Assert.Contains("(Mova para cima e para baixo para ver mais opções)", console.Output);
        Assert.DoesNotContain("task-10", console.Output);
    }
}
