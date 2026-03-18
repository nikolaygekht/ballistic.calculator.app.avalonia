using Avalonia.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views;

public partial class TestTrajectoryView : UserControl, ITrajectoryChildWindow
{
    private MeasurementSystem _measurementSystem;
    private AngularUnit _angularUnits = AngularUnit.MOA;
    private DropBase _dropBase = DropBase.SightLine;
    private TrajectoryChartMode _chartMode = TrajectoryChartMode.Velocity;

    public TestTrajectoryView()
    {
        InitializeComponent();
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            _measurementSystem = value;
            UpdateDisplay();
        }
    }

    public AngularUnit AngularUnits
    {
        get => _angularUnits;
        set
        {
            _angularUnits = value;
            UpdateDisplay();
        }
    }

    public DropBase DropBase
    {
        get => _dropBase;
        set
        {
            _dropBase = value;
            UpdateDisplay();
        }
    }

    public TrajectoryChartMode ChartMode
    {
        get => _chartMode;
        set
        {
            _chartMode = value;
            UpdateDisplay();
        }
    }

    public ShotData? ShotData { get; set; }
    public TrajectoryPoint[]? Trajectory { get; set; }
    public string? FileName { get; set; }

    public void ShowTable() { }
    public void ShowChart() { }
    public void ShowReticle() { }
    public void ZoomYToVisibleRange() { }

    public void UpdateDisplay()
    {
        LabelSystem.Text = _measurementSystem.ToString();
        LabelAngular.Text = _angularUnits.ToString();
        LabelDrop.Text = _dropBase.ToString();
        LabelChart.Text = _chartMode.ToString();
    }
}
