// Data for the education mini-series, computed with the same library the application uses (BallisticCalculator 1.1.13).
// Usage: dotnet run -c Release -- <out.json>     (the Remotion project reads remotion/public/data.json)
// Load: bc2-demo of the first series — 175 gr .308, G7 0.253, 2600 fps, standard air.
using BallisticCalculator;
using Gehtsoft.Measurements;
using System.Globalization;
using System.Text.Json;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
var outPath = args.Length > 0 ? args[0] : "data.json";
double R(double v, int d = 3) => Math.Round(v, d);

const double BC = 0.253, GRAINS = 175, DIAM_IN = 0.308, G = 9.80665;
var ammo = new Ammunition(new(GRAINS, WeightUnit.Grain), new BallisticCoefficient(BC, DragTableId.G7),
                          new(2600, VelocityUnit.FeetPerSecond), new(DIAM_IN, DistanceUnit.Inch), new(1.394, DistanceUnit.Inch));
var air = new Atmosphere();
var rifle = new Rifle(new Sight(new(1.5, DistanceUnit.Inch), Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
                      new ZeroingParameters(new(100, DistanceUnit.Meter), null, null), null);
var calc = new TrajectoryCalculator();
var shot = new ShotParameters { Step = new(5, DistanceUnit.Meter), MaximumDistance = new(1600, DistanceUnit.Meter) };
shot.Apply(calc.CalculateZeroParameters(ammo, air, rifle, rifle.Zero));
var pts = calc.Calculate(ammo, rifle, air, shot).Where(p => p != null).ToArray();

// The bullet's own Cd: the G7 curve scaled by sectional density / BC (docs/method/ballistic-coefficient.md).
var g7 = DragTable.Get(DragTableId.G7);
double G7Cd(double m)
{
    for (int i = 1; i < g7.Count; i++)
        if (g7[i].Mach >= m)
        {
            var a = g7[i - 1]; var b = g7[i];
            return a.DragCoefficient + (b.DragCoefficient - a.DragCoefficient) * (m - a.Mach) / (b.Mach - a.Mach);
        }
    return g7[g7.Count - 1].DragCoefficient;
}
double sd = GRAINS / 7000.0 / (DIAM_IN * DIAM_IN);                  // lb/in²
double BulletCd(double m) => G7Cd(m) * sd / BC;
double rho = air.Density.In(DensityUnit.KilogramPerCubicMeter);
double mass = GRAINS * 64.79891e-6;                                // kg
double d = DIAM_IN * 0.0254, area = Math.PI * d * d / 4;          // m²
double DragAccel(double v, double m) => 0.5 * rho * v * v * BulletCd(m) * area / mass;   // m/s²

var rows = pts.Select((p, i) =>
{
    double v = p.Velocity.In(VelocityUnit.MetersPerSecond);
    // Engine check: the slope of the engine's own speed curve, minus gravity's component along the path.
    double fd = double.NaN;
    if (i > 0 && i < pts.Length - 1)
    {
        double dv = pts[i + 1].Velocity.In(VelocityUnit.MetersPerSecond) - pts[i - 1].Velocity.In(VelocityUnit.MetersPerSecond);
        double dt = (pts[i + 1].Time - pts[i - 1].Time).TotalSeconds;
        fd = -dv / dt;
    }
    return new
    {
        x = R(p.Distance.In(DistanceUnit.Meter), 1),
        t = R(p.Time.TotalSeconds, 4),
        v = R(v, 1),
        mach = R(p.Mach, 4),
        cd = R(BulletCd(p.Mach), 4),
        drag = R(DragAccel(v, p.Mach), 2),                         // m/s², along the path, backwards
        engineDecel = double.IsNaN(fd) ? (double?)null : R(fd, 2),
        dropCm = R(p.Drop.In(DistanceUnit.Centimeter), 1),
    };
}).ToArray();

var cdCurve = Enumerable.Range(0, 111).Select(i => 0.5 + i * 0.02)
    .Select(m => new { mach = R(m, 2), cd = R(BulletCd(m), 4) }).ToArray();

// Episode 1 scenes 7-9: the standard curves, the bullet's own curve from each BC, Lapua's radar curve.
const string RADAR = @"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg";
var g1 = DragTable.Get(DragTableId.G1);
var radar = DrgDragTable.Open(RADAR);
double Interp(DragTable t, double m)
{
    if (m <= t[0].Mach) return t[0].DragCoefficient;
    for (int i = 1; i < t.Count; i++)
        if (t[i].Mach >= m)
        {
            var a = t[i - 1]; var b = t[i];
            return a.DragCoefficient + (b.DragCoefficient - a.DragCoefficient) * (m - a.Mach) / (b.Mach - a.Mach);
        }
    return t[t.Count - 1].DragCoefficient;
}
var machs = Enumerable.Range(0, 111).Select(i => R(0.5 + i * 0.02, 2)).ToArray();
var curves = new
{
    mach = machs,
    g1 = machs.Select(m => R(Interp(g1, m), 4)).ToArray(),
    g7 = machs.Select(m => R(Interp(g7, m), 4)).ToArray(),
    fromG1 = machs.Select(m => R(Interp(g1, m) * sd / 0.505, 4)).ToArray(),   // own Cd implied by G1 0.505
    fromG7 = machs.Select(m => R(Interp(g7, m) * sd / BC, 4)).ToArray(),      // own Cd implied by G7 0.253
    radar = machs.Select(m => R(Interp(radar, m), 4)).ToArray(),              // a .drg stores the own Cd
};

// The race: the same bullet with three drag models, zero 100 m, sight 50 mm (as numbers/ and NUMBERS.md).
double[] Race(BallisticCoefficient bcr, DragTable table)
{
    var am = new Ammunition(new(GRAINS, WeightUnit.Grain), bcr, new(2600, VelocityUnit.FeetPerSecond),
                            new(DIAM_IN, DistanceUnit.Inch), new(1.394, DistanceUnit.Inch));
    var rf = new Rifle(new Sight(new(50, DistanceUnit.Millimeter), Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
                       new ZeroingParameters(new(100, DistanceUnit.Meter), null, null), null);
    var sp = new ShotParameters { Step = new(10, DistanceUnit.Meter), MaximumDistance = new(1200, DistanceUnit.Meter) };
    sp.Apply(calc.CalculateZeroParameters(am, air, rf, rf.Zero, dragTable: table));
    return calc.Calculate(am, rf, air, sp, null, table).Where(p => p != null).Select(p => R(p.Drop.In(DistanceUnit.Centimeter), 1)).ToArray();
}
var race = new
{
    stepM = 10,
    g1 = Race(new BallisticCoefficient(0.505, DragTableId.G1), null),
    g7 = Race(new BallisticCoefficient(BC, DragTableId.G7), null),
    radar = Race(new BallisticCoefficient(1, DragTableId.GC, BallisticCoefficientValueType.FormFactor), radar),
};

var worst = rows.Where(r => r.engineDecel != null && r.x > 20)
                .Max(r => Math.Abs(r.drag - r.engineDecel.Value) / r.drag);

var data = new
{
    load = new { name = "175 gr .308, G7 0.253", grains = GRAINS, diameterIn = DIAM_IN, bcG7 = BC, muzzleFps = 2600 },
    air = new { rho = R(rho, 4), sound = R(air.SoundVelocity.In(VelocityUnit.MetersPerSecond), 2) },
    g = G,
    rows,
    cdCurve,
    curves,
    race,
};
File.WriteAllText(outPath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false }));

