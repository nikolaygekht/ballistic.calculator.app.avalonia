using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;

namespace BallisticCalculator.Panels.Tests;

/// <summary>
/// Guards the shipped <c>data/drg</c> library against data typos. One file was broken for years by a
/// period where a comma belonged in the header, and nothing but a hand check would have caught it.
/// The library is not copied to the test output (it is large), so these tests read it from the source
/// tree and skip silently when the tree is not there — a published test run is not a data check.
/// </summary>
public class ShippedDrgLibraryTests
{
    /// <summary>The repository's <c>data/drg</c> folder, or null when the source tree is unavailable.</summary>
    private static string? DrgLibraryFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "BallisticCalculator2.sln")))
            {
                var drg = Path.Combine(dir.FullName, "data", "drg");
                return Directory.Exists(drg) ? drg : null;
            }
            dir = dir.Parent;
        }
        return null;
    }

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
}
