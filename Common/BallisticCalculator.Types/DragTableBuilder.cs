using BallisticCalculator.Tools;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>
/// The metadata a <c>.drg</c> header can carry (BallisticCalculator 1.1.11.2): name, source, bullet weight,
/// diameter and length.
/// <para>
/// Weight and diameter are <b>required by both</b> table builders and are not merely documentation: the radar
/// factory recovers drag from them, and since 1.1.11.3 the BC-curve factory scales its curve by the sectional
/// density they define. Only <see cref="Length"/> is header-only. Caliber, ammunition type, barrel length and
/// muzzle velocity have no slot in the format and are deliberately absent.
/// </para>
/// </summary>
public sealed record DrgMetadata(
    string Name,
    string? Source,
    Measurement<WeightUnit>? Weight = null,
    Measurement<DistanceUnit>? Diameter = null,
    Measurement<DistanceUnit>? Length = null);

/// <summary>
/// Builds custom (<c>GC</c>) drag tables from a BC-vs-Mach curve or from measured downrange velocities.
/// <para>
/// Everything is validated here, before the library is called, so the editors can show a sentence instead of
/// an exception; the messages are written for the user, not the log.
/// </para>
/// </summary>
public static class DragTableBuilder
{
    /// <summary>Minimum radar readings; <see cref="RadarDragTableFactory"/> itself requires three.</summary>
    public const int MinimumRadarReadings = 3;

    /// <summary>Converts a velocity to a Mach number (standard atmosphere when none is given).</summary>
    public static double VelocityToMach(Measurement<VelocityUnit> velocity, Atmosphere? atmosphere = null)
    {
        var sound = (atmosphere ?? new Atmosphere()).SoundVelocity.In(VelocityUnit.MetersPerSecond);
        return velocity.In(VelocityUnit.MetersPerSecond) / sound;
    }

    /// <summary>Converts a Mach number to a velocity in <paramref name="unit"/>.</summary>
    public static Measurement<VelocityUnit> MachToVelocity(double mach, VelocityUnit unit, Atmosphere? atmosphere = null)
    {
        var sound = (atmosphere ?? new Atmosphere()).SoundVelocity.In(VelocityUnit.MetersPerSecond);
        return new Measurement<VelocityUnit>(mach * sound, VelocityUnit.MetersPerSecond).To(unit);
    }

    /// <summary>
    /// Expresses every knot against <paramref name="baseTable"/>, converting the ones quoted against another
    /// standard table at <b>their own Mach</b>, and reports how many were converted.
    /// <para>
    /// A published data sheet often lists both a G1 and a G7 column, so a hand-typed curve can end up mixing
    /// them — and <c>BcAtMach</c> carries no table, so a G1 number handed to a G7 base curve would simply be
    /// misread. Converting at each knot's own Mach is exact for this purpose: the synthesized table computes
    /// <c>Cd_base(M)/BC(M)</c>, and the conversion multiplies the coefficient by <c>Cd_target(M)/Cd_source(M)</c>,
    /// so the base-curve factors cancel and the resulting Cd at every knot is identical whichever table the
    /// knot was quoted against. Only the interpolation between knots follows the chosen base curve's shape.
    /// (The library's own accuracy caveat concerns converting a whole trajectory at a single reference
    /// velocity, which is not what happens here.)
    /// </para>
    /// </summary>
    public static IReadOnlyList<BcAtMach> NormalizeCurve(IEnumerable<(double Mach, BallisticCoefficient Bc)> knots,
                                                        DragTableId baseTable, out int converted)
    {
        ArgumentNullException.ThrowIfNull(knots);

        if (baseTable == DragTableId.GC)
            throw new ArgumentException("The base table must be a standard curve (G1…RA4), not GC (custom).",
                                       nameof(baseTable));

        converted = 0;
        var result = new List<BcAtMach>();

        foreach (var (mach, bc) in knots)
        {
            if (bc.Table == baseTable)
            {
                result.Add(new BcAtMach(mach, bc.Value));
                continue;
            }

            if (bc.Table == DragTableId.GC)
                throw new ArgumentException($"The knot at Mach {Format(mach)} is quoted against GC (custom), " +
                                            "which has no fixed curve to convert from.", nameof(knots));

            if (bc.ValueType != BallisticCoefficientValueType.Coefficient)
                throw new ArgumentException($"The knot at Mach {Format(mach)} is a form factor; only a " +
                                            "coefficient can be converted between tables.", nameof(knots));

            if (mach <= 0)
                throw new ArgumentException($"Mach {Format(mach)} is not valid — Mach must be greater than zero.",
                                            nameof(knots));

            var target = BallisticCoefficientConverter.Convert(bc, baseTable, mach);
            result.Add(new BcAtMach(mach, target.Value));
            converted++;
        }

        return result;
    }

    /// <summary>
    /// Synthesizes a table by scaling <paramref name="baseTable"/> with an effective-BC-vs-Mach curve.
    /// <para>
    /// Since BallisticCalculator 1.1.11.3 the result holds the projectile's own drag coefficient —
    /// <c>Cd_base(M)/BC(M) * SD</c> — which is the scale a <c>.drg</c> file stores, so it survives a save and
    /// reload and is run with the form factor of one that the factory stamps into the entry. That makes the
    /// bullet weight and diameter inputs rather than documentation: they set the sectional density.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentException">The metadata or the curve is unusable; the message is user-facing.</exception>
    public static DrgDragTable FromBcCurve(DrgMetadata metadata, DragTableId baseTable, IEnumerable<BcAtMach> curve)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(curve);
        RequireName(metadata);

