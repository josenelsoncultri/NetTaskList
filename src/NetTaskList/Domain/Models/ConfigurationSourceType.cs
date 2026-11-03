namespace NetTaskList.Domain.Models;

/// <summary>
/// Specifies the origin source of the task configuration.
/// </summary>
public enum ConfigurationSourceType
{
    /// <summary>
    /// Native nxtl configuration file (nxtl.json).
    /// </summary>
    NxtlJson,

    /// <summary>
    /// Node.js package file (package.json).
    /// </summary>
    PackageJson,

    /// <summary>
    /// Custom configuration file supplied explicitly via CLI arguments.
    /// </summary>
    Custom
}

