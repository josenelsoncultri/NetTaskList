using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure.Abstractions;
using Spectre.Console;

namespace NetTaskList.Presentation;

public sealed class TaskExecutionCoordinator
{
    private readonly IProcessExecutor _executor;
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;

    public TaskExecutionCoordinator(
        IProcessExecutor executor,
        IFileSystem fileSystem,
        IAnsiConsole console)
    {
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public int Execute(ScriptTask task, TaskConfiguration configuration)
    {
        if (configuration.SourceType == ConfigurationSourceType.PackageJson)
        {
            return ExecutePackageJsonTask(task, configuration);
        }

        return ExecuteSingleTask(task);
    }

    private int ExecutePackageJsonTask(ScriptTask task, TaskConfiguration configuration)
    {
        // If the task itself is a hook directly targeted (e.g. nxtl --run prebuild), run it directly without recursion
        bool isDirectHook = task.Name.StartsWith("pre", StringComparison.OrdinalIgnoreCase) ||
                             task.Name.StartsWith("post", StringComparison.OrdinalIgnoreCase);

        if (isDirectHook)
        {
            return ExecuteSingleTask(task);
        }

        // 1. Pre hook (pre<name>)
        var preTaskName = "pre" + task.Name;
        var preTask = configuration.Tasks.FirstOrDefault(t =>
            t.Name.Equals(preTaskName, StringComparison.OrdinalIgnoreCase));

        if (preTask != null)
        {
            var preExitCode = ExecuteSingleTask(preTask);
            if (preExitCode != 0)
            {
                return preExitCode;
            }
        }

        // 2. Main task
        var mainExitCode = ExecuteSingleTask(task);
        if (mainExitCode != 0)
        {
            return mainExitCode;
        }

        // 3. Post hook (post<name>)
        var postTaskName = "post" + task.Name;
        var postTask = configuration.Tasks.FirstOrDefault(t =>
            t.Name.Equals(postTaskName, StringComparison.OrdinalIgnoreCase));

        if (postTask != null)
        {
            return ExecuteSingleTask(postTask);
        }

        return mainExitCode;
    }

    private int ExecuteSingleTask(ScriptTask task)
    {
        var badgeColor = task.Type == TaskType.CSharp ? "green" : "cyan";
        var badgeText = task.Type == TaskType.CSharp ? "csharp" : "tool";
        _console.MarkupLine($"Tarefa selecionada: [bold white]{Markup.Escape(task.Name)}[/] [[[{badgeColor}]{badgeText}[/]]]");

        var resolvedCommands = task.ResolveExecutionCommands();
        int totalCommands = resolvedCommands.Count;

        for (int i = 0; i < totalCommands; i++)
        {
            var rawCommand = task.Commands[i].Trim();
            var commandLine = resolvedCommands[i];

            if (task.Type == TaskType.CSharp)
            {
                if (rawCommand.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !rawCommand.StartsWith("dotnet run", StringComparison.OrdinalIgnoreCase))
                {
                    var fullScriptPath = _fileSystem.GetFullPath(rawCommand);
                    if (!_fileSystem.FileExists(fullScriptPath) && !_fileSystem.FileExists(rawCommand))
                    {
                        _console.MarkupLine($"[bold red]Erro:[/] O arquivo de script C# '[yellow]{Markup.Escape(rawCommand)}[/]' não foi encontrado.");
                        if (totalCommands > 1)
                        {
                            _console.MarkupLine($"[bold red]Execução interrompida:[/] Comandos posteriores não serão executados.");
                        }
                        return 1;
                    }
                }
            }

            if (totalCommands > 1)
            {
                _console.MarkupLine($"Comando [[{i + 1}/{totalCommands}]]: [bold yellow]{Markup.Escape(commandLine)}[/]");
            }
            else
            {
                _console.MarkupLine($"Comando: [bold yellow]{Markup.Escape(commandLine)}[/]");
            }

            var result = _executor.Execute(commandLine);

            if (result.ErrorMessage != null)
            {
                _console.MarkupLine($"[bold red]Erro ao executar tarefa:[/] {Markup.Escape(result.ErrorMessage)}");
            }

            if (result.ExitCode != 0)
            {
                if (totalCommands > 1)
                {
                    _console.MarkupLine($"[bold red]Execução interrompida:[/] O comando falhou com código de saída {result.ExitCode}. Comandos posteriores não serão executados.");
                }

                return result.ExitCode;
            }
        }

        return 0;
    }
}
