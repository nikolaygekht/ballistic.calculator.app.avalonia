using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

public class CustomDragTableLoaderTests
{
    /// <summary>Builds a real .drg file on disk and returns its path (caller deletes).</summary>
    private static string CreateTempDrg()
    {
        var entry = new AmmunitionLibraryEntry
        {
            Name = "test",
            Ammunition = new Ammunition(
                weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
                ballisticCoefficient: new BallisticCoefficient(1.0, DragTableId.GC),
                muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
                bulletDiameter: new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
                bulletLength: new Measurement<DistanceUnit>(1.2, DistanceUnit.Inch)),
        };
        var curve = new List<BcAtMach>
        {
            new(0.0, 0.243), new(1.0, 0.243), new(2.0, 0.243), new(3.0, 0.243),
        };
        var table = DrgDragTableFactory.Build(entry, DragTableId.G7, curve);
        var path = Path.Combine(Path.GetTempPath(), $"drg_{Guid.NewGuid():N}.drg");
        table.Save(path);
        return path;
    }

    [Fact]
    public void ForAmmunition_StandardTable_ReturnsNull()
    {
        var ammo = new Ammunition(
            new Measurement<WeightUnit>(168, WeightUnit.Grain),
            new BallisticCoefficient(0.243, DragTableId.G7),
            new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        CustomDragTableLoader.ForAmmunition(ammo).Should().BeNull();
    }

    [Fact]
    public void ForAmmunition_GcWithFile_ReturnsTable()
    {
        var path = CreateTempDrg();
        try
        {
            var ammo = new Ammunition(
                new Measurement<WeightUnit>(168, WeightUnit.Grain),
                new BallisticCoefficient(1.0, DragTableId.GC),
                new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond))
            {
                CustomTableFileName = path,
            };

            CustomDragTableLoader.ForAmmunition(ammo).Should().NotBeNull();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ForAmmunition_GcWithMissingFile_ReturnsNull()
    {
        var ammo = new Ammunition(
            new Measurement<WeightUnit>(168, WeightUnit.Grain),
            new BallisticCoefficient(1.0, DragTableId.GC),
            new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond))
        {
            CustomTableFileName = "does-not-exist-anywhere.drg",
        };

        CustomDragTableLoader.ForAmmunition(ammo).Should().BeNull();
    }

    [Fact]
    public void ResolvePath_NullOrEmpty_ReturnsNull()
    {
        CustomDragTableLoader.ResolvePath(null).Should().BeNull();
        CustomDragTableLoader.ResolvePath("").Should().BeNull();
    }

    [Fact]
    public void ShotTrajectoryCalculator_GcAmmunition_ProducesTrajectory()
    {
        // A GC coefficient with no supplied drag table throws in the library; this proves the
        // calculator loads and threads the custom .drg through both zero and shot calls.
        var path = CreateTempDrg();
        try
        {
            var ammo = new Ammunition(
                weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
                ballisticCoefficient: new BallisticCoefficient(1.0, DragTableId.GC),
                muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
                bulletDiameter: new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
                bulletLength: new Measurement<DistanceUnit>(1.2, DistanceUnit.Inch))
            {
                CustomTableFileName = path,
            };

            var shotData = new ShotData
            {
                Ammunition = new AmmunitionLibraryEntry { Name = "t", Ammunition = ammo },
                Weapon = new Rifle(
                    new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                        Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
                    new ZeroingParameters(new Measurement<DistanceUnit>(100, DistanceUnit.Yard), null, null)),
                Atmosphere = new Atmosphere(),
                Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Yard) },
                Parameters = new ShotParameters
                {
                    MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Yard),
                    Step = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
                },
            };

            var trajectory = ShotTrajectoryCalculator.Calculate(shotData);

            trajectory.Should().NotBeNull();
            trajectory!.Length.Should().BeGreaterThan(1);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
