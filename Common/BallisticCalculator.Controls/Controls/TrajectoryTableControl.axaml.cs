using Avalonia.Controls;
using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Controls;

/// <summary>
/// Table control for displaying trajectory data in tabular form.
/// Works alongside TrajectoryChartControl (table shows numbers, chart visualizes).
/// Uses on-demand formatting via TrajectoryPointWrapper for efficiency.
/// </summary>
public partial class TrajectoryTableControl : UserControl
{
    private MeasurementSystemController _controller;
    private DropBase _dropBase = DropBase.SightLine;
    private Measurement<AngularUnit>? _verticalClick;
    private Measurement<AngularUnit>? _horizontalClick;
    private List<TrajectoryPointWrapper>? _wrappers;

    // DataGrid and columns - found after InitializeComponent
    private DataGrid? _dataGrid;

    public TrajectoryTableControl()
    {
        InitializeComponent();
        _controller = new MeasurementSystemController(MeasurementSystem.Metric);

        // Find the DataGrid after initialization
        _dataGrid = this.FindControl<DataGrid>("DataGrid");

        UpdateColumnHeaders();
    }

    #region Configuration Properties

    /// <summary>
    /// Gets or sets the measurement system (Metric/Imperial).
    /// Changing this updates column headers and refreshes table.
    /// </summary>
    public MeasurementSystem MeasurementSystem
    {
        get => _controller.MeasurementSystem;
        set
        {
            _controller.MeasurementSystem = value;
            UpdateColumnHeaders();
            RefreshTable();
        }
    }

    /// <summary>
    /// Gets or sets the angular units for adjustment columns.
    /// Changing this updates column headers and refreshes table.
    /// </summary>
    public AngularUnit AngularUnits
    {
        get => _controller.AngularUnit;
        set
        {
            _controller.AngularUnit = value;
            UpdateColumnHeaders();
            RefreshTable();
        }
    }

    /// <summary>
    /// Gets or sets the drop base reference point.
    /// SightLine: shows Drop (relative to sight line)
    /// MuzzlePoint: shows DropFlat (relative to muzzle)
    /// </summary>
    public DropBase DropBase
    {
        get => _dropBase;
        set
        {
            _dropBase = value;
            RefreshTable();
        }
    }

    /// <summary>
    /// Gets or sets the vertical (elevation) click size for scope adjustments.
    /// Set to null to show "n/a" in clicks column.
    /// </summary>
    public Measurement<AngularUnit>? VerticalClick
    {
        get => _verticalClick;
        set
        {
            _verticalClick = value;
            RefreshTable();
        }
    }

    /// <summary>
    /// Gets or sets the horizontal (windage) click size for scope adjustments.
    /// Set to null to show "n/a" in clicks column.
    /// </summary>
    public Measurement<AngularUnit>? HorizontalClick
    {
        get => _horizontalClick;
        set
        {
            _horizontalClick = value;
            RefreshTable();
        }
    }

    #endregion

    #region Data Management

    /// <summary>
    /// Sets the trajectory data to display.
    /// </summary>
    public void SetTrajectory(TrajectoryPoint[] trajectory)
    {
        _wrappers = trajectory.Select(p => new TrajectoryPointWrapper(
            p,
            () => _controller,
            () => _dropBase,
            () => _verticalClick,
            () => _horizontalClick
        )).ToList();

        if (_dataGrid != null)
            _dataGrid.ItemsSource = _wrappers;

        UpdateColumnHeaders();
    }

    /// <summary>
    /// Clears the trajectory data.
    /// </summary>
    public void Clear()
    {
        _wrappers = null;
        if (_dataGrid != null)
            _dataGrid.ItemsSource = null;
    }

    /// <summary>
    /// Gets the number of rows in the table.
    /// </summary>
    public int RowCount => _wrappers?.Count ?? 0;

    #endregion

    #region Internal Methods

    /// <summary>
    /// Refreshes the table without recreating wrappers.
    /// Wrappers format on-demand using current settings.
    /// </summary>
    private void RefreshTable()
    {
        if (_wrappers == null || _dataGrid == null) return;

        // Force DataGrid to re-read all cell values by resetting ItemsSource
        var items = _dataGrid.ItemsSource;
        _dataGrid.ItemsSource = null;
        _dataGrid.ItemsSource = items;
    }

    /// <summary>
    /// Updates column headers to reflect current units.
    /// </summary>
    private void UpdateColumnHeaders()
    {
        if (_dataGrid == null) return;

        var columns = _dataGrid.Columns;
        if (columns.Count < 12) return;

        columns[0].Header = _controller.RangeHeader;
        columns[1].Header = _controller.VelocityHeader;
        columns[2].Header = _controller.MachHeader;
        columns[3].Header = _controller.DropHeader;
        columns[4].Header = _controller.DropAdjustmentHeader;
        columns[5].Header = _controller.ClicksHeader;
        columns[6].Header = _controller.WindageHeader;
        columns[7].Header = _controller.WindageAdjustmentHeader;
        columns[8].Header = _controller.ClicksHeader;
        columns[9].Header = _controller.TimeHeader;
        columns[10].Header = _controller.EnergyHeader;
        columns[11].Header = _controller.WeightHeader;
    }

    #endregion
}
