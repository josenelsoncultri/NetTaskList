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

        // Upfront validation of shortcut reference tree (cycle detection, existence, type check)
        if (!ValidateShortcutTree(task, configuration, out var validationError))
        {
            _console.MarkupLine(validationError!);
            return 1;
        }

        var activeStack = new Stack<string>();
        return ExecuteTaskWithShortcuts(task, configuration, activeStack, isRootTask: true);
    }

    private bool ValidateShortcutTree(ScriptTask rootTask, TaskConfiguration configuration, out string? errorMessage)
    {
        var currentPath = new List<string> { rootTask.Name };
        var currentPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootTask.Name };

        return ValidateTaskRecursive(rootTask, configuration, currentPath, currentPathSet, out errorMessage);
    }

    private bool ValidateTaskRecursive(
        ScriptTask currentTask,
        TaskConfiguration configuration,
        List<string> currentPath,
        HashSet<string> currentPathSet,
        out string? errorMessage)
    {
        foreach (var step in currentTask.CommandSteps)
        {
            if (step.IsShortcut)
            {
                var targetName = step.TargetTaskName!;

                // 1. Cycle detection
                if (currentPathSet.Contains(targetName))
                {
                    var cyclePath = new List<string>(currentPath) { targetName };
                    var cycleChain = string.Join(" -> ", cyclePath);
                    errorMessage = $"[bold red]Erro:[/] Referência circular detectada entre tarefas: [yellow]{cycleChain}[/]. A execução causaria um loop infinito no nxtl.";
                    return false;
                }

                // 2. Existence check
                var targetTask = configuration.Tasks.FirstOrDefault(t =>
                    t.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

                if (targetTask == null)
                {
                    errorMessage = $"[bold red]Erro:[/] A tarefa referenciada '[yellow]{Markup.Escape(targetName)}[/]' não foi encontrada no arquivo de configuração.";
                    return false;
                }

                // 3. C# script task check
                if (targetTask.Type == TaskType.CSharp)
                {
                    errorMessage = $"[bold red]Erro:[/] A tarefa '[yellow]{Markup.Escape(currentTask.Name)}[/]' tenta referenciar '[yellow]{Markup.Escape(targetName)}[/]', que é um script C#. Tarefas C# não podem ser referenciadas como atalhos.";
                    return false;
                }

                // Recurse into targetTask
                currentPath.Add(targetTask.Name);
                currentPathSet.Add(targetTask.Name);

                if (!ValidateTaskRecursive(targetTask, configuration, currentPath, currentPathSet, out errorMessage))
                {
                    return false;
                }

                // Backtracking for DAG / diamond dependencies
                currentPath.RemoveAt(currentPath.Count - 1);
                currentPathSet.Remove(targetTask.Name);
            }
        }

        errorMessage = null;
        return true;
    }

    private int ExecuteTaskWithShortcuts(
        ScriptTask task,
        TaskConfiguration configuration,
        Stack<string> activeStack,
        bool isRootTask)
    {
        // Runtime execution stack guard
        if (activeStack.Contains(task.Name, StringComparer.OrdinalIgnoreCase))
        {
            var cyclePath = activeStack.Reverse().Concat(new[] { task.Name });
            var cycleChain = string.Join(" -> ", cyclePath);
            _console.MarkupLine($"[bold red]Erro:[/] Referência circular detectada entre tarefas: [yellow]{cycleChain}[/]. A execução causaria um loop infinito no nxtl.");
            return 1;
        }

        activeStack.Push(task.Name);

        try
        {
            if (isRootTask)
            {
                var badgeColor = task.Type == TaskType.CSharp ? "green" : "cyan";
                var badgeText = task.Type == TaskType.CSharp ? "csharp" : "tool";
                _console.MarkupLine($"Tarefa selecionada: [bold white]{Markup.Escape(task.Name)}[/] [[[{badgeColor}]{badgeText}[/]]]");
            }

            int totalSteps = task.CommandSteps.Count;

            for (int i = 0; i < totalSteps; i++)
            {
                var step = task.CommandSteps[i];

                if (step.IsShortcut)
                {
                    _console.MarkupLine($"[bold cyan]Executando atalho:[/] [bold white]{Markup.Escape(step.TargetTaskName!)}[/]");

                    var targetTask = configuration.Tasks.FirstOrDefault(t =>
                        t.Name.Equals(step.TargetTaskName, StringComparison.OrdinalIgnoreCase));

                    if (targetTask == null)
                    {
                        _console.MarkupLine($"[bold red]Erro:[/] A tarefa referenciada '[yellow]{Markup.Escape(step.TargetTaskName!)}[/]' não foi encontrada no arquivo de configuração.");
                        return 1;
                    }

                    var shortcutExitCode = ExecuteTaskWithShortcuts(targetTask, configuration, activeStack, isRootTask: false);
                    if (shortcutExitCode != 0)
                    {
                        if (totalSteps > 1)
                        {
                            _console.MarkupLine($"[bold red]Execução interrompida:[/] O comando falhou com código de saída {shortcutExitCode}. Comandos posteriores não serão executados.");
                        }

                        return shortcutExitCode;
                    }
                }
                else
                {
                    var rawCommand = step.CommandLine!.Trim();
                    var commandLine = rawCommand;

                    if (task.Type == TaskType.CSharp)
                    {
                        if (rawCommand.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                            !rawCommand.StartsWith("dotnet run", StringComparison.OrdinalIgnoreCase))
                        {
                            commandLine = $"dotnet run {rawCommand}";
                            var fullScriptPath = _fileSystem.GetFullPath(rawCommand);
                            if (!_fileSystem.FileExists(fullScriptPath) && !_fileSystem.FileExists(rawCommand))
                            {
                                _console.MarkupLine($"[bold red]Erro:[/] O arquivo de script C# '[yellow]{Markup.Escape(rawCommand)}[/]' não foi encontrado.");
                                if (totalSteps > 1)
                                {
                                    _console.MarkupLine($"[bold red]Execução interrompida:[/] Comandos posteriores não serão executados.");
                                }
                                return 1;
                            }
                        }
                    }

                    if (totalSteps > 1)
                    {
                        _console.MarkupLine($"Comando [[{i + 1}/{totalSteps}]]: [bold yellow]{Markup.Escape(commandLine)}[/]");
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
                        if (totalSteps > 1)
                        {
                            _console.MarkupLine($"[bold red]Execução interrompida:[/] O comando falhou com código de saída {result.ExitCode}. Comandos posteriores não serão executados.");
                        }

                        return result.ExitCode;
                    }
                }
            }

            return 0;
        }
        finally
        {
            activeStack.Pop();
        }
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
