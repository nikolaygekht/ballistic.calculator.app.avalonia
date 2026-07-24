using System.Text.RegularExpressions;

namespace DependencyUpdater;

/// <summary>
/// Reads and rewrites <c>&lt;PackageReference&gt;</c> version attributes in .csproj files.
/// Rewrites operate on the raw file text so existing formatting, comments and child
/// elements are preserved exactly — only the Version attribute value changes.
/// </summary>
public static partial class CsprojScanner
{
    [GeneratedRegex(@"<PackageReference\b(?<attrs>[^>]*?)(/>|>)", RegexOptions.Compiled)]
    private static partial Regex PackageRefTag();

    [GeneratedRegex(@"\bInclude\s*=\s*""(?<v>[^""]*)""", RegexOptions.Compiled)]
    private static partial Regex IncludeAttr();

    [GeneratedRegex(@"(?<pre>\bVersion\s*=\s*"")(?<v>[^""]*)(?<post>"")", RegexOptions.Compiled)]
    private static partial Regex VersionAttr();

    public sealed record Reference(string PackageId, string RawVersion);

    public static IEnumerable<string> FindProjects(string root)
    {
        return Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !IsInBuildOutput(p))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsInBuildOutput(string path)
    {
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => p.Equals("bin", StringComparison.OrdinalIgnoreCase)
                           || p.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Parse every PackageReference that carries a Version attribute.</summary>
    public static List<Reference> Parse(string csprojText)
    {
        var result = new List<Reference>();
        foreach (Match tag in PackageRefTag().Matches(csprojText))
        {
            var attrs = tag.Groups["attrs"].Value;
            var include = IncludeAttr().Match(attrs);
            var version = VersionAttr().Match(attrs);
            if (include.Success && version.Success)
                result.Add(new Reference(include.Groups["v"].Value, version.Groups["v"].Value));
        }
        return result;
    }

    /// <summary>
    /// Rewrite the Version attribute for the given package ids. Returns the new text
    /// (or the original, unchanged, if nothing matched).
    /// </summary>
    public static string ApplyUpdates(string csprojText, IReadOnlyDictionary<string, string> newVersions)
    {
        return PackageRefTag().Replace(csprojText, tag =>
        {
            var attrs = tag.Groups["attrs"].Value;
            var include = IncludeAttr().Match(attrs);
            if (!include.Success || !newVersions.TryGetValue(include.Groups["v"].Value, out var newVersion))
                return tag.Value;

            var newAttrs = VersionAttr().Replace(attrs, m => $"{m.Groups["pre"].Value}{newVersion}{m.Groups["post"].Value}", 1);
            return tag.Value.Replace(attrs, newAttrs);
        });
    }
}