var transonic = rows.FirstOrDefault(r => r.mach < 1.0);
Console.WriteLine($"rows {rows.Length}; muzzle v {rows[0].v} m/s, M {rows[0].mach}, drag {rows[0].drag} m/s² = {rows[0].drag / G:F1} g; " +
                  $"M<1 at {transonic?.x} m, t {transonic?.t} s; end v {rows[^1].v} m/s at {rows[^1].x} m t {rows[^1].t}; " +
                  $"rho {rho:F4}; worst |formula-engine| {worst:P2}");
Console.WriteLine($"race at 300/1000 m: G1 {race.g1[30]}/{race.g1[100]}  G7 {race.g7[30]}/{race.g7[100]}  radar {race.radar[30]}/{race.radar[100]} cm; " +
                  $"own Cd at M2.33: fromG7 {Interp(g7, 2.33) * sd / BC:F3} fromG1 {Interp(g1, 2.33) * sd / 0.505:F3} radar {Interp(radar, 2.33):F3}; " +
                  $"M1.0: {Interp(g7, 1) * sd / BC:F3} {Interp(g1, 1) * sd / 0.505:F3} {Interp(radar, 1):F3}; M0.8: {Interp(g7, .8) * sd / BC:F3} {Interp(g1, .8) * sd / 0.505:F3} {Interp(radar, .8):F3}");
