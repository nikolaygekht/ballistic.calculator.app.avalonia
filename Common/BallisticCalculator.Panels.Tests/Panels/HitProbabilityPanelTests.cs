using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

public class HitProbabilityPanelTests
{
    private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

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

    /// <summary>A panel bound to a shot, estimated once — the same path the Estimate button takes.</summary>
    private static HitProbabilityPanel PanelWithShot(double maxYards = 600)
    {
        var panel = new HitProbabilityPanel { ShotData = Shot(maxYards) };
        panel.RunEstimate();
        return panel;
    }

    #region Defaults

    [AvaloniaFact]
    public void Panel_ShouldNotEstimateUntilAsked()
    {
        // Arrange & Act: a probability computed from untouched defaults would imply they had been considered
        var panel = new HitProbabilityPanel { ShotData = Shot() };

        // Assert
        panel.Estimate.Should().BeNull();
        panel.ProbabilityText.Text.Should().BeEmpty();
        panel.Status.Should().Contain("press Estimate");
    }

    [AvaloniaFact]
    public void EstimateButton_ShouldRunTheEstimate()
    {
        // Arrange
        var panel = new HitProbabilityPanel { ShotData = Shot() };

        // Act
        Click(panel.EstimateButton);

        // Assert
        panel.Estimate.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void ChangingAnInputAfterAnEstimate_ShouldLeaveTheResultAlone()
    {
        // Arrange
        var panel = PanelWithShot();
        var shown = panel.ProbabilityText.Text;

        // Act — the previous answer stays readable while the next set-up is entered
        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(900, DistanceUnit.Yard));
        panel.SelectPosition("Standing");

        // Assert
        panel.ProbabilityText.Text.Should().Be(shown);
        panel.Estimate.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void RunEstimate_WithoutAShot_ShouldSayItNeedsOne()
    {
        var panel = new HitProbabilityPanel();

        panel.RunEstimate();

        panel.Estimate.Should().BeNull();
        panel.Status.Should().Contain("trajectory window");
    }

    [AvaloniaFact]
    public void Panel_ShouldDefaultToThreeHundredYards()
    {
        // Arrange & Act: a table run out to 1000 yd is not a statement that anyone means to shoot that far,
        // so the shot's maximum distance is deliberately not used
        var panel = new HitProbabilityPanel { ShotData = Shot(maxYards: 1000) };

        // Assert
        panel.DistanceControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Yard)
             .Should().BeApproximately(300, 1e-6);
    }

    [AvaloniaFact]
    public void Panel_ShouldStartFromASupportedPositionWithUnitMultipliers()
    {
        var panel = new HitProbabilityPanel();

        panel.SelectedPosition!.Name.Should().Be("Supported");
        panel.SpreadHInput.Value.Should().Be(1);
        panel.SpreadVInput.Value.Should().Be(1);
    }

    [AvaloniaFact]
    public void Panel_ShouldStartWithTheDefaultErrorBudgetAndShotCount()
    {
        var panel = new HitProbabilityPanel();

        panel.RangeErrorInput.Value.Should().Be(2);
        panel.WindErrorInput.Value.Should().Be(30);
        panel.MvDeviationInput.Value.Should().Be(0.7m);
        panel.ShotsInput.Value.Should().Be(10000);
        panel.SeedInput.Value.Should().Be(1);
    }

    [AvaloniaFact]
    public void MeasurementSystem_Metric_ShouldUseMetresAndMillimetres()
    {
        var panel = new HitProbabilityPanel { ShotData = Shot(), MeasurementSystem = MeasurementSystem.Metric };

        var distance = panel.DistanceControl.GetValue<DistanceUnit>()!.Value;
        distance.Unit.Should().Be(DistanceUnit.Meter);
        distance.Value.Should().Be(300, "a metric user gets 300 m, not a converted 300 yd");

        var target = panel.TargetSizeControl.GetValue<DistanceUnit>()!.Value;
        target.Unit.Should().Be(DistanceUnit.Millimeter);
        target.Value.Should().Be(500);
    }

