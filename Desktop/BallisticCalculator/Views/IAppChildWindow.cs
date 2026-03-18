using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views;

public interface IAppChildWindow
{
    MeasurementSystem MeasurementSystem { get; set; }
    AngularUnit AngularUnits { get; set; }
    DropBase DropBase { get; set; }
    TrajectoryChartMode ChartMode { get; set; }
}
