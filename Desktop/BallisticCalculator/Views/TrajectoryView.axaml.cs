using Avalonia.Controls;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views;

public partial class TrajectoryView : UserControl, ITrajectoryChildWindow
{
    private MeasurementSystem _measurementSystem;
    private AngularUnit _angularUnits = AngularUnit.Mil;
    private DropBase _dropBase = DropBase.SightLine;
    private TrajectoryChartMode _chartMode = TrajectoryChartMode.Drop;
    private ShotData? _shotData;
    private TrajectoryPoint[]? _trajectory;

    public TrajectoryView()
    {
        InitializeComponent();
    }

    public IFileDialogService? FileDialogService
    {
        get => ReticleControl.FileDialogService;
        set => ReticleControl.FileDialogService = value;
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            _measurementSystem = value;
            TableControl.MeasurementSystem = value;
            ChartControl.MeasurementSystem = value;
            ReticleControl.MeasurementSystem = value;
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
            ReticleControl.ShotData = value;
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

    public void ApplyDefaults()
    {
        if (_shotData?.Weapon?.Sight?.VerticalClick != null)
            _angularUnits = _shotData.Weapon.Sight.VerticalClick.Value.Unit;
        else
            _angularUnits = AngularUnit.Mil;

        _chartMode = TrajectoryChartMode.Drop;
        _dropBase = DropBase.SightLine;

        TableControl.MeasurementSystem = _measurementSystem;
        TableControl.AngularUnits = _angularUnits;
        TableControl.DropBase = _dropBase;

        ChartControl.MeasurementSystem = _measurementSystem;
        ChartControl.AngularUnits = _angularUnits;
        ChartControl.ChartMode = _chartMode;
        ChartControl.DropBase = _dropBase;

        ReticleControl.MeasurementSystem = _measurementSystem;
    }

    private void UpdateClickValues()
    {
        var sight = _shotData?.Weapon?.Sight;
        TableControl.VerticalClick = sight?.VerticalClick;
        TableControl.HorizontalClick = sight?.HorizontalClick;
    }
}
