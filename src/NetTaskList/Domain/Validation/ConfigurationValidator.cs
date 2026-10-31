using NetTaskList.Infrastructure.Dtos;

namespace NetTaskList.Domain.Validation;

public interface IConfigurationValidator
{
    ValidationResult Validate(IReadOnlyList<ScriptTaskDto>? tasks);
}

public sealed class ConfigurationValidator : IConfigurationValidator
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "tool",
        "csharp"
    };

    public ValidationResult Validate(IReadOnlyList<ScriptTaskDto>? tasks)
    {
        if (tasks == null || tasks.Count == 0)
        {
            return ValidationResult.Success();
        }

        var errors = new List<ValidationError>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];

            // VR-002: Name is required
            if (string.IsNullOrWhiteSpace(task.Name))
            {
                errors.Add(new ValidationError(
                    nameof(task.Name),
                    "Foi encontrada uma tarefa sem nome válido no arquivo de configuração."));
            }
            else
            {
                var trimmedName = task.Name.Trim();

                // VR-001: Unique name (case-insensitive)
                if (!seenNames.Add(trimmedName))
                {
                    errors.Add(new ValidationError(
                        nameof(task.Name),
                        $"O script com o nome '{trimmedName}' está duplicado no arquivo de configuração. Cada script deve possuir um nome único."));
                }
            }

            var displayName = string.IsNullOrWhiteSpace(task.Name) ? $"índice [{i}]" : task.Name.Trim();

            // VR-003: Command is required
            if (string.IsNullOrWhiteSpace(task.Command))
            {
                errors.Add(new ValidationError(
                    nameof(task.Command),
                    $"A tarefa '{displayName}' não define um comando a ser executado."));
            }

            // VR-004: Type validation
            if (!string.IsNullOrWhiteSpace(task.Type))
            {
                var trimmedType = task.Type.Trim();
                if (!AllowedTypes.Contains(trimmedType))
                {
                    errors.Add(new ValidationError(
                        nameof(task.Type),
                        $"A tarefa '{displayName}' possui o tipo '{trimmedType}' não reconhecido. Os tipos aceitos são 'tool' e 'csharp'."));
                }
            }
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
