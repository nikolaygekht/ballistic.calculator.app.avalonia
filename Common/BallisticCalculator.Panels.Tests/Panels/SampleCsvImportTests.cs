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
/// End-to-end over the real sample exports in <c>TestData/</c>: read the file, build a drag table, save it
/// and read it back. These files are the reason the reader tolerates `;` separators, CRLF, a header line,
/// inline units (<c>0yd</c>, <c>3078.800ft/s</c>) and a missing final end-of-line.
/// </summary>
public class SampleCsvImportTests
{
    private static string Sample(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", name);
        File.Exists(path).Should().BeTrue($"{name} must be copied to the output directory");
        return path;
    }

    private static string? BcRow(string a, string b)
    {
        if (!MeasurementTextParser.TryParseDouble(a, out _)) return "the Mach value is not a number";
        if (!MeasurementTextParser.TryParseBc(b, DragTableId.G7, out _)) return "the ballistic coefficient is not valid";
        return null;
    }

    // Units are required on import: a bare number would be guesswork (see the panels).
    private static string? VelocityRow(string a, string b)
    {
        if (!MeasurementTextParser.TryParseDistance(a, null, out _)) return "no unit given for the distance";
        if (!MeasurementTextParser.TryParseVelocity(b, null, out _)) return "no unit given for the velocity";
        return null;
    }

    private static DrgMetadata Metadata(string name) =>
        new(name, "imported",
            new Measurement<WeightUnit>(168, WeightUnit.Grain),
            new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(1.215, DistanceUnit.Inch));

    [Theory]
    [InlineData("mbc1.csv", 5, DragTableId.G7)]
    [InlineData("mbc2.csv", 8, DragTableId.G1)]
    public void BcSample_ShouldImportWholeAndCarryItsOwnDragTable(string file, int expectedKnots, DragTableId expectedTable)
    {
        var ok = CsvTextTableReader.TryReadFile(Sample(file), BcRow, out var table, out var error);

        ok.Should().BeTrue(error);
        table.Separator.Should().Be(';');
        table.HeaderFirst.Should().Be("mach");
        table.HeaderSecond.Should().Be("bc");
        table.Rows.Should().HaveCount(expectedKnots);

        // Every BC in these files names its own table, so the editor can set the base table from the file.
        var tables = table.Rows.Select(r =>
        {
            MeasurementTextParser.TryParseBc(r.Second, DragTableId.G7, out var bc).Should().BeTrue();
            return bc.Table;
        }).Distinct().ToArray();

        tables.Should().Equal(new[] { expectedTable });
    }

    [Theory]
    [InlineData("velocity1.csv")]
    [InlineData("velocity2.csv")]
    public void VelocitySample_ShouldImportWhole(string file)
    {
        var ok = CsvTextTableReader.TryReadFile(Sample(file), VelocityRow, out var table, out var error);

        ok.Should().BeTrue(error);
        table.HeaderFirst.Should().Be("distance");
        table.Rows.Should().HaveCount(16);

        MeasurementTextParser.TryParseDistance(table.Rows[0].First, DistanceUnit.Yard, out var first).Should().BeTrue();
        first.In(DistanceUnit.Yard).Should().Be(0);        // radar data starts at the muzzle

        MeasurementTextParser.TryParseDistance(table.Rows[15].First, DistanceUnit.Yard, out var last).Should().BeTrue();
        last.In(DistanceUnit.Yard).Should().Be(1500);      // last line has no trailing end-of-line
    }

    [Fact]
    public void BcSample_ShouldBuildAndRoundTripADragTable()
    {
        CsvTextTableReader.TryReadFile(Sample("mbc1.csv"), BcRow, out var csv, out _).Should().BeTrue();

        var knots = csv.Rows.Select(r =>
        {
            MeasurementTextParser.TryParseDouble(r.First, out var mach);
            MeasurementTextParser.TryParseBc(r.Second, DragTableId.G7, out var bc);
            return new BcAtMach(mach, bc.Value);
        }).ToArray();

        var drg = DragTableBuilder.FromBcCurve(Metadata("mbc1 sample"), DragTableId.G7, knots);

        drg.TableId.Should().Be(DragTableId.GC);
        drg.Count.Should().BeGreaterThan(0);
        RoundTrip(drg, "mbc1 sample");
    }

    [Fact]
    public void VelocitySample_ShouldBuildAndRoundTripADragTable()
    {
        CsvTextTableReader.TryReadFile(Sample("velocity1.csv"), VelocityRow, out var csv, out _).Should().BeTrue();

        var readings = csv.Rows.Select(r =>
        {
            MeasurementTextParser.TryParseDistance(r.First, DistanceUnit.Yard, out var d);
            MeasurementTextParser.TryParseVelocity(r.Second, VelocityUnit.FeetPerSecond, out var v);
            return new RadarReading(d, v);
        }).ToArray();

        var drg = DragTableBuilder.FromRadarReadings(Metadata("velocity1 sample"), readings);

        drg.TableId.Should().Be(DragTableId.GC);
        drg.Count.Should().BeGreaterThan(0);
        RoundTrip(drg, "velocity1 sample");
    }

    private static void RoundTrip(DrgDragTable drg, string expectedName)
    {
        var path = Path.Combine(Path.GetTempPath(), $"sample-{Guid.NewGuid():N}.drg");
        try
        {
            drg.Save(path);
            var back = DrgDragTable.Open(path);

            back.Count.Should().Be(drg.Count);
            back.Ammunition!.Name.Should().Be(expectedName);
            back.Ammunition.Source.Should().Be("imported");
            back.Ammunition.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.215, 1e-4);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
