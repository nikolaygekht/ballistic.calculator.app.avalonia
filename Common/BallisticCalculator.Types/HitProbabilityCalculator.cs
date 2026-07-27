using BallisticCalculator.Tools;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>A shooting position and the aim-scatter multipliers it implies.</summary>
/// <param name="Name">The display name.</param>
/// <param name="Horizontal">Horizontal multiplier on the supported group.</param>
/// <param name="Vertical">Vertical multiplier on the supported group.</param>
/// <param name="IsCustom">True for the placeholder chosen when the multipliers match no preset.</param>
public sealed record ShootingPosition(string Name, double Horizontal, double Vertical, bool IsCustom = false)
{
    public override string ToString() => Name;
}

/// <summary>
/// Everything the user enters for a hit probability estimate. The measurements are nullable because the panel
/// reads them from possibly-empty controls; <see cref="HitProbabilityCalculator.Estimate"/> says which one is
/// missing.
/// </summary>
public sealed record HitProbabilityInputs
{
    /// <summary>The target range. Independent of the shot's own maximum distance.</summary>
    public Measurement<DistanceUnit>? Distance { get; init; }

    /// <summary>Diameter of the circular vital zone.</summary>
    public Measurement<DistanceUnit>? TargetSize { get; init; }

    /// <summary>The shooter's group, as a one-standard-deviation per-axis angle from a supported position.</summary>
    public Measurement<AngularUnit>? GroupSize { get; init; }

    /// <summary>Horizontal multiplier on the supported group for the shooting position.</summary>
    public double HorizontalSpread { get; init; } = 1;

    /// <summary>Vertical multiplier on the supported group for the shooting position.</summary>
    public double VerticalSpread { get; init; } = 1;

    /// <summary>Range estimation error, one standard deviation as a percent of the range.</summary>
    public double RangeErrorPercent { get; init; }

    /// <summary>Wind estimation error, one standard deviation as a percent of the wind speed.</summary>
    public double WindErrorPercent { get; init; }

    /// <summary>
    /// How consistently the ammunition leaves the barrel: the muzzle velocity's standard deviation as a percent
    /// of the muzzle velocity. A property of the ammunition, not an estimation error the shooter makes.
    /// </summary>
    public double MuzzleVelocityDeviationPercent { get; init; }

    /// <summary>Shots to simulate, between <see cref="HitProbabilityCalculator.MinimumShots"/> and
    /// <see cref="HitProbabilityCalculator.MaximumShots"/>.</summary>
    public int Shots { get; init; } = 10000;

    /// <summary>Random seed; null re-rolls on every run, so the answer moves by the Monte-Carlo noise.</summary>
    public int? Seed { get; init; } = 1;
}

/// <summary>
/// A hit probability estimate: the library's result plus the two radii that describe the spread in a line of
/// text.
/// </summary>
/// <param name="HitProbability">Single-shot probability, 0…1.</param>
/// <param name="Impacts">Every simulated impact relative to the target centre (positive left and up).</param>
/// <param name="MeanRadialMiss">Mean distance from the target centre.</param>
/// <param name="NinetiethPercentileMiss">The radius containing 90% of the impacts.</param>
/// <param name="ShotsFor50Percent">Shots for a 50% chance of at least one hit; null when a hit is impossible.</param>
/// <param name="ShotsFor75Percent">As above, 75%.</param>
/// <param name="ShotsFor90Percent">As above, 90%.</param>
/// <param name="ShotsFor95Percent">As above, 95%.</param>
/// <param name="ShotsFor98Percent">As above, 98%.</param>
public sealed record HitProbabilityEstimate(
    double HitProbability,
    IReadOnlyList<ShotImpact> Impacts,
    Measurement<DistanceUnit> MeanRadialMiss,
    Measurement<DistanceUnit> NinetiethPercentileMiss,
    int? ShotsFor50Percent,
    int? ShotsFor75Percent,
    int? ShotsFor90Percent,
    int? ShotsFor95Percent,
    int? ShotsFor98Percent);

