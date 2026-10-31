using NetTaskList.Domain.Models;
using Spectre.Console;

namespace NetTaskList.Presentation;

public static class InteractiveTaskSelector
{
    public static ScriptTask? SelectTask(IReadOnlyList<ScriptTask> tasks, IAnsiConsole? console = null)
    {
        var ansiConsole = console ?? AnsiConsole.Console;

        var prompt = new SelectionPrompt<TaskChoice>()
            .Title("[bold yellow]Selecione uma tarefa para executar:[/]")
            .PageSize(15)
            .MoreChoicesText("[grey](Mova para cima e para baixo para ver mais opções)[/]")
            .UseConverter(choice => choice.DisplayName);

        foreach (var task in tasks)
        {
            var typeTag = task.Type == TaskType.CSharp ? "csharp" : "tool";
            var description = string.IsNullOrWhiteSpace(task.Description) ? task.Command : task.Description;
            var label = $"{task.Name} [{typeTag}] - {description}";
            prompt.AddChoice(new TaskChoice(task, Markup.Escape(label)));
        }

        prompt.AddChoice(new TaskChoice(null, "[Sair]"));

        var selected = ansiConsole.Prompt(prompt);
        return selected.Task;
    }

    private sealed record TaskChoice(ScriptTask? Task, string DisplayName);
}
