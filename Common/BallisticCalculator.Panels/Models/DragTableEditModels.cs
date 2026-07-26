using System.ComponentModel;
using System.Globalization;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Models;

/// <summary>
/// One BC-vs-Mach knot in the drag table editor. Knots are always keyed by Mach, which is what
/// <c>DrgDragTableFactory</c> takes; the coefficient keeps the drag table it was quoted against, so a curve
/// read from a report that lists G1 and G7 columns records what the user actually typed.
/// </summary>
public sealed class BcKnotEditModel : INotifyPropertyChanged
{
    private double _mach;
    private BallisticCoefficient _bc;

    public double Mach
    {
        get => _mach;
        set
        {
            if (_mach.Equals(value)) return;
            _mach = value;
            Notify(nameof(Mach));
            Notify(nameof(MachText));
        }
    }

    public BallisticCoefficient Bc
    {
        get => _bc;
        set
        {
            if (_bc.Equals(value)) return;
            _bc = value;
            Notify(nameof(Bc));
            Notify(nameof(BcText));
        }
    }

    /// <summary>Grid text for the Mach column.</summary>
    public string MachText => _mach.ToString("0.####", CultureInfo.CurrentCulture);

    /// <summary>Grid text for the BC column, including its drag table (for example <c>0.462G7</c>).</summary>
    public string BcText => _bc.ToString(CultureInfo.CurrentCulture);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public override string ToString() => $"{MachText} {BcText}";
}

/// <summary>One measured downrange velocity in the drag table editor.</summary>
public sealed class RadarReadingEditModel : INotifyPropertyChanged
{
    private Measurement<DistanceUnit> _distance;
    private Measurement<VelocityUnit> _velocity;

    public Measurement<DistanceUnit> Distance
    {
        get => _distance;
        set
        {
            if (_distance.Equals(value) && _distance.Unit.Equals(value.Unit)) return;
            _distance = value;
            Notify(nameof(Distance));
            Notify(nameof(DistanceText));
        }
    }

    public Measurement<VelocityUnit> Velocity
    {
        get => _velocity;
        set
        {
            if (_velocity.Equals(value) && _velocity.Unit.Equals(value.Unit)) return;
            _velocity = value;
            Notify(nameof(Velocity));
            Notify(nameof(VelocityText));
        }
    }

    // "ND" formats with the unit's own default accuracy, so a grid shows 100m and 3078.8ft/s rather than
    // the full binary expansion of the value.
    public string DistanceText => _distance.ToString("ND", CultureInfo.CurrentCulture);

    public string VelocityText => _velocity.ToString("ND", CultureInfo.CurrentCulture);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public override string ToString() => $"{DistanceText} {VelocityText}";
}
