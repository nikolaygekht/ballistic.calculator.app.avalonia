# YouTube — "Physics of a Ballistic Calculator"

Ready-to-paste text for the four physics episodes, their playlist and the channel. Uploads go the same way as
the app series (YouTube Studio driven over CDP, see `claude/video/youtube.md`): **private first**, the user
publishes by hand. Category Science & Technology, language English, not made for kids.

| # | File | Thumbnail | Length |
|---|---|---|---|
| 1 | `remotion/out/ep01-drag.mp4` | `thumbs/ep01.jpg` | 5:18 |
| 2 | `remotion/out/ep02-point-mass.mp4` | `thumbs/ep02.jpg` | 7:53 |
| 3 | `remotion/out/ep03-wind.mp4` | `thumbs/ep03.jpg` | 3:53 |
| 4 | `remotion/out/ep04-spin-earth.mp4` | `thumbs/ep04.jpg` | 8:44 |

Thumbnails: 1280×720 JPEG, `node thumbs.mjs [1 2 3 4]` in `remotion/` (compositions `Thumb01`…`Thumb04`,
`src/thumbs/Thumbs.tsx`). Chapter times are the scene starts of the current cuts; re-check them after any
re-render that changes a scene's length.

## Channel description

Replaces the one in `claude/video/youtube-metadata.md` (that file is generated from `demo-script.md`; change it
there too if it is regenerated).

```
Ballistic Calculator 2 is a free, open-source ballistic calculator for Windows, Linux and macOS.

Two series live here. "Ballistic Calculator 2" shows how to use the program, one step at a time: a first trajectory, weather and wind, spin drift, Coriolis, scope reticles, measured drag curves, shooting at an angle, and building your own drag table. "Physics of a Ballistic Calculator" is the inside view: animated explainers of what the program actually computes — drag and the ballistic coefficient, the point-mass model, the wind, and the small effects a point mass can't do: spin drift, aerodynamic jump and the turning Earth.

The calculations come from the open-source BallisticCalculator library for .NET. It is a full model of the bullet's flight, but still a model: always check it against your own shots.

Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
Source: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
Library: https://github.com/gehtsoft-usa/BallisticCalculator1
```

## Playlist

**Title:** Physics of a Ballistic Calculator

```
What a ballistic calculator actually computes, drawn and animated, with every formula on screen. Four episodes: drag and the ballistic coefficient; the point-mass model that flies the bullet as a dot; the wind; and what a dot can't do — spin drift, aerodynamic jump, Coriolis and the east–west effect, and how the program adds them on top. The numbers come from the open-source BallisticCalculator library, the engine of Ballistic Calculator 2.
```

## Episode 1

**Title:** Drag: where the velocity goes — Physics of a Ballistic Calculator, ep. 1

**Description:**

```
A bullet leaves the muzzle at 790 m/s and arrives at one kilometre with less than half of it. Where does the velocity go? Into the air. This episode shows what the air does to a bullet, the shock wave, and the drag formula term by term: air density, the velocity squared, the drag coefficient, the cross-section and the mass. Then why the drag coefficient is not one number but a curve over the Mach number, what the standard bullets G1 and G7 are, what a ballistic coefficient really scales, and where one number stops being enough.

Chapters:
0:00 Intro
0:19 What the air does
0:44 The shock wave
1:07 The formula, in words
2:16 Watch it slow down
2:52 Cd is not one number
3:18 Standard bullets and the BC
4:00 One bullet, two BCs
4:21 Where one number breaks
4:59 Outro

The method behind it:
The ballistic coefficient: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/method/ballistic-coefficient.html
Choosing a drag model: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/choosing-a-drag-model.html

Ballistic Calculator 2 is free and open source (Windows, Linux, macOS):
Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
```

**Tags:** ballistics, external ballistics, drag, drag coefficient, ballistic coefficient, G1, G7, BC, Mach number, transonic, bullet drop, long range shooting, ballistic calculator, physics explained

