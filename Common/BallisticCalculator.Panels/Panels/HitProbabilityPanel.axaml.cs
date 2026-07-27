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
        set => _shotData = value;
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
        catch (ArgumentException ex)
        {
            Clear(ex.Message);
            return;
        }

        Show(_estimate, inputs);
    }

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
