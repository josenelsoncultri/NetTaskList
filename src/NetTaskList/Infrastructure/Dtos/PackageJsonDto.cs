using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetTaskList.Infrastructure.Dtos;

public sealed class PackageJsonDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("scripts")]
    public Dictionary<string, JsonElement>? Scripts { get; set; }
}

