namespace NetTaskList.Domain.Models;

/// <summary>
/// Represents the consolidated and validated configuration document loaded in memory.
/// </summary>
public sealed class TaskConfiguration
{
    public TaskConfiguration(string sourcePath, IReadOnlyList<ScriptTask> tasks)
    {
        SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
        Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
    }

    /// <summary>
    /// Absolute or relative path to the configuration file source.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// Immutable list of validated tasks.
    /// </summary>
    public IReadOnlyList<ScriptTask> Tasks { get; }

    /// <summary>
    /// True if at least one task is configured.
    /// </summary>
    public bool HasTasks => Tasks.Count > 0;

    /// <summary>
    /// Total count of configured tasks.
    /// </summary>
    public int Count => Tasks.Count;
}
