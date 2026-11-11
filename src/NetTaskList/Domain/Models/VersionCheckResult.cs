namespace NetTaskList.Domain.Models;

public sealed class VersionCheckResult
{
    public bool HasUpdate { get; }
    public string CurrentVersion { get; }
    public string? LatestVersion { get; }
    public bool IsSuccess { get; }

    public VersionCheckResult(bool hasUpdate, string currentVersion, string? latestVersion, bool isSuccess)
    {
        HasUpdate = hasUpdate;
        CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
        LatestVersion = latestVersion;
        IsSuccess = isSuccess;
    }

    public static VersionCheckResult UpdateAvailable(string currentVersion, string latestVersion) =>
        new(hasUpdate: true, currentVersion, latestVersion, isSuccess: true);

    public static VersionCheckResult NoUpdate(string currentVersion) =>
        new(hasUpdate: false, currentVersion, latestVersion: null, isSuccess: true);

    public static VersionCheckResult Failed(string currentVersion) =>
        new(hasUpdate: false, currentVersion, latestVersion: null, isSuccess: false);
}
