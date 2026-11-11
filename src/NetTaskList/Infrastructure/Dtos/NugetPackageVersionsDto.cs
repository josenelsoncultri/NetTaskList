using System.Text.Json.Serialization;

namespace NetTaskList.Infrastructure.Dtos;

public sealed class NugetPackageVersionsDto
{
    [JsonPropertyName("versions")]
    public List<string>? Versions { get; set; }
}
