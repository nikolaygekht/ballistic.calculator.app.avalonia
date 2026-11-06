// Data for the 30 s ad, computed with the same library the application uses (BallisticCalculator 1.1.13).
// Usage: dotnet run -- <out.json>      (the Remotion project reads remotion/public/data.json)
// Loads: bc2-demo of the series (175 gr, G7 0.253, 2600 fps, 3 in sight, 300 yd zero, standard air) and the
// Warner .338 Flatline 285 gr from episode 10.
using BallisticCalculator;
using BallisticCalculator.Tools;
using Gehtsoft.Measurements;
using System.Globalization;
using System.Text.Json;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
var outPath = args.Length > 0 ? args[0] : "data.json";
const string WARNER = @"D:\develop\components\BusinessSpecificComponents\BallisticCalculator.Net\BallisticCalculator.Test\resources\warner_338_flatline.txt";

Measurement<DistanceUnit> Yd(double v) => new(v, DistanceUnit.Yard);
Measurement<DistanceUnit> In(double v) => new(v, DistanceUnit.Inch);
double R(double v, int d = 3) => Math.Round(v, d);

TrajectoryPoint[] Shoot(Ammunition ammo, double sightIn, double zeroYd, double maxYd, double stepYd, DragTable table = null,
                        Rifling rifling = null, Wind[] wind = null, double? lat = null, double? az = null)
{
    var rifle = new Rifle(new Sight(In(sightIn), Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
                          new ZeroingParameters(Yd(zeroYd), null, null), rifling);
    var calc = new TrajectoryCalculator();
    var shot = new ShotParameters { Step = Yd(stepYd), MaximumDistance = Yd(maxYd) };
    shot.Apply(calc.CalculateZeroParameters(ammo, new Atmosphere(), rifle, rifle.Zero, dragTable: table));
    if (lat != null) { shot.Latitude = new(lat.Value, AngularUnit.Degree); shot.BarrelAzimuth = new(az.Value, AngularUnit.Degree); }
    return calc.Calculate(ammo, rifle, new Atmosphere(), shot, wind, table).Where(p => p != null).ToArray();
}

// ---- bc2-demo
var demo = new Ammunition(new(175, WeightUnit.Grain), new BallisticCoefficient(0.253, DragTableId.G7),
                          new(2600, VelocityUnit.FeetPerSecond), In(0.308), In(1.394));
var wind = new[] { new Wind(new(10, VelocityUnit.MilesPerHour), new(90, AngularUnit.Degree)) };
var baseT = Shoot(demo, 3, 300, 1000, 10);
var windT = Shoot(demo, 3, 300, 1000, 10, wind: wind);
var spinT = Shoot(demo, 3, 300, 1000, 10, rifling: new Rifling(In(10), TwistDirection.Right));
var corT = Shoot(demo, 3, 300, 1000, 10, lat: 45, az: 90);
var traj = baseT.Select((p, i) => new
{
    yd = R(p.Distance.In(DistanceUnit.Yard), 1),
    path = R(p.Drop.In(DistanceUnit.Inch), 2),          // inches vs the line of sight
    hold = R(p.DropAdjustment.In(AngularUnit.MRad), 3),
    fps = R(p.Velocity.In(VelocityUnit.FeetPerSecond), 0),
    mach = R(p.Mach, 3),
    t = R(p.Time.TotalSeconds, 3),
    wind = R(windT[i].Windage.In(DistanceUnit.Inch), 2), // left positive (library sign)
    spin = R(spinT[i].Windage.In(DistanceUnit.Inch), 2),
    corH = R(corT[i].Windage.In(DistanceUnit.Inch), 2),
    corV = R(corT[i].Drop.In(DistanceUnit.Inch) - p.Drop.In(DistanceUnit.Inch), 2),
}).ToArray();

// ---- .338 Flatline: G7 0.484 box BC, and the drag table built from Warner's velocities in the range's air
var lines = File.ReadAllLines(WARNER);
var readings = lines.Where(l => l.StartsWith("v;")).Select(l => l.Split(';')).Select(p => new RadarReading(
    Yd(double.Parse(p[1].Replace("yd", ""))), new Measurement<VelocityUnit>(double.Parse(p[2].Replace("ft/s", "")), VelocityUnit.FeetPerSecond))).ToList();
var W = new Measurement<WeightUnit>(285, WeightUnit.Grain); var D = In(0.338);
var socorro = new Atmosphere(new(6000, DistanceUnit.Foot), new(23.98, PressureUnit.InchesOfMercury), new(78, TemperatureUnit.Fahrenheit), 0.25);
var built = RadarDragTableFactory.Create(readings, W, D, socorro, "338 Flatline");
var flat = new Ammunition(W, new BallisticCoefficient(0.484, DragTableId.G7), new(3079, VelocityUnit.FeetPerSecond), D);
var flatT = Shoot(flat, 3, 300, 1000, 10);

var g7 = DragTable.Get(DragTableId.G7);
double sd338 = (285.0 / 7000) / (0.338 * 0.338);
double G7Cd(double m) { for (int i = 1; i < g7.Count; i++) if (g7[i].Mach >= m) { var a = g7[i - 1]; var b = g7[i]; return a.DragCoefficient + (b.DragCoefficient - a.DragCoefficient) * (m - a.Mach) / (b.Mach - a.Mach); } return g7[g7.Count - 1].DragCoefficient; }
var builtPts = Enumerable.Range(0, built.Count).Select(i => new { mach = R(built[i].Mach), cd = R(built[i].DragCoefficient, 4) }).ToArray();
double m0 = builtPts.Min(p => p.mach), m1 = builtPts.Max(p => p.mach);
var g7Shape = Enumerable.Range(0, 61).Select(i => m0 + (m1 - m0) * i / 60).Select(m => new { mach = R(m), cd = R(G7Cd(m) * sd338 / 0.484, 4) }).ToArray();

// ---- Lapua's Doppler-radar drag table of the 175 gr Scenar-L (episodes 6 and 10) and the G7 shape at the box BC
var radar = DrgDragTable.Open(@"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg");
double sdL = (11.30 / 1000 * 2.20462) / Math.Pow(7.83 / 25.4, 2);
var radarPts = Enumerable.Range(0, radar.Count).Select(i => new { mach = R(radar[i].Mach), cd = R(radar[i].DragCoefficient, 4) }).ToArray();
var radarG7 = radarPts.Select(p => new { p.mach, cd = R(G7Cd(p.mach) * sdL / 0.253, 4) }).ToArray();

// ---- reticle holds (bc2-demo) and a 18 in target at 700 yd
double HoldAt(double yd) => baseT.First(p => Math.Abs(p.Distance.In(DistanceUnit.Yard) - yd) < 0.5).DropAdjustment.In(AngularUnit.MRad);
var holds = new[] { 400, 500, 600, 700, 800, 900, 1000 }.Select(y => new { yd = y, mrad = R(HoldAt(y), 2) }).ToArray();

// ---- hit probability: bc2-demo at 300 yd, 8 in vital zone, 2 MOA group, 10 mph wind estimated with 25 % error
var hpRifle = new Rifle(new Sight(In(3), Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO), new ZeroingParameters(Yd(300), null, null));
var hpCalc = new TrajectoryCalculator();
var hpShot = new ShotParameters { Step = Yd(10), MaximumDistance = Yd(300) };
hpShot.Apply(hpCalc.CalculateZeroParameters(demo, new Atmosphere(), hpRifle, hpRifle.Zero));
var hpPar = new HitProbabilityParameters
{
    TargetSize = In(8), GroupSize = new(2, AngularUnit.MOA), MuzzleVelocityDeviationPercent = 1,
    DistanceErrorPercent = 5, WindErrorPercent = 25, Shots = 10000, Seed = 20261106,
};
var hp = HitProbability.Estimate(hpCalc, demo, new Atmosphere(), hpRifle, hpShot, wind, hpPar);
var impacts = hp.Shots.Take(1500).Select(s => new[] { R(s.Horizontal.In(DistanceUnit.Inch), 2), R(s.Vertical.In(DistanceUnit.Inch), 2) }).ToArray();

var data = new
{
    generated = "BallisticCalculator 1.1.13, claude/video/ad30/datagen",
    demo = new { name = ".308 · 175 gr · G7 0.253 · 2600 fps · 300 yd zero", traj },
    compare = new
    {
        a = new { name = ".308 · 175 gr", pts = baseT.Select(p => new[] { R(p.Distance.In(DistanceUnit.Yard), 1), R(p.Drop.In(DistanceUnit.Inch), 2) }) },
        b = new { name = ".338 · 285 gr", pts = flatT.Select(p => new[] { R(p.Distance.In(DistanceUnit.Yard), 1), R(p.Drop.In(DistanceUnit.Inch), 2) }) },
    },
    drag = new
    {
        name = ".338 · 285 gr",
        readings = readings.Select(r => new[] { R(r.Distance.In(DistanceUnit.Yard), 0), R(r.Velocity.In(VelocityUnit.FeetPerSecond), 1) }),
        built = builtPts,
        g7 = g7Shape,
    },
    radar = new { name = ".308 · 175 gr · Doppler radar", pts = radarPts, g7 = radarG7 },
    reticle = new { holds, target = new { yd = 700, sizeIn = 18, mrad = R(18 * 0.0254 / (700 * 0.9144) * 1000, 3) } },
    hit = new
    {
        rangeYd = 300, zoneIn = 8, groupMoa = 2, windMph = 10,
        probability = R(hp.HitProbability, 4), simulated = hp.Shots.Count, impacts,
    },
};
File.WriteAllText(outPath, JsonSerializer.Serialize(data));
Console.WriteLine($"{outPath}: traj {traj.Length} pts, 1000 yd path {traj[^1].path} in, wind {traj[^1].wind} in, spin {traj[^1].spin} in, " +
                  $"coriolis H {traj[^1].corH} V {traj[^1].corV} in; built Cd pts {builtPts.Length}; hit p {hp.HitProbability:P1}; holds " +
                  string.Join(" ", holds.Select(h => $"{h.yd}:{h.mrad}")));
