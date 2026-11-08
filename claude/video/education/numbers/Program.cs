// Numbers quoted in the education scripts (SCRIPTS.md), computed with BallisticCalculator 1.1.13.
// Usage: dotnet run -c Release      (prints; NUMBERS.md records the output)
// Load: 175 gr Lapua Scenar-L .308, 790 m/s (2592 fps), sight 50 mm, 100 m zero, standard air.
using BallisticCalculator;
using Gehtsoft.Measurements;
using System.Globalization;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
const string RADAR = @"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg";
Measurement<DistanceUnit> M(double v) => new(v, DistanceUnit.Meter);

Ammunition Ammo(BallisticCoefficient bc) => new(new(175, WeightUnit.Grain), bc, new(2600, VelocityUnit.FeetPerSecond),
                                                 new(0.308, DistanceUnit.Inch), new(1.394, DistanceUnit.Inch));
TrajectoryPoint[] Shoot(Ammunition ammo, DragTable table = null, Rifling rifling = null, Wind[] wind = null,
                        double? lat = null, double? az = null, TrajectoryCalculator calc = null, double max = 1200, double step = 100)
{
    calc ??= new TrajectoryCalculator();
    var rifle = new Rifle(new Sight(new(50, DistanceUnit.Millimeter), Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
                          new ZeroingParameters(M(100), null, null), rifling);
    var shot = new ShotParameters { Step = M(step), MaximumDistance = M(max) };
    shot.Apply(calc.CalculateZeroParameters(ammo, new Atmosphere(), rifle, rifle.Zero, dragTable: table));
    if (lat != null) { shot.Latitude = new(lat.Value, AngularUnit.Degree); shot.BarrelAzimuth = new(az.Value, AngularUnit.Degree); }
    return calc.Calculate(ammo, rifle, new Atmosphere(), shot, wind, table).Where(p => p != null).ToArray();
}
TrajectoryPoint At(TrajectoryPoint[] t, double m) => t.First(p => Math.Abs(p.Distance.In(DistanceUnit.Meter) - m) < 0.01);
string Row(TrajectoryPoint p) => $"{p.Distance.In(DistanceUnit.Meter),5:F0} m  v {p.Velocity.In(VelocityUnit.MetersPerSecond),5:F0} m/s  " +
    $"M {p.Mach:F2}  drop {p.Drop.In(DistanceUnit.Centimeter),7:F1} cm  ({p.DropAdjustment.In(AngularUnit.MRad),6:F2} mrad)  " +
    $"windage {p.Windage.In(DistanceUnit.Centimeter),6:F1} cm  t {p.Time.TotalSeconds:F3} s";

Console.WriteLine("== Episode 1: the same bullet, three drag models (zero 100 m)");
var radar = DrgDragTable.Open(RADAR);
var g1 = Shoot(Ammo(new BallisticCoefficient(0.505, DragTableId.G1)));
var g7 = Shoot(Ammo(new BallisticCoefficient(0.253, DragTableId.G7)));
var rd = Shoot(Ammo(new BallisticCoefficient(1, DragTableId.GC, BallisticCoefficientValueType.FormFactor)), table: radar);
foreach (var m in new[] { 300.0, 600, 800, 1000, 1200 })
{
    Console.WriteLine("G1 0.505 " + Row(At(g1, m)));
    Console.WriteLine("G7 0.253 " + Row(At(g7, m)));
    Console.WriteLine("radar    " + Row(At(rd, m)));
}

Console.WriteLine("== Episode 2: semi-implicit Euler, step cap -> drop at 1000 m (RK2 1 cm as reference)");
var refc = new TrajectoryCalculator { Integrator = IntegrationMethod.MidpointRK2, MaximumCalculationStepSize = M(0.01) };
var refP = At(Shoot(Ammo(new BallisticCoefficient(0.253, DragTableId.G7)), calc: refc, step: 1000, max: 1000), 1000);
Console.WriteLine("reference " + Row(refP));
foreach (var cap in new[] { 200.0, 100, 50, 10, 1, 0.1 })
{
    var c = new TrajectoryCalculator { Integrator = IntegrationMethod.Euler, MaximumCalculationStepSize = M(cap) };
    try
    {
        var p = At(Shoot(Ammo(new BallisticCoefficient(0.253, DragTableId.G7)), calc: c, step: 1000, max: 1000), 1000);
        Console.WriteLine($"Euler cap {cap,6} m: drop {p.Drop.In(DistanceUnit.Centimeter):F1} cm, error {(p.Drop - refP.Drop).In(DistanceUnit.Centimeter):F2} cm, v {p.Velocity.In(VelocityUnit.MetersPerSecond):F1}");
    }
    catch (Exception e) { Console.WriteLine($"Euler cap {cap} m: {e.GetType().Name}"); }
}

Console.WriteLine("== Episode 3: the add-ons at 1000 m (G7 0.253)");
var bc = new BallisticCoefficient(0.253, DragTableId.G7);
var twist = new Rifling(new(10, DistanceUnit.Inch), TwistDirection.Right);
var cross = new[] { new Wind(new(4.5, VelocityUnit.MetersPerSecond), new(90, AngularUnit.Degree)) };   // ~10 mph from the right
var bse = Shoot(Ammo(bc));
var spin = Shoot(Ammo(bc), rifling: twist);
var windNoSpin = Shoot(Ammo(bc), wind: cross);
var windSpin = Shoot(Ammo(bc), rifling: twist, wind: cross);
var corE = Shoot(Ammo(bc), lat: 45, az: 90);
var corW = Shoot(Ammo(bc), lat: 45, az: 270);
var corN = Shoot(Ammo(bc), lat: 45, az: 0);
foreach (var m in new[] { 500.0, 1000 })
{
    Console.WriteLine($"-- {m} m");
    Console.WriteLine("base          " + Row(At(bse, m)));
    Console.WriteLine("twist 10 R    " + Row(At(spin, m)));
    Console.WriteLine("wind, no twist" + Row(At(windNoSpin, m)));
    Console.WriteLine("wind + twist  " + Row(At(windSpin, m)));
    Console.WriteLine("45N east      " + Row(At(corE, m)));
    Console.WriteLine("45N west      " + Row(At(corW, m)));
    Console.WriteLine("45N north     " + Row(At(corN, m)));
}
