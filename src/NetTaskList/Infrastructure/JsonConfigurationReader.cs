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

    public TaskConfiguration Read(string filePath)
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

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return new TaskConfiguration(fullPath, Array.Empty<ScriptTask>());
        }

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

                tasks.Add(new ScriptTask(
                    dto.Name!.Trim(),
                    dto.Command!.Trim(),
                    type,
                    dto.Description));
            }
        }

        return new TaskConfiguration(fullPath, tasks);
    }
}
