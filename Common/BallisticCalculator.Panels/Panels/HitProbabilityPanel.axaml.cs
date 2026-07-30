using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Tools;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Estimates the chance of hitting a target with one shot, by Monte Carlo over the shooter's error budget
/// (<see cref="HitProbabilityCalculator"/>). Everything about the shot itself — ammunition, rifle, zero,
/// weather, wind — comes from the active trajectory window; only the target and the error budget are entered
/// here.
/// <para>
/// The estimate runs only when <b>Estimate</b> is pressed. It is cheap enough to run live (three trajectory
/// runs plus arithmetic, about 28 ms at 10 000 shots), but the inputs are guesses about the shooter and the
/// conditions, and showing a probability derived from untouched defaults would lend them an authority they have
/// not earned. Once a result is shown it stays until the next press, so two set-ups can be compared.
/// </para>
/// </summary>
public partial class HitProbabilityPanel : UserControl
{
    /// <summary>Impacts drawn on the plot; more than this is unreadable and slow to render.</summary>
    private const int MaximumPlottedImpacts = 2000;

    /// <summary>
    /// The default target range — a distance most shots are actually taken at. Deliberately not the shot's own
    /// maximum distance: a table run out to 1000 yd is not a statement that anyone intends to shoot that far.
    /// </summary>
    private const double DefaultDistance = 300;

    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
    private AngularUnit _angularUnits = AngularUnit.MOA;
    private ShotData? _shotData;
    private HitProbabilityEstimate? _estimate;

    public HitProbabilityPanel()
    {
        InitializeComponent();
        InitializeControls();
        ApplyMeasurementSystem();
        WireEvents();
        ShowInfo(Prompt);
    }

    private const string Prompt = "Set the target, the shooter and the error budget, then press Estimate.";

