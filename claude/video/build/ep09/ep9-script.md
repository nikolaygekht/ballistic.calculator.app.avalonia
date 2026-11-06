# Episode 9 — Shooting at an angle   (DRAFT for OK — no voice yet)

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 9   ·   Shooting at an Angle`}
{ACTION: the shared cold open clip.}

Numbers: read from the app 2026-11-05 (`build/ep09/read_numbers.py`, `demo-numbers.md` §9).
The shot is `bc2-demo` from episode 2: .308, 175 gr, G7 0.253, 2600 ft/s, zero 300 yd, Hold in mrad.
Agreed with Nikolay 2026-11-05: the two kinds of drop only on the chart, not in the table; the rifleman's rule
with a picture (`card-rifleman.png`); no BDC; the program is "a full model of the flight", never "exact".

## Intro
i01
This is the second special case: shooting uphill and downhill.

i02
In the mountains the target is often above you or below you. The distance can be the same as on the flat, but the hold is not.

## The flat shot
a01 {Trajectory → Open → bc2-demo}
Trajectory, Open. The same file from episode two: our rifle, our load, and a zero at three hundred yards.

a02 {point at the 1000 yd Hold}
On the flat, at one thousand yards, we hold ten milliradians.

a03 {View → Drop → Over Muzzle Level; Chart tab}
Now the chart, with the drop over muzzle level. View, Drop, Over Muzzle Level, and the Chart tab.

a04 {point along the two lines near the muzzle, the crossing, 300 yd}
Two lines: the bullet, and the line of sight. The bullet starts below the line of sight, goes above it, and comes back to it at three hundred yards, our zero. On the flat the line of sight is level, so both kinds of drop are the same.

## Uphill
u01 {View → Edit Parameters → Parameters tab; Shot Angle 30}
Now the same target, thirty degrees uphill. Edit Parameters, the Parameters tab, and the shot angle: thirty.

u02 {point at the Zero tab, no click}
Not the shot angle on the Zero tab. That one is the angle you zeroed at, and this rifle was zeroed on the flat.

u03 {OK; the chart: two slanted lines}
OK. Now both lines go up. At one thousand yards the target is five hundred yards higher than we are: fifteen hundred feet.

u04 {point at the 1000 yd end, ~18,000 in}
This is the drop over muzzle level: where the bullet is in space. At the end it is almost fifteen hundred feet above the muzzle, and still a little below the line of sight.

u05 {zoom into 800–1000 yd; View → Chart → Zoom Y Axis to Visible Range}
Let us zoom in at the end. Now you see the gap. How far the bullet is from the line of sight, measured across it, is the other kind of drop: drop over the line of sight. This is the one you hold.

u06 {View → Drop → Over Line of Sight; Table tab; point at the 1000 yd Hold}
So back to drop over the line of sight, and the table. At one thousand yards the hold is eight point two milliradians, almost two less than on the flat.

u07
If you forget the angle and hold ten, the bullet goes more than five feet over the target.

u08
Why less? Gravity always pulls straight down. When the line of sight is tilted, only part of it pulls the bullet away from the line of sight.

## Downhill
d01 {Edit Parameters; Shot Angle −30; OK; point at the 1000 yd Hold}
And thirty degrees downhill: minus thirty. Eight point four.

d02
Almost the same. Uphill or downhill, the bullet drops less than on the flat, for the same reason: only part of gravity pulls it away from the line of sight.

## The rifleman's rule
r01 {CARD `card-rifleman.png`}
There is an old rule of thumb for this: the rifleman's rule. Hold as if the target were on the flat, at the horizontal distance. Here it is one thousand yards times the cosine of thirty degrees: eight hundred sixty-six yards.

r02 {back to the table; point at the 1000 yd Hold}
On the flat at eight hundred sixty-six yards, the hold is seven point three. The full calculation says eight point two. The rule is much better than nothing, but at one thousand yards the difference is almost one milliradian: almost three feet.

r03
The program does not use a shortcut. It calculates the whole flight along the tilted line of sight. It is still a model, but a full model of the flight, not a simplified rule.

## Next up
n01
Next time: your own drag table. The program can build one from several ballistic coefficients, or from velocities you measured.

{CARD 5 s: `Next → Episode 10: Your Own Drag Table`}
