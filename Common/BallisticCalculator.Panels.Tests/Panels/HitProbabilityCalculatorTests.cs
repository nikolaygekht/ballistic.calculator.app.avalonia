using System;
using System.Linq;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// The estimate is Monte Carlo, so every test that compares two runs pins the seed. What can be asserted is
/// direction — a bigger target or a tighter group must raise the probability — plus the refusals.
/// </summary>
public class HitProbabilityCalculatorTests
{
    private static ShotData Shot(double maxYards = 600)
    {
        var ammo = new Ammunition(
            new Measurement<WeightUnit>(168, WeightUnit.Grain),
            new BallisticCoefficient(0.223, DragTableId.G7),
            new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        return new ShotData
        {
            Ammunition = new AmmunitionLibraryEntry { Ammunition = ammo, Name = "308win 168gr" },
            Weapon = new Rifle(
                new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                          Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
                new ZeroingParameters(new Measurement<DistanceUnit>(100, DistanceUnit.Yard), null, null)),
            Atmosphere = new Atmosphere(),
            Winds = new[]
            {
                new Wind(new Measurement<VelocityUnit>(10, VelocityUnit.MilesPerHour),
                         new Measurement<AngularUnit>(90, AngularUnit.Degree)),
            },
            Parameters = new ShotParameters
            {
                Step = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
                MaximumDistance = new Measurement<DistanceUnit>(maxYards, DistanceUnit.Yard),
            },
        };
    }

    private static HitProbabilityInputs Inputs(double targetInches = 20, double groupMoa = 1.0,
                                               double h = 1, double v = 1, int shots = 2000, int? seed = 1) =>
        new()
        {
            Distance = new Measurement<DistanceUnit>(600, DistanceUnit.Yard),
            TargetSize = new Measurement<DistanceUnit>(targetInches, DistanceUnit.Inch),
            GroupSize = new Measurement<AngularUnit>(groupMoa, AngularUnit.MOA),
            HorizontalSpread = h,
            VerticalSpread = v,
            RangeErrorPercent = 2,
            WindErrorPercent = 30,
            MuzzleVelocityDeviationPercent = 0.7,
            Shots = shots,
            Seed = seed,
        };

    #region Estimating

    [Fact]
    public void Estimate_ForARealisticShot_ShouldReturnAProbabilityAndShotCounts()
    {
        var result = HitProbabilityCalculator.Estimate(Shot(), Inputs());

        result.HitProbability.Should().BeInRange(0, 1);
        result.Impacts.Should().HaveCount(2000);
        result.ShotsFor50Percent.Should().NotBeNull().And.BePositive();
        result.ShotsFor98Percent.Should().BeGreaterThan(result.ShotsFor50Percent!.Value);
    }

    [Fact]
    public void Estimate_WithTheSameSeed_ShouldRepeatExactly()
    {
        var first = HitProbabilityCalculator.Estimate(Shot(), Inputs());
        var second = HitProbabilityCalculator.Estimate(Shot(), Inputs());

        second.HitProbability.Should().Be(first.HitProbability);
    }

    [Fact]
    public void Estimate_WithABiggerTarget_ShouldBeMoreLikelyToHit()
    {
        var small = HitProbabilityCalculator.Estimate(Shot(), Inputs(targetInches: 10));
        var large = HitProbabilityCalculator.Estimate(Shot(), Inputs(targetInches: 40));

        large.HitProbability.Should().BeGreaterThan(small.HitProbability);
    }

    [Fact]
    public void Estimate_WithATighterGroup_ShouldBeMoreLikelyToHit()
    {
        var loose = HitProbabilityCalculator.Estimate(Shot(), Inputs(groupMoa: 4));
        var tight = HitProbabilityCalculator.Estimate(Shot(), Inputs(groupMoa: 0.5));

        tight.HitProbability.Should().BeGreaterThan(loose.HitProbability);
    }

    [Fact]
    public void Estimate_FromAnUnsupportedPosition_ShouldBeLessLikelyToHit()
    {
        // Arrange & Act: standing multiplies the aim scatter by 5 horizontally and 4 vertically
        var supported = HitProbabilityCalculator.Estimate(Shot(), Inputs(h: 1, v: 1));
        var standing = HitProbabilityCalculator.Estimate(Shot(), Inputs(h: 5, v: 4));

        // Assert
        standing.HitProbability.Should().BeLessThan(supported.HitProbability);
    }

    [Fact]
    public void Estimate_ShouldReportTheMeanAndNinetiethPercentileMiss()
    {
        var result = HitProbabilityCalculator.Estimate(Shot(), Inputs());

        result.MeanRadialMiss.In(DistanceUnit.Inch).Should().BePositive();
        result.NinetiethPercentileMiss.In(DistanceUnit.Inch)
              .Should().BeGreaterThan(result.MeanRadialMiss.In(DistanceUnit.Inch));
    }

    [Fact]
    public void Estimate_AtAShorterDistance_ShouldBeMoreLikelyToHit()
    {
        // Arrange: the target distance is the dialog's input, independent of the shot's own maximum
        var far = HitProbabilityCalculator.Estimate(Shot(), Inputs() with
        {
            Distance = new Measurement<DistanceUnit>(900, DistanceUnit.Yard),
        });
        var near = HitProbabilityCalculator.Estimate(Shot(), Inputs() with
        {
            Distance = new Measurement<DistanceUnit>(200, DistanceUnit.Yard),
        });

        near.HitProbability.Should().BeGreaterThan(far.HitProbability);
    }

    [Fact]
    public void Estimate_WithNoSeed_ShouldStillProduceAResult()
    {
        var result = HitProbabilityCalculator.Estimate(Shot(), Inputs(seed: null));

        result.HitProbability.Should().BeInRange(0, 1);
    }

    #endregion

    #region Refused input

    [Fact]
    public void Estimate_WithIncompleteShotData_ShouldSayWhatIsMissing()
    {
        var act = () => HitProbabilityCalculator.Estimate(new ShotData(), Inputs());

        act.Should().Throw<ArgumentException>().WithMessage("*ammunition*");
    }

    [Fact]
    public void Estimate_WithoutATargetSize_ShouldSayWhatIsMissing()
    {
        var act = () => HitProbabilityCalculator.Estimate(Shot(), Inputs() with { TargetSize = null });

        act.Should().Throw<ArgumentException>().WithMessage("*target size*");
    }

    [Fact]
    public void Estimate_WithoutADistance_ShouldSayWhatIsMissing()
    {
        var act = () => HitProbabilityCalculator.Estimate(Shot(), Inputs() with { Distance = null });

        act.Should().Throw<ArgumentException>().WithMessage("*distance*");
    }

    [Fact]
    public void Estimate_WithoutAGroupSize_ShouldSayWhatIsMissing()
    {
        var act = () => HitProbabilityCalculator.Estimate(Shot(), Inputs() with { GroupSize = null });

        act.Should().Throw<ArgumentException>().WithMessage("*group*");
    }

    [Theory]
    [InlineData(999)]
    [InlineData(50_001)]
    public void Estimate_WithAShotCountOutOfRange_ShouldSayTheRange(int shots)
    {
        var act = () => HitProbabilityCalculator.Estimate(Shot(), Inputs(shots: shots));

        act.Should().Throw<ArgumentException>().WithMessage("*1000*50000*");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(50_000)]
    public void Estimate_AtTheShotCountLimits_ShouldBeAccepted(int shots)
    {
        var result = HitProbabilityCalculator.Estimate(Shot(), Inputs(shots: shots));

        result.Impacts.Should().HaveCount(shots);
    }

    [Fact]
    public void Estimate_WithANegativeErrorPercent_ShouldSayWhatIsWrong()
    {
        var act = () => HitProbabilityCalculator.Estimate(Shot(), Inputs() with { WindErrorPercent = -1 });

        act.Should().Throw<ArgumentException>().WithMessage("*negative*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Estimate_WithANonPositiveSpread_ShouldSayWhatIsWrong(double spread)
    {
        var act = () => HitProbabilityCalculator.Estimate(Shot(), Inputs(h: spread));

        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    #endregion

    #region Position presets

    [Fact]
    public void ShootingPositions_ShouldCarryTheLibrarysDocumentedMultipliers()
    {
        var positions = HitProbabilityCalculator.ShootingPositions;

        positions.Should().HaveCount(5);
        positions[0].Name.Should().Be("Supported");
        positions[0].Horizontal.Should().Be(1);
        positions[0].Vertical.Should().Be(1);
        positions.Should().ContainSingle(p => p.Name == "Standing" && p.Horizontal == 5 && p.Vertical == 4);
        positions.Should().ContainSingle(p => p.Name == "Kneeling" && p.Horizontal == 4 && p.Vertical == 3);
        positions.Should().ContainSingle(p => p.Name == "Prone" && p.Horizontal == 2 && p.Vertical == 2);
        positions[^1].Name.Should().Be("Custom", "Custom is chosen by editing a multiplier, so it comes last");
        positions[^1].IsCustom.Should().BeTrue();
    }

    [Fact]
    public void ShootingPositions_ForKnownMultipliers_ShouldMatchThePreset()
    {
        HitProbabilityCalculator.PositionFor(2, 2)!.Name.Should().Be("Prone");
        HitProbabilityCalculator.PositionFor(5, 4)!.Name.Should().Be("Standing");
        HitProbabilityCalculator.PositionFor(3, 3).Should().BeNull("no preset uses 3/3");
    }

    #endregion

    #region Plotting helper

    [Fact]
    public void SampleImpacts_WhenThereAreMoreThanTheLimit_ShouldThinThemEvenly()
    {
        // Arrange
        var result = HitProbabilityCalculator.Estimate(Shot(), Inputs(shots: 50_000));

        // Act
        var sample = HitProbabilityCalculator.SampleImpacts(result.Impacts, 2000);

        // Assert
        sample.Should().HaveCount(2000);
        sample[0].Horizontal.Should().Be(result.Impacts[0].Horizontal, "thinning starts at the first shot");
    }

    [Fact]
    public void SampleImpacts_WhenThereAreFewerThanTheLimit_ShouldReturnThemAll()
    {
        var result = HitProbabilityCalculator.Estimate(Shot(), Inputs(shots: 1000));

        var sample = HitProbabilityCalculator.SampleImpacts(result.Impacts, 2000);

        sample.Should().HaveCount(1000);
    }

    #endregion
}
