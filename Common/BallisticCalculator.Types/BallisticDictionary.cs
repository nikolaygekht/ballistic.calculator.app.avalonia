using BallisticCalculator;
using Gehtsoft.Measurements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BallisticCalculator.Types;

/// <summary>A named sight preset from the dictionary (sight height, clicks, and a default zero).</summary>
public sealed class SightDictionaryEntry
{
    public required string Name { get; init; }
    public Measurement<DistanceUnit> SightHeight { get; init; }
    public Measurement<DistanceUnit>? DefaultZero { get; init; }
    public Measurement<AngularUnit>? HorizontalClick { get; init; }
    public Measurement<AngularUnit>? VerticalClick { get; init; }
}

/// <summary>A named barrel preset from the dictionary (twist step and direction).</summary>
public sealed class BarrelDictionaryEntry
{
    public required string Name { get; init; }
    public Measurement<DistanceUnit> Step { get; init; }
    public TwistDirection Direction { get; init; }
}

/// <summary>
/// Loads the app dictionary (<c>data/dictionaries.xml</c>) of predefined sights and barrels used to
/// prefill the Rifle / Zero inputs. Pure and UI-free; malformed entries are skipped rather than throwing.
/// </summary>
public sealed class BallisticDictionary
{
    public IReadOnlyList<SightDictionaryEntry> Sights { get; }
    public IReadOnlyList<BarrelDictionaryEntry> Barrels { get; }

    public BallisticDictionary(
        IReadOnlyList<SightDictionaryEntry> sights,
        IReadOnlyList<BarrelDictionaryEntry> barrels)
    {
        Sights = sights;
        Barrels = barrels;
    }

    /// <summary>An empty dictionary (used when the file is missing or unreadable).</summary>
    public static BallisticDictionary Empty { get; } =
        new(Array.Empty<SightDictionaryEntry>(), Array.Empty<BarrelDictionaryEntry>());

    /// <summary>
    /// The presets that ship with the application: <c>data/dictionaries.xml</c>. An update replaces this
    /// file wholesale, so nothing the user owns may live in it — the app only ever reads it.
    /// </summary>
    public static string ShippedPath =>
        Path.Combine(AppContext.BaseDirectory, "data", "dictionaries.xml");

    /// <summary>
    /// The user's own presets, <c>user-dictionaries.xml</c> beside the executable — the same place
    /// <c>appstate.json</c> lives, so the application stays portable and an update cannot overwrite it.
    /// This is the only dictionary file the editors write.
    /// </summary>
    public static string UserPath =>
        Path.Combine(AppContext.BaseDirectory, "user-dictionaries.xml");

    /// <summary>
    /// The presets the application works with: the user's file, created from the shipped one when it does
    /// not exist yet and topped up with any shipped entries it has not seen. Never throws.
    /// <para>
    /// Entries the user already has are left <b>exactly</b> as they are, so an update cannot overwrite
    /// their edits. The cost of that rule is that it cannot deliver a correction to an entry either, and
    /// that a shipped entry the user deleted comes back on the next start — <b>Reset to Defaults</b> in
    /// the editors is the escape hatch for a list that has drifted.
    /// </para>
    /// </summary>
    public static BallisticDictionary LoadForUse() => LoadForUse(ShippedPath, UserPath);

    /// <summary>As <see cref="LoadForUse()"/>, against explicit paths (the seam the tests use).</summary>
    public static BallisticDictionary LoadForUse(string shippedPath, string userPath)
    {
        var shipped = ReadOrEmpty(shippedPath);

        if (!File.Exists(userPath))
        {
            // A missing shipped file means a broken or partial install; do not answer it by writing an
            // empty user dictionary, which would then look like a deliberately emptied list forever.
            if (shipped.Sights.Count == 0 && shipped.Barrels.Count == 0)
                return shipped;

            TrySave(shipped, userPath);
            return shipped;
        }

        var user = ReadOrEmpty(userPath);
        var merged = AddMissing(user, shipped);

        // Only write when the top-up actually added something, so an ordinary start touches no files.
        if (merged.Sights.Count != user.Sights.Count || merged.Barrels.Count != user.Barrels.Count)
            TrySave(merged, userPath);

        return merged;
    }

    /// <summary>
    /// The shipped presets alone, for <b>Reset to Defaults</b>. Each editor owns one list, so a reset
    /// replaces that list and leaves the other one as the user has it.
    /// </summary>
    public static BallisticDictionary LoadShipped() => ReadOrEmpty(ShippedPath);

    /// <summary>Saves to <see cref="UserPath"/>. The editors' only write path.</summary>
    public void SaveUser() => SaveUser(UserPath);

    /// <summary>As <see cref="SaveUser()"/>, to an explicit path (the seam the tests use).</summary>
    public void SaveUser(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        Save(path);
    }

