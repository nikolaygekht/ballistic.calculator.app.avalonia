using System;
using System.IO;

namespace BallisticCalculator.Types;

/// <summary>
/// Standard data folders shipped next to the executable (see the <c>data/</c> copy in the desktop
/// project). Used as the default open/save locations for the corresponding file types.
/// </summary>
public static class DataFolders
{
    /// <summary>The <c>data</c> folder next to the executable.</summary>
    public static string Root => Path.Combine(AppContext.BaseDirectory, "data");

    /// <summary>Reticle definitions (<c>.reticle</c>).</summary>
    public static string Reticles => Path.Combine(Root, "reticle");

    /// <summary>Legacy and current ammunition files (<c>.ammo</c> / <c>.ammox</c>).</summary>
    public static string LegacyAmmo => Path.Combine(Root, "legacy-ammo");

    /// <summary>Radar/custom drag tables (<c>.drg</c>).</summary>
    public static string Drg => Path.Combine(Root, "drg");
}
