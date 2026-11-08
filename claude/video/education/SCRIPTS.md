# Education mini-series — scripts

Three animated explainers about how the calculator works inside. Each is about 5–6 minutes long.
Every number is in `NUMBERS.md`, and each one comes from the library.

**How to read this file**

| | |
|---|---|
| `{ANIM: …}` | what the animation shows. Not spoken. |
| `VOICE:` | the text for ElevenLabs, written the way it should be **said**. Numbers are written in words. |

**Rules for the whole series**

- **Voice.** The author's own clone, plain B2 English, short sentences, and spoken rather than read.
  No idioms. A term is explained the first time it is used.
- **Look.** Line art: one champagne line on deep midnight, coral only for drag. Everything is drawn by the
  line; nothing just pops in.
- **Motion is real.** Every arrow, velocity and curve comes from the library's data, not from a drawn curve.
  If a picture exaggerates something (the nose angle in episode 3), the screen says so ("×20").
- **Units.** Metric. The bullet is the first series' 175 gr Scenar-L at 790 m/s.
- **Bridges.** Each episode ends on the picture the next one starts from: episode 1's bullet turns into
  a dot, and episode 2's dot grows back into a spinning bullet.
- **Sync.** The animation follows the voice: a term lights up while it is being said, not after.
- **Formulas** (episodes 1 and 3) are written on screen by the line itself, term by term, in the same
  champagne. **Every term is acted out, full screen, on the object itself** — the bullet, the barrel, the
  globe — not as a small icon beside the formula: the bullet turns to show its front, the arrow folds into
  a square, the outline fills up like a glass. Then the picture shrinks and flies back into its term, which
  stays lit. Use the graphics fully: camera moves, 3D turns, morphs, flow lines. Units used inside a
  formula (grains, inches, mph, MOA) get a small tag, because the formulas are published in those units.

---

# Episode 1 — Drag: where the velocity goes

## 1. Hook  (~20 s)

{ANIM: Dark screen. The line draws the bullet in mid-air, frozen. Readout: 790 m/s. Time runs for one
second: the bullet shoots right, the velocity counter falls fast. It freezes again at 1000 m: 322 m/s.}

VOICE:
This bullet just left the barrel. It flies at seven hundred ninety metres per second.
Two seconds later, it's one kilometre away. And it's not even half as fast.
So where did the velocity go? The air took it. Today, we talk about drag.

## 2. What the air does  (~40 s)

{ANIM: The bullet is frozen, and the air is shown as small dots. Time runs very slowly. Dots pile up in
front of the nose, slide along the sides, and leave an empty, swirling space behind the base. Three small
labels draw on one by one: "pressure in front", "friction on the sides", "a hole behind".}

VOICE:
Air is not empty. It's made of very small particles, and the bullet has to push them out of the way.
In front of the nose, the air gets pressed together, and it pushes back.
Along the sides, the air rubs against the bullet.
And behind it, there is a hole in the air. That hole pulls the bullet back, a little.
All of this together is drag.

## 3. The shock wave  (~30 s)

{ANIM: The dots fade into streaks. Each moment the nose sends out a thin ring, a sound wave. The bullet
is faster than the rings, so they pile up into a cone. The rings fade and the cone stays as two clean
lines. Then the velocity goes up and the cone gets narrower; the velocity goes down and the cone opens.}

VOICE:
Our bullet is faster than sound. So the air in front gets no warning. It can't move away in time.
Instead, the air builds a wall. This is a shock wave, the cone you see here.
The faster the bullet, the narrower the cone. And making this wave costs a lot of energy. It's a big part of the drag.

## 4. The formula, in words  (~75 s)

