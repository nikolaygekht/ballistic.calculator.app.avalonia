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

    /// <summary>
    /// All zeroing-related inputs (distance, zero ammo/atmosphere, impact offsets, zeroing wind and
    /// shot angle). <see cref="Weapon"/> carries only sight + rifling; the zero is built from this.
    /// </summary>
    public ZeroingData? Zeroing { get; set; }
}
