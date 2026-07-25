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

    /// <summary>The standard location: <c>data/dictionaries.xml</c> next to the executable.</summary>
    public static string DefaultPath =>
        Path.Combine(AppContext.BaseDirectory, "data", "dictionaries.xml");

    /// <summary>
    /// Loads the dictionary from <see cref="DefaultPath"/>, returning <see cref="Empty"/> if the file
    /// is missing or cannot be read/parsed. Never throws.
    /// </summary>
    public static BallisticDictionary LoadDefault()
    {
        try
        {
            if (!File.Exists(DefaultPath))
                return Empty;
            using var stream = File.OpenRead(DefaultPath);
            return Load(stream);
        }
        catch
        {
            return Empty;
        }
    }

    /// <summary>Saves the dictionary to <see cref="DefaultPath"/> (creating the folder if needed).</summary>
    public void SaveDefault()
    {
        var dir = Path.GetDirectoryName(DefaultPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        Save(DefaultPath);
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
