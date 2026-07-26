using System.ComponentModel;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Models;

/// <summary>
/// One BC-vs-Mach knot being edited. <see cref="Mach"/> is the canonical key — the editor can display and
/// accept the knot as a velocity instead, and converting for display only keeps the toggle lossless.
/// </summary>
public sealed class BcKnotEditModel : INotifyPropertyChanged
{
    private string _display = "";

    public double Mach { get; set; }

    public double Bc { get; set; }

    /// <summary>List text, recomputed by the panel because it depends on the current display mode.</summary>
    public string Display
    {
        get => _display;
        set
        {
            if (_display == value) return;
            _display = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => _display;
}

/// <summary>One measured downrange velocity being edited.</summary>
public sealed class RadarReadingEditModel : INotifyPropertyChanged
{
    private string _display = "";

    public Measurement<DistanceUnit> Distance { get; set; }

    public Measurement<VelocityUnit> Velocity { get; set; }

    /// <summary>List text, recomputed by the panel so it follows the values as they are edited.</summary>
    public string Display
    {
        get => _display;
        set
        {
            if (_display == value) return;
            _display = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => _display;
}
