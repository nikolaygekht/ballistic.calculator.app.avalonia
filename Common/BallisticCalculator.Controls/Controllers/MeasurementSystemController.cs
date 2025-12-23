using System.Globalization;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Helper controller for unit conversion based on measurement system (Metric/Imperial).
/// Reusable by charts, tables, and other views.
/// </summary>
public class MeasurementSystemController
{
    public MeasurementSystem MeasurementSystem { get; set; }
    public AngularUnit AngularUnit { get; set; }

    public MeasurementSystemController(MeasurementSystem measurementSystem, AngularUnit angularUnit = AngularUnit.MOA)
    {
        MeasurementSystem = measurementSystem;
        AngularUnit = angularUnit;
    }

    // Accuracy (decimal places) for each measurement type
    public static int RangeAccuracy => 0;
    public static int AdjustmentAccuracy => 2;
    public static int VelocityAccuracy => 0;
    public static int EnergyAccuracy => 0;
    public static int WeightAccuracy => 1;
    public static int MachAccuracy => 2;
    public static int AngularAccuracy => 2;
    public static int ClickAccuracy => 0;

    // Format strings for numeric display (N = number with group separators)
    public static string RangeFormatString => $"N{RangeAccuracy}";
    public static string AdjustmentFormatString => $"N{AdjustmentAccuracy}";
    public static string VelocityFormatString => $"N{VelocityAccuracy}";
    public static string EnergyFormatString => $"N{EnergyAccuracy}";
    public static string WeightFormatString => $"N{WeightAccuracy}";
    public static string MachFormatString => $"N{MachAccuracy}";
    public static string AngularFormatString => $"N{AngularAccuracy}";
    public static string ClickFormatString => $"N{ClickAccuracy}";
    public static string TimeFormatString => @"mm\:ss\.fff";

    // Format strings for fixed-point display (F = fixed decimal)
    public static string RangeFormatStringF => $"F{RangeAccuracy}";
    public static string AdjustmentFormatStringF => $"F{AdjustmentAccuracy}";
    public static string VelocityFormatStringF => $"F{VelocityAccuracy}";
    public static string EnergyFormatStringF => $"F{EnergyAccuracy}";
    public static string WeightFormatStringF => $"F{WeightAccuracy}";
    public static string MachFormatStringF => $"F{MachAccuracy}";
    public static string AngularFormatStringF => $"F{AngularAccuracy}";
    public static string ClickFormatStringF => $"F{ClickAccuracy}";

    // Units based on measurement system
    public DistanceUnit RangeUnit => MeasurementSystem == MeasurementSystem.Metric ? DistanceUnit.Meter : DistanceUnit.Yard;
    public DistanceUnit AdjustmentUnit => MeasurementSystem == MeasurementSystem.Metric ? DistanceUnit.Centimeter : DistanceUnit.Inch;
    public VelocityUnit VelocityUnit => MeasurementSystem == MeasurementSystem.Metric ? VelocityUnit.MetersPerSecond : VelocityUnit.FeetPerSecond;
    public EnergyUnit EnergyUnit => MeasurementSystem == MeasurementSystem.Metric ? EnergyUnit.Joule : EnergyUnit.FootPound;
    public WeightUnit WeightUnit => MeasurementSystem == MeasurementSystem.Metric ? WeightUnit.Kilogram : WeightUnit.Pound;

    // Unit name abbreviations
    public string RangeUnitName => MeasurementSystem == MeasurementSystem.Metric ? "m" : "yd";
    public string AdjustmentUnitName => MeasurementSystem == MeasurementSystem.Metric ? "cm" : "in";
    public string VelocityUnitName => MeasurementSystem == MeasurementSystem.Metric ? "m/s" : "ft/s";
    public string EnergyUnitName => MeasurementSystem == MeasurementSystem.Metric ? "J" : "ft·lb";
    public string WeightUnitName => MeasurementSystem == MeasurementSystem.Metric ? "kg" : "lb";

