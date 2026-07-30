using BallisticCalculator;
using BallisticCalculator.Serialization;
using Gehtsoft.Measurements;
using BallisticCalculator.Types;
using System.Collections;
using System.Collections.Generic;

namespace BallisticCalculator.Models;

[BXmlElement("trajectory")]
public class TrajectoryFormState
{
    [BXmlProperty(Name = "parameters", ChildElement = true)]
    public TrajectoryFormShotData? ShotData { get; set; }

    [BXmlProperty(Name = "measurement-system")]
    public MeasurementSystem MeasurementSystem { get; set; }

    [BXmlProperty(Name = "angular-units")]
    public AngularUnit AngularUnits { get; set; }

    [BXmlProperty(Name = "chart-mode", Optional = true)]
    public TrajectoryChartMode? ChartMode { get; set; }
}

/// <summary>
/// BXml-serializable wrapper for ShotData.
/// Matches the old WinForms app's format for file compatibility.
/// </summary>
[BXmlElement("shot-data")]
public class TrajectoryFormShotData
{
    [BXmlProperty(Name = "ammunition", ChildElement = true)]
    public AmmunitionLibraryEntry? Ammunition { get; set; }

    // The rifle is now stored as sight + rifling only; all zeroing lives in the <zeroing> element.
    [BXmlProperty(Name = "sight", ChildElement = true, Optional = true)]
    public Sight? Sight { get; set; }

    [BXmlProperty(Name = "rifling", ChildElement = true, Optional = true)]
    public Rifling? Rifling { get; set; }

    [BXmlProperty(Name = "zeroing", ChildElement = true, Optional = true)]
    public ZeroingData? Zeroing { get; set; }

    // Legacy: older files stored the full rifle (incl. zeroing) in <weapon>. Read-only migration
    // path — never written on save (see FromShotData).
    [BXmlProperty(Name = "weapon", ChildElement = true, Optional = true)]
    public Rifle? Weapon { get; set; }

    [BXmlProperty(Name = "atmosphere", ChildElement = true)]
    public Atmosphere? Atmosphere { get; set; }

    [BXmlProperty(Name = "winds", Collection = true, Optional = true)]
    public WindCollection? Wind { get; set; }

    [BXmlProperty(Name = "parameters", ChildElement = true)]
    public ShotParameters? Parameters { get; set; }

    public static TrajectoryFormShotData FromShotData(ShotData data)
    {
        WindCollection? winds = null;
        if (data.Winds != null && data.Winds.Length > 0)
        {
            winds = new WindCollection();
            foreach (var w in data.Winds)
                winds.Add(w);
        }
        return new TrajectoryFormShotData
        {
            Ammunition = data.Ammunition,
            Sight = data.Weapon?.Sight,
            Rifling = data.Weapon?.Rifling,
            Zeroing = data.Zeroing,
            Atmosphere = data.Atmosphere,
            Wind = winds,
            Parameters = data.Parameters,
        };
    }

    public ShotData ToShotData()
    {
        Rifle? weapon;
        ZeroingData? zeroing;

        if (Zeroing != null)
        {
            zeroing = Zeroing;
            var zeroParams = new ZeroingParameters(
                    Zeroing.Distance ?? Measurement<DistanceUnit>.ZERO,
                    Zeroing.Ammunition, Zeroing.Atmosphere)
            {
                VerticalOffset = Zeroing.VerticalOffset,
                HorizontalOffset = Zeroing.HorizontalOffset,
            };
            weapon = Sight == null ? null : new Rifle(Sight, zeroParams, Rifling);
        }
        else if (Weapon != null)
        {
            // Legacy file: zeroing was stored inside <weapon> (no wind / shot angle existed).
            weapon = Weapon;
            zeroing = new ZeroingData
            {
                Distance = Weapon.Zero.Distance,
                Ammunition = Weapon.Zero.Ammunition,
                Atmosphere = Weapon.Zero.Atmosphere,
                VerticalOffset = Weapon.Zero.VerticalOffset,
                HorizontalOffset = Weapon.Zero.HorizontalOffset,
            };
        }
        else
        {
            weapon = null;
            zeroing = null;
        }

        return new ShotData
        {
            Ammunition = Ammunition,
            Weapon = weapon,
            Atmosphere = Atmosphere,
            Winds = Wind?.ToArray(),
            Parameters = Parameters,
            Zeroing = zeroing,
        };
    }
}

/// <summary>
/// BXml-compatible wind collection with Add method required by BXml serializer.
/// </summary>
public class WindCollection : IReadOnlyList<Wind>
{
    private readonly List<Wind> _list = new();

    public Wind this[int index] => _list[index];
    public int Count => _list.Count;

    public void Add(Wind wind) => _list.Add(wind);

    public Wind[] ToArray() => _list.ToArray();

    public IEnumerator<Wind> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
