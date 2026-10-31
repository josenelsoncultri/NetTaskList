namespace NetTaskList.Domain.Models;

/// <summary>
/// Represents an individually configured executable script or task.
/// </summary>
public sealed class ScriptTask
{
    public ScriptTask(string name, string command, TaskType type = TaskType.Tool, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Command = command ?? throw new ArgumentNullException(nameof(command));
        Type = type;
        Description = description;
    }

    /// <summary>
    /// User-friendly unique identifier of the task.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Raw command or file path specified in configuration.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Category of the task (Tool or CSharp).
    /// </summary>
    public TaskType Type { get; }

    /// <summary>
    /// Optional description text displayed in terminal tables.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Resolves the final execution command line. For CSharp scripts ending with .cs without a dotnet run prefix,
    /// infers 'dotnet run {Command}'. For Tool tasks, returns the command unchanged.
    /// </summary>
    public string ResolveExecutionCommand()
    {
        if (Type == TaskType.CSharp)
        {
            var trimmed = Command.Trim();
            if (trimmed.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("dotnet run", StringComparison.OrdinalIgnoreCase))
            {
                return $"dotnet run {trimmed}";
            }

            return trimmed;
        }

        return Command;
    }
}
