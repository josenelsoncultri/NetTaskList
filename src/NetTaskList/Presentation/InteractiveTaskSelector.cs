using NetTaskList.Domain.Models;
using Spectre.Console;

namespace NetTaskList.Presentation;

public static class InteractiveTaskSelector
{
    public static IReadOnlyList<ScriptTask> FilterDisplayTasks(
        IReadOnlyList<ScriptTask> tasks,
        ConfigurationSourceType sourceType)
    {
        if (sourceType != ConfigurationSourceType.PackageJson)
        {
            return tasks;
        }

        var result = new List<ScriptTask>();
        foreach (var task in tasks)
        {
            if (task.Name.StartsWith("pre", StringComparison.OrdinalIgnoreCase) && task.Name.Length > 3)
            {
                var baseName = task.Name[3..];
                if (tasks.Any(t => t.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue; // Hide pre hook when matching main task exists
                }
            }

            if (task.Name.StartsWith("post", StringComparison.OrdinalIgnoreCase) && task.Name.Length > 4)
            {
                var baseName = task.Name[4..];
                if (tasks.Any(t => t.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue; // Hide post hook when matching main task exists
                }
            }

            result.Add(task);
        }

        return result;
    }

    public static ScriptTask? SelectTask(
        IReadOnlyList<ScriptTask> tasks,
        IAnsiConsole? console = null)
    {
        return SelectTask(tasks, ConfigurationSourceType.NxtlJson, console);
    }

    public static ScriptTask? SelectTask(
        IReadOnlyList<ScriptTask> tasks,
        ConfigurationSourceType sourceType,
        IAnsiConsole? console = null)
    {
        var ansiConsole = console ?? AnsiConsole.Console;
        var displayTasks = FilterDisplayTasks(tasks, sourceType);

        var prompt = new SelectionPrompt<TaskChoice>()
            .Title("[cyan]Selecionar uma tarefa para executar[/]")
            .PageSize(8)
            .MoreChoicesText("[grey](Mova para cima e para baixo para ver mais opções)[/]")
            .UseConverter(choice => choice.DisplayName);

        foreach (var task in displayTasks)
        {
            var typeTag = task.Type == TaskType.CSharp ? "csharp" : "tool";
            string description;
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                description = task.Description;
            }
            else if (task.Commands.Count > 1)
            {
                description = "<sem descrição>";
            }
            else
            {
                description = task.Command;
            }

            var label = $"{task.Name} [{typeTag}] - {description}";
            prompt.AddChoice(new TaskChoice(task, Markup.Escape(label)));
        }

        prompt.AddChoice(new TaskChoice(null, Markup.Escape("[Sair]")));

        var selected = ansiConsole.Prompt(prompt);
        return selected.Task;
    }

    private sealed record TaskChoice(ScriptTask? Task, string DisplayName);
}