{ANIM: One big bullet in the middle of the screen, the same line drawing as the hook; the air streams past
it. The formula writes itself above it: `drag = ½ ρ · v² · Cd · A / m`. Every term is acted out **on the
bullet itself**, full screen, then the picture shrinks and flies back into its term, which stays lit:

- **ρ** — the air is drawn as dots. The camera rises along a mountain line to 3000 m: the dots get sparser
  (label "−26 %"), and the drag arrow behind the bullet shrinks by the same amount. Back to sea level.
- **v²** — a velocity arrow grows out of the nose. Then the arrow is folded into a square with side v
  ("v × v"). The arrow doubles: the square splits into four squares, and the drag arrow grows four times.
- **A** — the bullet turns towards the camera in a slow 3D turn, until we look straight at its nose. The
  outline becomes a circle; a diameter line draws across it, "7.8 mm", and the circle fills with hatching:
  "A = 0.48 cm²". The air dots fly at the circle and hit it. A wider circle draws around it: more dots
  hit. The bullet turns back to the side.
- **m** — the outline fills from the base up, like a glass of water: "11.3 g". A twin of the same shape
  draws beside it, filled darker: "heavier". Both get the same drag arrow — the air pushes them the same.
  Under each, a slowing-down arrow: the heavy one's is shorter.
- **A / m → SD** — a soft bracket draws under `A / m`, and the two terms fold into one: `1 / SD`, label
  "sectional density = m / d²". Beside it, a short fat bullet and a long slim one of the same weight: the
  long one's front circle is smaller, its SD bar is longer.
- **Cd** — the flow lines draw around a blunt, flat-nosed shape: a wide bow wave in front, a messy swirl
  behind, "Cd ≈ 1". The shape morphs into our pointed, boat-tailed bullet: the wave folds back into a thin
  cone, the swirl shrinks to a narrow tail, and the number counts down to "0.29".

At the end the formula reads `drag = ½ ρ · v² · Cd / SD` (× π/4), all terms lit, the bullet points right,
and the formula gives "= 48 g". The camera
pulls back into the pilot shot.}

VOICE:
So how much does the air slow the bullet down? Here's the formula. Don't worry, let's just read it piece by piece.
Rho is the air density. High in the mountains the air is thinner, so there's less drag.
V is the velocity. V squared is the velocity, times itself. Like a square. Two times faster, and the square is four times bigger. So four times more drag. That's why a fast bullet loses velocity so quickly.
A is the area of the bullet from the front. Let's turn it around. Here: a little circle, less than half a square centimetre. A wider bullet has to push more air.
M is the mass. Here are two bullets of the same shape. The air pushes them just the same. But the heavy one slows down less. It's just harder to stop.
The area and the mass go together. Together, they're called the sectional density. A long, heavy bullet has a big one.
And C d is the drag coefficient. It tells you how good the shape is. Look at the air behind a flat nose. It's a mess. A sharp nose and a boat tail make it small.

## 5. Watch it slow down  (~45 s)

