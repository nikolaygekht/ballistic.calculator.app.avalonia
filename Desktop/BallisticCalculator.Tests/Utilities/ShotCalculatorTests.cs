using AwesomeAssertions;
using BallisticCalculator.Types;
using BallisticCalculator.Utilities;
using Gehtsoft.Measurements;
using Xunit;

namespace BallisticCalculator.Tests.Utilities;

/// <summary>
/// Cover for the last line of defense between the UI and the engine. The engine raises meaningful
/// exceptions for uncomputable shots (findings F-1, F-1b, F-1c); before this, nothing on the path caught
/// them and <c>async void</c> handlers turned them into unhandled UI-thread exceptions. These tests prove
/// each one now comes back as a value, so the caller can show it.
/// </summary>
/// <remarks>
/// This is deliberately NOT a substitute for validating the same states in the dialog — a caught
/// exception is still a dead end for the user. It is the net under the specific checks.
/// </remarks>
public class ShotCalculatorTests
{
    #region The three known uncomputable shots come back as errors, not crashes

    /// <summary>
    /// F-1: a form factor is turned into a coefficient through the bullet's sectional density, which needs
    /// the diameter. Every <c>.drg</c> shot is a form-factor shot, so this is not an exotic combination.
    /// </summary>
    [Fact]
    public void TryCalculate_FormFactorWithoutDiameter_ReturnsTheError()
    {
        // Arrange
        var data = BuildShotData();
        data.Ammunition!.Ammunition!.BallisticCoefficient = new BallisticCoefficient(
            0.5, DragTableId.G7, BallisticCoefficientValueType.FormFactor);
        data.Ammunition.Ammunition.BulletDiameter = null;

        // Act
        var ok = ShotCalculator.TryCalculate(data, MeasurementSystem.Imperial, out var trajectory, out var error);

        // Assert
        ok.Should().BeFalse();
        trajectory.Should().BeEmpty();
        error.Should().BeOfType<ArgumentException>();
        error!.Message.Should().Contain("diameter");
    }

    /// <summary>
    /// F-1b: a GC ("custom") coefficient has no built-in drag curve, so the engine needs the
    /// <c>.drg</c> file. The realistic route is opening a shared file that references a table you
    /// do not have.
    /// </summary>
    [Fact]
    public void TryCalculate_CustomCoefficientWithNoResolvableTable_ReturnsTheError()
    {
        // Arrange
        var data = BuildShotData();
        data.Ammunition!.Ammunition!.BallisticCoefficient = new BallisticCoefficient(1, DragTableId.GC);
        data.Ammunition.Ammunition.CustomTableFileName = "no-such-table-4e9c1f.drg";

        // Act
        var ok = ShotCalculator.TryCalculate(data, MeasurementSystem.Imperial, out var trajectory, out var error);

        // Assert
        ok.Should().BeFalse();
        trajectory.Should().BeEmpty();
        error.Should().BeOfType<ArgumentNullException>();
    }

    /// <summary>
    /// F-1c: a zero further out than the load can carry. Since BallisticCalculator 1.1.13 the engine names
    /// this failure with its own exception type, which is what lets the UI say something useful instead of
    /// showing a stack trace.
    /// </summary>
    [Fact]
    public void TryCalculate_ZeroDistanceTheBulletCannotReach_ReturnsTheNamedError()
    {
        // Arrange — a 230gr .45 ACP asked for a 2,000 m zero
        var data = BuildShotData();
        data.Ammunition!.Ammunition = new Ammunition(
            weight: new Measurement<WeightUnit>(230, WeightUnit.Grain),
            ballisticCoefficient: new BallisticCoefficient(0.195, DragTableId.G1),
            muzzleVelocity: new Measurement<VelocityUnit>(850, VelocityUnit.FeetPerSecond));
        data.Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(2000, DistanceUnit.Meter) };

        // Act
        var ok = ShotCalculator.TryCalculate(data, MeasurementSystem.Metric, out var trajectory, out var error);

