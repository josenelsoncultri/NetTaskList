using System.Text.Json.Serialization;

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
    public string? Command { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
