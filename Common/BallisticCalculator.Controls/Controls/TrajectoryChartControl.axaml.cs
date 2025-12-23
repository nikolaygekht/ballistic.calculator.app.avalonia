using Avalonia.Controls;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using ScottPlot;

namespace BallisticCalculator.Controls.Controls;

/// <summary>
/// Unified chart control for displaying single or multiple ballistic trajectories.
/// Supports various chart modes: Velocity, Mach, Drop, DropAdjustment, Windage, WindageAdjustment, Energy.
/// </summary>
public partial class TrajectoryChartControl : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;
    private AngularUnit _angularUnits = AngularUnit.MOA;
    private TrajectoryChartMode _chartMode = TrajectoryChartMode.Drop;
    private DropBase _dropBase = DropBase.SightLine;
    private readonly List<ChartTrajectory> _trajectories = new();

    // ScottPlot colors for multiple series
    private static readonly Color[] SeriesColors = new[]
    {
        Colors.Blue,
        Colors.Red,
        Colors.Green,
        Colors.Orange,
        Colors.Purple,
        Colors.Brown,
        Colors.Magenta,
        Colors.Teal
    };

    public TrajectoryChartControl()
    {
        InitializeComponent();
    }

    #region Configuration Properties

    /// <summary>
    /// Gets or sets the measurement system (Metric/Imperial).
    /// Changing this triggers chart update.
    /// </summary>
    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            _measurementSystem = value;
            UpdateChart();
        }
    }

    /// <summary>
    /// Gets or sets the angular units for adjustment modes.
    /// Changing this triggers chart update.
    /// </summary>
    public AngularUnit AngularUnits
    {
        get => _angularUnits;
        set
        {
            _angularUnits = value;
            UpdateChart();
        }
    }

    /// <summary>
    /// Gets or sets the chart mode (what data to display).
    /// Changing this triggers chart update.
    /// </summary>
    public TrajectoryChartMode ChartMode
    {
        get => _chartMode;
        set
        {
            _chartMode = value;
            UpdateChart();
        }
    }

    /// <summary>
    /// Gets or sets the drop base reference point.
    /// Only affects Drop mode - determines whether to show sight line or muzzle point reference.
    /// </summary>
    public DropBase DropBase
    {
        get => _dropBase;
        set
        {
            _dropBase = value;
            UpdateChart();
        }
    }

    #endregion

    #region Data Management

    /// <summary>
    /// Sets a single trajectory (convenience method).
    /// Clears existing trajectories and sets the new one.
    /// </summary>
    public void SetTrajectory(string name, TrajectoryPoint[] points)
    {
        _trajectories.Clear();
        if (points.Length > 0)
        {
            _trajectories.Add(new ChartTrajectory(name, points));
        }
        UpdateChart();
    }

    /// <summary>
    /// Adds a named trajectory for multi-trajectory comparison.
    /// </summary>
    public void AddTrajectory(string name, TrajectoryPoint[] points)
    {
        _trajectories.Add(new ChartTrajectory(name, points));
        UpdateChart();
    }

    /// <summary>
    /// Removes the most recently added trajectory.
    /// </summary>
    public void RemoveLastTrajectory()
    {
        if (_trajectories.Count > 0)
        {
            _trajectories.RemoveAt(_trajectories.Count - 1);
            UpdateChart();
        }
    }

    /// <summary>
    /// Clears all trajectories from the chart.
    /// </summary>
    public void ClearTrajectories()
    {
        _trajectories.Clear();
        UpdateChart();
    }

    /// <summary>
    /// Gets the current number of trajectories.
    /// </summary>
    public int TrajectoryCount => _trajectories.Count;

    #endregion

    #region Chart Rendering

    private void UpdateChart()
    {
        if (AvaPlot == null)
            return;

        AvaPlot.Plot.Clear();

        if (_trajectories.Count == 0)
        {
            AvaPlot.Refresh();
            return;
        }

        int colorIndex = 0;

        foreach (var trajectory in _trajectories)
        {
            if (trajectory.Points.Length == 0)
                continue;

            var controller = new ChartController(
                _measurementSystem,
                _angularUnits,
                _chartMode,
                _dropBase,
                trajectory.Points);

            var xData = controller.GetXAxis();
            var ySeriesList = controller.GetYAxis();

            for (int seriesIndex = 0; seriesIndex < ySeriesList.Count; seriesIndex++)
            {
                var yData = ySeriesList[seriesIndex];
                var scatter = AvaPlot.Plot.Add.Scatter(xData, yData);

                // Set legend text based on trajectory name and series
                if (_trajectories.Count == 1 && ySeriesList.Count == 1)
                {
                    // Single trajectory, single series - no legend needed
                    scatter.LegendText = string.Empty;
                }
                else if (ySeriesList.Count > 1)
                {
                    // Multiple series (Drop + MuzzlePoint mode)
                    scatter.LegendText = $"{trajectory.Name}: {controller.GetSeriesTitle(seriesIndex)}";
                }
                else
                {
                    // Multiple trajectories
                    scatter.LegendText = trajectory.Name;
                }

                // Set color
                scatter.Color = SeriesColors[colorIndex % SeriesColors.Length];
                colorIndex++;
            }

            // Set axis labels from the first trajectory's controller
            if (trajectory == _trajectories[0])
            {
                AvaPlot.Plot.Axes.Bottom.Label.Text = controller.XAxisTitle;
                AvaPlot.Plot.Axes.Left.Label.Text = controller.YAxisTitle;
            }
        }

        // Show legend if multiple trajectories or series
        if (_trajectories.Count > 1 || (_trajectories.Count == 1 && _chartMode == TrajectoryChartMode.Drop && _dropBase == DropBase.MuzzlePoint))
        {
            AvaPlot.Plot.ShowLegend();
        }
        else
        {
            AvaPlot.Plot.Legend.IsVisible = false;
        }

        AvaPlot.Plot.Axes.AutoScale();
        AvaPlot.Refresh();
    }

    /// <summary>
    /// Adjusts Y axis to visible X range after user zoom.
    /// Call this after user has zoomed to a specific range.
    /// </summary>
    public void UpdateYAxisToVisibleRange()
    {
        if (AvaPlot == null || _trajectories.Count == 0)
            return;

        // Get current X axis limits
        var xLimits = AvaPlot.Plot.Axes.Bottom.Range;

        // Find min/max Y values within visible X range
        double minY = double.MaxValue;
        double maxY = double.MinValue;

        foreach (var trajectory in _trajectories)
        {
            if (trajectory.Points.Length == 0)
                continue;

            var controller = new ChartController(
                _measurementSystem,
                _angularUnits,
                _chartMode,
                _dropBase,
                trajectory.Points);

            var xData = controller.GetXAxis();
            var ySeriesList = controller.GetYAxis();

            foreach (var yData in ySeriesList)
            {
                for (int i = 0; i < xData.Length; i++)
                {
                    if (xData[i] >= xLimits.Min && xData[i] <= xLimits.Max)
                    {
                        minY = Math.Min(minY, yData[i]);
                        maxY = Math.Max(maxY, yData[i]);
                    }
                }
            }
        }

        if (minY < double.MaxValue && maxY > double.MinValue)
        {
            // Add 5% padding
            var padding = (maxY - minY) * 0.05;
            AvaPlot.Plot.Axes.Left.Min = minY - padding;
            AvaPlot.Plot.Axes.Left.Max = maxY + padding;
            AvaPlot.Refresh();
        }
    }

    #endregion
}
