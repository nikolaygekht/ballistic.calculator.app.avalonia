using BallisticCalculator.Tools;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>
/// A ballistic coefficient expressed against another standard drag table, together with the reference it was
/// computed at — the reference is part of the answer, not a setting, because the number is only exact there.
/// </summary>
/// <param name="Converted">The coefficient against the target table.</param>
/// <param name="ReferenceVelocity">The velocity the two curves were matched at.</param>
/// <param name="ReferenceMach">That velocity as a Mach number, in the atmosphere supplied.</param>
/// <param name="IsTransonic">
/// True when the reference sits below <see cref="BcConversionCalculator.TransonicMach"/>, where the standard
/// curves diverge in shape and the converted number stops being trustworthy.
/// </param>
public sealed record BcConversion(
    BallisticCoefficient Converted,
    Measurement<VelocityUnit> ReferenceVelocity,
    double ReferenceMach,
    bool IsTransonic);

/// <summary>
/// Converts a published ballistic coefficient from one standard drag table to another (the everyday G1 ↔ G7
/// question) at a reference velocity.
/// <para>
/// The physics is a ratio of the two reference curves at one Mach number:
/// <c>BC_target = BC_source · Cd_target(M) / Cd_source(M)</c>. The same projectile has the same drag whichever
/// curve describes it, but the G1 and G7 curves differ in <em>shape</em>, so a single converted number is exact
/// only at the reference it was computed for — within about 1% of manufacturer-published pairs between Mach 1.8
/// and 2.5, degrading to roughly 9% low near Mach 1.3. Hence <see cref="BcConversion.IsTransonic"/>: the caller
/// is expected to say so rather than present the number bare.
/// </para>
/// <para>
/// Validation lives here, ahead of the library call, so the UI can show a sentence instead of an exception; the
/// messages are written for the user, not the log.
/// </para>
/// </summary>
public static class BcConversionCalculator
{
    /// <summary>
    /// Below this Mach number the two reference curves diverge in shape and the conversion loses accuracy.
    /// </summary>
    public const double TransonicMach = 1.5;

    /// <summary>
    /// The tables a conversion can name: every standard curve, never <see cref="DragTableId.GC"/>, which is a
    /// placeholder for a custom table and has no fixed curve to take a Cd from.
    /// </summary>
    public static IReadOnlyList<DragTableId> StandardTables { get; } =
        Enum.GetValues<DragTableId>().Where(id => id != DragTableId.GC).ToArray();

    /// <summary>
    /// Converts <paramref name="source"/> to <paramref name="targetTable"/> at <paramref name="referenceVelocity"/>.
    /// </summary>
    /// <param name="source">
    /// The published coefficient. Must be a coefficient rather than a form factor, and quoted against a standard
    /// table.
    /// </param>
    /// <param name="targetTable">The table to convert to; not <see cref="DragTableId.GC"/>.</param>
    /// <param name="referenceVelocity">
    /// The velocity representative of the intended range band. Use a supersonic one where possible.
    /// </param>
    /// <param name="atmosphere">
    /// The air whose speed of sound turns the velocity into a Mach number; null means sea-level standard. Nothing
    /// else about the atmosphere matters here.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Any input the conversion cannot use, with a message meant for the user.
    /// </exception>
    public static BcConversion Convert(BallisticCoefficient? source, DragTableId targetTable,
                                       Measurement<VelocityUnit>? referenceVelocity,
                                       Atmosphere? atmosphere = null)
    {
        // No paramName on these: the message is shown to the user verbatim, and ArgumentException appends
        // "(Parameter 'source')" to Message when one is supplied.
        if (source == null)
            throw new ArgumentException("Enter the source ballistic coefficient.");

        var bc = source.Value;

        if (bc.ValueType != BallisticCoefficientValueType.Coefficient)
            throw new ArgumentException(
                "A form factor cannot be converted between tables — enter a ballistic coefficient.");

        if (bc.Value <= 0)
            throw new ArgumentException("The source ballistic coefficient must be greater than zero.");

        if (bc.Table == DragTableId.GC)
            throw new ArgumentException(
                "A coefficient against the custom (GC) table cannot be converted: GC has no fixed curve to " +
                "compare with. Use the Approximate Drag Table tools for custom curves.");

        if (targetTable == DragTableId.GC)
            throw new ArgumentException(
                "The custom (GC) table has no fixed curve to convert to. Pick a standard table (G1…RA4).");

        if (referenceVelocity == null)
            throw new ArgumentException("Enter the reference velocity the conversion should match at.");

        var velocity = referenceVelocity.Value;

        if (velocity.In(VelocityUnit.MetersPerSecond) <= 0)
            throw new ArgumentException("The reference velocity must be greater than zero.");

        var mach = DragTableBuilder.VelocityToMach(velocity, atmosphere);
        var converted = BallisticCoefficientConverter.Convert(bc, targetTable, velocity, atmosphere);

        return new BcConversion(converted, velocity, mach, mach < TransonicMach);
    }
}
