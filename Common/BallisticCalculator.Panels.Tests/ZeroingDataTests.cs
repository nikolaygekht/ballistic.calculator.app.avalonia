using System;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests;

public class ZeroingDataTests
{
    [Fact]
    public void BxmlSerialization_FullZeroingData_ShouldRoundTrip()
    {
        var zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            Ammunition = new Ammunition(
                weight: new Measurement<WeightUnit>(150, WeightUnit.Grain),
                ballisticCoefficient: new BallisticCoefficient(0.415, DragTableId.G1),
                muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond)),
            Atmosphere = new Atmosphere(
                new Measurement<DistanceUnit>(300, DistanceUnit.Meter),
                new Measurement<PressureUnit>(740, PressureUnit.MillimetersOfMercury),
                new Measurement<TemperatureUnit>(25, TemperatureUnit.Celsius),
                0.60),
            VerticalOffset = new Measurement<DistanceUnit>(20, DistanceUnit.Millimeter),
            HorizontalOffset = new Measurement<DistanceUnit>(10, DistanceUnit.Millimeter),
            Wind = new Wind()
            {
                Velocity = new Measurement<VelocityUnit>(4, VelocityUnit.MetersPerSecond),
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            },
            ShotAngle = new Measurement<AngularUnit>(3, AngularUnit.Mil),
        };

        var element = new BallisticXmlSerializer().Serialize(zeroing);
        var result = new BallisticXmlDeserializer().Deserialize<ZeroingData>(element);

        result.Should().NotBeNull();
        result!.Distance!.Value.In(DistanceUnit.Meter).Should().BeApproximately(100, 0.5);
        result.Ammunition!.Weight.In(WeightUnit.Grain).Should().BeApproximately(150, 0.5);
        result.Atmosphere!.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(25, 0.5);
        result.VerticalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(20, 0.5);
        result.HorizontalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(10, 0.5);
        result.Wind!.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(4, 0.5);
        result.ShotAngle!.Value.In(AngularUnit.Mil).Should().BeApproximately(3, 0.01);
    }

    [Fact]
    public void BxmlSerialization_MinimalZeroingData_ShouldRoundTrip()
    {
        var zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
        };

        var element = new BallisticXmlSerializer().Serialize(zeroing);
        var result = new BallisticXmlDeserializer().Deserialize<ZeroingData>(element);

        result.Should().NotBeNull();
        result!.Distance!.Value.In(DistanceUnit.Yard).Should().BeApproximately(100, 0.5);
        result.Ammunition.Should().BeNull();
        result.Atmosphere.Should().BeNull();
        result.Wind.Should().BeNull();
        result.ShotAngle.Should().BeNull();
        result.VerticalOffset.Should().BeNull();
        result.HorizontalOffset.Should().BeNull();
    }

    // The two assertions below mirror the ZeroingData -> library conversion done at the calc sites
    // (ShotCalculator / ReticlePanel): they prove the zeroing wind and shot angle actually reach
    // CalculateZeroParameters and change the computed zero.

    [Fact]
    public void ZeroingWind_ShouldProduceHorizontalZeroCorrection()
    {
        var calc = new TrajectoryCalculator();
        var (ammo, atmosphere, rifle, zeroParams) = CreateSetup();

        var wind = new[]
        {
            new Wind(new Measurement<VelocityUnit>(10, VelocityUnit.MilesPerHour),
                     new Measurement<AngularUnit>(90, AngularUnit.Degree)),
        };
        var withWind = calc.CalculateZeroParameters(ammo, atmosphere, rifle, zeroParams, wind: wind);

        withWind.ZeroWindageAdjustment.Should().NotBeNull();
        Math.Abs(withWind.ZeroWindageAdjustment!.Value.In(AngularUnit.Mil)).Should().BeGreaterThan(0.001);
    }

    [Fact]
    public void ZeroingShotAngle_ShouldChangeZeroDropAdjustment()
    {
        var calc = new TrajectoryCalculator();
        var (ammo, atmosphere, rifle, zeroParams) = CreateSetup();

        var flat = calc.CalculateZeroParameters(ammo, atmosphere, rifle, zeroParams);
        var inclinedShot = new ShotParameters { ShotAngle = new Measurement<AngularUnit>(45, AngularUnit.Degree) };
        var inclined = calc.CalculateZeroParameters(ammo, atmosphere, rifle, zeroParams, shot: inclinedShot);

        inclined.ZeroDropAdjustment.In(AngularUnit.Mil)
            .Should().NotBe(flat.ZeroDropAdjustment.In(AngularUnit.Mil));
    }

    private static (Ammunition, Atmosphere, Rifle, ZeroingParameters) CreateSetup()
    {
        var ammo = new Ammunition(
            weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
            ballisticCoefficient: new BallisticCoefficient(0.223, DragTableId.G7),
            muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));
        var atmosphere = new Atmosphere();
        var sight = new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
            Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO);
        var zeroParams = new ZeroingParameters(new Measurement<DistanceUnit>(100, DistanceUnit.Yard), null, null);
        var rifle = new Rifle(sight, zeroParams, null);
        return (ammo, atmosphere, rifle, zeroParams);
    }
}
