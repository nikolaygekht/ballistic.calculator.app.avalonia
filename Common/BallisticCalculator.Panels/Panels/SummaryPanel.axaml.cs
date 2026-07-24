using Avalonia.Controls;
using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Read-only output panel: shows the zeroing adjustments (window angular units), the point-blank
/// "dead zone" for a bottom-aimed target, the near/far zero ranges, and the subsonic distance
/// (distances in the same units as the chart). Values are recomputed whenever inputs change.
/// </summary>
public partial class SummaryPanel : UserControl
{
    private readonly SummaryController _controller = new();
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;
    private AngularUnit _angularUnits = AngularUnit.Mil;
    private ShotData? _shotData;
    private TrajectoryPoint[]? _trajectory;

    public SummaryPanel()
    {
        InitializeComponent();
        Update();
    }

    #region Properties

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set { _measurementSystem = value; Update(); }
    }

    public AngularUnit AngularUnits
    {
        get => _angularUnits;
        set { _angularUnits = value; Update(); }
    }

    public ShotData? ShotData
    {
        get => _shotData;
        set { _shotData = value; Update(); }
    }

    public TrajectoryPoint[]? Trajectory
    {
        get => _trajectory;
        set { _trajectory = value; Update(); }
    }

    #endregion

    private void Update()
    {
        var units = new MeasurementSystemController(_measurementSystem, _angularUnits);
        var result = _controller.Compute(_shotData, _trajectory, _measurementSystem);

        ZeroVValue.Text = FormatAngular(result.ZeroVertical, units);
        ZeroHValue.Text = FormatAngular(result.ZeroHorizontal, units);
        DeadZoneValue.Text = FormatRange(result.DeadZone, units);
        NearZeroValue.Text = FormatRange(result.NearZero, units);
        FarZeroValue.Text = FormatRange(result.FarZero, units);
        SubsonicValue.Text = FormatRange(result.SubsonicDistance, units);
    }

    private static string FormatAngular(Measurement<AngularUnit>? value, MeasurementSystemController units)
        => value == null ? "n/a" : $"{units.FormatAngular(value.Value)} {units.AngularUnitName}";

    private static string FormatRange(Measurement<DistanceUnit>? value, MeasurementSystemController units)
        => value == null ? "n/a" : $"{units.FormatRange(value.Value)} {units.RangeUnitName}";
}
