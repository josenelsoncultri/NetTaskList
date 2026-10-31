using System.Text.Json.Serialization;
using NetTaskList.Infrastructure.Dtos;

namespace NetTaskList.Infrastructure.Serialization;

/// <summary>
/// Source generation context for System.Text.Json ensuring 100% trim-safety and zero reflection for Native AOT.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ConfigurationDto))]
[JsonSerializable(typeof(List<ScriptTaskDto>))]
[JsonSerializable(typeof(ScriptTaskDto))]
internal sealed partial class TaskJsonContext : JsonSerializerContext
{
}
