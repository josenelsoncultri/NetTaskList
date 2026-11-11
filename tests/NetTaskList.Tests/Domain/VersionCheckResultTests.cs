using NetTaskList.Domain.Models;
using Xunit;

namespace NetTaskList.Tests.Domain;

public class VersionCheckResultTests
{
    [Fact]
    public void UpdateAvailable_CreatesResultWithUpdateTrue()
    {
        // Act
        var result = VersionCheckResult.UpdateAvailable("1.6.1", "1.7.0");

        // Assert
        Assert.True(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Equal("1.7.0", result.LatestVersion);
    }

    [Fact]
    public void NoUpdate_CreatesResultWithUpdateFalse()
    {
        // Act
        var result = VersionCheckResult.NoUpdate("1.6.1");

        // Assert
        Assert.False(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Null(result.LatestVersion);
    }

    [Fact]
    public void Failed_CreatesResultWithSuccessFalseAndNoUpdate()
    {
        // Act
        var result = VersionCheckResult.Failed("1.6.1");

        // Assert
        Assert.False(result.HasUpdate);
        Assert.False(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Null(result.LatestVersion);
    }
}
