using System.Text.Json.Serialization;
using NetTaskList.Infrastructure.Serialization;

namespace NetTaskList.Infrastructure.Dtos;

public sealed class ConfigurationDto
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    [JsonPropertyName("scripts")]
    public List<ScriptTaskDto>? Scripts { get; set; }

    [JsonPropertyName("tasks")]
    public List<ScriptTaskDto>? Tasks { get; set; }
}

public sealed class ScriptTaskDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("command")]
    [JsonConverter(typeof(CommandPropertyJsonConverter))]
    public List<TaskCommandDto>? Commands { get; set; }

    [JsonIgnore]
    public string? Command
    {
        get => Commands != null && Commands.Count > 0 ? (Commands[0].IsShortcut ? $"task: {Commands[0].TargetTask}" : Commands[0].RawCommand) : null;
        set => Commands = value == null ? null : new List<TaskCommandDto> { new TaskCommandDto { RawCommand = value } };
    }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
