using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Models;

/// <summary>
/// Lightweight wrapper around TrajectoryPoint that formats values on-demand.
/// When settings change, just refresh the DataGrid - no need to recreate wrappers.
/// </summary>
public class TrajectoryPointWrapper
{
    private readonly TrajectoryPoint _point;
    private readonly Func<MeasurementSystemController> _getController;
    private readonly Func<DropBase> _getDropBase;
    private readonly Func<Measurement<AngularUnit>?> _getVerticalClick;
    private readonly Func<Measurement<AngularUnit>?> _getHorizontalClick;

    public TrajectoryPointWrapper(
        TrajectoryPoint point,
        Func<MeasurementSystemController> getController,
        Func<DropBase> getDropBase,
        Func<Measurement<AngularUnit>?> getVerticalClick,
        Func<Measurement<AngularUnit>?> getHorizontalClick)
    {
        _point = point;
        _getController = getController;
        _getDropBase = getDropBase;
        _getVerticalClick = getVerticalClick;
        _getHorizontalClick = getHorizontalClick;
    }

    /// <summary>
    /// Access to the underlying TrajectoryPoint if needed.
    /// </summary>
    public TrajectoryPoint Point => _point;

    // Formatted properties - format on each access using current settings

    public string Range => _getController().FormatRange(_point.Distance);

    public string Velocity => _getController().FormatVelocity(_point.Velocity);

    public string Mach => _getController().FormatMach(_point.Mach);

    public string Drop => _getController().FormatAdjustment(
        _getDropBase() == DropBase.SightLine ? _point.Drop : _point.DropFlat);

    public string DropAdjustment => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatAngular(_point.DropAdjustment);

    public string DropClicks => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatClicks(_point.DropAdjustment, _getVerticalClick());

    public string Windage => _getController().FormatAdjustment(_point.Windage);

    public string WindageAdjustment => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatAngular(_point.WindageAdjustment);

    public string WindageClicks => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatClicks(_point.WindageAdjustment, _getHorizontalClick());

    public string Time => _getController().FormatTime(_point.Time);

    public string Energy => _getController().FormatEnergy(_point.Energy);

    public string OptimalGameWeight => _getController().FormatWeight(_point.OptimalGameWeight);
}
