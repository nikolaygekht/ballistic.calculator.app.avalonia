using System;
using System.IO;
using System.Linq;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Tools;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// The wrapper over the two .drg factories: validates inputs before the library sees them (so dialogs can
/// show a message instead of an exception) and carries the header metadata the format supports.
/// </summary>
public class DragTableBuilderTests
{
    private static DrgMetadata Metadata(string name = "220gr .308", string? source = "BC curve") =>
        new(name, source,
            new Measurement<WeightUnit>(220, WeightUnit.Grain),
            new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(1.226, DistanceUnit.Inch));

    private static BcAtMach[] Curve() => new[]
    {
        new BcAtMach(1.20, 0.307),
        new BcAtMach(1.65, 0.301),
        new BcAtMach(2.25, 0.318),
    };

    private static RadarReading[] Readings() => new[]
    {
        Reading(0, 3078.8), Reading(100, 3001.2), Reading(200, 2923.9), Reading(300, 2847.2),
    };

    private static RadarReading Reading(double yards, double fps) =>
        new(new Measurement<DistanceUnit>(yards, DistanceUnit.Yard),
            new Measurement<VelocityUnit>(fps, VelocityUnit.FeetPerSecond));

    #region Mach conversion

    [Fact]
    public void VelocityToMach_AtStandardAtmosphere_ShouldUseSoundVelocity()
    {
        var sound = new Atmosphere().SoundVelocity;

        DragTableBuilder.VelocityToMach(sound).Should().BeApproximately(1.0, 1e-9);
        DragTableBuilder.VelocityToMach(sound * 2).Should().BeApproximately(2.0, 1e-9);
    }

    [Fact]
    public void MachToVelocity_ShouldRoundTrip()
    {
        var velocity = DragTableBuilder.MachToVelocity(2.25, VelocityUnit.FeetPerSecond);

        velocity.Unit.Should().Be(VelocityUnit.FeetPerSecond);
        DragTableBuilder.VelocityToMach(velocity).Should().BeApproximately(2.25, 1e-9);
    }

    #endregion

    #region From a BC curve

    [Fact]
    public void FromBcCurve_ShouldReturnCustomTableWithMetadata()
    {
        var table = DragTableBuilder.FromBcCurve(Metadata(), DragTableId.G7, Curve());

        table.TableId.Should().Be(DragTableId.GC);
        table.Count.Should().BeGreaterThan(0);

        var entry = table.Ammunition;
        entry.Should().NotBeNull();
        entry!.Name.Should().Be("220gr .308");
        entry.Source.Should().Be("BC curve");
        entry.Ammunition.Weight.In(WeightUnit.Grain).Should().BeApproximately(220, 0.01);
        entry.Ammunition.BulletDiameter!.Value.In(DistanceUnit.Inch).Should().BeApproximately(0.308, 1e-6);
        entry.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.226, 1e-6);

