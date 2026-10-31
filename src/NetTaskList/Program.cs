using NetTaskList.Domain.Models;
using NetTaskList.Domain.Validation;
using NetTaskList.Infrastructure;
using NetTaskList.Presentation;
using Spectre.Console;

namespace NetTaskList;

public static class Program
{
    public static int Main(string[] args)
    {
        var options = CommandLineParser.Parse(args);

        if (!options.IsValid)
        {
            AnsiConsole.MarkupLine($"[bold red]Erro:[/] {Markup.Escape(options.ErrorMessage ?? "Argumentos inválidos.")}");
            return 2;
        }

        if (options.ShowHelp)
        {
            ShowHelp();
            return 0;
        }

        if (options.ShowVersion)
        {
            AnsiConsole.MarkupLine("[bold green]nxtl[/] versão [yellow]1.0.0[/]");
            return 0;
        }

        if (options.Init)
        {
            return HandleInit(options.ConfigPath ?? "nxtl.json");
        }

        var configFilePath = options.ConfigPath ?? "nxtl.json";
        var fileSystem = new FileSystem();
        var fullPath = fileSystem.GetFullPath(configFilePath);

        if (!fileSystem.FileExists(fullPath))
        {
            AnsiConsole.MarkupLine($"[bold red]Erro:[/] O arquivo de configuração '[yellow]{Markup.Escape(configFilePath)}[/]' não foi encontrado.");
            AnsiConsole.MarkupLine("Execute '[bold cyan]nxtl --init[/]' para criar um arquivo inicial ou use '[bold cyan]nxtl --config <caminho>[/]' para especificar outro arquivo.");
            return 1;
        }

        TaskConfiguration configuration;
        try
        {
            var reader = new JsonConfigurationReader(fileSystem);
            configuration = reader.Read(fullPath);
        }
        catch (ConfigurationValidationException ex)
        {
            AnsiConsole.MarkupLine("[bold red]Erros de validação no arquivo de configuração:[/]");
            foreach (var error in ex.ValidationResult.Errors)
            {
                AnsiConsole.MarkupLine($"  [red]•[/] {Markup.Escape(error.Message)}");
            }
            return 1;
        }
        catch (JsonConfigurationException ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Erro de Configuração:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Erro inesperado:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }

        if (!configuration.HasTasks)
        {
            AnsiConsole.MarkupLine("[yellow]Nenhuma tarefa configurada no arquivo.[/]");
            return 0;
        }

        // Direct execution mode via --run / -r
        if (!string.IsNullOrWhiteSpace(options.RunTaskName))
        {
            var target = configuration.Tasks.FirstOrDefault(t =>
                t.Name.Equals(options.RunTaskName, StringComparison.OrdinalIgnoreCase));

            if (target is null)
            {
                AnsiConsole.MarkupLine($"[bold red]Erro:[/] Tarefa '[yellow]{Markup.Escape(options.RunTaskName)}[/]' não encontrada no arquivo de configuração '[yellow]{Markup.Escape(configFilePath)}[/]'.");
                AnsiConsole.MarkupLine("Execute '[bold cyan]nxtl[/]' para listar todas as tarefas disponíveis.");
                return 1;
            }

            var typeColor = target.Type == TaskType.CSharp ? "green" : "cyan";
            var typeText = target.Type == TaskType.CSharp ? "csharp" : "tool";
            AnsiConsole.MarkupLine($"Tarefa selecionada: [bold white]{Markup.Escape(target.Name)}[/] [[[{typeColor}]{typeText}[/]]]");
            AnsiConsole.MarkupLine($"Comando: [bold yellow]{Markup.Escape(target.ResolveExecutionCommand())}[/]");
            return 0;
        }

        // Display tasks table
        TaskTableRenderer.Render(configuration.Tasks);

        // Fallback for non-interactive / redirected streams
        if (Console.IsInputRedirected)
        {
            AnsiConsole.MarkupLine("[yellow]Aviso:[/] Execução interativa não disponível em console redirecionado. Utilize '[bold cyan]nxtl --run <nome>[/]' para executar uma tarefa diretamente.");
            return 0;
        }

        // Interactive selection mode
        var selectedTask = InteractiveTaskSelector.SelectTask(configuration.Tasks);
        if (selectedTask is null)
        {
            AnsiConsole.MarkupLine("[grey]Operação cancelada pelo usuário.[/]");
            return 0;
        }

        var badgeColor = selectedTask.Type == TaskType.CSharp ? "green" : "cyan";
        var badgeText = selectedTask.Type == TaskType.CSharp ? "csharp" : "tool";
        AnsiConsole.MarkupLine($"Tarefa selecionada: [bold white]{Markup.Escape(selectedTask.Name)}[/] [[[{badgeColor}]{badgeText}[/]]]");
        AnsiConsole.MarkupLine($"Comando: [bold yellow]{Markup.Escape(selectedTask.ResolveExecutionCommand())}[/]");

        return 0;
    }

    private static int HandleInit(string targetPath)
    {
        var fileSystem = new FileSystem();
        var fullPath = fileSystem.GetFullPath(targetPath);

        if (fileSystem.FileExists(fullPath))
        {
            AnsiConsole.MarkupLine($"[bold red]Erro:[/] O arquivo de configuração '[yellow]{Markup.Escape(targetPath)}[/]' já existe e não será sobrescrito.");
            return 1;
        }

        var template = TemplateConfigurationGenerator.GenerateDefaultJson();
        fileSystem.WriteAllText(fullPath, template);
        AnsiConsole.MarkupLine($"[bold green]Sucesso:[/] Arquivo de configuração '[yellow]{Markup.Escape(targetPath)}[/]' criado com êxito!");
        return 0;
    }

    private static void ShowHelp()
    {
        AnsiConsole.MarkupLine("[bold yellow]NetTaskList (nxtl)[/] - Gerenciador interativo de tarefas e scripts");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Uso:[/] [cyan]nxtl[/] [grey][[opções]][/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Opções:[/]");
        AnsiConsole.MarkupLine("  [cyan]-c, --config <caminho>[/]  Especifica um caminho personalizado para o arquivo de configuração JSON");
        AnsiConsole.MarkupLine("  [cyan]-r, --run <nome>[/]         Seleciona e direciona para execução direta uma tarefa específica");
        AnsiConsole.MarkupLine("  [cyan]-i, --init[/]               Gera um arquivo modelo 'nxtl.json' com exemplos práticos");
        AnsiConsole.MarkupLine("  [cyan]-h, --help[/]               Exibe esta mensagem de ajuda");
        AnsiConsole.MarkupLine("  [cyan]-v, --version[/]            Exibe a versão instalada da ferramenta");
    }
}
