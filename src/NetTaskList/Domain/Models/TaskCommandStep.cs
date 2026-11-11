namespace NetTaskList.Domain.Models;

/// <summary>
/// Represents an individual step within a task, which can be a shell command line or a shortcut to another task.
/// </summary>
public sealed class TaskCommandStep
{
    public string? CommandLine { get; }
    public string? TargetTaskName { get; }
    public bool IsShortcut => !string.IsNullOrWhiteSpace(TargetTaskName);

    public TaskCommandStep(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            throw new ArgumentException("O comando não pode ser vazio ou nulo.", nameof(commandLine));
        }

        CommandLine = commandLine;
        TargetTaskName = null;
    }

    public TaskCommandStep(string targetTaskName, bool isShortcut)
    {
        if (!isShortcut)
        {
            throw new ArgumentException("isShortcut deve ser verdadeiro para atalhos de tarefas.", nameof(isShortcut));
        }

        if (string.IsNullOrWhiteSpace(targetTaskName))
        {
            throw new ArgumentException("O nome da tarefa de destino não pode ser vazio ou nulo.", nameof(targetTaskName));
        }

        CommandLine = null;
        TargetTaskName = targetTaskName;
    }

    public static TaskCommandStep FromCommandLine(string commandLine) => new(commandLine);

    public static TaskCommandStep FromShortcut(string targetTaskName) => new(targetTaskName, true);

    public override string ToString() => IsShortcut ? $"task: {TargetTaskName}" : (CommandLine ?? string.Empty);
}