    /// <summary>
    /// <paramref name="user"/> plus every shipped entry it has no entry of that name for. Matching is by
    /// name, case-insensitively; an entry the user already has is copied across untouched, whether or not
    /// they have edited it.
    /// </summary>
    public static BallisticDictionary AddMissing(BallisticDictionary user, BallisticDictionary shipped)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(shipped);

        var sights = user.Sights.ToList();
        var haveSight = new HashSet<string>(sights.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
        sights.AddRange(shipped.Sights.Where(s => haveSight.Add(s.Name)));

        var barrels = user.Barrels.ToList();
        var haveBarrel = new HashSet<string>(barrels.Select(b => b.Name), StringComparer.OrdinalIgnoreCase);
        barrels.AddRange(shipped.Barrels.Where(b => haveBarrel.Add(b.Name)));

        sights.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        barrels.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));

        return new BallisticDictionary(sights, barrels);
    }

    private static BallisticDictionary ReadOrEmpty(string path)
    {
        try
        {
            if (!File.Exists(path))
                return Empty;
            using var stream = File.OpenRead(path);
            return Load(stream);
        }
        catch
        {
            return Empty;
        }
    }

    /// <summary>
    /// Writes the user's file, swallowing failure: an install folder the user cannot write to must not
    /// stop the application, it just means the presets are not remembered (as with <c>appstate.json</c>).
    /// </summary>
    private static void TrySave(BallisticDictionary dictionary, string path)
    {
        try
        {
            dictionary.SaveUser(path);
        }
        catch
        {
            // Nothing to do and nothing worth saying at load time; the editors report their own failures.
        }
    }

    public void Save(string path)
    {
        using var stream = File.Create(path);
        Save(stream);
    }

    public void Save(Stream stream)
    {
        var sightsElement = new XElement("sights");
        foreach (var s in Sights)
        {
            var e = new XElement("sight",
                new XAttribute("name", s.Name),
                new XAttribute("sight-height", s.SightHeight.ToString(CultureInfo.InvariantCulture)));
            if (s.DefaultZero.HasValue)
                e.Add(new XAttribute("default-zero", s.DefaultZero.Value.ToString(CultureInfo.InvariantCulture)));
            if (s.HorizontalClick.HasValue)
                e.Add(new XAttribute("horizontal-click", s.HorizontalClick.Value.ToString(CultureInfo.InvariantCulture)));
            if (s.VerticalClick.HasValue)
                e.Add(new XAttribute("vertical-click", s.VerticalClick.Value.ToString(CultureInfo.InvariantCulture)));
            sightsElement.Add(e);
        }

        var barrelsElement = new XElement("barrels");
        foreach (var b in Barrels)
        {
            barrelsElement.Add(new XElement("barrel",
                new XAttribute("name", b.Name),
                new XAttribute("step", b.Step.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("direction", b.Direction.ToString())));
        }

        new XDocument(new XElement("dictionary", sightsElement, barrelsElement)).Save(stream);
    }

    public static BallisticDictionary Load(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var root = doc.Root;
        if (root == null)
            return Empty;

        var sights = new List<SightDictionaryEntry>();
        foreach (var e in root.Element("sights")?.Elements("sight") ?? Enumerable.Empty<XElement>())
        {
            var name = (string?)e.Attribute("name");
            var height = ParseDistance((string?)e.Attribute("sight-height"));
            if (string.IsNullOrWhiteSpace(name) || height == null)
                continue;
            sights.Add(new SightDictionaryEntry
            {
                Name = name!,
                SightHeight = height.Value,
                DefaultZero = ParseDistance((string?)e.Attribute("default-zero")),
                HorizontalClick = ParseAngular((string?)e.Attribute("horizontal-click")),
                VerticalClick = ParseAngular((string?)e.Attribute("vertical-click")),
            });
        }

        var barrels = new List<BarrelDictionaryEntry>();
        foreach (var e in root.Element("barrels")?.Elements("barrel") ?? Enumerable.Empty<XElement>())
        {
            var name = (string?)e.Attribute("name");
            var step = ParseDistance((string?)e.Attribute("step"));
            if (string.IsNullOrWhiteSpace(name) || step == null ||
                !Enum.TryParse<TwistDirection>((string?)e.Attribute("direction"), out var direction))
                continue;
            barrels.Add(new BarrelDictionaryEntry
            {
                Name = name!,
                Step = step.Value,
                Direction = direction,
            });
        }

        // Present sights and barrels alphabetically by name (the combos index into these lists).
        sights.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        barrels.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));

        return new BallisticDictionary(sights, barrels);
    }

    private static Measurement<DistanceUnit>? ParseDistance(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        try { return new Measurement<DistanceUnit>(text); }
        catch { return null; }
    }

    private static Measurement<AngularUnit>? ParseAngular(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        try { return new Measurement<AngularUnit>(text); }
        catch { return null; }
    }
}
