using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Utilities;

public class CsvExportController
{
    private readonly TrajectoryPoint[] _trajectory;
    private readonly Sight? _sight;
    private readonly MeasurementSystemController _controller;
    private readonly CultureInfo _culture;
    private readonly char _separator;

    /// <summary>
    /// Creates a CSV export controller.
    /// </summary>
    /// <param name="useLocalCulture">
    /// true: use current locale for numbers and list separator (e.g. semicolon for comma-decimal locales) — opens correctly in local Excel.
    /// false: use invariant culture with comma separator — portable across systems.
    /// </param>
    public CsvExportController(TrajectoryPoint[] trajectory, MeasurementSystem measurementSystem,
                               Sight? sight, AngularUnit angularUnits, bool useLocalCulture = false)
    {
        _trajectory = trajectory;
        _sight = sight;
        _controller = new MeasurementSystemController(measurementSystem, angularUnits);

        if (useLocalCulture)
        {
            _culture = CultureInfo.CurrentCulture;
            // Use the locale's list separator (semicolon in comma-decimal locales)
            _separator = _culture.TextInfo.ListSeparator.Length > 0
                ? _culture.TextInfo.ListSeparator[0]
                : ',';
        }
        else
        {
            _culture = CultureInfo.InvariantCulture;
            _separator = ',';
        }
    }

    public IEnumerable<string> Prepare()
    {
        yield return Header();
        for (int i = 0; i < _trajectory.Length; i++)
            yield return Line(_trajectory[i]);
    }

    private string Header()
    {
        var s = _separator;
        var sb = new StringBuilder();
        sb.Append($"Range ({_controller.RangeUnitName})").Append(s)
          .Append($"Velocity ({_controller.VelocityUnitName})").Append(s)
          .Append("Mach").Append(s)
          .Append($"Path ({_controller.AdjustmentUnitName})").Append(s)
          .Append($"Hold ({_controller.AngularUnitName})").Append(s)
          .Append("Clicks").Append(s)
          .Append($"Windage ({_controller.AdjustmentUnitName})").Append(s)
          .Append($"Win. Adj. ({_controller.AngularUnitName})").Append(s)
          .Append("Clicks").Append(s)
          .Append("Time").Append(s)
          .Append($"Energy ({_controller.EnergyUnitName})").Append(s)
          .Append($"OGW ({_controller.WeightUnitName})");
        return sb.ToString();
    }

    private string Line(TrajectoryPoint point)
    {
        var sb = new StringBuilder();
        var s = _separator;

        sb.Append(point.Distance.In(_controller.RangeUnit).ToString(MeasurementSystemController.RangeFormatStringF, _culture))
          .Append(s)
          .Append(point.Velocity.In(_controller.VelocityUnit).ToString(MeasurementSystemController.VelocityFormatStringF, _culture))
          .Append(s)
          .Append(point.Mach.ToString(MeasurementSystemController.MachFormatStringF, _culture))
          .Append(s)
          .Append(point.Drop.In(_controller.AdjustmentUnit).ToString(MeasurementSystemController.AdjustmentFormatStringF, _culture))
          .Append(s)
          .Append(point.DropAdjustment.In(_controller.AngularUnit).ToString(MeasurementSystemController.AngularFormatStringF, _culture))
          .Append(s)
          .Append(FormatClicks(point.Distance, point.DropAdjustment, _sight?.VerticalClick))
          .Append(s)
          .Append(point.Windage.In(_controller.AdjustmentUnit).ToString(MeasurementSystemController.AdjustmentFormatStringF, _culture))
          .Append(s)
          .Append(point.WindageAdjustment.In(_controller.AngularUnit).ToString(MeasurementSystemController.AngularFormatStringF, _culture))
          .Append(s)
          .Append(FormatClicks(point.Distance, point.WindageAdjustment, _sight?.HorizontalClick))
          .Append(s)
          .Append(point.Time.TotalSeconds.ToString("F3", _culture))
          .Append(s)
          .Append(point.Energy.In(_controller.EnergyUnit).ToString(MeasurementSystemController.EnergyFormatStringF, _culture))
          .Append(s)
          .Append(point.OptimalGameWeight.In(_controller.WeightUnit).ToString(MeasurementSystemController.WeightFormatStringF, _culture));
        return sb.ToString();
    }

    private string FormatClicks(Measurement<DistanceUnit> distance,
                                Measurement<AngularUnit> adjustment,
                                Measurement<AngularUnit>? clickValue)
    {
        if (distance.Value < 1 || clickValue == null || clickValue.Value.Value <= 0)
            return "n/a";
        return ((int)Math.Round(adjustment / clickValue.Value))
            .ToString(MeasurementSystemController.ClickFormatStringF, _culture);
    }
}
