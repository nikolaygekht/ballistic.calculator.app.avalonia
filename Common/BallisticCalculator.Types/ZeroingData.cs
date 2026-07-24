using BallisticCalculator;
using BallisticCalculator.Serialization;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>
/// App-side model that collects every zeroing-related input in one place: the zero distance,
/// optional zero ammunition / atmosphere overrides, vertical and horizontal impact offsets,
/// a zeroing wind, and a zeroing shot (incline) angle.
///
/// This is a plain data holder. Conversion into the library's <see cref="ZeroingParameters"/> /
/// <see cref="Rifle"/> happens at the calculation sites, and persistence is handled by the
/// serializer via the BXml attributes below (it is stored as its own &lt;zeroing&gt; element).
/// </summary>
[BXmlElement("zeroing")]
public class ZeroingData
{
    [BXmlProperty(Name = "distance", Optional = true)]
    public Measurement<DistanceUnit>? Distance { get; set; }

    [BXmlProperty(Name = "ammunition", ChildElement = true, Optional = true)]
    public Ammunition? Ammunition { get; set; }

    [BXmlProperty(Name = "atmosphere", ChildElement = true, Optional = true)]
    public Atmosphere? Atmosphere { get; set; }

    /// <summary>Vertical impact offset at zero (positive is up).</summary>
    [BXmlProperty(Name = "vertical-offset", Optional = true)]
    public Measurement<DistanceUnit>? VerticalOffset { get; set; }

    /// <summary>Horizontal impact offset at zero (positive is left, per the library convention).</summary>
    [BXmlProperty(Name = "horizontal-offset", Optional = true)]
    public Measurement<DistanceUnit>? HorizontalOffset { get; set; }

    /// <summary>Wind present while zeroing.</summary>
    [BXmlProperty(Name = "wind", ChildElement = true, Optional = true)]
    public Wind? Wind { get; set; }

    /// <summary>Shot (line-of-sight incline) angle while zeroing.</summary>
    [BXmlProperty(Name = "shot-angle", Optional = true)]
    public Measurement<AngularUnit>? ShotAngle { get; set; }
}