    public string AngularUnitName => AngularUnit switch
    {
        AngularUnit.Radian => "rad",
        AngularUnit.Degree => "deg",
        AngularUnit.Gradian => "grad",
        AngularUnit.Turn => "turn",
        AngularUnit.MOA => "moa",
        AngularUnit.Mil => "mil",
        AngularUnit.Thousand => "th",
        AngularUnit.MRad => "mrad",
        AngularUnit.CmPer100Meters => "cm/100m",
        AngularUnit.InchesPer100Yards => "in/100yd",
        _ => "?"
    };

    #region Formatting Methods

    /// <summary>
    /// Formats a distance value as range (e.g., "100" for 100m or 100yd).
    /// Returns just the numeric value; units are in column headers.
    /// </summary>
    public string FormatRange(Measurement<DistanceUnit> distance, CultureInfo? culture = null)
        => distance.To(RangeUnit).Value.ToString(RangeFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a velocity value (e.g., "800" for 800 m/s or ft/s).
    /// Returns just the numeric value; units are in column headers.
    /// </summary>
    public string FormatVelocity(Measurement<VelocityUnit> velocity, CultureInfo? culture = null)
        => velocity.To(VelocityUnit).Value.ToString(VelocityFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a Mach number (e.g., "2.35").
    /// </summary>
    public string FormatMach(double mach, CultureInfo? culture = null)
        => mach.ToString(MachFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a distance value as adjustment (drop/windage in cm or in).
    /// Returns just the numeric value; units are in column headers.
    /// </summary>
    public string FormatAdjustment(Measurement<DistanceUnit> value, CultureInfo? culture = null)
        => value.To(AdjustmentUnit).Value.ToString(AdjustmentFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats an angular value (e.g., "4.30" for 4.3 MOA).
    /// Returns just the numeric value; units are in column headers.
    /// </summary>
    public string FormatAngular(Measurement<AngularUnit> value, CultureInfo? culture = null)
        => value.To(AngularUnit).Value.ToString(AngularFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats an energy value (e.g., "3200" for 3200 J or ft-lb).
    /// Returns just the numeric value; units are in column headers.
    /// </summary>
    public string FormatEnergy(Measurement<EnergyUnit> energy, CultureInfo? culture = null)
        => energy.To(EnergyUnit).Value.ToString(EnergyFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a weight value (e.g., "150.0" for 150 kg or lb).
    /// Returns just the numeric value; units are in column headers.
    /// </summary>
    public string FormatWeight(Measurement<WeightUnit> weight, CultureInfo? culture = null)
        => weight.To(WeightUnit).Value.ToString(WeightFormatString, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a time span (e.g., "00:01.234").
    /// </summary>
    public string FormatTime(TimeSpan time)
        => time.ToString(TimeFormatString);

    /// <summary>
    /// Formats scope clicks (adjustment divided by click size).
    /// Returns "n/a" if click size is null or zero.
    /// </summary>
    public string FormatClicks(Measurement<AngularUnit> adjustment, Measurement<AngularUnit>? clickSize, CultureInfo? culture = null)
    {
        if (clickSize == null || clickSize.Value.Value <= 0)
            return "n/a";
        return (adjustment / clickSize.Value).ToString(ClickFormatString, culture ?? CultureInfo.CurrentCulture);
    }

    #endregion

    #region Column Headers (with units)

    public string RangeHeader => $"Range ({RangeUnitName})";
    public string VelocityHeader => $"Velocity ({VelocityUnitName})";
    public string MachHeader => "Mach";
    public string DropHeader => $"Drop ({AdjustmentUnitName})";
    public string DropAdjustmentHeader => $"Hold ({AngularUnitName})";
    public string ClicksHeader => "Clicks";
    public string WindageHeader => $"Windage ({AdjustmentUnitName})";
    public string WindageAdjustmentHeader => $"Wnd.Adj. ({AngularUnitName})";
    public string TimeHeader => "Time";
    public string EnergyHeader => $"Energy ({EnergyUnitName})";
    public string WeightHeader => $"O.G.W. ({WeightUnitName})";

    #endregion
}
