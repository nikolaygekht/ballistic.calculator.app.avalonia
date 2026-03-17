using BallisticCalculator;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

public class ShotData
{
    public AmmunitionLibraryEntry? Ammunition { get; set; }
    public Rifle? Weapon { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public Wind[]? Winds { get; set; }
    public ShotParameters? Parameters { get; set; }
}
