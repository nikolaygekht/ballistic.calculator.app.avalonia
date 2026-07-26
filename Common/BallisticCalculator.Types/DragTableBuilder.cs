using BallisticCalculator.Tools;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>
/// The metadata a <c>.drg</c> header can carry (BallisticCalculator 1.1.11.2): name, source, bullet weight,
/// diameter and length. Weight and diameter are optional for a BC-curve table, where they are documentation
/// only, but required for a radar table, where they drive the drag recovery. Caliber, ammunition type,
/// barrel length and muzzle velocity have no slot in the format and are deliberately absent.
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
    /// Synthesizes a table by scaling <paramref name="baseTable"/> with an effective-BC-vs-Mach curve.
    /// Use the result with a ballistic coefficient of 1.0 on table <c>GC</c>.
    /// </summary>
    /// <exception cref="ArgumentException">The metadata or the curve is unusable; the message is user-facing.</exception>
    public static DrgDragTable FromBcCurve(DrgMetadata metadata, DragTableId baseTable, IEnumerable<BcAtMach> curve)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(curve);
        RequireName(metadata);

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

        // Unlike the BC curve, these two are physics inputs rather than documentation.
        if (metadata.Weight == null || metadata.Weight.Value.Value <= 0)
            throw new ArgumentException("The bullet weight is required and must be greater than zero.",
                                        nameof(metadata));
        if (metadata.Diameter == null || metadata.Diameter.Value.Value <= 0)
            throw new ArgumentException("The bullet diameter is required and must be greater than zero.",
                                        nameof(metadata));

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

        return RadarDragTableFactory.Create(sorted, metadata.Weight.Value, metadata.Diameter.Value,
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

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Format(double value) => value.ToString("0.####", System.Globalization.CultureInfo.CurrentCulture);
}