        // A custom table is used with a form factor of 1 on GC.
        entry.Ammunition.BallisticCoefficient.Table.Should().Be(DragTableId.GC);
        entry.Ammunition.BallisticCoefficient.Value.Should().BeApproximately(1.0, 1e-9);
    }

    [Fact]
    public void FromBcCurve_SingleKnot_ShouldBeAccepted()
    {
        // One knot means a constant BC scaling of the base curve — the library permits it.
        var table = DragTableBuilder.FromBcCurve(Metadata(), DragTableId.G1, new[] { new BcAtMach(1.5, 0.462) });

        table.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void FromBcCurve_UnsortedKnots_ShouldGiveTheSameTableAsSorted()
    {
        var sorted = DragTableBuilder.FromBcCurve(Metadata(), DragTableId.G7, Curve());
        var shuffled = DragTableBuilder.FromBcCurve(Metadata(), DragTableId.G7,
            new[] { Curve()[2], Curve()[0], Curve()[1] });

        shuffled.Count.Should().Be(sorted.Count);
        for (int i = 0; i < sorted.Count; i++)
            shuffled[i].DragCoefficient.Should().BeApproximately(sorted[i].DragCoefficient, 1e-12);
    }

    [Fact]
    public void FromBcCurve_HigherBc_ShouldGiveLowerDrag()
    {
        var low = DragTableBuilder.FromBcCurve(Metadata(), DragTableId.G7, new[] { new BcAtMach(1.5, 0.250) });
        var high = DragTableBuilder.FromBcCurve(Metadata(), DragTableId.G7, new[] { new BcAtMach(1.5, 0.500) });

        high[10].DragCoefficient.Should().BeLessThan(low[10].DragCoefficient);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("zeroBc")]
    [InlineData("negativeBc")]
    [InlineData("duplicateMach")]
    [InlineData("negativeMach")]
    [InlineData("customBaseTable")]
    [InlineData("noName")]
    public void FromBcCurve_InvalidInput_ShouldThrowWithAReadableMessage(string scenario)
    {
        var metadata = Metadata();
        var table = DragTableId.G7;
        BcAtMach[] curve = Curve();

        switch (scenario)
        {
            case "empty": curve = Array.Empty<BcAtMach>(); break;
            case "zeroBc": curve = new[] { new BcAtMach(1.5, 0) }; break;
            case "negativeBc": curve = new[] { new BcAtMach(1.5, -0.3) }; break;
            case "duplicateMach": curve = new[] { new BcAtMach(1.5, 0.3), new BcAtMach(1.5, 0.4) }; break;
            case "negativeMach": curve = new[] { new BcAtMach(-1, 0.3) }; break;
            case "customBaseTable": table = DragTableId.GC; break;
            case "noName": metadata = Metadata(name: "  "); break;
        }

        var act = () => DragTableBuilder.FromBcCurve(metadata, table, curve);

        act.Should().Throw<ArgumentException>().Which.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void FromBcCurve_SavedFile_ShouldRoundTripAllMetadata()
    {
        // Requires BallisticCalculator 1.1.11.2, where Save/Open carry length and source.
        var table = DragTableBuilder.FromBcCurve(Metadata("308 168gr", "Litz"), DragTableId.G7, Curve());
        var path = Path.Combine(Path.GetTempPath(), $"builder-{Guid.NewGuid():N}.drg");

        try
        {
            table.Save(path);
            var reopened = DrgDragTable.Open(path);

            var ammo = reopened.Ammunition!;
            ammo.Name.Should().Be("308 168gr");
            ammo.Source.Should().Be("Litz");
            ammo.Ammunition.Weight.In(WeightUnit.Grain).Should().BeApproximately(220, 0.01);
            ammo.Ammunition.BulletDiameter!.Value.In(DistanceUnit.Inch).Should().BeApproximately(0.308, 1e-4);
            ammo.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.226, 1e-4);
            reopened.Count.Should().Be(table.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion

    #region From radar readings

    [Fact]
    public void FromRadarReadings_ShouldReturnCustomTableWithMetadata()
    {
        var table = DragTableBuilder.FromRadarReadings(Metadata("6.5 140gr", "LabRadar"), Readings());

        table.TableId.Should().Be(DragTableId.GC);
        table.Count.Should().BeGreaterThan(0);
        table.Ammunition!.Name.Should().Be("6.5 140gr");
        table.Ammunition.Source.Should().Be("LabRadar");
        table.Ammunition.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.226, 1e-4);
    }

    [Fact]
    public void FromRadarReadings_UnsortedReadings_ShouldBeAccepted()
    {
        var shuffled = new[] { Reading(200, 2923.9), Reading(0, 3078.8), Reading(300, 2847.2), Reading(100, 3001.2) };

        var table = DragTableBuilder.FromRadarReadings(Metadata(), shuffled);

        table.Count.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("tooFew")]
    [InlineData("risingVelocity")]
    [InlineData("duplicateDistance")]
    [InlineData("noWeight")]
    [InlineData("zeroWeight")]
    [InlineData("noDiameter")]
    [InlineData("zeroDiameter")]
    [InlineData("zeroVelocity")]
    public void FromRadarReadings_InvalidInput_ShouldThrowWithAReadableMessage(string scenario)
    {
        var metadata = Metadata();
        var readings = Readings();

        switch (scenario)
        {
            case "tooFew": readings = new[] { Reading(0, 3078.8), Reading(100, 3001.2) }; break;
            case "risingVelocity": readings = new[] { Reading(0, 3000), Reading(100, 3010), Reading(200, 2900) }; break;
            case "duplicateDistance": readings = new[] { Reading(0, 3078.8), Reading(100, 3001.2), Reading(100, 2990) }; break;
            case "noWeight": metadata = metadata with { Weight = null }; break;
            case "zeroWeight": metadata = metadata with { Weight = new Measurement<WeightUnit>(0, WeightUnit.Grain) }; break;
            case "noDiameter": metadata = metadata with { Diameter = null }; break;
            case "zeroDiameter": metadata = metadata with { Diameter = new Measurement<DistanceUnit>(0, DistanceUnit.Inch) }; break;
            case "zeroVelocity": readings = new[] { Reading(0, 3078.8), Reading(100, 3001.2), Reading(200, 0) }; break;
        }

        var act = () => DragTableBuilder.FromRadarReadings(metadata, readings);

        act.Should().Throw<ArgumentException>().Which.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void FromRadarReadings_ShouldUseTheSuppliedAtmosphere()
    {
        // Thinner air means less drag for the same velocity decay, so the recovered Cd must differ.
        var sea = DragTableBuilder.FromRadarReadings(Metadata(), Readings(), new Atmosphere());
        var high = DragTableBuilder.FromRadarReadings(Metadata(), Readings(),
            Atmosphere.CreateICAOAtmosphere(new Measurement<DistanceUnit>(10000, DistanceUnit.Foot)));

        high[0].DragCoefficient.Should().NotBeApproximately(sea[0].DragCoefficient, 1e-6);
    }

    #endregion
}