        // Assert
        ok.Should().BeFalse();
        trajectory.Should().BeEmpty();
        error.Should().BeOfType<ZeroRangeCantBeReachedException>();
        ShotCalculator.Explain(error).Should().NotBeNull("this is the user's to fix, not a crash");
        ShotCalculator.Explain(error).Should().Contain("zero distance");
    }

    #endregion

    #region Which failures get a plain message instead of a stack trace

    [Fact]
    public void Explain_ZeroRangeCantBeReached_SaysWhatToChange()
    {
        // Act
        var explanation = ShotCalculator.Explain(new ZeroRangeCantBeReachedException());

        // Assert
        explanation.Should().NotBeNull();
        explanation.Should().Contain("zero");
        explanation.Should().Contain("muzzle velocity");
    }

    [Fact]
    public void Explain_TrajectoryCannotBeCalculated_PointsAtTheAmmunition()
    {
        // Act
        var explanation = ShotCalculator.Explain(new TrajectoryCannotBeCalculatedException());

        // Assert
        explanation.Should().NotBeNull();
        explanation.Should().Contain("Ammunition tab");
    }

    /// <summary>
    /// Anything the engine has not named is a bug rather than a bad input, and must keep its stack trace —
    /// explaining it away in friendly words would hide the thing worth reporting.
    /// </summary>
    [Fact]
    public void Explain_AnythingElse_IsNotExplainedAway()
    {
        ShotCalculator.Explain(new ArgumentException("If form-factor is used, the bullet diameter must be set"))
            .Should().BeNull();
        ShotCalculator.Explain(new ArgumentNullException("dragTable")).Should().BeNull();
        ShotCalculator.Explain(new InvalidOperationException("something else entirely")).Should().BeNull();
        ShotCalculator.Explain(null).Should().BeNull();
    }

    #endregion

    #region A computable shot is unaffected

    [Fact]
    public void TryCalculate_ComputableShot_ReturnsTheTrajectory()
    {
        // Arrange
        var data = BuildShotData();

        // Act
        var ok = ShotCalculator.TryCalculate(data, MeasurementSystem.Imperial, out var trajectory, out var error);

        // Assert
        ok.Should().BeTrue();
        error.Should().BeNull();
        trajectory.Should().NotBeEmpty();
        trajectory.Should().NotContainNulls();
    }

    /// <summary>
    /// The defaults <see cref="ShotCalculator.Calculate"/> applies (atmosphere, rifle, parameters) must
    /// still be applied on the guarded path — it is the same call, not a reduced one.
    /// </summary>
    [Fact]
    public void TryCalculate_MissingRifleAndParameters_StillAppliesDefaults()
    {
        // Arrange
        var data = BuildShotData();
        data.Weapon = null;
        data.Parameters = null;
        data.Atmosphere = null;
        data.Zeroing = null;

        // Act
        var ok = ShotCalculator.TryCalculate(data, MeasurementSystem.Imperial, out var trajectory, out var error);

        // Assert
        ok.Should().BeTrue($"defaults should fill the gaps, but: {error?.Message}");
        data.Weapon.Should().NotBeNull();
        data.Parameters.Should().NotBeNull();
        data.Atmosphere.Should().NotBeNull();
        trajectory.Should().NotBeEmpty();
    }

    #endregion

    private static ShotData BuildShotData()
    {
        var ammo = new Ammunition(
            weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
            ballisticCoefficient: new BallisticCoefficient(0.223, DragTableId.G7),
            muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        var rifle = new Rifle(
            new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
            new ZeroingParameters(new Measurement<DistanceUnit>(100, DistanceUnit.Yard), null, null));

        return new ShotData
        {
            Ammunition = new AmmunitionLibraryEntry { Name = "168gr .308", Ammunition = ammo },
            Weapon = rifle,
            Atmosphere = new Atmosphere(),
            Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Yard) },
            Parameters = new ShotParameters
            {
                MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Yard),
                Step = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
            },
        };
    }
}
