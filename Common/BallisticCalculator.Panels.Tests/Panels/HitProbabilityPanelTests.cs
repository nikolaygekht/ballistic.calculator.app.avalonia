using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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

    #region Calculation failures

    /// <summary>A shot whose zero the load cannot reach: 168gr at 300 ft/s, zeroed at 1000 yd.</summary>
    private static ShotData UnzeroableShot()
    {
        var shot = Shot();
        shot.Ammunition = new AmmunitionLibraryEntry
        {
            Name = "too slow",
            Ammunition = new Ammunition(
                new Measurement<WeightUnit>(168, WeightUnit.Grain),
                new BallisticCoefficient(0.223, DragTableId.G7),
                new Measurement<VelocityUnit>(300, VelocityUnit.FeetPerSecond)),
        };
        shot.Weapon = new Rifle(
            new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                      Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
            new ZeroingParameters(new Measurement<DistanceUnit>(1000, DistanceUnit.Yard), null, null));
        shot.Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(1000, DistanceUnit.Yard) };
        return shot;
    }

    [AvaloniaFact]
    public void RunEstimate_WhenTheLoadCannotReachItsZero_ShouldExplainInsteadOfThrowing()
    {
        // Arrange: the engine's own answer, not a property of the input — nothing here can be validated first
        var panel = new HitProbabilityPanel { ShotData = UnzeroableShot() };

        // Act
        var run = () => panel.RunEstimate();

        // Assert
        run.Should().NotThrow();
        panel.Estimate.Should().BeNull();
        panel.ProbabilityText.Text.Should().BeEmpty();
        panel.Status.Should().Contain("zero");
    }

    [AvaloniaFact]
    public void RunEstimate_WhenTheEngineFails_ShouldNotLeaveTheEarlierResultShowing()
    {
        // Arrange: an estimate on the board, then a shot that cannot be computed
        var panel = PanelWithShot();
        panel.Estimate.Should().NotBeNull();

        // Act
        panel.ShotData = UnzeroableShot();
        panel.RunEstimate();

        // Assert — a stale probability beside a new set-up would read as this set-up's answer
        panel.Estimate.Should().BeNull();
        panel.ProbabilityText.Text.Should().BeEmpty();
    }

    [Fact]
    public void Explain_ZeroRangeCantBeReached_ShouldSayWhereToFixIt()
    {
        var explained = HitProbabilityPanel.Explain(new ZeroRangeCantBeReachedException());

        explained.Should().Contain("zero");
        explained.Should().Contain("trajectory window");
    }

    [Fact]
    public void Explain_TrajectoryCannotBeCalculated_ShouldPointAtTheNumbers()
    {
        var explained = HitProbabilityPanel.Explain(new TrajectoryCannotBeCalculatedException());

        explained.Should().Contain("ballistic coefficient");
        explained.Should().Contain("muzzle velocity");
    }

    [Fact]
    public void Explain_ArgumentException_ShouldUseItsOwnMessage()
    {
        // The library states its own argument faults well enough to show as they are — a zero ballistic
        // coefficient arrives as ArgumentOutOfRangeException, a missing .drg as ArgumentNullException
        var explained = HitProbabilityPanel.Explain(
            new System.ArgumentOutOfRangeException("ammunition", "The ballistic coefficient must be positive."));

        explained.Should().Contain("The ballistic coefficient must be positive.");
    }

    [Fact]
    public void Explain_AnythingElse_ShouldNameTheFaultSoItCanBeReported()
    {
        var explained = HitProbabilityPanel.Explain(new System.InvalidOperationException("something odd"));

        explained.Should().Contain("InvalidOperationException");
        explained.Should().Contain("something odd");
    }

    #endregion

    #region What the percentages amount to

    /// <summary>The same shot, with the wind removed rather than set to zero.</summary>
    private static ShotData ShotWithoutWind()
    {
        var shot = Shot();
        shot.Winds = null;
        return shot;
    }

    /// <summary>
    /// How the panel renders a measurement, so the expectations below survive a machine whose culture
    /// uses a comma for the decimal point.
    /// </summary>
    private static string Shown<T>(double value, T unit) where T : System.Enum =>
        new Measurement<T>(value, unit).ToString("ND", CultureInfo.CurrentCulture);

    /// <summary>The same shot carrying a single wind.</summary>
    private static ShotData ShotWithWind(double milesPerHour)
    {
        var shot = Shot();
        shot.Winds = new[]
        {
            new Wind(new Measurement<VelocityUnit>(milesPerHour, VelocityUnit.MilesPerHour),
                     new Measurement<AngularUnit>(90, AngularUnit.Degree)),
        };
        return shot;
    }

    [AvaloniaFact]
    public void WindErrorAbsolute_WithNoWindOnTheShot_ShouldSayItChangesNothing()
    {
        // The whole point: the library scales the drift the wind causes, so with no wind the field is inert
        // however high it reads. 30% of nothing is nothing, and the panel has to say so.
        var panel = new HitProbabilityPanel { ShotData = ShotWithoutWind() };

        panel.WindErrorInput.Value.Should().Be(30);
        panel.WindErrorAbsText.Text.Should().Contain("no wind");
        panel.WindErrorAbsText.Text.Should().Contain("changes nothing");
    }

    [AvaloniaFact]
    public void WindErrorAbsolute_WithAZeroSpeedWind_ShouldSayItChangesNothing()
    {
        // A wind entry of 0 mph is the same as no wind for this purpose.
        var panel = new HitProbabilityPanel { ShotData = ShotWithWind(0) };

        panel.WindErrorAbsText.Text.Should().Contain("no wind");
    }

    [AvaloniaFact]
    public void WindErrorAbsolute_WithWindOnTheShot_ShouldShowTheAbsoluteFigure()
    {
        // 30% of a 10 mph wind is 3 mph, shown in the unit the wind was entered in.
        var panel = new HitProbabilityPanel { ShotData = Shot() };

        panel.WindErrorAbsText.Text.Should().Contain(Shown(3, VelocityUnit.MilesPerHour));
        panel.WindErrorAbsText.Text.Should().Contain(Shown(10, VelocityUnit.MilesPerHour));
        panel.WindErrorAbsText.Text.Should().NotContain("no wind");
    }

    [AvaloniaFact]
    public void RangeErrorAbsolute_ShouldShowThePercentOfTheTargetDistance()
    {
        // The default is 2% of 300 yd = 6 yd, in the distance's own unit.
        var panel = new HitProbabilityPanel { ShotData = Shot() };

        panel.RangeErrorInput.Value.Should().Be(2);
        panel.RangeErrorAbsText.Text.Should().Contain(Shown(6, DistanceUnit.Yard));
    }

    [AvaloniaFact]
    public void RangeErrorAbsolute_WhenTheDistanceChanges_ShouldFollowIt()
    {
        // UpdateDeviationHints is called explicitly for the reason BcConverterPanelTests gives: a
        // programmatic SetValue raises no change event in headless Avalonia. In the app the control's
        // Changed event does the calling, which is why these figures keep up as the user types.
        var panel = new HitProbabilityPanel { ShotData = Shot() };

        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(1000, DistanceUnit.Yard));
        panel.UpdateDeviationHints();

        panel.RangeErrorAbsText.Text.Should().Contain(Shown(20, DistanceUnit.Yard));
    }

    [AvaloniaFact]
    public void MvDeviationAbsolute_ShouldShowThePercentOfTheMuzzleVelocity()
    {
        // 0.7% of 2700 ft/s is 18.9 ft/s, at the unit's own default accuracy of one decimal.
        var panel = new HitProbabilityPanel { ShotData = Shot() };

        panel.MvDeviationInput.Value.Should().Be(0.7m);
        panel.MvDeviationAbsText.Text.Should().Contain(Shown(18.9, VelocityUnit.FeetPerSecond));
        panel.MvDeviationAbsText.Text.Should().Contain(Shown(2700, VelocityUnit.FeetPerSecond));
    }

    [AvaloniaFact]
    public void MvDeviationAbsolute_WithNoShot_ShouldStaySilent()
    {
        // Nothing to take a percentage of yet, and a guess would be worse than a blank.
        var panel = new HitProbabilityPanel();

        panel.MvDeviationAbsText.Text.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void InputColumn_ShouldScrollWithoutTakingThePlotWithIt()
    {
        // Five sections of fields outgrow a short dialog, so the column scrolls — but only the column:
        // the plot has to keep whatever space it is given rather than scroll off with the inputs.
        // (Asserting the scroll extent would prove nothing in headless, which has no font metrics and
        // so measures the form short enough to fit — see BcConverterPanelTests.)
        var panel = PanelWithShot();
        var window = new Window { Content = panel, Width = 820, Height = 420 };
        window.Show();

        IsInsideAScrollViewer(panel.DistanceControl, panel).Should().BeTrue(
            "the input column should scroll");
        IsInsideAScrollViewer(panel.ImpactPlot, panel).Should().BeFalse(
            "the plot must not scroll with the inputs");
        IsInsideAScrollViewer(panel.EstimateButton, panel).Should().BeFalse(
            "nor must the Estimate button");
    }

    /// <summary>Whether <paramref name="control"/> sits in scrolled content somewhere inside the panel.</summary>
    private static bool IsInsideAScrollViewer(Control control, Control root) =>
        control.GetVisualAncestors()
               .TakeWhile(ancestor => !ReferenceEquals(ancestor, root))
               .OfType<ScrollViewer>()
               .Any();

    #endregion
}
