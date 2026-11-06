# Episode 8 — Zero one load, shoot another   (DRAFT for OK — no voice yet)

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 8   ·   Zero One Load, Shoot Another`}
{ACTION: the shared cold open clip.}

Numbers: read from the app 2026-11-04 (`build/ep08/read_numbers.py`, `demo-numbers.md` §8).
Both loads come from the ammunition library; the MVs are corrected to the ones Trijicon's reticle was made for.
Order (Nikolay, 2026-11-05): the supersonic load first — the rifle is zeroed with it — then the subsonic one.

## Intro
i01
This is the first special case. A rifle in three hundred Blackout, with a Trijicon V-COG scope.

i02
Three hundred Blackout is one cartridge with many loads. Two of them matter here. A fast supersonic load, close to the seven point six two by thirty-nine, makes it a classic assault rifle. A heavy, slow subsonic load, close to the nine by thirty-nine, makes it a quiet rifle with a suppressor.

i03
The V-COG for three hundred Blackout is made for both. The rifle is zeroed with the supersonic load, the same reticle has marks for the subsonic one, and you change loads without touching the scope.

## First trajectory: the supersonic load
a01 {Trajectory → New → Metric}
Trajectory, New, and Metric, not by chance: the V-COG ladder is marked in metres.

a02 {Ammunition: Load → .300 AAC → .300 AAC 115gr}
We start with the supersonic load, because this is the load the rifle is zeroed with. Load, three hundred A A C, the other name of the same cartridge, and the one hundred fifteen grain load.

a03 {point at Weight, BC; MV 2295 → 2330}
One hundred fifteen grains, B C zero point two nine, G one. The library says two thousand two hundred ninety-five feet per second. The V-COG reticle was made for two thousand three hundred thirty, so I type that.

a04 {Rifle tab; Sight preset: Standard Optics (High, Mil)}
On the Rifle tab, a scope with milliradian clicks.

a05 {Sight Height 3 → 2.9 in}
The V-COG sits on a tall mount: two point nine inches above the barrel.

a06 {Rifling: Right; Twist 8 in}
And the barrel: right hand, one turn in eight inches.

a07 {Zero tab; point at Distance "100 yd"; switch the unit to m}
On the Zero tab, the sight preset gave us one hundred yards. We zero at one hundred metres, so I switch it to metres.

a08 {OK; standard weather, Yes}
The rifle is zeroed with the load we shoot, so there is nothing more to set. OK, and standard weather.

q01 {Reticle tab; Load… → VCOG16-300BLK; Far BDC}
Now the scope. The Reticle tab, Load, the V-COG for three hundred Blackout, and Far B D C.

q02 {point at the crosshair, then down the ladder: 200, 300, 400, 500, 600 m}
The crosshair is the zero, one hundred metres. And every mark below it is where Trijicon put it: two hundred, three hundred, four hundred, five hundred and six hundred metres.

## Second trajectory: the subsonic load
b01 {Trajectory → New → Metric; Load → .300 AAC 208gr Hornady}
Now the subsonic load, in a second trajectory. New, Metric, Load, and the two hundred eight grain Hornady.

b02 {MV 1020 → 1010}
B C zero point six four eight, G one. The library says one thousand twenty feet per second, the reticle was made for one thousand ten.

b03 {Rifle tab: Standard Optics (High, Mil); 2.9 in; Right; 8 in}
The same rifle: the scope with milliradian clicks, two point nine inches, right hand, one in eight.

b04 {Zero tab: 100 yd → m; tick Other ammunition for zero}
Zero at one hundred metres. And now the important part: Other ammunition for zero. The rifle was zeroed with the supersonic load, not with this one.

b05 {its Load → .300 AAC 115gr; MV 2295 → 2330}
Load, the one hundred fifteen grain load, at two thousand three hundred thirty, the same as in the first trajectory.

b06 {Parameters: 200 m, step 25 m; OK; standard weather, Yes}
Parameters: two hundred metres, every twenty-five. OK, and standard weather.

## What it costs
t01 {point at the 100 m drop}
Look at one hundred metres, the distance the rifle is zeroed at. The subsonic bullet is forty-two centimetres low.

t02 {point at the 200 m drop}
At two hundred metres it is almost two metres low. The scope is right for one load, and very wrong for the other.

t03
This is not a mistake. It is what a shared zero costs, and now you see it before you shoot.

## The same scope for the subsonic load
r01 {Reticle tab; Load… → VCOG16-300BLK; Far BDC}
The same reticle, and Far B D C.

r02 {point at the crosshair, then the top of the ladder and the next two bars}
Trijicon uses one ladder for both loads. For the supersonic load it starts at the crosshair. For the subsonic load it starts lower, at the top of the ladder: fifty, one hundred, one hundred fifty.

r03 {point at Split at "100 m"; type 50}
Far B D C labels the marks after the Split at distance. By default it is the zero, one hundred metres, so the top of the ladder has no label. The subsonic count starts at fifty, so I type fifty.

r04 {point at the 55, 102 and 151 m labels}
And the program agrees: fifty-five, one hundred two, and one hundred fifty-one metres.

r05 {click the first window: the supersonic labels again}
One scope, two loads, one zero. Tell the program which load the zero was made with, and it labels the reticle for the load you shoot.

## Next up
n01
Next time: shooting uphill and downhill, and why the program shows the drop in two different ways.

{CARD 5 s: `Next → Episode 9: Shooting at an Angle`}