    #region Properties

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            ApplyMeasurementSystem();
        }
    }

    /// <summary>The angular unit the group size is entered in; matches the active window.</summary>
    public AngularUnit AngularUnits
    {
        get => _angularUnits;
        set
        {
            if (_angularUnits == value) return;
            _angularUnits = value;
            GroupControl.ChangeUnit(_angularUnits, MeasurementSystemController.AngularAccuracy);
        }
    }

    /// <summary>The shot being asked about. Nothing is estimated until Estimate is pressed.</summary>
    public ShotData? ShotData
    {
        get => _shotData;
        set
        {
            _shotData = value;

            // The wind and muzzle velocity the percentages are percentages *of* both live on the shot,
            // so the absolute figures beside those fields change with it.
            UpdateDeviationHints();
        }
    }

    /// <summary>The last successful estimate; null while the inputs cannot produce one.</summary>
    internal HitProbabilityEstimate? Estimate => _estimate;

    /// <summary>The line at the bottom: what was estimated, or why nothing was.</summary>
    internal string Status => StatusText.Text ?? "";

    /// <summary>The shooting position currently selected.</summary>
    internal ShootingPosition? SelectedPosition => PositionCombo.SelectedItem as ShootingPosition;

    #endregion

    #region Events

    /// <summary>Raised by the Close button; the hosting window closes itself.</summary>
    public event EventHandler? CloseRequested;

    #endregion

    #region Setup

    private void InitializeControls()
    {
        DistanceControl.UnitType = typeof(DistanceUnit);
        DistanceControl.Minimum = 0;
        TargetSizeControl.UnitType = typeof(DistanceUnit);
        TargetSizeControl.Minimum = 0;
        GroupControl.UnitType = typeof(AngularUnit);
        GroupControl.Minimum = 0;
        GroupControl.DecimalPoints = MeasurementSystemController.AngularAccuracy;

        foreach (var position in HitProbabilityCalculator.ShootingPositions)
            PositionCombo.Items.Add(position);

        RangeErrorInput.Value = 2;
        WindErrorInput.Value = 30;
        MvDeviationInput.Value = 0.7m;
        ShotsInput.Value = 10000;
        SeedInput.Value = 1;

        // Supported (1/1) is the neutral default: the group size is itself defined from a supported position.
        SelectPosition(HitProbabilityCalculator.ShootingPositions[0].Name);
        WritePosition(HitProbabilityCalculator.ShootingPositions[0]);
    }

    /// <summary>
    /// Only the position ⇄ spread relationship is wired: it is UI state, not a result, so it stays in step as
    /// the user works. Nothing here estimates — that waits for the button.
    /// </summary>
    private void WireEvents()
    {
        PositionCombo.SelectionChanged += (_, _) => OnPositionSelected();
        SpreadHInput.ValueChanged += (_, _) => SyncPositionToSpreads();
        SpreadVInput.ValueChanged += (_, _) => SyncPositionToSpreads();

        // The absolute figures under the percentages are arithmetic on the inputs, not results of the
        // simulation, so they keep up as the user types for the same reason the position and the spread
        // multipliers do.
        DistanceControl.Changed += (_, _) => UpdateDeviationHints();
        RangeErrorInput.ValueChanged += (_, _) => UpdateDeviationHints();
        WindErrorInput.ValueChanged += (_, _) => UpdateDeviationHints();
        MvDeviationInput.ValueChanged += (_, _) => UpdateDeviationHints();
    }

    /// <summary>
    /// Sets the units and the defaults that go with them — 300 yd or 300 m, and the vital zone the Summary
    /// panel already assumes so the two agree. The defaults are written outright rather than converted: the
    /// host sets the measurement system once, before the dialog is shown, so there is nothing to preserve, and
    /// a metric user should see 300 m rather than 274.32 m.
    /// </summary>
    private void ApplyMeasurementSystem()
    {
        var metric = _measurementSystem == MeasurementSystem.Metric;

        DistanceControl.ChangeUnit(metric ? DistanceUnit.Meter : DistanceUnit.Yard);
        TargetSizeControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);

        DistanceControl.SetValue(new Measurement<DistanceUnit>(
            DefaultDistance, metric ? DistanceUnit.Meter : DistanceUnit.Yard));
        TargetSizeControl.SetValue(SummaryController.TargetSize(_measurementSystem));

        if (GroupControl.IsEmpty)
            GroupControl.SetValue(new Measurement<AngularUnit>(1, AngularUnit.MOA).To(_angularUnits));

        UpdateDeviationHints();
    }

    #endregion

    #region What the percentages amount to

    /// <summary>
    /// Echoes each 1σ percentage as the absolute figure it stands for, in the unit it was entered in.
    /// <para>
    /// The wind row is the reason this exists. The library's model scales the <b>drift the wind causes</b>
    /// by the wind error, so on a shot with no wind there is nothing to scale and the field changes the
    /// answer by exactly nothing — while still reading like a live input. The same is true of any of the
    /// three whose base quantity is zero, so all three say what they come to.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Internal rather than private for the same reason <c>BcConverterPanel.Recalculate</c> is: a
    /// programmatic <c>SetValue</c> raises no change event in headless Avalonia, so a test has to call
    /// this itself. In the app the control events above do the calling.
    /// </remarks>
    internal void UpdateDeviationHints()
    {
        var culture = CultureInfo.CurrentCulture;

        var distance = DistanceControl.IsEmpty ? null : DistanceControl.GetValue<DistanceUnit>();
        RangeErrorAbsText.Text = Fraction(RangeErrorInput) is { } rangeFraction && distance != null
            ? $"= ±{(distance.Value * rangeFraction).ToString("ND", culture)} at this range"
            : "";

        var wind = WindAtTarget(distance);
        WindErrorAbsText.Text = wind == null
            ? "no wind on this shot — this changes nothing"
            : Fraction(WindErrorInput) is { } windFraction
                ? $"= ±{(wind.Velocity * windFraction).ToString("ND", culture)} of a " +
                  $"{wind.Velocity.ToString("ND", culture)} wind"
                : "";

        var muzzleVelocity = _shotData?.Ammunition?.Ammunition?.MuzzleVelocity;
        MvDeviationAbsText.Text = muzzleVelocity == null
            ? ""
            : Fraction(MvDeviationInput) is { } mvFraction
                ? $"= ±{(muzzleVelocity.Value * mvFraction).ToString("ND", culture)} of " +
                  $"{muzzleVelocity.Value.ToString("ND", culture)}"
                : "";
    }

    /// <summary>A percentage field as a plain fraction, or null when it has been cleared.</summary>
    private static double? Fraction(NumericUpDown input) =>
        input.Value == null ? (double?)null : (double)input.Value.Value / 100.0;

    /// <summary>
    /// The wind that reaches the target, or null when the shot has none that would move the bullet.
    /// Winds are segmented by maximum range, so the one that matters is the first whose segment covers
    /// the target; the last one in the array runs to the end.
    /// </summary>
    private Wind? WindAtTarget(Measurement<DistanceUnit>? distance)
    {
        var winds = _shotData?.Winds;
        if (winds == null || winds.Length == 0)
            return null;

        Wind? applicable = null;
        foreach (var wind in winds)
        {
            applicable = wind;

            if (wind.MaximumRange == null || distance == null ||
                wind.MaximumRange.Value.In(DistanceUnit.Meter) >= distance.Value.In(DistanceUnit.Meter))
                break;
        }

        // A wind of zero speed is the same as no wind for this purpose: nothing for the error to scale.
        return applicable != null && applicable.Velocity.In(VelocityUnit.MetersPerSecond) > 0
            ? applicable
            : null;
    }

    #endregion

    #region Position and spread

    /// <summary>Selects a shooting position by name; used by the host and the tests.</summary>
    internal void SelectPosition(string name)
    {
        var position = HitProbabilityCalculator.ShootingPositions.FirstOrDefault(p => p.Name == name);
        if (position != null)
            PositionCombo.SelectedItem = position;
    }

    /// <summary>
    /// A preset writes the two multipliers — but only when they differ, so selecting the preset that the current
    /// numbers already match cannot fight what the user typed.
    /// </summary>
    private void OnPositionSelected()
    {
        var position = SelectedPosition;
        if (position == null || position.IsCustom)
            return;

        WritePosition(position);
    }

    private void WritePosition(ShootingPosition position)
    {
        var h = (decimal)position.Horizontal;
        var v = (decimal)position.Vertical;

        if (SpreadHInput.Value != h)
            SpreadHInput.Value = h;
        if (SpreadVInput.Value != v)
            SpreadVInput.Value = v;
    }

    /// <summary>
    /// Points the combo at whatever the two multipliers now describe — the matching preset, or Custom. The
    /// selection this makes writes nothing back (a preset only writes when the values differ, and Custom never
    /// writes), so there is no loop between the combo and the fields.
    /// </summary>
    private void SyncPositionToSpreads()
    {
        var preset = HitProbabilityCalculator.PositionFor((double)(SpreadHInput.Value ?? 0),
                                                          (double)(SpreadVInput.Value ?? 0));
        PositionCombo.SelectedItem = preset ?? HitProbabilityCalculator.ShootingPositions[^1];
    }

    #endregion

    #region Estimating

    /// <summary>
    /// Runs the estimate from the current inputs. Called by the Estimate button — and by the tests, which is
    /// the same path the button takes.
    /// </summary>
    internal void RunEstimate()
    {
        SyncPositionToSpreads();

        if (!TryReadInputs(out var inputs, out var problem))
        {
            Clear(problem);
            return;
        }

        if (_shotData == null)
        {
            Clear("Open a trajectory window to estimate against a shot.");
            return;
        }

        try
        {
            _estimate = HitProbabilityCalculator.Estimate(_shotData, inputs);
        }
        catch (Exception ex)
        {
            Clear(Explain(ex));
            return;
        }

        Show(_estimate, inputs);
    }

    /// <summary>
    /// A sentence for a failed estimate. Whether a load can be zeroed at all, or whether a set of numbers
    /// integrates, is the calculation's own answer rather than a property of the input, so no validation
    /// here can prevent either — the estimate has to be able to fail and say so.
    /// </summary>
    /// <remarks>
    /// The sibling mapping for the main window is <c>ShotCalculator.Explain</c>; the advice differs because
    /// this dialog cannot edit the shot. Anything unrecognised is named rather than smoothed over: it is a
    /// fault worth reporting, and the status line is all this panel has to report it with — the app's
    /// exception dialog, with its stack trace and Copy button, is no longer reached from here.
    /// </remarks>
    internal static string Explain(Exception ex) => ex switch
    {
        ZeroRangeCantBeReachedException =>
            "This load cannot be zeroed at the shot's zero distance, so there is no come-up to estimate " +
            "from. Fix the zero in the trajectory window — zero it closer, or give the load a faster " +
            "muzzle velocity.",

        TrajectoryCannotBeCalculatedException =>
            "The trajectory cannot be calculated from these numbers. Check the shot's ballistic " +
            "coefficient, bullet weight and muzzle velocity — a zero or absurd one is the usual cause — " +
            "and the muzzle velocity deviation entered here.",

        // The library states its own argument faults well enough to show as they are: a non-positive
        // ballistic coefficient arrives as ArgumentOutOfRangeException, an unresolved .drg for a GC
        // coefficient as ArgumentNullException.
        ArgumentException => ex.Message,

        _ => $"The estimate failed: {ex.GetType().Name}: {ex.Message}",
    };

    private bool TryReadInputs(out HitProbabilityInputs inputs, out string problem)
    {
        inputs = new HitProbabilityInputs();
        problem = "";

        if (!TryRead(SpreadHInput, "the horizontal spread multiplier", out var spreadH, ref problem))
            return false;

        if (!TryRead(SpreadVInput, "the vertical spread multiplier", out var spreadV, ref problem))
            return false;

        if (!TryRead(RangeErrorInput, "the range estimation error", out var rangeError, ref problem))
            return false;

        if (!TryRead(WindErrorInput, "the wind estimation error", out var windError, ref problem))
            return false;

        if (!TryRead(MvDeviationInput, "the muzzle velocity deviation", out var mvDeviation, ref problem))
            return false;

        if (!TryRead(ShotsInput, "the number of shots to simulate", out var shots, ref problem))
            return false;

        // An empty seed is not a mistake: it means re-roll, and the status line says so afterwards.
        var seed = SeedInput.Value == null ? (int?)null : (int)SeedInput.Value.Value;

        inputs = new HitProbabilityInputs
        {
            Distance = DistanceControl.IsEmpty ? null : DistanceControl.GetValue<DistanceUnit>(),
            TargetSize = TargetSizeControl.IsEmpty ? null : TargetSizeControl.GetValue<DistanceUnit>(),
            GroupSize = GroupControl.IsEmpty ? null : GroupControl.GetValue<AngularUnit>(),
            HorizontalSpread = spreadH,
            VerticalSpread = spreadV,
            RangeErrorPercent = rangeError,
            WindErrorPercent = windError,
            MuzzleVelocityDeviationPercent = mvDeviation,
            Shots = (int)shots,
            Seed = seed,
        };

        return true;
    }

    /// <summary>Reads a numeric field, which is empty only when the user has cleared it.</summary>
    private static bool TryRead(NumericUpDown input, string what, out double value, ref string problem)
    {
        if (input.Value == null)
        {
            value = 0;
            problem = $"Enter a value for {what}.";
            return false;
        }

        value = (double)input.Value.Value;
        return true;
    }

    private void Show(HitProbabilityEstimate estimate, HitProbabilityInputs inputs)
    {
        var culture = CultureInfo.CurrentCulture;

        ProbabilityText.Text = (estimate.HitProbability * 100).ToString("N1", culture) + " %";

        Hit50Text.Text = Shots(estimate.ShotsFor50Percent);
        Hit75Text.Text = Shots(estimate.ShotsFor75Percent);
        Hit90Text.Text = Shots(estimate.ShotsFor90Percent);
        Hit95Text.Text = Shots(estimate.ShotsFor95Percent);
        Hit98Text.Text = Shots(estimate.ShotsFor98Percent);

        var unit = LinearUnit;
        SpreadText.Text =
            $"Mean miss {estimate.MeanRadialMiss.To(unit).ToString("ND", culture)} from centre; " +
            $"90% within {estimate.NinetiethPercentileMiss.To(unit).ToString("ND", culture)}.";

        var plotted = DrawPlot(estimate, inputs, unit);

        var drawn = plotted < estimate.Impacts.Count
            ? $" Plot shows {plotted.ToString("N0", culture)} of them."
            : "";
        ShowInfo($"{estimate.Impacts.Count.ToString("N0", culture)} simulated shots." + drawn +
                 (inputs.Seed == null ? " Unseeded — the answer moves a little on every change." : ""));
    }

    private static string Shots(int? value) =>
        value?.ToString("N0", CultureInfo.CurrentCulture) ?? "—";

    /// <summary>The linear unit the misses and the plot are shown in.</summary>
    private DistanceUnit LinearUnit =>
        _measurementSystem == MeasurementSystem.Metric ? DistanceUnit.Centimeter : DistanceUnit.Inch;

    /// <summary>
    /// Draws the impacts and the vital zone, and returns how many impacts were drawn. The horizontal axis is
    /// negated: the library reports windage positive to the <b>left</b>, and a plot must show left on the left.
    /// </summary>
    private int DrawPlot(HitProbabilityEstimate estimate, HitProbabilityInputs inputs, DistanceUnit unit)
    {
        var sample = HitProbabilityCalculator.SampleImpacts(estimate.Impacts, MaximumPlottedImpacts);

        var xs = new double[sample.Count];
        var ys = new double[sample.Count];
        for (var i = 0; i < sample.Count; i++)
        {
            xs[i] = -sample[i].Horizontal.In(unit);
            ys[i] = sample[i].Vertical.In(unit);
        }

        var plot = ImpactPlot.Plot;
        plot.Clear();

        var impacts = plot.Add.ScatterPoints(xs, ys);
        impacts.MarkerSize = 3;
        impacts.LegendText = "impacts";

        var radius = inputs.TargetSize!.Value.In(unit) / 2.0;
        var zone = plot.Add.Circle(0, 0, radius);
        zone.FillStyle.IsVisible = false;
        zone.LineWidth = 2;
        zone.LegendText = "vital zone";

        plot.Add.HorizontalLine(0, 1, ScottPlot.Colors.Gray);
        plot.Add.VerticalLine(0, 1, ScottPlot.Colors.Gray);

        plot.XLabel($"Horizontal miss ({Measurement<DistanceUnit>.GetUnitName(unit)})");
        plot.YLabel($"Vertical miss ({Measurement<DistanceUnit>.GetUnitName(unit)})");

        // Equal scaling is not cosmetic: stretched axes turn a round group into an ellipse and misrepresent
        // the one thing this plot exists to show.
        plot.Axes.SquareUnits();
        plot.Axes.AutoScale();
        ImpactPlot.Refresh();

        return sample.Count;
    }

    #endregion

    #region Status

    private void Clear(string problem)
    {
        _estimate = null;

        ProbabilityText.Text = "";
        Hit50Text.Text = "";
        Hit75Text.Text = "";
        Hit90Text.Text = "";
        Hit95Text.Text = "";
        Hit98Text.Text = "";
        SpreadText.Text = "";

        ImpactPlot.Plot.Clear();
        ImpactPlot.Refresh();

        ShowError(problem);
    }

    private void OnEstimate(object? sender, RoutedEventArgs e) => RunEstimate();

    private void OnClose(object? sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void ShowInfo(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = Avalonia.Media.Brushes.Gray;
    }

    private void ShowError(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = Avalonia.Media.Brushes.Firebrick;
    }

    #endregion
}
