using Avalonia.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views;

public partial class TrajectoryView : UserControl, ITrajectoryChildWindow
{
    private MeasurementSystem _measurementSystem;
    private AngularUnit _angularUnits = AngularUnit.MOA;
    private DropBase _dropBase = DropBase.SightLine;
    private TrajectoryChartMode _chartMode = TrajectoryChartMode.Velocity;
    private ShotData? _shotData;
    private TrajectoryPoint[]? _trajectory;

    public TrajectoryView()
    {
        InitializeComponent();
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            _measurementSystem = value;
            TableControl.MeasurementSystem = value;
            ChartControl.MeasurementSystem = value;
        }
    }

    public AngularUnit AngularUnits
    {
        get => _angularUnits;
        set
        {
            _angularUnits = value;
            TableControl.AngularUnits = value;
            ChartControl.AngularUnits = value;
        }
    }

    public DropBase DropBase
    {
        get => _dropBase;
        set
        {
            _dropBase = value;
            TableControl.DropBase = value;
            ChartControl.DropBase = value;
        }
    }

    public TrajectoryChartMode ChartMode
    {
        get => _chartMode;
        set
        {
            _chartMode = value;
            ChartControl.ChartMode = value;
        }
    }

    public ShotData? ShotData
    {
        get => _shotData;
        set
        {
            _shotData = value;
            UpdateClickValues();
        }
    }

    public TrajectoryPoint[]? Trajectory
    {
        get => _trajectory;
        set
        {
            _trajectory = value;
            if (value != null)
            {
                var name = _shotData?.Ammunition?.Name ?? "Trajectory";
                TableControl.SetTrajectory(value);
                ChartControl.SetTrajectory(name, value);
            }
            else
            {
                TableControl.Clear();
                ChartControl.ClearTrajectories();
            }
        }
    }

    public string? FileName { get; set; }

    public void ShowTable() => Tabs.SelectedIndex = 0;
    public void ShowChart() => Tabs.SelectedIndex = 1;
    public void ShowReticle() => Tabs.SelectedIndex = 2;
    public void ZoomYToVisibleRange() => ChartControl.UpdateYAxisToVisibleRange();

    public double[] GetColumnWidths() => TableControl.GetColumnWidths();
    public void SetColumnWidths(double[] widths) => TableControl.SetColumnWidths(widths);

    private void UpdateClickValues()
    {
        var sight = _shotData?.Weapon?.Sight;
        TableControl.VerticalClick = sight?.VerticalClick;
        TableControl.HorizontalClick = sight?.HorizontalClick;
    }
}