    [AvaloniaFact]
    public void Panel_ShouldDefaultTheVitalZoneToTheSummarysTargetSize()
    {
        var panel = new HitProbabilityPanel();

        panel.TargetSizeControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Inch)
             .Should().BeApproximately(20, 1e-6);
    }

    #endregion

    #region Estimating

    [AvaloniaFact]
    public void Recalculate_WithAShot_ShouldShowAProbabilityAndTheShotsToHit()
    {
        var panel = PanelWithShot();

        panel.Estimate.Should().NotBeNull();
        panel.ProbabilityText.Text.Should().Contain("%");
        panel.Hit50Text.Text.Should().NotBeNullOrEmpty();
        panel.Hit98Text.Text.Should().NotBeNullOrEmpty();
        panel.SpreadText.Text.Should().Contain("90%");
    }

    [AvaloniaFact]
    public void Recalculate_AtAShorterDistance_ShouldRaiseTheProbability()
    {
        // Arrange
        var panel = PanelWithShot();
        var far = panel.Estimate!.HitProbability;

        // Act
        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(200, DistanceUnit.Yard));
        panel.RunEstimate();

        // Assert
        panel.Estimate!.HitProbability.Should().BeGreaterThan(far);
    }

    [AvaloniaFact]
    public void Recalculate_ShouldPlotTheImpactsAndTheVitalZone()
    {
        var panel = PanelWithShot();

        panel.ImpactPlot.Plot.GetPlottables().Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public void Recalculate_WithMoreShotsThanThePlotLimit_ShouldSayHowManyAreDrawn()
    {
        // Arrange
        var panel = PanelWithShot();

        // Act
        panel.ShotsInput.Value = 50000;
        panel.RunEstimate();

        // Assert
        panel.Estimate!.Impacts.Should().HaveCount(50000);
        panel.Status.Should().Contain("Plot shows", "the plot is thinned and says so");
    }

    #endregion

    #region Position and spread

    [AvaloniaFact]
    public void SelectingAPosition_ShouldFillTheSpreadMultipliers()
    {
        // Arrange
        var panel = PanelWithShot();

        // Act
        panel.SelectPosition("Kneeling");

        // Assert
        panel.SpreadHInput.Value.Should().Be(4);
        panel.SpreadVInput.Value.Should().Be(3);
    }

    [AvaloniaFact]
    public void SelectingAWiderPosition_ShouldLowerTheProbability()
    {
        // Arrange
        var panel = PanelWithShot();
        var supported = panel.Estimate!.HitProbability;

        // Act
        panel.SelectPosition("Standing");
        panel.RunEstimate();

        // Assert
        panel.Estimate!.HitProbability.Should().BeLessThan(supported);
    }

    [AvaloniaFact]
    public void EditingASpreadMultiplier_ShouldSwitchThePositionToCustom()
    {
        // Arrange
        var panel = PanelWithShot();

        // Act — in the app the text-changed event calls Recalculate; headless raises none for a programmatic set
        panel.SpreadHInput.Value = 2.5m;
        panel.RunEstimate();

        // Assert
        panel.SelectedPosition!.IsCustom.Should().BeTrue();
        panel.SpreadVInput.Value.Should().Be(1, "switching to Custom must not rewrite the other field");
    }

    [AvaloniaFact]
    public void EditingSpreadsToMatchAPreset_ShouldSelectThatPreset()
    {
        // Arrange
        var panel = PanelWithShot();

        // Act
        panel.SpreadHInput.Value = 2;
        panel.SpreadVInput.Value = 2;
        panel.RunEstimate();

        // Assert
        panel.SelectedPosition!.Name.Should().Be("Prone");
        panel.SpreadHInput.Value.Should().Be(2, "the preset must not fight what was typed");
    }

    #endregion

    #region Refused input

    [AvaloniaFact]
    public void RunEstimate_WithTooManyShots_ShouldReportTheRangeAndClearTheResult()
    {
        // Arrange: NumericUpDown does not clip to Minimum/Maximum (ClipValueToMinMax is false by default), and
        // it is left that way on purpose — the app reports the range rather than silently rewriting the entry
        var panel = PanelWithShot();

        // Act
        panel.ShotsInput.Value = 500000;
        panel.RunEstimate();

        // Assert
        panel.Estimate.Should().BeNull();
        panel.ProbabilityText.Text.Should().BeEmpty();
        panel.Status.Should().Contain("1000").And.Contain("50000");
    }

    [AvaloniaFact]
    public void RunEstimate_WithTooFewShots_ShouldReportTheRange()
    {
        var panel = PanelWithShot();

        panel.ShotsInput.Value = 10;
        panel.RunEstimate();

        panel.Estimate.Should().BeNull();
        panel.Status.Should().Contain("1000");
    }

    [AvaloniaFact]
    public void RunEstimate_WithAnEmptyShotCount_ShouldSayWhatIsMissing()
    {
        var panel = PanelWithShot();

        panel.ShotsInput.Value = null;
        panel.RunEstimate();

        panel.Estimate.Should().BeNull();
        panel.Status.Should().Contain("shots");
    }

    [AvaloniaFact]
    public void Recalculate_WithAnUnreadableErrorPercent_ShouldSayWhichField()
    {
        var panel = PanelWithShot();

        panel.WindErrorInput.Value = null;
        panel.RunEstimate();

        panel.Estimate.Should().BeNull();
        panel.Status.Should().Contain("wind");
    }

    [AvaloniaFact]
    public void Recalculate_WithAnUnreadableSpread_ShouldSayWhichField()
    {
        var panel = PanelWithShot();

        panel.SpreadVInput.Value = null;
        panel.RunEstimate();

        panel.Estimate.Should().BeNull();
        panel.Status.Should().Contain("vertical");
    }

    [AvaloniaFact]
    public void Recalculate_WithAnEmptySeed_ShouldStillEstimate()
    {
        // Arrange: an empty seed means "reroll", which is allowed
        var panel = PanelWithShot();

        // Act
        panel.SeedInput.Value = null;
        panel.RunEstimate();

        // Assert
        panel.Estimate.Should().NotBeNull();
        panel.Status.Should().Contain("Unseeded");
    }

    [AvaloniaFact]
    public void Recalculate_WithoutADistance_ShouldSayWhatIsMissing()
    {
        var panel = PanelWithShot();

        panel.DistanceControl.NumericPart.Text = "";
        panel.RunEstimate();

        panel.Estimate.Should().BeNull();
        panel.Status.Should().Contain("distance");
    }

    #endregion

    #region Layout and buttons

    [AvaloniaFact]
    public void Panel_ShouldLayOutInsideAWindow()
    {
        var panel = PanelWithShot();
        var window = new Window { Content = panel, Width = 820, Height = 560 };

        window.Show();

        panel.Bounds.Height.Should().BeGreaterThan(0);
        panel.ImpactPlot.Bounds.Height.Should().BeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Panel_ShouldWarnThatGroupSizeIsOneSigma()
    {
        var panel = new HitProbabilityPanel();

        panel.NoteText.Text.Should().Contain("extreme spread");
        panel.NoteText.Text.Should().Contain("come-up");
    }

    [AvaloniaFact]
    public void CloseButton_ShouldAskTheHostToClose()
    {
        var panel = new HitProbabilityPanel();
        var asked = false;
        panel.CloseRequested += (_, _) => asked = true;

        Click(panel.CloseButton);

        asked.Should().BeTrue();
    }

    #endregion
}
