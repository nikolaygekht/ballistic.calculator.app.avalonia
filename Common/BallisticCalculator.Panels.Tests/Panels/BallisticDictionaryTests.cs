using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

public class BallisticDictionaryTests
{
    private const string Sample = """
        <dictionary>
            <sights>
                <sight name="Optics Mil" sight-height="3in" default-zero="100yd" horizontal-click="0.1mil" vertical-click="0.1mil" />
                <sight name="Iron" sight-height="5cm" default-zero="100m" />
            </sights>
            <barrels>
                <barrel name="AK-74" step="200mm" direction="Right" />
                <barrel name="Lefty" step="10in" direction="Left" />
            </barrels>
        </dictionary>
        """;

    private static BallisticDictionary LoadSample()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Sample));
        return BallisticDictionary.Load(stream);
    }

    [Fact]
    public void Load_ParsesSightsAndBarrels()
    {
        var dict = LoadSample();

        dict.Sights.Should().HaveCount(2);
        dict.Barrels.Should().HaveCount(2);
    }

    [Fact]
    public void Load_SightsAndBarrels_SortedByName()
    {
        var dict = LoadSample();

        dict.Sights.Select(s => s.Name).Should().BeInAscendingOrder();
        dict.Barrels.Select(b => b.Name).Should().BeInAscendingOrder();
        dict.Sights[0].Name.Should().Be("Iron"); // "Iron" sorts before "Optics Mil"
    }

    [Fact]
    public void Load_SightWithClicks_ParsesAllFields()
    {
        var dict = LoadSample();
        var sight = dict.Sights.Single(s => s.Name == "Optics Mil");

        sight.Name.Should().Be("Optics Mil");
        sight.SightHeight.In(DistanceUnit.Inch).Should().BeApproximately(3, 0.01);
        sight.DefaultZero!.Value.In(DistanceUnit.Yard).Should().BeApproximately(100, 0.01);
        sight.VerticalClick!.Value.In(AngularUnit.Mil).Should().BeApproximately(0.1, 0.001);
        sight.HorizontalClick!.Value.In(AngularUnit.Mil).Should().BeApproximately(0.1, 0.001);
    }

    [Fact]
    public void Load_SightWithoutClicks_LeavesClicksNull()
    {
        var dict = LoadSample();
        var sight = dict.Sights.Single(s => s.Name == "Iron");

        sight.Name.Should().Be("Iron");
        sight.VerticalClick.Should().BeNull();
        sight.HorizontalClick.Should().BeNull();
        sight.SightHeight.In(DistanceUnit.Centimeter).Should().BeApproximately(5, 0.01);
    }

    [Fact]
    public void Load_Barrel_ParsesStepAndDirection()
    {
        var dict = LoadSample();

        dict.Barrels[0].Name.Should().Be("AK-74");
        dict.Barrels[0].Step.In(DistanceUnit.Millimeter).Should().BeApproximately(200, 0.5);
        dict.Barrels[0].Direction.Should().Be(TwistDirection.Right);
        dict.Barrels[1].Direction.Should().Be(TwistDirection.Left);
    }

    [Fact]
    public void Load_SkipsMalformedEntries()
    {
        const string bad = """
            <dictionary>
                <sights>
                    <sight name="Good" sight-height="3in" />
                    <sight name="NoHeight" />
                    <sight sight-height="3in" />
                </sights>
                <barrels>
                    <barrel name="BadDir" step="10in" direction="Sideways" />
                    <barrel name="GoodBarrel" step="10in" direction="Right" />
                </barrels>
            </dictionary>
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(bad));

        var dict = BallisticDictionary.Load(stream);

        dict.Sights.Should().ContainSingle(s => s.Name == "Good");
        dict.Barrels.Should().ContainSingle(b => b.Name == "GoodBarrel");
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsAllFields()
    {
        var original = new BallisticDictionary(
            new[]
            {
                new SightDictionaryEntry
                {
                    Name = "Optic",
                    SightHeight = new Measurement<DistanceUnit>(3, DistanceUnit.Inch),
                    DefaultZero = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
                    HorizontalClick = new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                    VerticalClick = new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                },
                new SightDictionaryEntry
                {
                    Name = "Iron",
                    SightHeight = new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                },
            },
            new[]
            {
                new BarrelDictionaryEntry
                {
                    Name = "AR",
                    Step = new Measurement<DistanceUnit>(7, DistanceUnit.Inch),
                    Direction = TwistDirection.Right,
                },
            });

        using var stream = new MemoryStream();
        original.Save(stream);
        stream.Position = 0;
        var loaded = BallisticDictionary.Load(stream);

        var optic = loaded.Sights.Single(s => s.Name == "Optic");
        optic.SightHeight.In(DistanceUnit.Inch).Should().BeApproximately(3, 0.001);
        optic.DefaultZero!.Value.In(DistanceUnit.Yard).Should().BeApproximately(100, 0.001);
        optic.VerticalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.001);

        var iron = loaded.Sights.Single(s => s.Name == "Iron");
        iron.DefaultZero.Should().BeNull();
        iron.HorizontalClick.Should().BeNull();

        loaded.Barrels.Should().ContainSingle();
        loaded.Barrels[0].Step.In(DistanceUnit.Inch).Should().BeApproximately(7, 0.001);
        loaded.Barrels[0].Direction.Should().Be(TwistDirection.Right);
    }

    [Fact]
    public void Save_OmitsOptionalAttributes_WhenNull()
    {
        var dict = new BallisticDictionary(
            new[] { new SightDictionaryEntry { Name = "Bare", SightHeight = new Measurement<DistanceUnit>(2, DistanceUnit.Inch) } },
            System.Array.Empty<BarrelDictionaryEntry>());

        using var stream = new MemoryStream();
        dict.Save(stream);
        var xml = Encoding.UTF8.GetString(stream.ToArray());

        xml.Should().NotContain("default-zero");
        xml.Should().NotContain("horizontal-click");
        xml.Should().NotContain("vertical-click");
        xml.Should().Contain("sight-height");
    }

    [Fact]
    public void LoadForUse_MissingFiles_ReturnsEmpty_DoesNotThrow()
    {
        // A broken or partial install must degrade to an empty dictionary rather than throw.
        var act = () => BallisticDictionary.LoadForUse(
            Path.Combine(Path.GetTempPath(), $"no-such-shipped-{Guid.NewGuid():N}.xml"),
            Path.Combine(Path.GetTempPath(), $"no-such-user-{Guid.NewGuid():N}.xml"));

        act.Should().NotThrow();
        act().Sights.Should().BeEmpty();
    }

    #region Shipped vs user: keeping the user's edits across an update

    private static BallisticDictionary FromXml(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return BallisticDictionary.Load(stream);
    }

    private const string Shipped = """
        <dictionary>
            <sights>
                <sight name="Optics Mil" sight-height="3in" default-zero="100m" />
                <sight name="Iron" sight-height="5cm" default-zero="100m" />
                <sight name="Brand New Optic" sight-height="2in" default-zero="100m" />
            </sights>
            <barrels>
                <barrel name="AK-74" step="200mm" direction="Right" />
                <barrel name="Brand New Barrel" step="8in" direction="Right" />
            </barrels>
        </dictionary>
        """;

    [Fact]
    public void AddMissing_ShouldAddShippedEntriesTheUserDoesNotHave()
    {
        var user = LoadSample();

        var merged = BallisticDictionary.AddMissing(user, FromXml(Shipped));

        merged.Sights.Select(s => s.Name).Should().Contain("Brand New Optic");
        merged.Barrels.Select(b => b.Name).Should().Contain("Brand New Barrel");
    }

    [Fact]
    public void AddMissing_ShouldNotTouchAnEntryTheUserAlreadyHas()
    {
        // The rule is add-only. "Optics Mil" ships with a 100 m default zero and the user's copy says
        // 100 yd; theirs survives, which is the whole point — an update cannot overwrite their work.
        var user = LoadSample();

        var merged = BallisticDictionary.AddMissing(user, FromXml(Shipped));

        var optic = merged.Sights.Single(s => s.Name == "Optics Mil");
        optic.DefaultZero!.Value.In(DistanceUnit.Yard).Should().BeApproximately(100, 1e-6);
        optic.HorizontalClick.Should().NotBeNull("the user's own clicks must survive too");
    }

    [Fact]
    public void AddMissing_ShouldNotDuplicateAnEntryDifferingOnlyInCase()
    {
        var user = FromXml("""
            <dictionary><sights>
                <sight name="OPTICS MIL" sight-height="9in" />
            </sights><barrels/></dictionary>
            """);

        var merged = BallisticDictionary.AddMissing(user, FromXml(Shipped));

        merged.Sights.Count(s => s.Name.Equals("optics mil", StringComparison.OrdinalIgnoreCase))
              .Should().Be(1);
        merged.Sights.Single(s => s.Name == "OPTICS MIL").SightHeight.In(DistanceUnit.Inch)
              .Should().BeApproximately(9, 1e-6, "the user's entry wins, not the shipped one");
    }

    [Fact]
    public void AddMissing_ShouldKeepEntriesTheUserAddedThemselves()
    {
        var user = FromXml("""
            <dictionary><sights>
                <sight name="My Own Scope" sight-height="1.8in" />
            </sights><barrels/></dictionary>
            """);

        var merged = BallisticDictionary.AddMissing(user, FromXml(Shipped));

        merged.Sights.Select(s => s.Name).Should().Contain("My Own Scope");
        merged.Sights.Should().HaveCount(4, "their own entry plus the three shipped ones");
    }

    #endregion

    #region The user file on disk

    /// <summary>A scratch pair of paths; the user file deliberately does not exist yet.</summary>
    private static (string Shipped, string User, string Dir) Scratch(string shippedXml)
    {
        var dir = Path.Combine(Path.GetTempPath(), $"dict-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        var shipped = Path.Combine(dir, "dictionaries.xml");
        File.WriteAllText(shipped, shippedXml);
        return (shipped, Path.Combine(dir, "user-dictionaries.xml"), dir);
    }

    [Fact]
    public void LoadForUse_FirstRun_ShouldCreateTheUserFileFromTheShippedOne()
    {
        var (shipped, user, dir) = Scratch(Shipped);
        try
        {
            var loaded = BallisticDictionary.LoadForUse(shipped, user);

            File.Exists(user).Should().BeTrue("the first run seeds the user's own copy");
            loaded.Sights.Should().HaveCount(3);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadForUse_ShouldTopUpTheUserFileWithNewShippedEntries()
    {
        var (shipped, user, dir) = Scratch(Shipped);
        try
        {
            File.WriteAllText(user, Sample);   // an older user file, without the two new entries

            var loaded = BallisticDictionary.LoadForUse(shipped, user);

            loaded.Sights.Select(s => s.Name).Should().Contain("Brand New Optic");
            loaded.Barrels.Select(b => b.Name).Should().Contain("Brand New Barrel");

            // and the top-up is persisted, not recomputed on every start
            BallisticDictionary.LoadForUse(Path.Combine(dir, "gone.xml"), user)
                               .Sights.Select(s => s.Name).Should().Contain("Brand New Optic");
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadForUse_ShouldNeverWriteToTheShippedFile()
    {
        var (shipped, user, dir) = Scratch(Shipped);
        try
        {
            var before = File.ReadAllText(shipped);
            File.WriteAllText(user, Sample);

            BallisticDictionary.LoadForUse(shipped, user);

            File.ReadAllText(shipped).Should().Be(before,
                "an update replaces the shipped file, so nothing the user owns may be written into it");
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadForUse_WithNoShippedFile_ShouldNotCreateAnEmptyUserFile()
    {
        // A partial install must not leave behind an empty user dictionary, which would then look
        // forever like a list the user had deliberately emptied.
        var dir = Path.Combine(Path.GetTempPath(), $"dict-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var user = Path.Combine(dir, "user-dictionaries.xml");

            BallisticDictionary.LoadForUse(Path.Combine(dir, "missing.xml"), user);

            File.Exists(user).Should().BeFalse();
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadForUse_ShouldLeaveADeletedShippedEntryOutUntilTheNextStart()
    {
        // Documenting the accepted cost of the add-only rule: a shipped entry the user deletes comes
        // back, because "absent" and "never seen" are the same state. Reset in the editors is the
        // escape hatch, and the manual says so.
        var (shipped, user, dir) = Scratch(Shipped);
        try
        {
            BallisticDictionary.LoadForUse(shipped, user);

            var trimmed = BallisticDictionary.LoadForUse(shipped, user);
            var kept = trimmed.Sights.Where(s => s.Name != "Iron").ToList();
            new BallisticDictionary(kept, trimmed.Barrels).SaveUser(user);

            BallisticDictionary.LoadForUse(shipped, user)
                               .Sights.Select(s => s.Name).Should().Contain("Iron");
        }
        finally { Directory.Delete(dir, true); }
    }

    #endregion
}
