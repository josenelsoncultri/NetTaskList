using NetTaskList.Domain.Models;

namespace NetTaskList.Infrastructure.Abstractions;

public interface INugetVersionChecker
{
    VersionCheckResult CheckForUpdate(string currentVersion, TimeSpan timeout);
    Task<VersionCheckResult> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default);
}
