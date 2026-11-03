using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure.Abstractions;

namespace NetTaskList.Infrastructure;

public sealed record ConfigurationResolutionResult
{
    public bool Found { get; init; }
    public string? FilePath { get; init; }
    public ConfigurationSourceType? SourceType { get; init; }
    public string? ErrorMessage { get; init; }

    public static ConfigurationResolutionResult Success(string filePath, ConfigurationSourceType sourceType) =>
        new() { Found = true, FilePath = filePath, SourceType = sourceType };

    public static ConfigurationResolutionResult NotFound(string errorMessage) =>
        new() { Found = false, ErrorMessage = errorMessage };
}

public sealed class ConfigurationFileResolver
{
    private readonly IFileSystem _fileSystem;

    public ConfigurationFileResolver(IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
    }

    public ConfigurationResolutionResult Resolve(string? explicitConfigPath = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitConfigPath))
        {
            var fullPath = _fileSystem.GetFullPath(explicitConfigPath);
            if (!_fileSystem.FileExists(fullPath))
            {
                return ConfigurationResolutionResult.NotFound(
                    $"O arquivo de configuração '{explicitConfigPath}' não foi encontrado.");
            }

            var fileName = Path.GetFileName(fullPath);
            var sourceType = ConfigurationSourceType.Custom;
            if (fileName.Equals("nxtl.json", StringComparison.OrdinalIgnoreCase))
            {
                sourceType = ConfigurationSourceType.NxtlJson;
            }
            else if (fileName.Equals("package.json", StringComparison.OrdinalIgnoreCase))
            {
                sourceType = ConfigurationSourceType.PackageJson;
            }

            return ConfigurationResolutionResult.Success(fullPath, sourceType);
        }

        var nxtlPath = _fileSystem.GetFullPath("nxtl.json");
        if (_fileSystem.FileExists(nxtlPath))
        {
            return ConfigurationResolutionResult.Success(nxtlPath, ConfigurationSourceType.NxtlJson);
        }

        var packageJsonPath = _fileSystem.GetFullPath("package.json");
        if (_fileSystem.FileExists(packageJsonPath))
        {
            return ConfigurationResolutionResult.Success(packageJsonPath, ConfigurationSourceType.PackageJson);
        }

        return ConfigurationResolutionResult.NotFound(
            "Nenhum arquivo de configuração ('nxtl.json' ou 'package.json') foi encontrado no diretório atual.");
    }
}

