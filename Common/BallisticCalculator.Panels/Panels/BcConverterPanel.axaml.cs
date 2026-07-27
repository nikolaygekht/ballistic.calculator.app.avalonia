using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Converts a published ballistic coefficient from one standard drag table to another (the everyday G1 ↔ G7
/// question). There is no Convert button: the target value follows the three inputs, because the interesting
/// action is moving the reference velocity and watching the answer move with it.
/// <para>
/// The atmosphere is here only for the speed of sound that turns the reference velocity into a Mach number, and
/// it is edited in the same dialog the measured-velocities drag table editor uses — the panel raises
/// <see cref="AtmosphereRequested"/> and the host owns the window.
/// </para>
/// </summary>
public partial class BcConverterPanel : UserControl
{
    /// <summary>A reference in the band where the conversion is trustworthy, so the default is not misleading.</summary>
    private static readonly Measurement<VelocityUnit> DefaultReference = new(2700, VelocityUnit.FeetPerSecond);

    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
    private Atmosphere? _atmosphere;
    private BcConversion? _conversion;

    public BcConverterPanel()
    {
        InitializeComponent();
        InitializeControls();
        ApplyMeasurementSystem();
        WireEvents();
        Recalculate();
    }

    #region Properties

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            ApplyMeasurementSystem();
            Recalculate();
        }
    }

    /// <summary>
    /// The air whose speed of sound sets the Mach number. Null means sea-level standard; nothing else about the
    /// atmosphere affects the conversion.
    /// </summary>
    public Atmosphere? Atmosphere
    {
        get => _atmosphere;
        set
        {
            _atmosphere = value;
            Recalculate();
        }
    }

    /// <summary>
    /// Takes the source coefficient from an ammunition (typically the active shot's). A custom-table (GC) or
    /// form-factor coefficient is ignored: it cannot be converted, so prefilling it would only show an error.
    /// </summary>
    public Ammunition? Prefill
    {
        set
        {
            var bc = value?.BallisticCoefficient;
            if (bc == null ||
                bc.Value.Table == DragTableId.GC ||
                bc.Value.ValueType != BallisticCoefficientValueType.Coefficient ||
                bc.Value.Value <= 0)
                return;

            SourceBcControl.Value = bc.Value;
            SelectTargetTable(DefaultTargetFor(bc.Value.Table));
            Recalculate();
        }
    }

    /// <summary>The table the source is being converted to.</summary>
    internal DragTableId TargetTable =>
        TableCombo.SelectedItem is DragTableId id ? id : DragTableId.G1;

    /// <summary>The last successful conversion; null while the inputs cannot produce one.</summary>
    internal BcConversion? Conversion => _conversion;

    /// <summary>The converted coefficient as shown, in the text form the app parses (for example <c>0.235G7</c>).</summary>
    internal string TargetText => TargetBox.Text ?? "";

    /// <summary>The line under the fields: what was converted at what reference, or why nothing was.</summary>
    internal string Status => StatusText.Text ?? "";

    #endregion

    #region Events

    /// <summary>Raised by the Close button; the hosting window closes itself.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Raised by Set Atmosphere. The host shows the atmosphere editor and writes the result back to
    /// <see cref="Atmosphere"/> — windows belong to the application, not to a panel library.
    /// </summary>
    public event EventHandler? AtmosphereRequested;

    #endregion

    #region Setup

    private void InitializeControls()
    {
        VelocityControl.UnitType = typeof(VelocityUnit);
        VelocityControl.Minimum = 0;
        VelocityControl.DecimalPoints = 1;
        VelocityControl.SetValue(DefaultReference);

        foreach (var table in BcConversionCalculator.StandardTables)
            TableCombo.Items.Add(table);

        // The source control starts on G1, so G7 is the other half of the question being asked.
        SelectTargetTable(DragTableId.G7);
    }

    private void WireEvents()
    {
        SourceBcControl.Changed += (_, _) => Recalculate();
        VelocityControl.Changed += (_, _) => Recalculate();
        TableCombo.SelectionChanged += (_, _) => Recalculate();
    }

    private void ApplyMeasurementSystem()
    {
        var metric = _measurementSystem == MeasurementSystem.Metric;
        VelocityControl.ChangeUnit(metric ? VelocityUnit.MetersPerSecond : VelocityUnit.FeetPerSecond);
    }

    /// <summary>Selects a destination table; used by the host, the prefill and the tests.</summary>
    internal void SelectTargetTable(DragTableId table) => TableCombo.SelectedItem = table;

    private static DragTableId DefaultTargetFor(DragTableId source) =>
        source == DragTableId.G7 ? DragTableId.G1 : DragTableId.G7;

    #endregion

    #region Conversion

    /// <summary>
    /// Recomputes the target coefficient from the current inputs. Wired to every input's change event; also
    /// called directly by the tests, because a programmatic <c>SetValue</c> raises no change event in headless
    /// Avalonia.
    /// </summary>
    internal void Recalculate()
    {
        var source = SourceBcControl.IsEmpty ? null : SourceBcControl.Value;
        var velocity = VelocityControl.IsEmpty ? null : VelocityControl.GetValue<VelocityUnit>();

        try
        {
            _conversion = BcConversionCalculator.Convert(source, TargetTable, velocity, _atmosphere);
        }
        catch (ArgumentException ex)
        {
            _conversion = null;
            TargetBox.Text = "";
            WarningText.IsVisible = false;
            ShowError(ex.Message);
            return;
        }

        TargetBox.Text = _conversion.Converted.ToString("F3", CultureInfo.InvariantCulture);

        WarningText.IsVisible = _conversion.IsTransonic;
        if (_conversion.IsTransonic)
            WarningText.Text =
                $"Below about Mach {BcConversionCalculator.TransonicMach:F1} the standard curves diverge in " +
                "shape and this conversion loses accuracy — roughly 9% low near Mach 1.3. Use a supersonic " +
                "reference where you can.";

        ShowInfo(Describe(source!.Value, _conversion));
    }

    private string Describe(BallisticCoefficient source, BcConversion conversion)
    {
        var reference = $"{conversion.ReferenceVelocity.ToString("ND", CultureInfo.CurrentCulture)} " +
                        $"(Mach {conversion.ReferenceMach.ToString("F2", CultureInfo.CurrentCulture)}), {Air()}";

        if (source.Table == conversion.Converted.Table)
            return $"Source and target are the same table — nothing to convert. Reference {reference}.";

        return $"{source.ToString("F3", CultureInfo.InvariantCulture)} → " +
               $"{conversion.Converted.ToString("F3", CultureInfo.InvariantCulture)} at {reference}.";
    }

    private string Air() =>
        _atmosphere == null
            ? "standard atmosphere"
            : $"{_atmosphere.Altitude.ToString("ND", CultureInfo.CurrentCulture)}, " +
              $"{_atmosphere.Temperature.ToString("ND", CultureInfo.CurrentCulture)}";

    #endregion

    #region Buttons and status

    private void OnSetAtmosphere(object? sender, RoutedEventArgs e) =>
        AtmosphereRequested?.Invoke(this, EventArgs.Empty);

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
