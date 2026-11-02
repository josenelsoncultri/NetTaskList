using System.Reflection;

namespace NetTaskList.Presentation;

public static class AppVersion
{
    public static string Current { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var assembly = typeof(AppVersion).Assembly;
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(infoVersion))
        {
            return infoVersion.Split('+')[0];
        }

        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }
}
