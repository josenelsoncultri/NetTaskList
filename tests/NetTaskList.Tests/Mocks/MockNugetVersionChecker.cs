using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure.Abstractions;

namespace NetTaskList.Tests.Mocks;

public sealed class MockNugetVersionChecker : INugetVersionChecker
{
    private VersionCheckResult? _configuredResult;

    public int CallCount { get; private set; }
    public string? LastCheckedVersion { get; private set; }
    public TimeSpan? LastTimeout { get; private set; }

    public MockNugetVersionChecker(VersionCheckResult? defaultResult = null)
    {
        _configuredResult = defaultResult;
    }

    public void SetResult(VersionCheckResult result)
    {
        _configuredResult = result;
    }

    public VersionCheckResult CheckForUpdate(string currentVersion, TimeSpan timeout)
    {
        CallCount++;
        LastCheckedVersion = currentVersion;
        LastTimeout = timeout;

        return _configuredResult ?? VersionCheckResult.NoUpdate(currentVersion);
    }

    public Task<VersionCheckResult> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CheckForUpdate(currentVersion, TimeSpan.FromSeconds(2)));
    }
}
