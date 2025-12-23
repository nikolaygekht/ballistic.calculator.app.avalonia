using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Controller for extracting chart data from trajectory points.
/// Converts trajectory data to X/Y arrays for plotting.
/// </summary>
public class ChartController
{
    private readonly AngularUnit _angularUnits;
    private readonly TrajectoryChartMode _chartMode;
    private readonly DropBase _dropBase;
    private readonly TrajectoryPoint[] _trajectory;
    private readonly MeasurementSystemController _measurementSystemController;

    public ChartController(
        MeasurementSystem measurementSystem,
        AngularUnit angularUnits,
        TrajectoryChartMode chartMode,
        DropBase dropBase,
        TrajectoryPoint[] trajectory)
    {
        _angularUnits = angularUnits;
        _chartMode = chartMode;
        _trajectory = trajectory;
        _dropBase = dropBase;
        _measurementSystemController = new MeasurementSystemController(measurementSystem, angularUnits);
    }

    public string YAxisTitle => _chartMode switch
    {
        TrajectoryChartMode.Velocity => $"Velocity ({_measurementSystemController.VelocityUnitName})",
        TrajectoryChartMode.Mach => "Mach",
        TrajectoryChartMode.Energy => $"Energy ({_measurementSystemController.EnergyUnitName})",
        TrajectoryChartMode.Drop => $"Drop ({_measurementSystemController.AdjustmentUnitName})",
        TrajectoryChartMode.DropAdjustment => $"Drop ({_measurementSystemController.AngularUnitName})",
        TrajectoryChartMode.Windage => $"Windage ({_measurementSystemController.AdjustmentUnitName})",
        TrajectoryChartMode.WindageAdjustment => $"Windage ({_measurementSystemController.AngularUnitName})",
        _ => "No data"
    };

    public string XAxisTitle => $"Range ({_measurementSystemController.RangeUnitName})";

    /// <summary>
    /// Number of Y series: 2 for Drop+MuzzlePoint (shows both drop and line-of-sight elevation), 1 otherwise.
    /// </summary>
    public int SeriesCount => (_chartMode == TrajectoryChartMode.Drop && _dropBase == DropBase.MuzzlePoint) ? 2 : 1;

    public string GetSeriesTitle(int seriesIndex)
    {
        if (_chartMode == TrajectoryChartMode.Drop && _dropBase == DropBase.MuzzlePoint)
        {
            return seriesIndex switch
            {
                0 => $"Drop ({_measurementSystemController.AdjustmentUnitName})",
                1 => $"Line of Sight Elevation ({_measurementSystemController.AdjustmentUnitName})",
                _ => "Unknown"
            };
        }

        return YAxisTitle;
    }

    public double[] GetXAxis()
    {
        var result = new double[_trajectory.Length];
        for (int i = 0; i < _trajectory.Length; i++)
            result[i] = GetXAxisPoint(i);
        return result;
    }

    public double GetXAxisPoint(int index) =>
        _trajectory[index].Distance.In(_measurementSystemController.RangeUnit);

    public List<double[]> GetYAxis()
    {
        var result = new List<double[]>();

        for (int series = 0; series < SeriesCount; series++)
        {
            var seriesData = new double[_trajectory.Length];
            for (int i = 0; i < _trajectory.Length; i++)
                seriesData[i] = GetYAxisPoint(i, series);
            result.Add(seriesData);
        }

        return result;
    }

    public double GetYAxisPoint(int index, int seriesIndex = 0)
    {
        var pt = _trajectory[index];

        if (_chartMode == TrajectoryChartMode.Drop && _dropBase == DropBase.MuzzlePoint)
        {
            return seriesIndex switch
            {
                0 => pt.DropFlat.In(_measurementSystemController.AdjustmentUnit),
                1 => pt.LineOfSightElevation.In(_measurementSystemController.AdjustmentUnit),
                _ => 0
            };
        }

        return _chartMode switch
        {
            TrajectoryChartMode.Velocity => pt.Velocity.In(_measurementSystemController.VelocityUnit),
            TrajectoryChartMode.Mach => pt.Mach,
            TrajectoryChartMode.Energy => pt.Energy.In(_measurementSystemController.EnergyUnit),
            TrajectoryChartMode.Drop => pt.Drop.In(_measurementSystemController.AdjustmentUnit),
            TrajectoryChartMode.DropAdjustment => pt.DropAdjustment.In(_angularUnits),
            TrajectoryChartMode.Windage => pt.Windage.In(_measurementSystemController.AdjustmentUnit),
            TrajectoryChartMode.WindageAdjustment => pt.WindageAdjustment.In(_angularUnits),
            _ => 0
        };
    }
}
