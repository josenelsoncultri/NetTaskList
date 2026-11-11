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
        IAnsiConsole? console = null,
        INugetVersionChecker? nugetVersionChecker = null)
    {
        var fs = fileSystem ?? new FileSystem();
        var executor = processExecutor ?? new ProcessExecutor();
        var ansiConsole = console ?? AnsiConsole.Console;
        var versionChecker = nugetVersionChecker ?? new NugetVersionChecker();

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

        var updateResult = versionChecker.CheckForUpdate(AppVersion.Current, TimeSpan.FromSeconds(2));
        UpdateNotificationPresenter.Present(ansiConsole, updateResult);

        if (options.Init)
        {
            return HandleInit(options.ConfigPath ?? "nxtl.json", fs, ansiConsole);
        }

        var resolver = new ConfigurationFileResolver(fs);
        var resolution = resolver.Resolve(options.ConfigPath);

        if (!resolution.Found)
        {
            ansiConsole.MarkupLine($"[bold red]Erro:[/] {Markup.Escape(resolution.ErrorMessage ?? "Arquivo de configuração não encontrado.")}");
            ansiConsole.MarkupLine("Execute '[bold cyan]nxtl --init[/]' para criar um arquivo inicial ou use '[bold cyan]nxtl --config <caminho>[/]' para especificar outro arquivo.");
            return 1;
        }

        var fullPath = resolution.FilePath!;
        var configFilePath = options.ConfigPath ?? Path.GetFileName(fullPath);

        TaskConfiguration configuration;
        try
        {
            var reader = new JsonConfigurationReader(fs);
            configuration = reader.Read(fullPath, resolution.SourceType);
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

        var coordinator = new TaskExecutionCoordinator(executor, fs, ansiConsole);

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

            return coordinator.Execute(target, configuration);
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
        var selectedTask = InteractiveTaskSelector.SelectTask(configuration.Tasks, configuration.SourceType, ansiConsole);
        if (selectedTask is null)
        {
            ansiConsole.MarkupLine("[grey]Operação cancelada pelo usuário.[/]");
            return 0;
        }

        return coordinator.Execute(selectedTask, configuration);
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
        console.MarkupLine("  [cyan]-c, --config <caminho>[/]  Especifica um caminho personalizado para o arquivo de configuração ('nxtl.json' ou 'package.json')");
        console.MarkupLine("  [cyan]-r, --run <nome>[/]         Seleciona e direciona para execução direta uma tarefa específica");
        console.MarkupLine("  [cyan]-i, --init[/]               Gera um arquivo modelo 'nxtl.json' com exemplos práticos");
        console.MarkupLine("  [cyan]-h, --help[/]               Exibe esta mensagem de ajuda");
        console.MarkupLine("  [cyan]-v, --version[/]            Exibe a versão instalada da ferramenta");
    }
}
