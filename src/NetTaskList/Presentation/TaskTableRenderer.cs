using NetTaskList.Domain.Models;
using Spectre.Console;

namespace NetTaskList.Presentation;

public static class TaskTableRenderer
{
    public static void Render(IReadOnlyList<ScriptTask> tasks, IAnsiConsole? console = null)
    {
        var ansiConsole = console ?? AnsiConsole.Console;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold yellow]Tarefas Disponíveis (nxtl)[/]")
            .AddColumn(new TableColumn("[bold]Nome[/]").LeftAligned())
            .AddColumn(new TableColumn("[bold]Tipo[/]").Centered())
            .AddColumn(new TableColumn("[bold]Comando[/]").LeftAligned())
            .AddColumn(new TableColumn("[bold]Descrição[/]").LeftAligned());

        foreach (var task in tasks)
        {
            var badge = task.Type == TaskType.CSharp
                ? "[green]csharp[/]"
                : "[cyan]tool[/]";

            var description = string.IsNullOrWhiteSpace(task.Description)
                ? "[grey]-[/]"
                : Markup.Escape(task.Description);

            table.AddRow(
                $"[bold white]{Markup.Escape(task.Name)}[/]",
                badge,
                $"[yellow]{Markup.Escape(task.Command)}[/]",
                description
            );
        }

        ansiConsole.Write(table);
    }
}
