using BallisticCalculator.Types;

namespace BallisticCalculator.Views;

public interface IComparisonChartChildWindow : IAppChildWindow
{
    void AddTrajectory(ChartTrajectory trajectory);
    void RemoveLastTrajectory();
    int TrajectoryCount { get; }
}
