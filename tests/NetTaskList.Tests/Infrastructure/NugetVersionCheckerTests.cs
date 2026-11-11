using System.Net;
using System.Text;
using NetTaskList.Infrastructure;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class NugetVersionCheckerTests
{
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string jsonContent)
        {
            _handler = (_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request, cancellationToken);
        }
    }

    [Fact]
    public void CheckForUpdate_WhenNewerStableVersionExists_ReturnsUpdateAvailable()
    {
        // Arrange
        const string json = """
        {
          "versions": [
            "1.0.0",
            "1.6.0",
            "1.6.1",
            "1.7.0"
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Equal("1.7.0", result.LatestVersion);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenNewerStableVersionExists_ReturnsUpdateAvailable()
    {
        // Arrange
        const string json = """
        {
          "versions": [
            "1.6.1",
            "2.0.0"
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = await checker.CheckForUpdateAsync("1.6.1");

        // Assert
        Assert.True(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Equal("2.0.0", result.LatestVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenInstalledVersionIsEqualToLatest_ReturnsNoUpdate()
    {
        // Arrange
        const string json = """
        {
          "versions": [
            "1.0.0",
            "1.6.0",
            "1.6.1"
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Null(result.LatestVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenInstalledVersionIsAheadOfRemote_ReturnsNoUpdate()
    {
        // Arrange
        const string json = """
        {
          "versions": [
            "1.0.0",
            "1.6.1"
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("2.0.0", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("2.0.0", result.CurrentVersion);
        Assert.Null(result.LatestVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenRemoteHasOnlyNewerPrerelease_AndLocalIsStable_ReturnsNoUpdate()
    {
        // Arrange
        const string json = """
        {
          "versions": [
            "1.6.1",
            "1.7.0-preview.1"
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.True(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
        Assert.Null(result.LatestVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenHttp404_ReturnsFailed()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.False(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenHttp500_ReturnsFailed()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "Error");
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.False(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenNetworkThrowsHttpRequestException_ReturnsFailed()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((_, _) => throw new HttpRequestException("Network unreachable"));
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.False(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenRequestTimesOut_ReturnsFailed()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((_, ct) => Task.FromCanceled<HttpResponseMessage>(ct));
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromMilliseconds(50));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.False(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
    }

    [Fact]
    public void CheckForUpdate_WhenJsonIsMalformed_ReturnsFailed()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "not-a-valid-json");
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(result.HasUpdate);
        Assert.False(result.IsSuccess);
        Assert.Equal("1.6.1", result.CurrentVersion);
    }

    [Fact]
    public void CheckForUpdate_IsStateless_DoesNotCreateCacheFilesOnDisk()
    {
        // Arrange
        const string json = """{ "versions": ["1.0.0", "1.7.0"] }""";
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        using var client = new HttpClient(handler);
        var checker = new NugetVersionChecker(client);

        var tempDirBefore = Directory.GetFiles(Path.GetTempPath(), "*nxtl*.*");

        // Act
        var result = checker.CheckForUpdate("1.6.1", TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(result.HasUpdate);
        var tempDirAfter = Directory.GetFiles(Path.GetTempPath(), "*nxtl*.*");
        Assert.Equal(tempDirBefore.Length, tempDirAfter.Length);
    }
}
