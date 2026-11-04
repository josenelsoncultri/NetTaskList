namespace NetTaskList.Domain.Models;

/// <summary>
/// Represents an individually configured executable script or task.
/// </summary>
public sealed class ScriptTask
{
    public ScriptTask(string name, string command, TaskType type = TaskType.Tool, string? description = null)
        : this(name, new[] { command ?? throw new ArgumentNullException(nameof(command)) }, type, description)
    {
    }

    public ScriptTask(string name, IReadOnlyList<string> commands, TaskType type = TaskType.Tool, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        Type = type;
        Description = description;
    }

    /// <summary>
    /// User-friendly unique identifier of the task.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Raw commands specified in configuration.
    /// </summary>
    public IReadOnlyList<string> Commands { get; }

    /// <summary>
    /// Raw primary command or file path specified in configuration (returns first command or empty string).
    /// </summary>
    public string Command => Commands.Count > 0 ? Commands[0] : string.Empty;

    /// <summary>
    /// Category of the task (Tool or CSharp).
    /// </summary>
    public TaskType Type { get; }

    /// <summary>
    /// Optional description text displayed in terminal tables.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Resolves the final execution command lines for each command in Commands.
    /// For CSharp scripts ending with .cs without a dotnet run prefix, infers 'dotnet run {command}'.
    /// For Tool tasks, returns the commands unchanged.
    /// </summary>
    public IReadOnlyList<string> ResolveExecutionCommands()
    {
        if (Type == TaskType.CSharp)
        {
            var resolved = new List<string>(Commands.Count);
            foreach (var cmd in Commands)
            {
                var trimmed = cmd.Trim();
                if (trimmed.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !trimmed.StartsWith("dotnet run", StringComparison.OrdinalIgnoreCase))
                {
                    resolved.Add($"dotnet run {trimmed}");
                }
                else
                {
                    resolved.Add(trimmed);
                }
            }
            return resolved;
        }

        return Commands;
    }

    /// <summary>
    /// Resolves the final execution command line for the primary command.
    /// </summary>
    public string ResolveExecutionCommand()
    {
        var resolved = ResolveExecutionCommands();
        return resolved.Count > 0 ? resolved[0] : string.Empty;
    }
}
