using System.ComponentModel;
using BallisticCalculator;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Models;

/// <summary>Mutable working copy of a sight dictionary entry, edited by the Tools → Edit Sights dialog.</summary>
public sealed class SightEditModel : INotifyPropertyChanged
{
    private string _name = "";

    /// <summary>Display name (notifies so the list refreshes as it is typed).</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public Measurement<DistanceUnit>? SightHeight { get; set; }
    public Measurement<DistanceUnit>? DefaultZero { get; set; }
    public Measurement<AngularUnit>? HorizontalClick { get; set; }
    public Measurement<AngularUnit>? VerticalClick { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => _name;
}

/// <summary>Mutable working copy of a barrel dictionary entry, edited by the Tools → Edit Barrels dialog.</summary>
public sealed class BarrelEditModel : INotifyPropertyChanged
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public Measurement<DistanceUnit>? Step { get; set; }
    public TwistDirection Direction { get; set; } = TwistDirection.Right;

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => _name;
}
