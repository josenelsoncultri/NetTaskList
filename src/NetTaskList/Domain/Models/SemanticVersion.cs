namespace NetTaskList.Domain.Models;

public sealed class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }
    public bool IsPreRelease => !string.IsNullOrEmpty(PreRelease);

    public SemanticVersion(int major, int minor, int patch, string? preRelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = string.IsNullOrWhiteSpace(preRelease) ? null : preRelease.Trim();
    }

    public static bool TryParse(string? text, out SemanticVersion? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
        {
            normalized = normalized[1..];
        }

        // Remove build metadata (anything after +)
        var plusIndex = normalized.IndexOf('+');
        if (plusIndex >= 0)
        {
            normalized = normalized[..plusIndex];
        }

        string? preRelease = null;
        var dashIndex = normalized.IndexOf('-');
        if (dashIndex >= 0)
        {
            preRelease = normalized[(dashIndex + 1)..];
            normalized = normalized[..dashIndex];
        }

        var parts = normalized.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var major) || major < 0 ||
            !int.TryParse(parts[1], out var minor) || minor < 0 ||
            !int.TryParse(parts[2], out var patch) || patch < 0)
        {
            return false;
        }

        version = new SemanticVersion(major, minor, patch, preRelease);
        return true;
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        var majorCompare = Major.CompareTo(other.Major);
        if (majorCompare != 0) return majorCompare;

        var minorCompare = Minor.CompareTo(other.Minor);
        if (minorCompare != 0) return minorCompare;

        var patchCompare = Patch.CompareTo(other.Patch);
        if (patchCompare != 0) return patchCompare;

        // SemVer 2.0 rule: A normal version has greater precedence than a pre-release version at the same major/minor/patch.
        if (!IsPreRelease && other.IsPreRelease) return 1;
        if (IsPreRelease && !other.IsPreRelease) return -1;
        if (IsPreRelease && other.IsPreRelease)
        {
            return string.Compare(PreRelease, other.PreRelease, StringComparison.Ordinal);
        }

        return 0;
    }

    public bool IsNewerThan(SemanticVersion other) => CompareTo(other) > 0;

    public bool Equals(SemanticVersion? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as SemanticVersion);

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease);

    public override string ToString() =>
        IsPreRelease ? $"{Major}.{Minor}.{Patch}-{PreRelease}" : $"{Major}.{Minor}.{Patch}";
}
