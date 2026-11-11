using NetTaskList.Domain.Models;
using NetTaskList.Presentation;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class UpdateNotificationPresenterTests
{
    [Fact]
    public void Present_WhenHasUpdateIsTrue_RendersPanelWithVersionAndCommand()
    {
        // Arrange
        var console = new MockAnsiConsole();
        var result = VersionCheckResult.UpdateAvailable("1.6.1", "1.7.0");

        // Act
        UpdateNotificationPresenter.Present(console, result);

        // Assert
        var output = console.Output;
        Assert.Contains("Nova versão disponível", output);
        Assert.Contains("1.6.1", output);
        Assert.Contains("1.7.0", output);
        Assert.Contains("Para atualizar a ferramenta globalmente, execute:", output);
        Assert.Contains("dotnet tool update -g nettasklist", output);
    }

    [Fact]
    public void Present_WhenHasUpdateIsFalse_OutputsNothing()
    {
        // Arrange
        var console = new MockAnsiConsole();
        var result = VersionCheckResult.NoUpdate("1.6.1");

        // Act
        UpdateNotificationPresenter.Present(console, result);

        // Assert
        Assert.Empty(console.Output);
    }

    [Fact]
    public void Present_WhenResultIsNull_OutputsNothing()
    {
        // Arrange
        var console = new MockAnsiConsole();

        // Act
        UpdateNotificationPresenter.Present(console, null);

        // Assert
        Assert.Empty(console.Output);
    }
}