/// <summary>
/// Turns a <see cref="ShotData"/> plus the user's error budget into a <see cref="HitProbabilityEstimate"/>,
/// wrapping <see cref="HitProbability.Estimate"/>.
/// <para>
/// The shot geometry (incline, cant, azimuth, latitude) is carried over, but the <b>dialed click adjustments
/// are deliberately not</b>: the library's model computes the come-up and wind hold for the range and wind the
/// shooter estimated, so a scope already dialed for the target would count the hold twice.
/// </para>
/// <para>
/// Validation happens here, ahead of the library call, so the panel can show a sentence instead of an
/// exception; the messages are written for the user, not the log.
/// </para>
/// </summary>
public static class HitProbabilityCalculator
{
    /// <summary>Fewer shots than this and the Monte-Carlo noise (±1.3% at 1000) swamps the answer.</summary>
    public const int MinimumShots = 1000;

    /// <summary>
    /// More shots than this stalls a live-recomputing UI: 50 000 costs about 70 ms, 1 000 000 about 450 ms.
    /// </summary>
    public const int MaximumShots = 50000;

    /// <summary>
    /// The shooting positions offered, with the multipliers from the library's own documentation. Custom is
    /// last and carries no multipliers — it is what the panel selects when the numbers match no preset.
    /// </summary>
    public static IReadOnlyList<ShootingPosition> ShootingPositions { get; } = new ShootingPosition[]
    {
        new("Supported", 1, 1),
        new("Prone", 2, 2),
        new("Kneeling", 4, 3),
        new("Standing", 5, 4),
        new("Custom", 1, 1, IsCustom: true),
    };

    /// <summary>The preset with these multipliers, or null when they match none of them.</summary>
    public static ShootingPosition? PositionFor(double horizontal, double vertical) =>
        ShootingPositions.FirstOrDefault(p => !p.IsCustom &&
                                              Math.Abs(p.Horizontal - horizontal) < 1e-9 &&
                                              Math.Abs(p.Vertical - vertical) < 1e-9);

