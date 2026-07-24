using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace DependencyUpdater;

/// <summary>
/// Wraps the raw <c>Version="..."</c> string of a PackageReference so we can both
/// reason about it (via <see cref="NuGet.Versioning.VersionRange"/>) and rewrite only
/// its lower bound while preserving the author's original upper-bound text and bracket style.
/// </summary>
public sealed partial class VersionSpec
{
    // Matches interval notation: [lo,hi)  (lo,hi]  [lo,]  [,hi)  etc.
    [GeneratedRegex(@"^\s*([\[\(])\s*([^,\]\)]*?)\s*,\s*([^,\]\)]*?)\s*([\]\)])\s*$")]
    private static partial Regex IntervalRegex();

    // Matches an exact-pin interval: [1.2.3]
    [GeneratedRegex(@"^\s*\[\s*([^,\]\)]+?)\s*\]\s*$")]
    private static partial Regex ExactRegex();

    public string Raw { get; }
    public VersionRange Range { get; }

    private readonly RewriteKind _kind;
    private readonly string _open = "";
    private readonly string _close = "";
    private readonly string _upperToken = "";

    private enum RewriteKind
    {
        /// <summary>Bare "1.2.3" — a floor with an open upper bound.</summary>
        Bare,
        /// <summary>"[1.2.3]" — exact pin.</summary>
        Exact,
        /// <summary>"[lo,hi)" style interval — rewrite lo, keep hi verbatim.</summary>
        Interval,
    }

    private VersionSpec(string raw, VersionRange range, RewriteKind kind,
                        string open, string close, string upperToken)
    {
        Raw = raw;
        Range = range;
        _kind = kind;
        _open = open;
        _close = close;
        _upperToken = upperToken;
    }

    public static bool TryParse(string raw, out VersionSpec? spec)
    {
        spec = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        // NuGet's own parser is the source of truth for the semantic range.
        if (!VersionRange.TryParse(raw, out var range) || range is null)
            return false;

        var interval = IntervalRegex().Match(raw);
        if (interval.Success)
        {
            spec = new VersionSpec(raw, range, RewriteKind.Interval,
                open: interval.Groups[1].Value,
                close: interval.Groups[4].Value,
                upperToken: interval.Groups[3].Value);
            return true;
        }

        if (ExactRegex().IsMatch(raw))
        {
            spec = new VersionSpec(raw, range, RewriteKind.Exact, "[", "]", "");
            return true;
        }

        // Anything else NuGet accepted is treated as a bare floor.
        spec = new VersionSpec(raw, range, RewriteKind.Bare, "", "", "");
        return true;
    }

    public bool HasUpperBound => Range.HasUpperBound;

    /// <summary>The effective pinned/floor version this reference currently resolves to.</summary>
    public NuGetVersion? CurrentFloor => Range.MinVersion;

    /// <summary>
    /// Produce a new raw version string with the lower bound moved to <paramref name="target"/>,
    /// keeping the original upper bound text and bracket style untouched.
    /// </summary>
    public string RewriteFloor(NuGetVersion target)
    {
        var v = target.ToNormalizedString();
        return _kind switch
        {
            RewriteKind.Interval => $"{_open}{v},{_upperToken}{_close}",
            RewriteKind.Exact => $"[{v}]",
            _ => v,
        };
    }
}
