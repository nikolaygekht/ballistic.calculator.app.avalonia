using BallisticCalculator.Types;

namespace BallisticCalculator.Views;

public interface ITrajectoryChildWindow : IAppChildWindow
{
    ShotData? ShotData { get; set; }
    TrajectoryPoint[]? Trajectory { get; set; }
    string? FileName { get; set; }

    void ShowTable();
    void ShowChart();
    void ShowReticle();
    void ZoomYToVisibleRange();
}