    /// <summary>
    /// Estimates the hit probability for <paramref name="shotData"/> at the distance and error budget in
    /// <paramref name="inputs"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Any input the estimate cannot use, with a message for the user.</exception>
    public static HitProbabilityEstimate Estimate(ShotData shotData, HitProbabilityInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(shotData);
        ArgumentNullException.ThrowIfNull(inputs);

        // No paramName on these: the message is shown to the user verbatim, and ArgumentException would append
        // "(Parameter '…')" to it.
        if (shotData.Ammunition?.Ammunition == null)
            throw new ArgumentException("The shot has no ammunition to estimate with.");
        if (shotData.Weapon == null)
            throw new ArgumentException("The shot has no rifle to estimate with.");

        if (inputs.Distance == null || inputs.Distance.Value.In(DistanceUnit.Meter) <= 0)
            throw new ArgumentException("Enter the target distance.");
        if (inputs.TargetSize == null || inputs.TargetSize.Value.In(DistanceUnit.Meter) <= 0)
            throw new ArgumentException("Enter a target size (vital zone diameter) greater than zero.");
        if (inputs.GroupSize == null || inputs.GroupSize.Value.In(AngularUnit.Radian) <= 0)
            throw new ArgumentException("Enter a group size greater than zero.");

        if (inputs.Shots < MinimumShots || inputs.Shots > MaximumShots)
            throw new ArgumentException(
                $"The number of shots must be between {MinimumShots} and {MaximumShots}.");

        if (inputs.RangeErrorPercent < 0 || inputs.WindErrorPercent < 0 ||
            inputs.MuzzleVelocityDeviationPercent < 0)
            throw new ArgumentException("The errors and deviations cannot be negative.");

        if (inputs.HorizontalSpread <= 0 || inputs.VerticalSpread <= 0)
            throw new ArgumentException("The position spread multipliers must be greater than zero.");

        var ammunition = shotData.Ammunition.Ammunition;
        var atmosphere = shotData.Atmosphere ?? new Atmosphere();
        var zeroing = ZeroingCalculator.BuildInputs(shotData, atmosphere);
        var p = shotData.Parameters;
        var distance = inputs.Distance.Value;

        // The target is at the shot's maximum distance, so that is the distance being asked about. Step is
        // immaterial — the library rebuilds its own dense curve — but is set to something sane anyway.
        var shot = new ShotParameters
        {
            MaximumDistance = distance,
            Step = distance / 10,
            ShotAngle = p?.ShotAngle,
            CantAngle = p?.CantAngle,
            BarrelAzimuth = p?.BarrelAzimuth,
            Latitude = p?.Latitude,
        };

        // GC ballistic coefficients need their custom .drg supplied to both calls, as elsewhere in the app.
        var zeroTable = CustomDragTableLoader.ForAmmunition(zeroing.ZeroAmmunition);
        var shotTable = CustomDragTableLoader.ForAmmunition(ammunition);

        var calculator = new TrajectoryCalculator();
        shot.Apply(calculator.CalculateZeroParameters(
            zeroing.ZeroAmmunition, zeroing.ZeroAtmosphere, zeroing.Rifle, zeroing.ZeroParameters,
            shot: zeroing.ZeroShot, wind: zeroing.ZeroWind, dragTable: zeroTable));

        var parameters = new HitProbabilityParameters
        {
            TargetSize = inputs.TargetSize.Value,
            GroupSize = inputs.GroupSize.Value,
            HorizontalPositionMultiplier = inputs.HorizontalSpread,
            VerticalPositionMultiplier = inputs.VerticalSpread,
            DistanceErrorPercent = inputs.RangeErrorPercent,
            WindErrorPercent = inputs.WindErrorPercent,
            MuzzleVelocityDeviationPercent = inputs.MuzzleVelocityDeviationPercent,
            Shots = inputs.Shots,
            Seed = inputs.Seed,
        };

        var result = HitProbability.Estimate(calculator, ammunition, atmosphere, zeroing.Rifle, shot,
                                             shotData.Winds, parameters, shotTable);

        var (mean, ninetieth) = Radii(result.Shots);

        return new HitProbabilityEstimate(
            result.HitProbability, result.Shots, mean, ninetieth,
            result.ShotsFor50Percent, result.ShotsFor75Percent, result.ShotsFor90Percent,
            result.ShotsFor95Percent, result.ShotsFor98Percent);
    }

    /// <summary>
    /// Thins the impacts to at most <paramref name="limit"/> evenly spaced samples, so a 50 000-shot run does
    /// not ask the plot to draw 50 000 markers. Fewer than the limit are returned untouched.
    /// </summary>
    public static IReadOnlyList<ShotImpact> SampleImpacts(IReadOnlyList<ShotImpact> impacts, int limit)
    {
        ArgumentNullException.ThrowIfNull(impacts);

        if (limit <= 0)
            return Array.Empty<ShotImpact>();
        if (impacts.Count <= limit)
            return impacts;

        var sample = new ShotImpact[limit];
        for (var i = 0; i < limit; i++)
            sample[i] = impacts[(int)((long)i * impacts.Count / limit)];

        return sample;
    }

    /// <summary>Mean radial miss and the radius holding 90% of the impacts.</summary>
    private static (Measurement<DistanceUnit> Mean, Measurement<DistanceUnit> Ninetieth) Radii(
        IReadOnlyList<ShotImpact> impacts)
    {
        if (impacts.Count == 0)
            return (Measurement<DistanceUnit>.ZERO, Measurement<DistanceUnit>.ZERO);

        var radii = new double[impacts.Count];
        for (var i = 0; i < impacts.Count; i++)
        {
            var h = impacts[i].Horizontal.In(DistanceUnit.Meter);
            var v = impacts[i].Vertical.In(DistanceUnit.Meter);
            radii[i] = Math.Sqrt(h * h + v * v);
        }

        var mean = radii.Average();
        Array.Sort(radii);
        var index = Math.Min(radii.Length - 1, (int)Math.Ceiling(0.9 * radii.Length) - 1);

        return (new Measurement<DistanceUnit>(mean, DistanceUnit.Meter),
                new Measurement<DistanceUnit>(radii[Math.Max(0, index)], DistanceUnit.Meter));
    }
}