        // Checked here so the editor shows a sentence instead of the library's parameter-name exception.
        RequireBullet(metadata, "the drag curve is scaled by the bullet's sectional density");

        if (baseTable == DragTableId.GC)
            throw new ArgumentException("The base table must be a standard curve (G1…RA4), not GC (custom).",
                                       nameof(baseTable));

        var knots = curve.OrderBy(k => k.Mach).ToArray();
        if (knots.Length == 0)
            throw new ArgumentException("At least one BC knot is required.", nameof(curve));

        foreach (var knot in knots)
        {
            if (knot.Mach <= 0)
                throw new ArgumentException($"Mach {Format(knot.Mach)} is not valid — Mach must be greater than zero.",
                                           nameof(curve));
            if (knot.Bc <= 0)
                throw new ArgumentException($"The ballistic coefficient at Mach {Format(knot.Mach)} must be " +
                                           "greater than zero.", nameof(curve));
        }

        for (int i = 1; i < knots.Length; i++)
        {
            if (knots[i].Mach == knots[i - 1].Mach)
                throw new ArgumentException($"Mach {Format(knots[i].Mach)} appears twice — each knot needs its " +
                                           "own Mach value.", nameof(curve));
        }

        return DrgDragTableFactory.Build(BuildEntry(metadata), baseTable, knots);
    }

    /// <summary>
    /// Recovers a drag curve from measured downrange velocities. <paramref name="atmosphere"/> is the air the
    /// data was measured in (standard when null) — its density drives the recovered coefficients.
    /// </summary>
    /// <exception cref="ArgumentException">The metadata or the readings are unusable; the message is user-facing.</exception>
    public static DrgDragTable FromRadarReadings(DrgMetadata metadata, IEnumerable<RadarReading> readings,
                                                 Atmosphere? atmosphere = null)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(readings);
        RequireName(metadata);

        var (weight, diameter) = RequireBullet(metadata, "the drag recovery depends on them");

        var sorted = readings.OrderBy(r => r.Distance.In(DistanceUnit.Meter)).ToArray();
        if (sorted.Length < MinimumRadarReadings)
            throw new ArgumentException($"At least {MinimumRadarReadings} readings are required; " +
                                       $"there {(sorted.Length == 1 ? "is" : "are")} {sorted.Length}.",
                                       nameof(readings));

        for (int i = 0; i < sorted.Length; i++)
        {
            if (sorted[i].Velocity.In(VelocityUnit.MetersPerSecond) <= 0)
                throw new ArgumentException($"The velocity at {sorted[i].Distance} must be greater than zero.",
                                           nameof(readings));

            if (i == 0)
                continue;

            var previous = sorted[i - 1];
            if (sorted[i].Distance.In(DistanceUnit.Meter) == previous.Distance.In(DistanceUnit.Meter))
                throw new ArgumentException($"Two readings share the distance {sorted[i].Distance} — " +
                                           "each reading needs its own distance.", nameof(readings));

            // A bullet only slows down; a rising velocity means transposed rows or a mistyped value, and
            // the library would throw a less helpful error further in.
            if (sorted[i].Velocity.In(VelocityUnit.MetersPerSecond) >= previous.Velocity.In(VelocityUnit.MetersPerSecond))
                throw new ArgumentException($"The velocity at {sorted[i].Distance} ({sorted[i].Velocity}) is not " +
                                           $"lower than at {previous.Distance} ({previous.Velocity}) — velocity " +
                                           "must decrease with distance.", nameof(readings));
        }

        return RadarDragTableFactory.Create(sorted, weight, diameter,
                                            atmosphere, metadata.Name.Trim(),
                                            metadata.Length, NullIfBlank(metadata.Source));
    }

    /// <summary>
    /// The library entry written into the file. The ballistic coefficient records how the table must be
    /// used: a form factor of 1 on <c>GC</c>.
    /// </summary>
    private static AmmunitionLibraryEntry BuildEntry(DrgMetadata metadata) => new()
    {
        Name = metadata.Name.Trim(),
        Source = NullIfBlank(metadata.Source),
        Ammunition = new Ammunition
        {
            BallisticCoefficient = new BallisticCoefficient(1, DragTableId.GC, BallisticCoefficientValueType.FormFactor),
            Weight = metadata.Weight ?? Measurement<WeightUnit>.ZERO,
            BulletDiameter = metadata.Diameter,
            BulletLength = metadata.Length,
        },
    };

    private static void RequireName(DrgMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.Name))
            throw new ArgumentException("A name for the drag table is required.", nameof(metadata));
    }

    /// <summary>Both builders need a real bullet; <paramref name="why"/> says what it is used for.</summary>
    /// <summary>
    /// Checks the two inputs the curve is scaled by and hands them back non-null, so callers pass them on
    /// without a null-forgiving dereference the compiler cannot verify.
    /// </summary>
    private static (Measurement<WeightUnit> Weight, Measurement<DistanceUnit> Diameter) RequireBullet(
        DrgMetadata metadata, string why)
    {
        if (metadata.Weight == null || metadata.Weight.Value.Value <= 0)
            throw new ArgumentException($"The bullet weight is required — {why}.", nameof(metadata));

        if (metadata.Diameter == null || metadata.Diameter.Value.Value <= 0)
            throw new ArgumentException($"The bullet diameter is required — {why}.", nameof(metadata));

        return (metadata.Weight.Value, metadata.Diameter.Value);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Format(double value) => value.ToString("0.####", System.Globalization.CultureInfo.CurrentCulture);
}