{ANIM: The pilot sequence (`DecelPilot`). The velocity and drag arrows are to scale, the air streams past,
the velocity chart draws itself, and a dot rides the Cd curve. The bullet goes below Mach 1 at 930 m: the
shock wave lets go, and a ring spreads out from the nose. End lines: "48 g at the muzzle → 2.3 g at
1600 m" and "The bullet slowed 3 times — drag fell 21 times".}

VOICE:
Now let's watch it. The arrow behind the bullet is the drag. At the muzzle, it's forty-eight g. Every tenth of a second, the bullet loses forty-seven metres per second.
The bullet slows down, and the drag gets smaller, very fast. Remember: velocity squared.
Here, at nine hundred thirty metres, the bullet goes below the speed of sound. The shock wave is gone, and the drag drops again.
At one thousand six hundred metres, the bullet is three times slower. But the drag is twenty-one times smaller.

## 6. Cd is not one number  (~30 s)

{ANIM: The Cd chart moves to the centre and grows. The dot runs along it again, from right to left. Three
soft brackets draw under the curve: "slower than sound", "near the speed of sound", "faster than sound".
The hump lights up.}

VOICE:
Look at this curve again. C d is not one number. It changes with the velocity.
Slower than sound, it's low and flat. Near the speed of sound, it jumps up. This is where the shock waves are born. And faster than sound, it slowly goes down again.
Every bullet has its own curve like this. But to measure it, you need a radar and a lot of shots.

## 7. Standard bullets and the BC  (~55 s)

{ANIM: Two standard shapes draw on side by side: G1, short with a flat base, and G7, long with a boat
tail. Their Cd curves draw underneath. Then the formula from scene 4 comes back: `drag = ½ ρ · v² · Cd / SD`.
A soft bracket draws under `Cd / SD`, and it folds into `Cd_G7(M) / BC`, while a small typing box draws
in the corner with just one field, "BC 0.253". Under it, the G7 curve and our bullet's own curve: ours
lies almost on top of G7, just 4 % higher (label "our bullet ≈ G7 × 1.04"). Last beat: the BC slider
doubles, and the drag arrow on the bullet halves.}

VOICE:
So a long time ago, people measured a few standard bullets, very carefully. G one is an old one, with a flat base. G seven is long, with a boat tail. It's much closer to a modern match bullet.
But you never type in C d, or the area, or the mass. You type in one number: the ballistic coefficient.
It packs the rest: the sectional density, and how the shape of your bullet compares to the standard one.
So the program takes the standard curve, and divides it by the BC. A bigger BC means less drag. Twice the BC, half the drag. It's really that simple.

## 8. One bullet, two BCs  (~30 s)

{ANIM: Our bullet in the middle. On the left the G1 shape with "0.505", on the right the G7 shape with
"0.253". Two rulers with different scales lie under the same bullet.}

VOICE:
Our bullet has two BCs on the box. Zero point five oh five on G one, and zero point two five three on G seven. And both are right.
It's like measuring with two different rulers. The bullet is the same, only the standard is different.
That's why a BC without its table means nothing.

## 9. Where one number breaks  (~55 s)

{ANIM: The Cd chart again with three curves: G1 scaled for 0.505, G7 scaled for 0.253, and Lapua's
radar curve for this bullet in coral. They agree at high velocity and come apart near Mach 1.
Then three ghost bullets are fired together, one for each model, seen from the side. Target at 300 m:
three holes in one spot, label "3 mm". Target at 1000 m: the holes spread up and down, "G1 +57 cm" and
"G7 +28 cm" above the radar hole.}

VOICE:
But here's the problem. Our bullet's curve doesn't have the same shape as the standard curve. This one is the real curve. Lapua measured it with a radar.
One number can fit the curve at one velocity. But not at every velocity.
So let's fire the same bullet three times, with three drag models. At three hundred metres, all three hits are within three millimetres.
At one kilometre, it's different. With G one, the bullet hits fifty-seven centimetres higher than with the measured curve. With G seven, twenty-eight centimetres.

## 10. Outro  (~20 s)

{ANIM: The radar curve stays, and a small `.drg` file icon draws beside it. Then the bullet shrinks into
one bright dot. That dot opens episode 2.}

VOICE:
So if you shoot far, a measured curve is better than any single number. The program can use one, or build one for you from several BCs.
Next time, we use this drag, and see how the program flies the bullet, step by step.

---

# Episode 2 — How a dot flies: the point-mass model

## 1. Hook  (~20 s)

{ANIM: Episode 1's bullet. Its outline pulls together into one bright point.}

VOICE:
To calculate a trajectory, the program forgets what the bullet looks like. The nose, the length, the spin, all of it.
What's left is a dot. A dot with a mass. The shape stays only in the drag curve from the last episode.
Today: how this dot flies.

## 2. Six numbers  (~35 s)

{ANIM: Three axes draw out from the muzzle: downrange, up, and to the side. The dot sits at the
muzzle with three position numbers next to it. A velocity arrow grows from it, and its three parts draw
along the axes.}

VOICE:
To know everything about the dot, we need six numbers. Three tell us where it is: how far, how high, and how much to the side. Three more tell us how fast it moves, and in which direction.
That's why it's called a model with three degrees of freedom. The dot can move in three directions. But it can't turn. It has no nose to turn.

## 3. Two forces  (~40 s)

{ANIM: Two arrows on the dot, to scale. Drag is coral, long, and points backwards along the flight.
Gravity is champagne and points down; it is tiny. A magnifier circle draws over the gravity arrow and
shows it big: "1 g". Next to the drag arrow: "48 g". Then time runs: the drag arrow shrinks while the
gravity arrow stays the same.}

VOICE:
Only two forces act on the dot. Gravity pulls it down. It's always the same: one g.
And drag pushes it back, along the direction of flight. At the muzzle the drag is forty-eight g. Next to it, gravity is so small that we need a magnifier to see it.
But gravity doesn't change. The drag gets smaller as the bullet slows down. Gravity stays.

## 4. Stop the time: one step  (~50 s)

{ANIM: Time freezes and a small stopwatch line draws. Then three beats, each with a numbered tick:
1 — the two forces add up into one acceleration arrow;
2 — the velocity arrow turns a little down and gets a little shorter;
3 — the dot jumps along the new velocity arrow and leaves a thin trail.
The step repeats three times, slowly.}

VOICE:
So how does the program move the dot? It can't solve the whole flight in one go. It goes in small steps.
Stop the time. Step one: find the forces, and add them together.
Step two: change the velocity. The forces make the dot a little slower, and turn it a little down.
Step three: move the dot with this new velocity, for a short moment.
And then, again. And again. That's all there is.

## 5. How small a step  (~45 s)

{ANIM: The step first drawn much bigger than real, label "steps 1000× bigger than real": the broken line
is visibly off the faint real path. In a few beats the steps shrink until the line is smooth. A counter:
"thousands of steps to 1 km". Then a ruler under the path: every step is the same length in distance,
while a small stopwatch shows each step taking longer in time as the bullet slows.}

VOICE:
In this picture, the steps are huge, so you can see them. Between the steps, the dot flies straight, so the path is a little wrong.
The program takes much shorter steps. Less than a metre each, so there are thousands of steps in one kilometre. And the shorter the step, the smaller the error.
One more thing. The step is measured in distance, not in time. So when the bullet slows down, each step takes a little longer.
This simple way is called the Euler method. The program itself uses a smarter step, but the idea is exactly the same. That's a story for another video.

## 6. The whole flight  (~35 s)

{ANIM: The full trajectory draws as the dot runs along it. The drag arrow shrinks on the way. The path
is almost straight at first and bends down at the end. A dashed curve draws beside it: the same shot
with no air.}

VOICE:
Here is the whole flight, step by step. At the start, the bullet is fast, and the path is almost straight. At the end, it's slow, it has more time to fall, and the path bends down quickly.
The dashed line is the same shot with no air at all. The difference between them is the drag.

## 7. Wind  (~50 s)

{ANIM: The camera turns to look from above. Air particles drift from right to left. Three arrows on the
dot: its velocity, the wind, and the velocity through the air, which is the difference of the two. The
drag arrow points opposite to that last one: it's tilted and has a small side part. The dot curves to
the left. At 1000 m: "3.35 m".}

VOICE:
Now the wind. Remember: drag is about the air, not the ground.
When the air moves to the side, the bullet flies a little sideways through it. So the drag doesn't point straight back any more. It gets a small side part.
This side part moves the bullet. A wind of four and a half metres per second from the right, and at one kilometre, the bullet is more than three metres to the left.
So the wind doesn't push the bullet like a hand. It just turns the drag a little.

## 8. Outro  (~25 s)

{ANIM: The dot. A thin spiral line winds around it, and the dot grows back into the bullet, which starts
to spin.}

VOICE:
So that's the whole model. Two forces, six numbers, and a lot of small steps.
But a real bullet is not a dot. It spins, it has a nose, and it flies over a planet that turns.
Next time: what a dot can't do, and how the program fixes it.

---

# Episode 3 — What a dot can't do: spin, wind and the turning Earth

## 1. Hook  (~30 s)

{ANIM: The bullet spinning; the rifling marks on its side run around it. Counter: "3 100 turns per
second".}

VOICE:
A real bullet is not a dot. It spins. Our bullet leaves the barrel at more than three thousand turns per second.
In episode four of the first series, you saw what this does in the table. Today, let's see why it happens.
A dot can't spin. So anything that comes from the spin, the program has to add on top.

## 2. The spinning top  (~75 s)

{ANIM: A spinning top as a line drawing. A small push: it doesn't fall, its axis moves to the side and
slowly circles. Then the bullet: a push on the nose, and the nose moves sideways, not up.
The line writes Miller's formula across the top of the screen:

$$S_g = \frac{30\,w}{t^{2}\,d^{3}\,L\,(1+L^{2})}\cdot\left(\frac{V_0}{2800}\right)^{1/3}\cdot\frac{T+460}{519}\cdot\frac{29.92}{P}$$

Under it, our bullet, big. Each term is acted out on the bullet while it is said, then flies back into the
formula and stays lit:
- **w** — the bullet sits on a line-drawn scale pan, the needle swings: "175 gr".
- **d** — the bullet turns nose-on, a diameter draws across the circle: "0.308 in".
- **L** — it turns back; a ruler draws along its side, marked in calibres: "4.5 calibres". The label
  "L" splits in two and flies to both places in the formula.
- **t** — the barrel draws as a cutaway with the rifling inside. One full turn of the groove is marked
  along it: "one turn in 10 inches = 32 calibres". The groove tightens to a faster twist, and the Sg gauge
  needle jumps up hard: "squared".
- **V₀, T, P** — two small factors light up together, each with a tiny icon (a velocity arrow, a
  thermometer and a barometer), and show "≈ 1".
Unit tags in the corner: "w — grains, d — inches, t and L — in calibres".
A gauge for Sg draws on with marks at 1 and 1.4; the needle stops at 1.7. Then "L" is crossed out, and the
whole formula goes dark.}

VOICE:
A spinning bullet is like a spinning top. Push it, and it doesn't fall over. It moves to the side instead.
So how stable is our bullet? Here's the formula by Don Miller.
On top is the weight. Below it, the twist, the diameter, and the length. The length is there twice. A long bullet is harder to keep stable.
The twist is how far the bullet goes for one full turn in the barrel. It's squared, so a faster twist makes the number much bigger.
Then two small fixes: one for the velocity, and one for the air. In standard air, they're almost one.
The answer is the stability, S g. Our bullet has about one point seven. Above one point four is good.
And if the length or the twist is empty, the program can't get this number. Then it simply skips the next two effects. There's no warning. They are just not there.

## 3. Spin drift  (~75 s)

{ANIM: Side view: the path bends down. The nose stays a little above the path (label "×20"). Then the
view from above: the nose turns a little to the right of the flight direction, and a small side arrow
pushes right. The top-view path curves right.
The line writes Litz's formula:

$$\Delta z = 1.25\,(S_g + 1.2)\;t^{1.83}$$

Sg lights up and a thin line links it back to the gauge from scene 2; then t (label "time of flight, s");
then the power 1.83 glows. Beside it, two small curves: a straight line for "distance" and a steeper
curve for t^1.83. Unit tag: "Δz — inches". Numbers draw on: "4.7 cm" at 500 m, "31 cm" at 1000 m.}

VOICE:
The first effect is spin drift. The path bends down all the time, and the bullet tries to follow it with its nose. But the nose stays a little above the path. The air pushes on it, and the spinning bullet answers to the side.
With a right twist, the nose turns a little to the right. So the air pushes the bullet to the right.
A dot has no nose, so the program can't see any of this. Instead, it uses a formula by Bryan Litz. He made it from real measured shots.
Here it is. There are only two things in it. The stability number, from the last formula. And the time of flight.
Look at the time. It has the power one point eight three, so the drift grows faster than the time. And the time itself grows faster than the distance, because the bullet slows down.
At five hundred metres, it's about five centimetres. At one kilometre, thirty-one.

## 4. Aerodynamic jump  (~75 s)

{ANIM: The muzzle, with wind arrows from the right. The bullet comes out and the side air meets it; the
nose turns towards the wind, then the spin sends it up. The path tilts up by a tiny angle, and the
angle stays.
The line writes the second Litz formula:

$$\Delta y' = \left(0.01\,S_g - 0.0024\,L + 0.032\right)\cdot W_\perp$$

Sg glows with a small "+", L with a small "−", then W⊥ with a sideways wind arrow. Unit tags: "W⊥ — side
wind at the muzzle, mph", "Δy' — angle, MOA". The angle opens as a thin wedge from the muzzle: 5.6 cm at
500 m, 11 cm at 1000 m, on a straight line.
Then the same shot with a gust that starts at 300 m: no jump. At the 1000 m target, the hit moves 11 cm
up, while a long arrow shows the 3.35 m sideways push.}

VOICE:
The second effect is aerodynamic jump. It needs a side wind, and it happens right at the muzzle.
When the bullet leaves the barrel, the side wind hits it. The air tries to turn the nose towards the wind. And again, the spinning bullet answers to the side. With a right twist and a wind from the right, the nose goes up.
This is another formula by Bryan Litz. The stability adds a little. The length takes a little away. And all of it is multiplied by the side wind.
The answer is not a distance. It's an angle, and it's the same at every distance. So the jump grows in a straight line.
Four and a half metres per second from the right, and at one kilometre the bullet hits eleven centimetres higher. While the wind itself moves it more than three metres to the side.
And only the wind at the muzzle counts. A wind that starts three hundred metres away gives no jump at all.

## 5. The turning Earth: Coriolis  (~60 s)

{ANIM: A line-drawn globe turns. Zoom in on a shooter at 45° north, seen from above. During the
two-second flight the ground with the target turns a little under the bullet, and it lands 10 cm to the
right.
The line writes:

$$\Delta z = \Omega\,\sin\varphi\;R\;t$$

Ω glows with a small turning arrow around the globe ("one turn a day"); sin φ with the latitude line on
the globe, its value sliding from 0 at the equator to 1 at the pole; then R and t. The shooter turns to
face north, east, south and west: the 10 cm stays the same.}

VOICE:
The last two effects come from the Earth itself. It turns, and we turn with it. The bullet flies for two seconds, and in these two seconds, the ground under it turns a little.
This one doesn't come from measured shots. It comes straight from physics.
Omega is how fast the Earth turns: one turn a day. Then the sine of your latitude. It's zero at the equator, and the biggest at the poles. And then the distance, times the time of flight.
In the northern half of the world, this moves the bullet to the right. At forty-five degrees north and one kilometre, it's ten centimetres. There is no direction in the formula, so it's the same wherever you shoot.

## 6. East and west  (~55 s)

{ANIM: The same shooter. Facing east, the bullet flies with the Earth's turn: the label "lighter", and
the hit +12 cm. Facing west: "heavier", −12 cm. Facing north: 0.
The line writes:

$$\frac{g_\text{eff}}{g} = 1 - \frac{2\,\Omega\,\cos\varphi\,\sin(Az)\,V_0}{g}$$

sin(Az) lights up while a compass rose draws beside it: E "+1", W "−1", N and S "0". Then V₀. Then the
result for our shot: "0.992" east, "1.008" west.}

VOICE:
The second one depends on the direction. Shoot to the east, and the bullet flies with the turn of the Earth. It's as if it got a little lighter.
So here the program changes gravity itself, just a little.
Omega again, and now the cosine of the latitude. Then the direction of the shot: its sine is plus one to the east, minus one to the west, and zero to the north or south. And the muzzle velocity.
For our shot, gravity gets less than one percent lighter to the east. At one kilometre, the bullet hits twelve centimetres higher. Shooting west, twelve centimetres lower. To the north or to the south, nothing changes.

## 7. The honest part  (~50 s)

{ANIM: The dot's trajectory as a solid line. The four formula boxes snap onto its end one by one: spin
drift, jump, Coriolis, east-west. The velocity and time readouts beside the path stay exactly the same.
Then a separate picture: the bullet with an arc on its nose angle, "4DOF: computes this itself". Then a
bar chart by size: 31, 11, 10 and 12 cm, and one coral bar, "G1 vs radar: 57 cm", the longest.}

VOICE:
So, the program doesn't really fly a spinning bullet. It flies the dot first. Then it adds these four effects with their formulas. They change only where the bullet hits, not its velocity, and not the time of flight.
There are models that do more. A model with four degrees of freedom also follows the angle of the nose, so it gets spin drift by itself.
But look at the sizes. All four effects are smaller than the mistake you get from a wrong drag curve. Remember G one against the radar curve at one kilometre? Fifty-seven centimetres.
So for a long shot, get a good drag curve first. It matters more than anything else in this video.

## 8. Outro  (~20 s)

{ANIM: The three motifs in a row: the drag arrow, the dot with its steps, and the spinning bullet. Link
to the documentation.}

VOICE:
So that's what's inside the calculator: the drag, the dot, and a few small fixes on top. If you want the formulas, they're all in the documentation. The link is in the description.
