namespace NetTaskList.Domain.Models;

/// <summary>
/// Specifies the execution category of a configured script or task.
/// </summary>
public enum TaskType
{
    /// <summary>
    /// General CLI tools and shell commands (e.g., dotnet build, npm test).
    /// </summary>
    Tool,

    /// <summary>
    /// C# scripts executable in file-scoped mode (e.g., script.cs).
    /// </summary>
    CSharp
}
