using NetTaskList.Domain.Models;
using Xunit;

namespace NetTaskList.Tests.Domain;

public class SemanticVersionTests
{
    [Theory]
    [InlineData("1.0.0", 1, 0, 0, null, false)]
    [InlineData("1.6.1", 1, 6, 1, null, false)]
    [InlineData("1.7.0-preview.1", 1, 7, 0, "preview.1", true)]
    [InlineData("2.0.0-rc.2+metadata", 2, 0, 0, "rc.2", true)]
    [InlineData("v1.2.3", 1, 2, 3, null, false)]
    public void TryParse_ValidVersions_ParsesCorrectly(
        string input, int expectedMajor, int expectedMinor, int expectedPatch, string? expectedPreRelease, bool expectedIsPreRelease)
    {
        // Act
        var success = SemanticVersion.TryParse(input, out var version);

        // Assert
        Assert.True(success);
        Assert.NotNull(version);
        Assert.Equal(expectedMajor, version.Major);
        Assert.Equal(expectedMinor, version.Minor);
        Assert.Equal(expectedPatch, version.Patch);
        Assert.Equal(expectedPreRelease, version.PreRelease);
        Assert.Equal(expectedIsPreRelease, version.IsPreRelease);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("1.2.x")]
    public void TryParse_InvalidVersions_ReturnsFalse(string? input)
    {
        // Act
        var success = SemanticVersion.TryParse(input, out var version);

        // Assert
        Assert.False(success);
        Assert.Null(version);
    }

    [Theory]
    [InlineData("1.7.0", "1.6.1", true)]
    [InlineData("1.6.2", "1.6.1", true)]
    [InlineData("2.0.0", "1.9.9", true)]
    [InlineData("1.6.1", "1.6.1", false)]
    [InlineData("1.6.0", "1.6.1", false)]
    [InlineData("1.7.0", "1.7.0-preview.1", true)]
    [InlineData("1.7.0-preview.1", "1.7.0", false)]
    public void IsNewerThan_EvaluatesCorrectly(string higherCandidate, string lowerCandidate, bool expected)
    {
        // Arrange
        Assert.True(SemanticVersion.TryParse(higherCandidate, out var higher));
        Assert.True(SemanticVersion.TryParse(lowerCandidate, out var lower));

        // Act & Assert
        Assert.Equal(expected, higher!.IsNewerThan(lower!));
    }

    [Fact]
    public void ToString_FormatsExpectedRepresentation()
    {
        // Arrange
        SemanticVersion.TryParse("1.6.1-beta.2", out var version);

        // Assert
        Assert.NotNull(version);
        Assert.Equal("1.6.1-beta.2", version.ToString());
    }
}
