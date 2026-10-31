namespace NetTaskList.Domain.Validation;

public enum ValidationSeverity
{
    Error,
    Warning
}

public sealed record ValidationError(
    string PropertyName,
    string Message,
    ValidationSeverity Severity = ValidationSeverity.Error);

public sealed class ValidationResult
{
    public ValidationResult(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors ?? Array.Empty<ValidationError>();
    }

    public bool IsValid => Errors.Count == 0;

    public IReadOnlyList<ValidationError> Errors { get; }

    public static ValidationResult Success() => new(Array.Empty<ValidationError>());

    public static ValidationResult Failure(IReadOnlyList<ValidationError> errors) => new(errors);

    public static ValidationResult Failure(string propertyName, string message) =>
        new([new ValidationError(propertyName, message)]);
}