## Episode 2

**Title:** How a dot flies: the point-mass model — Physics of a Ballistic Calculator, ep. 2

**Description:**

```
A ballistic calculator doesn't fly a bullet. It flies a dot: a point with a mass and no shape. Six numbers describe it — where it is and how fast it moves. Two forces act on it: gravity and drag. This episode builds the model from these pieces: the velocity as an arrow, a = F/m, the drag arrow that always points back along the velocity, the three lines the program actually computes, and one time step. Then how big the step has to be, and the whole flight to one kilometre.

The step shown here is the simplest one, to explain the idea. The program itself uses a more accurate method (midpoint Runge-Kutta); the link below explains both.

Chapters:
0:00 Intro
0:20 Six numbers
1:17 Two forces
2:12 The drag arrow
3:14 Add the arrows
4:00 One step
4:58 How big a step
6:31 The whole flight
7:33 Next time: the wind

The method behind it:
The 3DOF point-mass model: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/method/3dof-model.html
Integration modes: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/method/integration.html

Ballistic Calculator 2 is free and open source (Windows, Linux, macOS):
Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
```

**Tags:** ballistics, external ballistics, point mass, 3DOF, trajectory, numerical integration, Euler method, Runge-Kutta, drag, gravity, bullet drop, ballistic calculator, physics explained

## Episode 3

**Title:** The wind: why 3.35 m and not 9 — Physics of a Ballistic Calculator, ep. 3

**Description:**

```
A 4.5 m/s cross wind, a bullet that flies for two seconds. Shouldn't it drift 9 metres? It drifts 3.35. This episode shows how the program sees the wind: it doesn't push the bullet, it moves the air, and drag works against the bullet's velocity through that air. Look from the air, and the side part of drag slowly builds the bullet's side velocity — that is where the 3.35 metres come from.

Chapters:
0:00 Intro
0:17 Look from the air
1:16 The side part
3:26 Outro

The method behind it:
The 3DOF point-mass model: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/method/3dof-model.html

Ballistic Calculator 2 is free and open source (Windows, Linux, macOS):
Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
```

**Tags:** ballistics, external ballistics, wind drift, cross wind, wind deflection, drag, point mass, 3DOF, long range shooting, ballistic calculator, physics explained

## Episode 4

**Title:** What a dot can't do: spin and the turning Earth — Physics of a Ballistic Calculator, ep. 4

**Description:**

```
A bullet spins about 3 100 times per second, and the Earth turns under it. A dot can do neither, so the program flies the dot first and then adds four small effects on top. For each one: the physics first, then the formula the program uses and where it comes from, then each term, then the numbers for our shot at one kilometre.

Why a bullet has to spin, and the rule of a spinning top: a push turns it a quarter turn further, the way it spins. Stability and Miller's formula. Spin drift (31 cm) and Litz's formula. Aerodynamic jump (11 cm for a cross wind). Coriolis (10 cm to the right in the northern half of the world). And east–west: shooting east, the bullet falls a little slower, as if gravity were a little weaker; shooting west, a little faster (12 cm each way). Finally, the honest part: all four together are still smaller than the error of a wrong drag curve.

Chapters:
0:00 Intro
0:37 Why a bullet spins
1:38 Stability
2:47 Spin drift
3:56 Aerodynamic jump
5:16 The turning Earth: Coriolis
6:19 East and west
7:44 The honest part
8:28 Outro

The method behind it:
The empirical corrections: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/method/empirical-corrections.html
What the model includes: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/what-the-model-includes.html

Ballistic Calculator 2 is free and open source (Windows, Linux, macOS):
Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
```

**Tags:** ballistics, external ballistics, spin drift, gyroscopic stability, Miller stability formula, Litz, aerodynamic jump, Coriolis effect, Eotvos effect, precession, long range shooting, ballistic calculator, physics explained
