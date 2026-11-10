# Education mini-series — scripts

Four animated explainers about how the calculator works inside. Each is about 5–6 minutes long.
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
  If a picture exaggerates something (the nose angle in episode 4), the screen says so ("×20").
- **Units.** Metric. The bullet is the first series' 175 gr Scenar-L at 790 m/s.
- **Bridges.** Each episode ends on the picture the next one starts from: episode 1's bullet turns into
  a dot, and episode 3's dot grows back into a spinning bullet.
- **Sync.** The animation follows the voice: a term lights up while it is being said, not after.
- **Formulas** (every episode) are written on screen by the line itself, term by term, in the same
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

Formulas are written on screen like in episode 1, term by term, and every term is acted out on the dot
and its arrows. A letter with an arrow on top (v⃗) is an arrow — three numbers; a plain letter is one number.

## 1. Hook  (~20 s)

{ANIM: Episode 1's bullet. Its outline pulls together into one bright point.}

VOICE:
To calculate a trajectory, the program forgets what the bullet looks like. The nose, the length, the spin, all of it.
What's left is a dot. A dot with a mass. The shape stays only in the drag curve from the last episode.
Today: how this dot flies.

## 2. Six numbers  (~50 s)

{ANIM: Three axes draw out from the muzzle: x downrange, y up, z to the side. The formula writes itself:
`r⃗ = (x, y, z)`   `v⃗ = (vx, vy, vz)`   `|v⃗| = √(vx² + vy² + vz²)`. Acted out:
- **r⃗** — an arrow from the muzzle to the dot. Dashed lines drop from the dot to the three axes and mark
  x, y and z; a thin box draws between them, the arrow is its diagonal.
- **v⃗** — the velocity arrow grows from the dot. Its own small box draws, and the three edges light up one
  by one: vx (long), vy (tiny), vz (zero for now — the edge is a dot).
- **|v⃗|** — Pythagoras on the box: the floor diagonal draws first, then the diagonal through the box,
  which lies on the arrow: "790 m/s".}

VOICE:
To know everything about the dot, we need six numbers. Here they are.
The first three are the position, R. The little arrow on top means it's not one number, but three. How far downrange: X. How high: Y. And how far to the side: Z.
The next three are the velocity, V. It's an arrow too. Its three parts tell how fast the dot moves downrange, up, and to the side.
And the length of this arrow? Put the arrow in a box. It goes from one corner to the opposite corner. That's Pythagoras, in three directions: square the three parts, add them, and take the square root. Here, seven hundred ninety metres per second.
Six numbers. Three of them are velocities: they tell how fast the other three change. So the dot can move in three directions. That's why it's called a model with three degrees of freedom.

## 3. Two forces  (~50 s)

{ANIM: The formula writes: `a⃗ = F⃗ / m`. Acted out:
- **F⃗ / m** — two dots, one light and one heavy (the heavy one drawn bigger and filled darker, like the
  glass in episode 1). The same force arrow on both; the light one's acceleration arrow is twice as long.
- **gravity** — the same two dots, now pulled down: the heavy one gets a longer force arrow, but both
  acceleration arrows are the same, and both fall side by side: "1 g = 9.8 m/s per second".
- **drag** — episode 1's formula draws in small, its `/ m` lights up: "48 g". Two arrows on our dot, to scale:
  coral drag, long, backwards; champagne gravity, tiny, down. A magnifier circle draws over the gravity
  arrow: "1 g". Then time runs: the drag arrow shrinks, the gravity arrow stays.}

VOICE:
Only two forces act on the dot: gravity and drag.
But a force alone doesn't tell us how the dot moves. We need the acceleration: how fast the velocity changes. Newton's law says: the acceleration is the force, divided by the mass. The same force, twice the mass, half the acceleration.
Gravity is special. It pulls a heavy bullet harder, but a heavy bullet is also harder to move. So everything falls with the same acceleration: one g. Every second, nine point eight metres per second more.
The drag formula from the last episode already divides by the mass. So it gives an acceleration too. At the muzzle, forty-eight g. Next to it, gravity is so small that we need a magnifier to see it.
But gravity never changes. The drag gets smaller as the bullet slows down.

## 4. The drag arrow  (~70 s)

{ANIM: The formula writes: `a⃗_drag = − D · v⃗ / |v⃗|`. Acted out on the dot:
- **D** — episode 1's formula draws beside it: `D = ½ ρ · v² · Cd(M) · A / m`, and `Cd(M)` lights up.
  `M = |v⃗| / c`: the velocity arrow "790" lies on a ruler next to a shorter arrow, the speed of sound
  "340"; the short one fits into the long one 2.3 times: "M = 2.3". Episode 1's Cd curve draws, a marker
  slides to Mach 2.3 and a dashed line reads Cd off the axis. The formula folds into one number:
  "D = 48 g". It is drawn as a bar, not an arrow — it has a length, but no direction.
- **v⃗ / |v⃗|** — the velocity arrow is divided by its own length: it shrinks to a short arrow of length 1,
  label "direction only".
- **−** — the short arrow flips and points backwards.
- **D ·** — it stretches to the length of the bar: the coral drag arrow, exactly against the velocity.}

VOICE:
Now, where does the drag point? Here's the formula. Remember the little arrow on top? A letter with it is an arrow: it has a size and a direction. A letter without it is just one number, a size. On the right, only V has an arrow. So the direction can only come from the velocity.
D is the drag from the last episode. It needs the Mach number: the velocity divided by the speed of sound. Seven hundred ninety, divided by three hundred forty, is two point three. With that, the program reads C d from the curve, and the formula gives forty-eight g. But D is just a number. It says how much, but not which way.
The direction comes from the velocity. Take the velocity arrow, and divide it by its own length. The direction stays, but the length becomes one. It's just a pointer.
The minus turns the pointer around. Now it points back.
And D stretches it to the right length. That's the drag arrow: forty-eight g, exactly against the velocity.

## 5. Add the arrows  (~60 s)

{ANIM: The formula writes: `a⃗ = a⃗_drag + g⃗`, with `g⃗ = (0, −g, 0)`. The gravity arrow slides to the tip
of the drag arrow; a new arrow closes the triangle: the acceleration, back and a little down.
Then the formula unfolds into three lines, one per axis:
`a_x = − D · vx / |v⃗|`
`a_y = − D · vy / |v⃗| − g`
`a_z = − D · vz / |v⃗|`
Acted out: the dot later in the flight, flying a little downward. The velocity arrow and the drag arrow
side by side, each in its box: the drag box is the velocity box turned around and resized. Matching edges
light up in pairs: vx ↔ drag x part, vy ↔ drag y part — the velocity points a little down, so the drag's
y part points a little **up**. `− g` lights up in the y line only. The z line: vz is a dot, so the drag's z
part is a dot too; the line dims, and a small bookmark stays on it.}

VOICE:
Now add the two arrows. Put gravity at the end of the drag arrow. The arrow from the start to the end is the total acceleration.
But the program doesn't work with arrows. It works with numbers. So it writes the same thing three times, once for each direction.
Look at the two boxes. The drag box is the velocity box, turned around and resized. So each part of the drag takes the same share as the same part of the velocity. When the dot flies a little downward, the drag pushes it a little upward.
Gravity shows up in one line only: up and down.
And the side line? There is no side velocity, so there is no side drag. Remember this line. We'll come back to it in the next episode, with the wind.

## 6. One step  (~60 s)

{ANIM: Time freezes, a stopwatch draws. The formula writes two lines:
`v⃗_new = v⃗ + a⃗ · Δt`
`r⃗_new = r⃗ + v⃗_new · Δt`. Acted out:
- **Δt** — the stopwatch: "Δt = 0.1 s (much bigger than real)".
- **a⃗ · Δt** — the acceleration arrow passes through the stopwatch and comes out short: the change of
  velocity. It splits into its two parts: coral "−47 m/s" along the flight, champagne "1 m/s" down.
- **v⃗ +** — the old velocity arrow "790 m/s"; the change clicks onto its tip; the new arrow closes the
  triangle, a little shorter and turned a little down: "743 m/s". The three velocity numbers roll.
- **r⃗_new** — the dot slides along the new velocity arrow for the same Δt and leaves a thin trail; the
  three position numbers roll.
Then the loop runs three more times, faster each time; on every lap the drag arrow is drawn again, a
little shorter.}

VOICE:
Now the program can make a step. Stop the time. Here is the step, in two lines.
Delta t is the length of the step in time. In this picture, it's one tenth of a second. That's much longer than real, so we can see it.
The acceleration says how much the velocity changes in one second. Times delta t, it says how much it changes in this step. Drag takes away forty-seven metres per second. Gravity adds about one metre per second, downward.
Add this change to the old velocity. The new velocity arrow is a little shorter, and turned a little down.
The second line moves the dot. The new velocity, times the same delta t, is how far the dot goes in this step.
Each line works on all three numbers at once. And then, again. The dot is slower now, so the drag is a little smaller. The program finds the forces again, and makes the next step. That's all there is.

## 7. How big a step  (~60 s)

{ANIM: The 0.1 s steps on the faint real path. Zoom into one step: the straight segment vs the real path,
which keeps bending down inside the step; the gap at the end glows coral: "error". The steps shrink until
the line lies on the real path. The formula writes: `Δt = Δs / vx`:
- **Δs** — the camera dives in until one real step fills the screen, with a ruler under it: "10–50 cm".
- **/ vx** — the ruler is divided by the downrange velocity; a stopwatch: "< 1 ms at the muzzle". A
  counter: "thousands of steps to 1 km".
Then the 1000 m mark, the real path vs the Euler dot, zoomed, the 13.6 m drop beside it for scale:
"½ m steps: 2 cm off"; the steps shrink 10× and the gap closes: "5 cm steps: 2 mm off".}

VOICE:
Here are two paths. The faint line is the real bullet. The bright line is our dot, step by step. These steps are huge, one tenth of a second, so you can see the problem.
Inside one step, the dot flies straight, with the same velocity. But the real bullet slows down and bends down all the time. So after each step, the dot is a little off. The red lines show how much.
The fix is simple: shorter steps. The program chooses each step by distance, delta s: from ten to fifty centimetres. To turn it into time, it divides by the velocity downrange. At the muzzle, that's less than one thousandth of a second. So there are thousands of steps in one kilometre.
This simple way is called the Euler method. Its error grows with the step. With our huge steps, a tenth of a second, the dot at one kilometre is almost three metres off. With half-metre steps, it's only two centimetres, out of more than thirteen metres of drop. That's already smaller than any real group at that distance. When the library uses Euler, it takes even shorter steps, five centimetres. Then the error is just two millimetres.
But by default, the program uses a smarter method. It takes steps ten times longer, half a metre. And its error is even smaller: less than a tenth of a millimetre. But the idea is exactly the same: small steps. And this method is a story for another video.

## 8. The whole flight  (~60 s)

{ANIM: Label "barrel level". The dot runs along the full trajectory; the drag arrow shrinks on the way.
The path is almost straight at first and bends down at the end. A dashed curve draws beside it: the same
shot with no air. The formula writes beside the dashed curve: `fall = ½ g t²`:
- **no air** — a clock "1.27 s", and a vertical bar from the barrel line to the dashed curve at 1000 m:
  "7.9 m".
- **with air** — a clock "2.0 s"; `½ g t²` gives a ghost bar "19.8 m", but the real bar stops at "14.9 m".
  The y line from scene 5 comes back, `− D · vy / |v⃗|` lights up, and a small coral arrow on the falling dot
  points up.}

VOICE:
Here is the whole flight, step by step. This time, we hold the barrel level. At the start, the bullet is fast, and the path is almost straight. At the end, it's slow, and the path bends down quickly.
Why? Because the bullet falls the whole time it flies. Without air, falling follows a simple rule: one half, g, t squared. Without air, the bullet reaches one kilometre in one point two seven seconds, and falls almost eight metres. That's the dashed line.
With air, it needs two seconds. Two seconds of falling would be almost twenty metres. But the bullet falls only fifteen. Why less? Remember the up-and-down line. The bullet flies a little downward, so the drag pushes it a little up. The drag slows the falling, too.
So the drag does two things here. It makes the flight longer, so the bullet falls more. And it brakes the fall, a little.

## 9. Outro  (~20 s)

{ANIM: The formulas of the episode gather on one screen and light up one by one as they are named:
`r⃗, v⃗` · `a⃗ = F⃗ / m` · `a⃗_drag = − D · v⃗ / |v⃗|` · `+ g⃗` · `v⃗_new = v⃗ + a⃗ · Δt`, `r⃗_new = r⃗ + v⃗_new · Δt`.
Then the three component lines from scene 5; the side line `a_z = − D · vz / |v⃗|` is dim with its bookmark,
and it glows on "empty". On "the wind", air particles start to drift across the screen.}

VOICE:
So that's the whole model. Six numbers. Two forces: the drag, against the velocity, and gravity, down. And a lot of small steps.
But so far, the air stood still, and the side line stayed empty.
Next time: the wind.

---

# Episode 3 — The wind: how the air moves the dot

Built from episode 2's scenes 9–11 (voice reused as is) and a new hook. Same kit, same data (`ep02.json`).

## 1. Hook  (~20 s)

{ANIM: The dot flies its steps along the trajectory, in still air. The three component lines from episode 2
write themselves; the side line `a_z = − D · vz / |v⃗|` stays dim with its bookmark: "vz = 0". On "Today",
air particles start to drift from right to left, and the side line lights up.}

VOICE:
Last time, the dot flew in still air. Six numbers, two forces, and a lot of small steps. One line stayed empty: the side line. No side velocity, no side drag.
Today, the wind fills it.

## 2. Wind: look from the air  (~60 s)

{ANIM: The camera turns to look from above. Air particles drift from right to left: "wind 4.5 m/s". A
balloon drifts with them. The camera rides the balloon: the particles stop, and the ground grid slides to
the right. The bullet flies past: a long forward arrow "790" and a tiny side arrow "4.5" to the right.
The formula writes: `v⃗_air = v⃗ − w⃗`:
- **w⃗** — the wind arrow, pointing left.
- **−** — it flips to point right and clicks onto the tip of the velocity arrow.
- **v⃗_air** — the new arrow closes the triangle, tilted to the right: "0.33°" (drawn ×20).
Then the drag formula from scene 4 comes back, and every v⃗ in it turns into v⃗_air; `M = |v⃗_air| / c`.}

VOICE:
Now the wind. Four and a half metres per second, from the right.
Drag comes from the air hitting the bullet. So what matters is how the bullet moves through the air, not over the ground.
Let's look from the air. We fly in a balloon, and the wind carries us. For us, the air stands still, and the ground slides to the right.
And the bullet? It still flies forward, at seven hundred ninety. But for us, it also moves to the right, four and a half metres per second, together with the ground. So through the air, the bullet flies a little sideways.
In the formula, that's the velocity minus the wind. Turn the wind arrow around, and put it at the end of the velocity arrow. The new arrow is the velocity through the air. It's tilted to the right, by one third of a degree.
And this is the velocity that goes into the drag formula. Everywhere we had V, the program uses this one.

## 3. Wind: the side part  (~110 s)

{ANIM: The drag arrow, now against v⃗_air: tilted, back and a little to the left. The bookmarked z line
from scene 5 comes back: `a_z = − D · v_air,z / |v⃗_air|`, and the values drop in:
`48 g × 4.5 / 790 = 0.27 g`, to the left; a magnifier over the tiny side arrow. Top view: the dot curves
to the left; at 1000 m: "3.35 m".
Then the balloon's path next to the bullet's: the balloon carries "side 4.5 m/s" the whole time and drifts
"9 m" in the same 2 s. A small pair: the air's push on the balloon vanishes as it catches up; on the bullet
it stays. A side-velocity counter rides the dot and leaves marks at 100 / 500 / 1000 m: "0.3 m/s · 2 cm",
"1.5 m/s · 65 cm", "2.7 m/s · 3.35 m". Finally "4.5 → 9 m" against "0 … 2.7, average 1.7 → 3.35 m".}

VOICE:
Drag points against the velocity through the air. So now the drag is tilted too: back, and a little to the left.
Remember the side line? Before, the side velocity was zero, so the side drag was zero. Now the side part of the velocity through the air is four and a half metres per second. So the side drag is D, times four and a half, divided by seven hundred ninety. At the muzzle, about a quarter of a g, to the left.
That's the whole trick. The wind doesn't push the bullet like a hand. It turns the drag a little. At one kilometre, the bullet is three point three five metres to the left.
Here's something interesting. The balloon drifts much further: four and a half metres per second, for two seconds, that's nine metres. Why does the bullet move only about a third of that?
Because the wind doesn't give anything its velocity. It can only push, and it pushes through the drag. When something moves sideways slower than the air, the air catches up with it and pushes it along. When it moves as fast as the air, there's nothing left to push.
The balloon is big and light. Lots of drag, very little mass. So it picks up the wind's velocity almost at once, and then it flies with the air, four and a half metres per second, the whole time. Two seconds, nine metres.
The bullet is small, and heavy for its size. It leaves the barrel with zero side velocity. Then the side drag starts to build it up, slowly, about a quarter of a g.
At one hundred metres, the bullet moves to the left at only zero point three metres per second. It has drifted two centimetres.
At five hundred metres, one and a half metres per second. Sixty-five centimetres.
At one kilometre, two point seven metres per second. It never gets to four and a half.
So the balloon moves at four and a half the whole time. The bullet starts at zero and ends at two point seven. On average, that's about one point seven metres per second. For two seconds, that's three point three five metres, not nine.
So no drag, no wind drift. A bullet with less drag picks up less side velocity, and the wind moves it less.

## 4. Outro  (~30 s)

{ANIM: All the formulas of the episode gather on one screen and light up one by one as they are named:
`r⃗, v⃗` · `a⃗ = F⃗ / m` · `a⃗_drag = − D · v⃗_air / |v⃗_air|` · `+ g⃗` · `v⃗_new = v⃗ + a⃗ · Δt`,
`r⃗_new = r⃗ + v⃗_new · Δt`. Then they fade, the dot stays; a thin spiral line winds around it, and the dot
grows back into the bullet, which starts to spin.}

VOICE:
So that's the whole model. Six numbers. Two forces: the drag, against the velocity through the air, and gravity, down. And a lot of small steps.
But a real bullet is not a dot. It spins, it has a nose, and it flies over a planet that turns.
Next time: what a dot can't do, and how the program fixes it.

---

# Episode 4 — What a dot can't do: spin, wind and the turning Earth

Every effect follows the same four beats: the physics, without numbers; "a dot can't do this, so the program
adds a correction", and whose formula it is (fitted to shots, or straight from physics); the formula, each term
acted out; the numbers for our shot.

## 1. Hook  (~40 s)

{ANIM: The dot from episode 2 flies its path. It grows a body: the bullet, nose first. Rifling marks on its
side start to run around it; counter "3 100 turns per second". Four small cards line up under the path: SPIN
DRIFT, AERODYNAMIC JUMP (with a little spin icon), CORIOLIS, EAST–WEST (with a little globe icon). The dot
flies again, alone, and the four cards snap onto its end.}

VOICE:
A real bullet is not a dot. It has a nose, and it spins. Our bullet leaves the barrel at more than three thousand turns per second.
In episode four of the first series, you saw four small effects in the table. Two of them come from the spin: spin drift and aerodynamic jump. Two come from the turning Earth.
A dot can't spin, and it doesn't know that the Earth turns. So the program flies the dot first, and then adds these four effects on top. Today, let's see where each of them comes from, and how the program counts it.

## 2. Why a bullet spins  (~75 s)

{ANIM: The bullet, side view, big. A dot marks the centre of mass ("CENTRE OF MASS"). The air hits it; the
air's push draws as one arrow, attached in front of the centre of mass ("THE AIR PUSHES HERE"). The nose
tips a little: the push grows and tips it more; a ghost of the bullet tumbles over. Beside it, an arrow with
feathers: the same tip, and the feathers turn it back. Back to the bullet: the rifling marks run, it spins.
Then a spinning top: a push from the side, and the top's axis moves at a right angle to the push.
Then the bullet seen from behind, as a clock face, spinning clockwise (a curved arrow). A push on the nose
towards 9 o'clock (an arrow): the nose moves to 12. A push towards 12: the nose moves to 3. Caption: "A PUSH
→ A QUARTER TURN FURTHER, THE WAY IT SPINS". The clock shrinks to a corner badge and stays there for
scenes 4 and 5.}

VOICE:
First, why does a bullet need to spin at all?
The air pushes on the bullet. But this push is not in the middle of the bullet. It's closer to the nose, in front of the centre of mass. So if the nose turns a little off the path, the air turns it even more. Without help, the bullet would tumble.
An arrow solves this with feathers at the back. A bullet has none. So it spins.
A spinning thing has a strange rule. Look at a spinning top. Push it, and it doesn't fall the way you pushed. It moves to the side instead.
For the bullet, it works like this. Look at it from behind. It spins clockwise, like the hands of a clock. Push the nose to the left, to nine o'clock, and the nose goes up, to twelve. Push it up, and it goes to the right, to three. Always a quarter turn further, the same way the bullet spins.
Remember this rule. Both spin effects come from it.

## 3. Stability  (~85 s)

{ANIM: A balance: on one side the spin (a curved arrow), on the other the air's turning push. The spin side
goes down: "S_g > 1: THE SPIN WINS". The line writes Miller's formula across the top of the screen:

$$S_g = \frac{30\,w}{t^{2}\,d^{3}\,L\,(1+L^{2})}\cdot\left(\frac{V_0}{2800}\right)^{1/3}\cdot\frac{T+460}{519}\cdot\frac{29.92}{P}$$

Under it, our bullet, big. Each term is acted out on the bullet while it is said, then flies back into the
formula and stays lit:
- **w** — the bullet sits on a line-drawn scale pan, the needle swings: "175 gr".
- **d** — the bullet turns nose-on, a diameter draws across the circle: "0.308 in".
- **L** — it turns back; a ruler draws along its side, marked in calibres: "4.5 calibres". The label
  "L" splits in two and flies to both places in the formula; the air's push arrow grows with it.
- **t** — the barrel draws as a cutaway with the rifling inside. One full turn of the groove is marked
  along it: "one turn in 10 inches = 32 calibres". The groove tightens to a faster twist, and the S_g gauge
  needle jumps up hard: "squared".
- **V₀, T, P** — two small factors light up together, each with a tiny icon (a velocity arrow, a
  thermometer and a barometer), and show "≈ 1".
Unit captions beside the formula: "w — grains, d — inches, t and L — in calibres". A gauge for S_g draws on
with marks at 1 and 1.4; the needle stops at 1.7. Then "L" is crossed out, and the whole formula goes dark.}

VOICE:
So the air tries to turn the bullet over, and the spin keeps it straight. How strong is the spin, compared with the air? This number is called the stability, S g. Above one, the spin wins.
To get it exactly, you need the whole shape of the bullet and a lot of math. The program uses a simple rule by Don Miller instead. He fitted it to the full theory, so it needs only a few numbers.
On top is the weight. Below it, the diameter and the length. The length is there twice. A long bullet gives the air more to turn, so it's harder to keep straight.
And the twist: how far the bullet goes for one full turn in the barrel. It's squared. A faster twist spins the bullet faster, so the number grows a lot.
Then two small fixes: one for the velocity, and one for the air. In standard air, they're almost one.
Our bullet gets about one point seven. Above one point four is good.
And if the length or the twist is empty, the program can't get this number. Then it simply skips both spin effects. There's no warning. They are just not there.

## 4. Spin drift  (~95 s)

{ANIM: Side view: the path bends down. The bullet follows it a little late: its nose stays a little above
the path (caption "THE NOSE: A LITTLE ABOVE THE PATH · DRAWN ×20"). The air's push on the nose: an arrow up.
The corner clock lights: a push to 12, the nose goes to 3. The view turns to the top view: the nose points a
little to the right of the path; a small side arrow pushes the bullet right, and the top-view path curves
right. The bullet fades into a dot: "A DOT HAS NO NOSE".
The line writes Litz's formula:

$$\Delta z = 1.25\,(S_g + 1.2)\;t^{1.83}$$

S_g lights up and a thin line links it back to the gauge from scene 3; a stiffer axis lags more, the nose
stays higher, the side arrow grows. Then t ("time of flight, s"); then the power 1.83 glows. Beside it, two
small curves: a straight line for "distance" and a steeper curve for t^1.83. Unit caption: "Δz — inches".
Numbers draw on: "4.7 cm" at 500 m, "31 cm" at 1000 m.}

VOICE:
The first spin effect is spin drift.
Gravity bends the path down all the time. The spinning bullet wants to keep its axis where it was, so it follows the path a little late. The nose stays a little above the path.
So the air pushes the nose up. And now the rule: push up, and the nose goes right. The nose looks a little to the right of the path, and the air pushes the whole bullet to the right, like a small wing.
A dot has no nose, so the program can't see any of this. Instead, it uses a formula by Bryan Litz. He fitted it to measured shots.
There are only two things in it. The stability, from the last formula. A more stable bullet holds its axis harder, so the nose stays higher above the path, and the drift is bigger.
And the time of flight. It has the power one point eight three, so the drift grows faster than the time. And the time itself grows faster than the distance, because the bullet slows down.
At five hundred metres, it's about five centimetres. At one kilometre, thirty-one.

## 5. Aerodynamic jump  (~100 s)

{ANIM: Top view at the muzzle, wind arrows from the right. The bullet's velocity arrow and, from episode 3,
the tilted arrow v_air ("0.33° · DRAWN ×20"). The bullet comes out with its nose along the barrel: a small
gap between the nose and v_air glows ("THE NOSE IS OFF THE AIR FLOW"). The air's push turns the nose to the
left, away from the wind. The corner clock lights: a push to 9, the nose goes to 12. Side view: the nose
swings up and circles a few times in a shrinking spiral, and settles; the path behind it is tilted up by a
tiny angle, and the angle stays.
The line writes the second Litz formula:

$$\Delta y' = \left(0.01\,S_g - 0.0024\,L + 0.032\right)\cdot W_\perp$$

S_g glows with a small "+", L with a small "−", then W⊥ with a sideways wind arrow. Unit captions: "W⊥ — side
wind at the muzzle, mph", "Δy' — an angle, MOA". The angle opens as a thin wedge from the muzzle: 5.6 cm at
500 m, 11 cm at 1000 m, on a straight line. At the 1000 m target, the hit moves 11 cm up, while a long arrow
shows the 3.35 m sideways push. Then the same shot with a wind that starts at 300 m: no jump.}

VOICE:
The second spin effect is aerodynamic jump. It needs a side wind, and it happens right at the muzzle.
Remember the last episode: with a side wind, the velocity through the air is tilted. Our wind from the right tilts it by a third of a degree. But the bullet leaves the barrel with its nose along the barrel. So from the very first moment, the nose is a little off the air flow.
The air pushes the nose further away from the wind, to the left. And the rule again: push left, and the nose goes up. The nose swings up and around a few times, and these swings quickly die out. But while they last, the air pushes the bullet up. So the path stays tilted up by a tiny angle.
This is another formula by Bryan Litz. The stability adds a little. The length takes a little away. And all of it is multiplied by the side wind.
The answer is not a distance. It's an angle, and it's the same at every distance. So the jump grows in a straight line.
Four and a half metres per second from the right, and at one kilometre the bullet hits eleven centimetres higher. While the wind itself moves it more than three metres to the side.
And only the wind at the muzzle counts. A wind that starts three hundred metres away gives no jump at all.

## 6. The turning Earth: Coriolis  (~85 s)

{ANIM: A playground carousel from above, turning counterclockwise. A ball is thrown across it: seen from
above it flies straight; then the camera rides the carousel, and the same flight bends to the right.
Then a line-drawn globe, the North Pole on top: the ground around the pole is the same disk, "ONE TURN A
DAY". A shooter moves down to 45° north, then to the equator: a small turning disk under the shooter's
feet shrinks to nothing at the equator ("sin φ": 1 at the pole, 0.71 at 45°, 0 at the equator).
The line writes:

$$\Delta z = \Omega\,\sin\varphi\;R\;t$$

Ω glows with the turning arrow around the globe ("one turn a day"); sin φ with the latitude line; then R and
t. Caption: "FROM PHYSICS, NOT FROM MEASURED SHOTS". The shooter at 45° north, seen from above, turns to
face north, east, south and west: the hit is 10 cm right every time.}

VOICE:
The last two effects come from the Earth. It turns, and we turn with it.
Picture a playground carousel. Throw a ball across it. The ball flies straight. But while it flies, the carousel turns under it. So for someone on the carousel, the ball bends to the side.
At the North Pole, the ground is exactly such a carousel. It turns once a day, and a bullet bends to the right. At the equator, the ground doesn't turn around you at all. In between, only a part of the turn is under your feet. This part is the sine of your latitude.
This one doesn't need measured shots. It comes straight from physics.
Omega is how fast the Earth turns: one turn a day. Then the sine of the latitude. And then the distance, times the time of flight.
In the northern half of the world, this moves the bullet to the right, and in the southern half, to the left. At forty-five degrees north and one kilometre, it's ten centimetres. There is no direction in the formula, so it's the same wherever you shoot.

## 7. East and west  (~85 s)

{ANIM: Two ways the ground can turn. At the pole, seen from above: a carousel around the shooter. At the
equator, seen from the north: the ground is the rim of a wheel standing on its edge, turning from west to
east; the shooter stands on top, and the ground in the east keeps dropping; a small sun comes up over it.
At 45° the turn splits into the two parts. The wheel again: a shot to the east, the ground ahead turns down
and away, the path falls slower, "FALLS SLOWER · g × A LITTLE LESS"; a shot to the west, the ground ahead
turns up, "FALLS FASTER · g × A LITTLE MORE"; a shot to the north goes along the wheel's axis, "NOTHING".
The line writes:

$$\frac{g_\text{eff}}{g} = 1 - \frac{2\,\Omega\,\cos\varphi\,\sin(Az)\,V_0}{g}$$

cos φ: the wheel part of the turn on the globe's cross-section: all of it at the equator, almost nothing near
the pole. sin(Az) lights up while a compass rose draws beside it: E "+1", W "−1", N and S "0". Then V₀. Then
the result for our shot: "g × 0.992" east, "g × 1.008" west; the hit +12 cm and −12 cm at 1000 m.}

VOICE:
The second Earth effect depends on the direction.
Remember the carousel? At the pole, the ground turns around you like a carousel. At the equator, it turns in a different way: like a wheel standing on its edge, turning from west to east. Here, at forty-five degrees, we get a part of each.
The wheel part always turns the ground in the east down. That's why the sun comes up in the east.
Shoot to the east, and while the bullet flies, the ground ahead turns down, away from it. So the bullet falls a little slower, as if gravity were a little weaker. Shoot to the west, and the ground ahead turns up, towards the bullet. It falls a little faster, as if gravity were a little stronger. To the north or to the south, the bullet flies along the wheel's axis, so nothing changes.
So here the program scales the drop that gravity makes, just a little.
Omega again. Now the cosine of the latitude: that's the wheel part of the turn. At the equator, it's all of it. Near the pole, almost nothing. Then the direction of the shot: its sine is plus one to the east, minus one to the west, and zero to the north or south. And the muzzle velocity.
For our shot, gravity gets less than one percent weaker to the east. At one kilometre, the bullet hits twelve centimetres higher. Shooting west, twelve centimetres lower.

## 8. The honest part  (~50 s)

{ANIM: The dot's trajectory as a solid line. The four formula cards snap onto its end one by one: spin
drift, jump, Coriolis, east-west. The velocity and time readouts beside the path stay exactly the same.
Then a separate picture: the bullet with an arc on its nose angle, "4DOF: COMPUTES THIS ITSELF". Then a bar
chart by size: 31, 11, 10 and 12 cm, and one coral bar, "G1 vs radar: 57 cm", the longest.}

VOICE:
So, the program doesn't really fly a spinning bullet. It flies the dot first. Then it adds these four effects with their formulas. They change only where the bullet hits, not its velocity, and not the time of flight.
There are models that do more. A model with four degrees of freedom also follows the angle of the nose, so it gets spin drift by itself.
But look at the sizes. All four effects are smaller than the mistake you get from a wrong drag curve. Remember G one against the radar curve at one kilometre? Fifty-seven centimetres.
So for a long shot, get a good drag curve first. It matters more than anything else in this video.

## 9. Outro  (~20 s)

{ANIM: The four motifs in a row: the drag arrow, the dot with its steps, the wind, and the spinning bullet. Link
to the documentation.}

VOICE:
So that's what's inside the calculator: the drag, the dot, the wind, and a few small fixes on top. If you want the formulas, they're all in the documentation. The link is in the description.

