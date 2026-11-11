using System.Text.Json;
using NetTaskList.Domain.Models;
using NetTaskList.Domain.Validation;
using NetTaskList.Infrastructure.Abstractions;
using NetTaskList.Infrastructure.Dtos;
using NetTaskList.Infrastructure.Serialization;

namespace NetTaskList.Infrastructure;

public sealed class JsonConfigurationException : Exception
{
    public JsonConfigurationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ConfigurationValidationException : Exception
{
    public ConfigurationValidationException(ValidationResult validationResult)
        : base("Falha na validação do arquivo de configuração.")
    {
        ValidationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
    }

    public ValidationResult ValidationResult { get; }
}

public sealed class JsonConfigurationReader
{
    private readonly IFileSystem _fileSystem;
    private readonly IConfigurationValidator _validator;

    public JsonConfigurationReader(IFileSystem? fileSystem = null, IConfigurationValidator? validator = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _validator = validator ?? new ConfigurationValidator();
    }

    public TaskConfiguration Read(string filePath, ConfigurationSourceType? sourceType = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("O caminho do arquivo de configuração não pode ser vazio.", nameof(filePath));

        var fullPath = _fileSystem.GetFullPath(filePath);

        if (!_fileSystem.FileExists(fullPath))
        {
            throw new FileNotFoundException(
                $"O arquivo de configuração '{filePath}' não foi encontrado no caminho especificado.", fullPath);
        }

        string jsonContent;
        try
        {
            jsonContent = _fileSystem.ReadAllText(fullPath);
        }
        catch (Exception ex)
        {
            throw new JsonConfigurationException(
                $"Erro ao ler o arquivo de configuração '{filePath}': {ex.Message}", ex);
        }

        var determinedSourceType = sourceType ??
            (Path.GetFileName(fullPath).Equals("package.json", StringComparison.OrdinalIgnoreCase)
                ? ConfigurationSourceType.PackageJson
                : ConfigurationSourceType.NxtlJson);

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return new TaskConfiguration(fullPath, Array.Empty<ScriptTask>(), determinedSourceType);
        }

        if (determinedSourceType == ConfigurationSourceType.PackageJson)
        {
            return ReadPackageJson(fullPath, jsonContent, filePath);
        }

        return ReadNxtlJson(fullPath, jsonContent, filePath, determinedSourceType);
    }

    private TaskConfiguration ReadPackageJson(string fullPath, string jsonContent, string filePath)
    {
        PackageJsonDto? packageDto;
        try
        {
            packageDto = JsonSerializer.Deserialize(
                jsonContent,
                TaskJsonContext.Default.PackageJsonDto);
        }
        catch (JsonException ex)
        {
            throw new JsonConfigurationException(
                $"O arquivo de configuração '{filePath}' não é um JSON válido: {ex.Message}", ex);
        }

        if (packageDto?.Scripts == null || packageDto.Scripts.Count == 0)
        {
            return new TaskConfiguration(fullPath, Array.Empty<ScriptTask>(), ConfigurationSourceType.PackageJson);
        }

        var tasks = new List<ScriptTask>();
        var validationErrors = new List<ValidationError>();

        foreach (var (scriptName, element) in packageDto.Scripts)
        {
            if (string.IsNullOrWhiteSpace(scriptName))
            {
                validationErrors.Add(new ValidationError("Name", "O nome do script no 'package.json' não pode ser vazio."));
                continue;
            }

            if (element.ValueKind != JsonValueKind.String)
            {
                validationErrors.Add(new ValidationError("Command", $"O comando da tarefa '{scriptName}' no 'package.json' deve ser uma string de texto válida."));
                continue;
            }

            var command = element.GetString();
            if (string.IsNullOrWhiteSpace(command))
            {
                validationErrors.Add(new ValidationError("Command", $"O comando da tarefa '{scriptName}' no 'package.json' não pode ser vazio."));
                continue;
            }

            tasks.Add(new ScriptTask(scriptName.Trim(), command.Trim(), TaskType.Tool, null));
        }

        if (validationErrors.Count > 0)
        {
            throw new ConfigurationValidationException(ValidationResult.Failure(validationErrors));
        }

        return new TaskConfiguration(fullPath, tasks, ConfigurationSourceType.PackageJson);
    }

    private TaskConfiguration ReadNxtlJson(
        string fullPath,
        string jsonContent,
        string filePath,
        ConfigurationSourceType determinedSourceType)
    {
        List<ScriptTaskDto>? rawTasks = null;
        var trimmed = jsonContent.TrimStart();

        try
        {
            if (trimmed.StartsWith('['))
            {
                rawTasks = JsonSerializer.Deserialize(
                    jsonContent,
                    TaskJsonContext.Default.ListScriptTaskDto);
            }
            else
            {
                var configDto = JsonSerializer.Deserialize(
                    jsonContent,
                    TaskJsonContext.Default.ConfigurationDto);

                if (configDto != null)
                {
                    rawTasks = configDto.Scripts ?? configDto.Tasks;
                }
            }
        }
        catch (JsonException ex)
        {
            throw new JsonConfigurationException(
                $"O arquivo de configuração '{filePath}' não é um JSON válido: {ex.Message}", ex);
        }

        // Validate raw tasks before domain mapping
        var validationResult = _validator.Validate(rawTasks);
        if (!validationResult.IsValid)
        {
            throw new ConfigurationValidationException(validationResult);
        }

        var tasks = new List<ScriptTask>();
        if (rawTasks != null)
        {
            foreach (var dto in rawTasks)
            {
                var type = TaskType.Tool;
                if (!string.IsNullOrWhiteSpace(dto.Type) &&
                    dto.Type.Trim().Equals("csharp", StringComparison.OrdinalIgnoreCase))
                {
                    type = TaskType.CSharp;
                }

                var steps = new List<TaskCommandStep>();
                if (dto.Commands != null && dto.Commands.Count > 0)
                {
                    foreach (var cmdDto in dto.Commands)
                    {
                        if (cmdDto.IsShortcut)
                        {
                            steps.Add(TaskCommandStep.FromShortcut(cmdDto.TargetTask!.Trim()));
                        }
                        else
                        {
                            steps.Add(TaskCommandStep.FromCommandLine(cmdDto.RawCommand!.Trim()));
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(dto.Command))
                {
                    steps.Add(TaskCommandStep.FromCommandLine(dto.Command.Trim()));
                }

                tasks.Add(new ScriptTask(
                    dto.Name!.Trim(),
                    steps,
                    type,
                    dto.Description));
            }
        }

        return new TaskConfiguration(fullPath, tasks, determinedSourceType);
    }
}
