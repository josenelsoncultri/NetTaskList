using NetTaskList.Domain.Models;
using Spectre.Console;

namespace NetTaskList.Presentation;

public static class UpdateNotificationPresenter
{
    public static void Present(IAnsiConsole console, VersionCheckResult? result)
    {
        if (result is null || !result.HasUpdate || string.IsNullOrWhiteSpace(result.LatestVersion))
        {
            return;
        }

        var content = new Markup(
            $"[bold yellow]Nova versão disponível:[/] [white]{Markup.Escape(result.CurrentVersion)}[/] → [bold green]{Markup.Escape(result.LatestVersion)}[/]\n" +
            "Para atualizar a ferramenta globalmente, execute:\n" +
            "[bold cyan]dotnet tool update -g nettasklist[/]");

        var panel = new Panel(content)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow),
            Padding = new Padding(1, 0, 1, 0)
        };

        console.Write(panel);
        console.WriteLine();
    }
}
