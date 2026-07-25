using System.Collections.Generic;
using System.IO;
using BallisticCalculator;

namespace BallisticCalculator.Types;

/// <summary>
/// Loads custom radar/measured drag tables (<c>.drg</c>) referenced by an ammunition's
/// <see cref="Ammunition.CustomTableFileName"/>. A GC ("custom") ballistic coefficient has no
/// built-in curve, so the loaded <see cref="DrgDragTable"/> must be supplied to both
/// <c>CalculateZeroParameters</c> and <c>Calculate</c>.
/// </summary>
public static class CustomDragTableLoader
{
    // Cache keyed by resolved path + last-write time so an edited .drg is reloaded.
    private static readonly Dictionary<string, DrgDragTable> Cache = new();

    /// <summary>
    /// The drag table an ammunition needs, or null when it uses a standard table (not GC) or no
    /// custom file is set / resolvable.
    /// </summary>
    public static DragTable? ForAmmunition(Ammunition? ammunition)
    {
        if (ammunition == null || ammunition.BallisticCoefficient.Table != DragTableId.GC)
            return null;
        return Load(ammunition.CustomTableFileName);
    }

    /// <summary>Loads (and caches) a <c>.drg</c> table by file name, or null if missing/unreadable.</summary>
    public static DrgDragTable? Load(string? fileName)
    {
        var path = ResolvePath(fileName);
        if (path == null)
            return null;

        try
        {
            var key = path + "|" + File.GetLastWriteTimeUtc(path).Ticks;
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            var table = DrgDragTable.Open(path);
            Cache[key] = table;
            return table;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves a stored file name to an existing path: the name as given if it exists, otherwise the
    /// same file name under <see cref="DataFolders.Drg"/> (and its subfolders). Null if not found.
    /// </summary>
    public static string? ResolvePath(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        if (File.Exists(fileName))
            return fileName;

        var drgRoot = DataFolders.Drg;
        if (Directory.Exists(drgRoot))
        {
            var bare = Path.GetFileName(fileName);
            var direct = Path.Combine(drgRoot, bare);
            if (File.Exists(direct))
                return direct;

            foreach (var found in Directory.EnumerateFiles(drgRoot, bare, SearchOption.AllDirectories))
                return found;
        }

        return null;
    }
}
