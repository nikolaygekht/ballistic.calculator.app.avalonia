using System.Text.RegularExpressions;
using DependencyUpdater;
using NuGet.Versioning;

// ---------------------------------------------------------------------------
// depupdate — bump every PackageReference to the latest published version that
// still satisfies the version range declared in the .csproj. Upper bounds set
// in the project (e.g. Avalonia "[11.3.12,12)") are always respected, so the
// tool will never cross a ceiling such as Avalonia 12.
// ---------------------------------------------------------------------------

var options = CliOptions.Parse(args);
if (options.ShowHelp)
{
    CliOptions.PrintUsage();
    return 0;
}

// Families that MUST carry an explicit upper bound. A reference in one of these
// families without a ceiling is reported as a warning.
var boundedFamilies = new Regex(@"^(Avalonia|SkiaSharp|ScottPlot)(\.|$)", RegexOptions.IgnoreCase);

var root = options.Root ?? FindRepoRoot(Directory.GetCurrentDirectory());
if (root is null)
{
    Console.Error.WriteLine("Could not locate a repo root (no NuGet.config or .sln found). Pass --root <path>.");
    return 1;
}

Console.WriteLine($"Root:  {root}");
using var resolver = new NuGetVersionResolver(root);
Console.WriteLine($"Feeds: {string.Join(", ", resolver.SourceNames)}");
Console.WriteLine(options.Apply ? "Mode:  APPLY (files will be modified)\n" : "Mode:  dry-run (use --apply to write changes)\n");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

int totalUpdates = 0, totalWarnings = 0;

foreach (var project in CsprojScanner.FindProjects(root))
{
    var text = await File.ReadAllTextAsync(project, cts.Token);
    var references = CsprojScanner.Parse(text);
    if (references.Count == 0)
        continue;

    var lines = new List<string>();
    var pendingWrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var reference in references)
    {
        if (!VersionSpec.TryParse(reference.RawVersion, out var spec) || spec is null)
        {
            lines.Add($"  {reference.PackageId,-40} {reference.RawVersion,-16} ?? unparseable version, skipped");
            continue;
        }

        var missingBound = boundedFamilies.IsMatch(reference.PackageId) && !spec.HasUpperBound;

        var allVersions = await resolver.GetVersionsAsync(reference.PackageId, cts.Token);
        if (allVersions.Count == 0)
        {
            lines.Add($"  {reference.PackageId,-40} {reference.RawVersion,-16} -- not found on any feed");
            continue;
        }

        bool allowPrerelease = options.Prerelease || (spec.CurrentFloor?.IsPrerelease ?? false);

        var bestInRange = allVersions
            .Where(v => spec!.Range.Satisfies(v))
            .Where(v => allowPrerelease || !v.IsPrerelease)
            .DefaultIfEmpty()
            .Max();

        var latestOverall = allVersions
            .Where(v => allowPrerelease || !v.IsPrerelease)
            .Max();

        // Note when a genuinely newer version exists but is deliberately held back
        // by the project's ceiling — proof the upper bound is doing its job.
        string ceilingNote = "";
        if (latestOverall is not null && bestInRange is not null && latestOverall > bestInRange)
            ceilingNote = $"  (ceiling holds back {latestOverall.ToNormalizedString()})";

        string status;
        if (bestInRange is null)
        {
            status = "no version in range";
        }
        else if (spec.CurrentFloor is not null && bestInRange > spec.CurrentFloor)
        {
            var newRaw = spec.RewriteFloor(bestInRange);
            status = $"-> {newRaw}";
            pendingWrites[reference.PackageId] = newRaw;
            totalUpdates++;
        }
        else
        {
            status = "up to date";
        }

        var warn = missingBound ? "  !! NO UPPER BOUND" : "";
        if (missingBound) totalWarnings++;

        lines.Add($"  {reference.PackageId,-40} {reference.RawVersion,-16} {status}{ceilingNote}{warn}");
    }

    Console.WriteLine(Path.GetRelativePath(root, project));
    foreach (var line in lines)
        Console.WriteLine(line);
    Console.WriteLine();

    if (options.Apply && pendingWrites.Count > 0)
    {
        var updated = CsprojScanner.ApplyUpdates(text, pendingWrites);
        await File.WriteAllTextAsync(project, updated, cts.Token);
    }
}

Console.WriteLine(new string('-', 60));
Console.WriteLine($"Updates available: {totalUpdates}   Bound warnings: {totalWarnings}");
if (totalUpdates > 0 && !options.Apply)
    Console.WriteLine("Re-run with --apply to write these changes.");

if (options.FailOnOutdated && totalUpdates > 0)
    return 2;
return 0;

// ---------------------------------------------------------------------------

static string? FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        bool hasMarker = dir.EnumerateFiles("NuGet.config").Any()
                      || dir.EnumerateFiles("nuget.config").Any()
                      || dir.EnumerateFiles("*.sln").Any();
        if (hasMarker)
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}

sealed class CliOptions
{
    public string? Root { get; private set; }
    public bool Apply { get; private set; }
    public bool Prerelease { get; private set; }
    public bool FailOnOutdated { get; private set; }
    public bool ShowHelp { get; private set; }

    public static CliOptions Parse(string[] args)
    {
        var o = new CliOptions();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--apply": o.Apply = true; break;
                case "--prerelease": o.Prerelease = true; break;
                case "--fail-on-outdated": o.FailOnOutdated = true; break;
                case "-h" or "--help": o.ShowHelp = true; break;
                case "--root": o.Root = i + 1 < args.Length ? args[++i] : null; break;
                default:
                    if (!args[i].StartsWith('-') && o.Root is null)
                        o.Root = args[i];
                    break;
            }
        }
        return o;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("""
            depupdate — update PackageReference versions within their declared ranges.

            Usage:
              depupdate [root] [options]

            Arguments:
              root                 Repo root to scan (default: nearest folder with NuGet.config/.sln).

            Options:
              --apply              Write the updates back to the .csproj files.
              --prerelease         Consider prerelease versions as update candidates.
              --fail-on-outdated   Exit with code 2 if any updates are available (for CI).
              -h, --help           Show this help.

            The tool never crosses an upper bound declared in a .csproj. A reference in the
            Avalonia / SkiaSharp / ScottPlot families that lacks an upper bound is flagged.
            """);
    }
}
