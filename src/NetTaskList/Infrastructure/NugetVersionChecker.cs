using System.Text.Json;
using NetTaskList.Domain.Models;
using NetTaskList.Infrastructure.Abstractions;
using NetTaskList.Infrastructure.Serialization;

namespace NetTaskList.Infrastructure;

public sealed class NugetVersionChecker : INugetVersionChecker
{
    private const string NugetFlatContainerUrl = "https://api.nuget.org/v3-flatcontainer/nettasklist/index.json";
    private readonly HttpClient _httpClient;

    public NugetVersionChecker(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };
    }

    public VersionCheckResult CheckForUpdate(string currentVersion, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            return CheckForUpdateAsync(currentVersion, cts.Token).GetAwaiter().GetResult();
        }
        catch
        {
            return VersionCheckResult.Failed(currentVersion);
        }
    }

    public async Task<VersionCheckResult> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var request = new HttpRequestMessage(HttpMethod.Get, NugetFlatContainerUrl);
            request.Headers.Add("User-Agent", $"nxtl/{currentVersion}");
            request.Headers.Add("Accept", "application/json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, linkedCts.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return VersionCheckResult.Failed(currentVersion);
            }

            var json = await response.Content.ReadAsStringAsync(linkedCts.Token).ConfigureAwait(false);
            var dto = JsonSerializer.Deserialize(json, TaskJsonContext.Default.NugetPackageVersionsDto);

            if (dto?.Versions is null || dto.Versions.Count == 0)
            {
                return VersionCheckResult.NoUpdate(currentVersion);
            }

            if (!SemanticVersion.TryParse(currentVersion, out var currentSemVer) || currentSemVer is null)
            {
                return VersionCheckResult.NoUpdate(currentVersion);
            }

            SemanticVersion? latestStable = null;
            foreach (var versionStr in dto.Versions)
            {
                if (SemanticVersion.TryParse(versionStr, out var parsed) && parsed is not null)
                {
                    // Only compare against stable releases unless current is already a prerelease
                    if (!parsed.IsPreRelease || currentSemVer.IsPreRelease)
                    {
                        if (latestStable is null || parsed.IsNewerThan(latestStable))
                        {
                            latestStable = parsed;
                        }
                    }
                }
            }

            if (latestStable is not null && latestStable.IsNewerThan(currentSemVer))
            {
                return VersionCheckResult.UpdateAvailable(currentVersion, latestStable.ToString());
            }

            return VersionCheckResult.NoUpdate(currentVersion);
        }
        catch
        {
            return VersionCheckResult.Failed(currentVersion);
        }
    }
}
