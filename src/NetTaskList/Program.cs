using NetTaskList.Domain.Models;
using NetTaskList.Domain.Validation;
using NetTaskList.Infrastructure;
using NetTaskList.Infrastructure.Abstractions;
using NetTaskList.Presentation;
using Spectre.Console;

namespace NetTaskList;

public static class Program
{
    public static int Main(string[] args)
    {
        return Run(args);
    }

    public static int Run(
        string[] args,
        IFileSystem? fileSystem = null,
        IProcessExecutor? processExecutor = null,
        IAnsiConsole? console = null)
    {
        var fs = fileSystem ?? new FileSystem();
        var executor = processExecutor ?? new ProcessExecutor();
        var ansiConsole = console ?? AnsiConsole.Console;

        var options = CommandLineParser.Parse(args);

        if (!options.IsValid)
        {
            ansiConsole.MarkupLine($"[bold red]Erro:[/] {Markup.Escape(options.ErrorMessage ?? "Argumentos inválidos.")}");
            return 2;
        }

        if (options.ShowHelp)
        {
            ShowHelp(ansiConsole);
            return 0;
        }

        if (options.ShowVersion)
        {
            ansiConsole.MarkupLine($"[bold green]nxtl[/] versão [yellow]{Markup.Escape(AppVersion.Current)}[/]");
            return 0;
        }

        if (options.Init)
        {
            return HandleInit(options.ConfigPath ?? "nxtl.json", fs, ansiConsole);
        }

        var configFilePath = options.ConfigPath ?? "nxtl.json";
        var fullPath = fs.GetFullPath(configFilePath);

        if (!fs.FileExists(fullPath))
        {
            ansiConsole.MarkupLine($"[bold red]Erro:[/] O arquivo de configuração '[yellow]{Markup.Escape(configFilePath)}[/]' não foi encontrado.");
            ansiConsole.MarkupLine("Execute '[bold cyan]nxtl --init[/]' para criar um arquivo inicial ou use '[bold cyan]nxtl --config <caminho>[/]' para especificar outro arquivo.");
            return 1;
        }

        TaskConfiguration configuration;
        try
        {
            var reader = new JsonConfigurationReader(fs);
            configuration = reader.Read(fullPath);
        }
        catch (ConfigurationValidationException ex)
        {
            ansiConsole.MarkupLine("[bold red]Erros de validação no arquivo de configuração:[/]");
            foreach (var error in ex.ValidationResult.Errors)
            {
                ansiConsole.MarkupLine($"  [red]•[/] {Markup.Escape(error.Message)}");
            }
            return 1;
        }
        catch (JsonConfigurationException ex)
        {
            ansiConsole.MarkupLine($"[bold red]Erro de Configuração:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
        catch (Exception ex)
        {
            ansiConsole.MarkupLine($"[bold red]Erro inesperado:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }

        if (!configuration.HasTasks)
        {
            ansiConsole.MarkupLine("[yellow]Nenhuma tarefa configurada no arquivo.[/]");
            return 0;
        }

        // Direct execution mode via --run / -r
        if (!string.IsNullOrWhiteSpace(options.RunTaskName))
        {
            var target = configuration.Tasks.FirstOrDefault(t =>
                t.Name.Equals(options.RunTaskName, StringComparison.OrdinalIgnoreCase));

            if (target is null)
            {
                ansiConsole.MarkupLine($"[bold red]Erro:[/] Tarefa '[yellow]{Markup.Escape(options.RunTaskName)}[/]' não encontrada no arquivo de configuração '[yellow]{Markup.Escape(configFilePath)}[/]'.");
                ansiConsole.MarkupLine("Execute '[bold cyan]nxtl[/]' para listar todas as tarefas disponíveis.");
                return 1;
            }

            return ExecuteTask(target, executor, fs, ansiConsole);
        }

        // Listing header with dynamic version in green
        ansiConsole.MarkupLine($"[green]nxtl versão {Markup.Escape(AppVersion.Current)}[/]");

        // Fallback for non-interactive / redirected streams
        if (Console.IsInputRedirected && console == null)
        {
            ansiConsole.MarkupLine("[yellow]Aviso:[/] Execução interativa não disponível em console redirecionado. Utilize '[bold cyan]nxtl --run <nome>[/]' para executar uma tarefa diretamente.");
            return 0;
        }

        // Interactive selection mode
        var selectedTask = InteractiveTaskSelector.SelectTask(configuration.Tasks, ansiConsole);
        if (selectedTask is null)
        {
            ansiConsole.MarkupLine("[grey]Operação cancelada pelo usuário.[/]");
            return 0;
        }

        return ExecuteTask(selectedTask, executor, fs, ansiConsole);
    }

    private static int ExecuteTask(ScriptTask task, IProcessExecutor executor, IFileSystem fileSystem, IAnsiConsole console)
    {
        var badgeColor = task.Type == TaskType.CSharp ? "green" : "cyan";
        var badgeText = task.Type == TaskType.CSharp ? "csharp" : "tool";
        console.MarkupLine($"Tarefa selecionada: [bold white]{Markup.Escape(task.Name)}[/] [[[{badgeColor}]{badgeText}[/]]]");

        if (task.Type == TaskType.CSharp)
        {
            var rawCommand = task.Command.Trim();
            if (rawCommand.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                !rawCommand.StartsWith("dotnet run", StringComparison.OrdinalIgnoreCase))
            {
                var fullScriptPath = fileSystem.GetFullPath(rawCommand);
                if (!fileSystem.FileExists(fullScriptPath) && !fileSystem.FileExists(rawCommand))
                {
                    console.MarkupLine($"[bold red]Erro:[/] O arquivo de script C# '[yellow]{Markup.Escape(rawCommand)}[/]' não foi encontrado.");
                    return 1;
                }
            }
        }

        var commandLine = task.ResolveExecutionCommand();
        console.MarkupLine($"Comando: [bold yellow]{Markup.Escape(commandLine)}[/]");

        var result = executor.Execute(commandLine);

        if (result.ErrorMessage != null)
        {
            console.MarkupLine($"[bold red]Erro ao executar tarefa:[/] {Markup.Escape(result.ErrorMessage)}");
        }

        return result.ExitCode;
    }

    private static int HandleInit(string targetPath, IFileSystem fileSystem, IAnsiConsole console)
    {
        var fullPath = fileSystem.GetFullPath(targetPath);

        if (fileSystem.FileExists(fullPath))
        {
            console.MarkupLine($"[bold red]Erro:[/] O arquivo de configuração '[yellow]{Markup.Escape(targetPath)}[/]' já existe e não será sobrescrito.");
            return 1;
        }

        var template = TemplateConfigurationGenerator.GenerateDefaultJson();
        fileSystem.WriteAllText(fullPath, template);
        console.MarkupLine($"[bold green]Sucesso:[/] Arquivo de configuração '[yellow]{Markup.Escape(targetPath)}[/]' criado com êxito!");
        return 0;
    }

    private static void ShowHelp(IAnsiConsole console)
    {
        console.MarkupLine("[bold yellow]NetTaskList (nxtl)[/] - Gerenciador interativo de tarefas e scripts");
        console.WriteLine();
        console.MarkupLine("[bold]Uso:[/] [cyan]nxtl[/] [grey][[opções]][/]");
        console.WriteLine();
        console.MarkupLine("[bold]Opções:[/]");
        console.MarkupLine("  [cyan]-c, --config <caminho>[/]  Especifica um caminho personalizado para o arquivo de configuração JSON");
        console.MarkupLine("  [cyan]-r, --run <nome>[/]         Seleciona e direciona para execução direta uma tarefa específica");
        console.MarkupLine("  [cyan]-i, --init[/]               Gera um arquivo modelo 'nxtl.json' com exemplos práticos");
        console.MarkupLine("  [cyan]-h, --help[/]               Exibe esta mensagem de ajuda");
        console.MarkupLine("  [cyan]-v, --version[/]            Exibe a versão instalada da ferramenta");
    }
}
