using Avalonia.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views;

public partial class CompareView : UserControl, IComparisonChartChildWindow
{
    public CompareView()
    {
        InitializeComponent();
    }

    public MeasurementSystem MeasurementSystem
    {
        get => ChartControl.MeasurementSystem;
        set => ChartControl.MeasurementSystem = value;
    }

    public AngularUnit AngularUnits
    {
        get => ChartControl.AngularUnits;
        set => ChartControl.AngularUnits = value;
    }

    public DropBase DropBase
    {
        get => ChartControl.DropBase;
        set => ChartControl.DropBase = value;
    }

    public TrajectoryChartMode ChartMode
    {
        get => ChartControl.ChartMode;
        set => ChartControl.ChartMode = value;
    }

    public int TrajectoryCount => ChartControl.TrajectoryCount;

    public void AddTrajectory(ChartTrajectory trajectory)
    {
        ChartControl.AddTrajectory(trajectory.Name, trajectory.Points);
    }

    public void RemoveLastTrajectory()
    {
        ChartControl.RemoveLastTrajectory();
    }

    public void ZoomYToVisibleRange() => ChartControl.UpdateYAxisToVisibleRange();
}
