using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System.IO;
using System.Linq;
using Xunit;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// Cover for <see cref="AmmoPanel.Problems"/> — the ammunition states the engine refuses, reported as
/// messages before the calculation is attempted (findings F-1 and F-1b). Two of them build a perfectly
/// good <see cref="Ammunition"/> object and only fail inside the engine, which is why "is the field
/// filled in" is not enough.
/// </summary>
public class AmmoPanelProblemsTests
{
    #region A complete, computable ammunition has no problems

    [AvaloniaFact]
    public void Problems_CompleteAmmunition_ShouldBeEmpty()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);

        // Act & Assert
        panel.Problems().Should().BeEmpty();
    }

    #endregion

    #region Missing fields are named individually, not as "ammunition is required"

    [AvaloniaFact]
    public void Problems_EmptyPanel_ShouldNameEveryMissingField()
    {
        // Arrange
        var panel = new AmmoPanel();

        // Act
        var problems = panel.Problems();

        // Assert — the generic "Ammunition data is required" said none of this
        problems.Should().HaveCount(3);
        problems.Should().Contain(p => p.Contains("weight"));
        problems.Should().Contain(p => p.Contains("Ballistic coefficient"));
        problems.Should().Contain(p => p.Contains("Muzzle velocity"));
    }

    [AvaloniaFact]
    public void Problems_OnlyMuzzleVelocityMissing_ShouldNameOnlyThat()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);
        panel.MuzzleVelocityControl.Value = null;

        // Act
        var problems = panel.Problems();

        // Assert
        problems.Should().ContainSingle().Which.Should().Contain("Muzzle velocity");
    }

    #endregion

    #region Zero and negative values, which the solver cannot use either

    /// <summary>
    /// A field that is present but zero passes "is it filled in" and then fails inside the engine as
    /// <c>TrajectoryCannotBeCalculatedException</c> (BallisticCalculator 1.1.13). Naming the field is cheaper
    /// than explaining the exception afterwards.
    /// </summary>
    [AvaloniaTheory]
    [InlineData(0.0, 168.0, 2700.0, "Ballistic coefficient must be greater than zero.")]
    [InlineData(0.223, 0.0, 2700.0, "Bullet weight must be greater than zero.")]
    [InlineData(0.223, 168.0, 0.0, "Muzzle velocity must be greater than zero.")]
    public void Problems_ZeroValue_NamesTheField(double bc, double weight, double velocity, string expected)
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.WeightControl.SetValue(new Measurement<WeightUnit>(weight, WeightUnit.Grain));
        panel.BCControl.Value = new BallisticCoefficient(bc, DragTableId.G7);
        panel.MuzzleVelocityControl.SetValue(new Measurement<VelocityUnit>(velocity, VelocityUnit.FeetPerSecond));

        // Act & Assert
        panel.Problems().Should().Contain(expected);
    }

    #endregion

    #region F-1: a form factor needs the bullet diameter

    /// <summary>
    /// The engine turns a form factor into a coefficient through the bullet's sectional density, so it
    /// throws without a diameter. Every <c>.drg</c> shot is a form-factor shot.
    /// </summary>
    [AvaloniaFact]
    public void Problems_FormFactorWithoutDiameter_ShouldReportTheDiameter()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);
        panel.FormFactorCheckBox.IsChecked = true;
        panel.BulletDiameterControl.Value = null;

        // Act
        var problems = panel.Problems();

        // Assert
        problems.Should().ContainSingle();
        problems[0].Should().Contain("diameter");
        problems[0].Should().Contain("form factor");
    }

    [AvaloniaFact]
    public void Problems_FormFactorWithDiameter_ShouldBeEmpty()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);
        panel.FormFactorCheckBox.IsChecked = true;
        panel.BulletDiameterControl.SetValue(new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch));

        // Act & Assert
        panel.Problems().Should().BeEmpty();
    }

    /// <summary>A zero diameter is as unusable as no diameter — the engine rejects both.</summary>
    [AvaloniaFact]
    public void Problems_FormFactorWithZeroDiameter_ShouldReportTheDiameter()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);
        panel.FormFactorCheckBox.IsChecked = true;
        panel.BulletDiameterControl.SetValue(new Measurement<DistanceUnit>(0, DistanceUnit.Inch));

        // Act & Assert
        panel.Problems().Should().ContainSingle().Which.Should().Contain("diameter");
    }

    #endregion

    #region F-1b: a custom (GC) coefficient needs a .drg that can be found

    [AvaloniaFact]
    public void Problems_CustomCoefficientWithNoTable_ShouldReportTheMissingTable()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);
        panel.BulletDiameterControl.SetValue(new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch));
        panel.BCControl.Value = new BallisticCoefficient(1, DragTableId.GC);

        // Act
        var problems = panel.Problems();

        // Assert
        problems.Should().ContainSingle().Which.Should().Contain("drag table");
    }

    [AvaloniaFact]
    public void Problems_CustomCoefficientWithUnresolvableFileName_ShouldNameTheFile()
    {
        // Arrange
        var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
        FillValidAmmo(panel);
        panel.BulletDiameterControl.SetValue(new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch));
        panel.BCControl.Value = new BallisticCoefficient(1, DragTableId.GC);
        panel.Ammunition = new Ammunition(
            weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
            ballisticCoefficient: new BallisticCoefficient(1, DragTableId.GC, BallisticCoefficientValueType.FormFactor),
            muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
            bulletDiameter: new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch))
        {
            CustomTableFileName = "gone-missing-8ab31c.drg",
        };

        // Act
        var problems = panel.Problems();

        // Assert
        problems.Should().ContainSingle();
        problems[0].Should().Contain("gone-missing-8ab31c.drg");
    }

    [AvaloniaFact]
    public void Problems_CustomCoefficientWithAResolvableTable_ShouldBeEmpty()
    {
        // Arrange — a real .drg on disk, referenced by full path
        var path = WriteDrg();
        try
        {
            var panel = new AmmoPanel { MeasurementSystem = MeasurementSystem.Imperial };
            panel.Ammunition = new Ammunition(
                weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
                ballisticCoefficient: new BallisticCoefficient(1, DragTableId.GC, BallisticCoefficientValueType.FormFactor),
                muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
                bulletDiameter: new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch))
            {
                CustomTableFileName = path,
            };

            // Act & Assert
            panel.Problems().Should().BeEmpty();
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion

    private static void FillValidAmmo(AmmoPanel panel)
    {
        panel.WeightControl.SetValue(new Measurement<WeightUnit>(168, WeightUnit.Grain));
        panel.BCControl.Value = new BallisticCoefficient(0.223, DragTableId.G7);
        panel.MuzzleVelocityControl.SetValue(new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));
    }

    /// <summary>A minimal but real <c>.drg</c>: the header plus two data rows.</summary>
    private static string WriteDrg()
    {
        var path = Path.Combine(Path.GetTempPath(), $"problems-{System.Guid.NewGuid():N}.drg");
        File.WriteAllLines(path, new[]
        {
            "CFM,308 168gr,0.010886,0.0078232,0.030861,radar data",
            "0.2 0.5",
            "0.3 1.0",
            "0.25 2.0",
        });
        return path;
    }
}
