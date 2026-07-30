using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Serialization;

namespace BallisticCalculator.Panels.Tests;

/// <summary>
/// Guards the shipped data libraries against typos. One <c>.drg</c> was broken for years by a period
/// where a comma belonged in the header, and nothing but a hand check would have caught it; an
/// ammunition entry can be wrong the same way. These libraries are not copied to the test output (the
/// drag tables are large), so the tests read them from the source tree and skip silently when the tree
/// is not there — a published test run is not a data check.
/// </summary>
public class ShippedDrgLibraryTests
{
    /// <summary>A folder under the repository's <c>data</c>, or null when the source tree is unavailable.</summary>
    private static string? DataFolder(string name)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "BallisticCalculator2.sln")))
            {
                var folder = Path.Combine(dir.FullName, "data", name);
                return Directory.Exists(folder) ? folder : null;
            }
            dir = dir.Parent;
        }
        return null;
    }

    private static string? DrgLibraryFolder() => DataFolder("drg");

    [Fact]
    public void EveryShippedDrgFile_Opens()
    {
        var folder = DrgLibraryFolder();
        if (folder is null)
            return;

        var files = Directory.GetFiles(folder, "*.drg", SearchOption.AllDirectories);
        files.Should().NotBeEmpty("the .drg library should be found when the source tree is present");

        var failures = new List<string>();
        foreach (var file in files)
        {
            try
            {
                DrgDragTable.Open(file).Should().NotBeNull();
            }
            catch (Exception e)
            {
                failures.Add($"{Path.GetRelativePath(folder, file)}: {e.GetType().Name}: {e.Message}");
            }
        }

        failures.Should().BeEmpty();
    }

    /// <summary>
    /// Every shipped ammunition entry loads, in both formats, through the same readers the Ammunition
    /// tab uses. These are hand-written XML, so a bad unit suffix or a stray character is exactly as
    /// likely here as it was in the drag table above — and a load that throws is swallowed by the
    /// panel's catch, so a broken entry would simply appear to do nothing.
    /// </summary>
    [Fact]
    public void EveryShippedAmmunitionFile_Loads()
    {
        var folder = DataFolder("ammo");
        if (folder is null)
            return;

        var files = new List<string>(Directory.GetFiles(folder, "*.ammox", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles(folder, "*.ammo", SearchOption.AllDirectories));
        files.Should().NotBeEmpty("the ammunition library should be found when the source tree is present");

        var failures = new List<string>();
        foreach (var file in files)
        {
            try
            {
                var entry = file.EndsWith(".ammo", StringComparison.OrdinalIgnoreCase)
                    ? BallisticXmlDeserializer.ReadLegacyAmmunitionLibraryEntryFromFile(file)
                    : BallisticXmlDeserializer.ReadFromFile<AmmunitionLibraryEntry>(file);

                if (entry?.Ammunition is null)
                    failures.Add($"{Path.GetRelativePath(folder, file)}: read, but carries no ammunition");
                else if (entry.Ammunition.MuzzleVelocity.Value <= 0 || entry.Ammunition.Weight.Value <= 0)
                    failures.Add($"{Path.GetRelativePath(folder, file)}: non-positive weight or muzzle velocity");
            }
            catch (Exception e)
            {
                failures.Add($"{Path.GetRelativePath(folder, file)}: {e.GetType().Name}: {e.Message}");
            }
        }

        failures.Should().BeEmpty();
    }
}
