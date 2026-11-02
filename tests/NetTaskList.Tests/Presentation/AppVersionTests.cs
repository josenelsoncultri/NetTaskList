using NetTaskList.Presentation;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class AppVersionTests
{
    [Fact]
    public void Current_ReturnsNonEmptySemanticVersion()
    {
        // Act
        var version = AppVersion.Current;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.Matches(@"^\d+\.\d+\.\d+", version);
        Assert.DoesNotContain("+", version);
    }
}
