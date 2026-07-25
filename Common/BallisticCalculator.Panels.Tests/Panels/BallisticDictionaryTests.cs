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
    public void LoadDefault_MissingFile_ReturnsEmpty_DoesNotThrow()
    {
        // In the test host there is no data/dictionaries.xml next to the assembly; LoadDefault must
        // degrade gracefully to an empty dictionary rather than throw.
        var act = () => BallisticDictionary.LoadDefault();
        act.Should().NotThrow();
    }
}
