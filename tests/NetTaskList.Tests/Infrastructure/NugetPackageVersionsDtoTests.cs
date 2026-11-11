using System.Text.Json;
using NetTaskList.Infrastructure.Dtos;
using NetTaskList.Infrastructure.Serialization;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class NugetPackageVersionsDtoTests
{
    [Fact]
    public void Deserialize_ValidJson_ReturnsPopulatedVersionsList()
    {
        // Arrange
        const string json = """
        {
          "versions": [
            "1.0.0",
            "1.2.0",
            "1.6.1"
          ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize(json, TaskJsonContext.Default.NugetPackageVersionsDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Versions);
        Assert.Equal(3, result.Versions.Count);
        Assert.Equal("1.0.0", result.Versions[0]);
        Assert.Equal("1.2.0", result.Versions[1]);
        Assert.Equal("1.6.1", result.Versions[2]);
    }

    [Fact]
    public void Deserialize_EmptyVersions_ReturnsEmptyList()
    {
        // Arrange
        const string json = """{ "versions": [] }""";

        // Act
        var result = JsonSerializer.Deserialize(json, TaskJsonContext.Default.NugetPackageVersionsDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Versions);
        Assert.Empty(result.Versions);
    }

    [Fact]
    public void Serialize_Roundtrip_PreservesVersions()
    {
        // Arrange
        var dto = new NugetPackageVersionsDto
        {
            Versions = ["1.0.0", "1.7.0"]
        };

        // Act
        var json = JsonSerializer.Serialize(dto, TaskJsonContext.Default.NugetPackageVersionsDto);
        var roundtrip = JsonSerializer.Deserialize(json, TaskJsonContext.Default.NugetPackageVersionsDto);

        // Assert
        Assert.NotNull(roundtrip);
        Assert.NotNull(roundtrip.Versions);
        Assert.Equal(2, roundtrip.Versions.Count);
        Assert.Equal("1.0.0", roundtrip.Versions[0]);
        Assert.Equal("1.7.0", roundtrip.Versions[1]);
    }
}
