# Ballistic Calculator 2 — video demo scripts

A **playlist that grows**: episodes 1–7 build one shot step by step, and from episode 8 on each episode is a
**special case**. The cards carry no "N of M" (decided 2026-11-04), since episodes are added as we go. For now
the plan stops at episode 9; later topics already in view: the Reticle Editor and the reticle-designer skill,
building `.drg` drag tables (Tools), and hit probability.
Everything below is verified against the application's own `ShotTrajectoryCalculator`; every number is
cross-referenced in `demo-numbers.md`, whose section numbers match the episode numbers.

**How to read this file**

| | |
|---|---|
| `{CARD: …}` | a graphic, not footage. Text to put on it is given verbatim. |
| `{ACTION: …}` | what to do on screen. Not spoken. |
| `{FILL: …}` | type these fields **with no narration running**. Cut or speed up in editing. |
| `{HOLD n s}` | stop moving the mouse and let the viewer read. |
| `VOICE:` | the text for ElevenLabs, written the way it should be **said**. |

**Recording order.** Record the audio first, then record the screen while listening to it. The other
way round means re-cutting every take.

**The golden rule.** Never narrate while typing. Numbers go in during `{FILL}` blocks in silence; the
voice speaks over the *answer*.

---

## The narrative device: one baseline, one feature at a time

Episode 2 ends by saving the shot as **`bc2-demo.trajectory`**. Episodes 3, 4, 5 and 6 each **open that
same file** and add exactly **one** feature. So:

- every episode is independently recordable and re-recordable — no continuous session to preserve;
- every number the viewer sees is a comparison against **one** table they have already been shown;
- the effects never get tangled up in each other.

Then **episode 7 turns everything on at once** and shows what that does to the sight picture — which is
the honest way to end, because on a real range the effects arrive together.

Episode 6 saves its result as a **second** file, `bc2-demo-radar.trajectory`, leaving the baseline
untouched. Episode 7 opens the radar one, so the drag curve is already in place before it starts piling
on the weather.

| Episode | Opens | Adds | Saves |
|---|---|---|---|
| 1 | — (metric, from nothing) | — | — |
| 2 | — (imperial, from nothing) | the four views | `bc2-demo.trajectory` |
| 3 | `bc2-demo` | weather, then wind | — |
| 4 | `bc2-demo` | shape + twist (with wind as prerequisite), then Coriolis | — |
| 5 | `bc2-demo` | dialled clicks; then the zero's own weather | — |
| 6 | `bc2-demo` | the radar drag curve | `bc2-demo-radar.trajectory` |
| 7 | `bc2-demo-radar` | everything else, then the reticle | — |
| 8 | — (new rifle) | 300 Blackout + the V-COG | — |

**What a `.trajectory` does not restore:** the selected reticle and the active view. `Open` starts a
fresh window on the default view, so Mil-Dot (episodes 2 and 7) and the V-COG (episode 8) are always
re-selected on camera. Everything on the six dialog tabs — including the zero's own atmosphere and
ammunition, and every wind zone — does come back.

## The wrapper, on every episode

| Slot | Length | Produced how |
|---|---|---|
| Title card | 3 s | graphic, per episode |
| Cold open | 8 s | **one audio file and one clip, reused verbatim on every episode** |
| Recap | ~10 s | footage of `Trajectory → Open` and the restored table; narration per episode |
| Body | — | the episode script |
| Next up | ~8 s | narration per episode, over the last frame of the body |
| End card | 5 s | graphic, per episode |

## The load (episodes 1 to 7)

.308 Winchester, Lapua GB550 175 gr Scenar-L. Chosen because `data/drg` already ships that exact
bullet's radar curve, so episode 6 changes the drag model without changing the bullet.

| | Metric (ep. 1) | Imperial (ep. 2 on) |
|---|---|---|
| Weight | 11.3 g | 175 gr |
| Ballistic coefficient | 0.253 G7 | 0.253 G7 |
| Muzzle velocity | 790 m/s | 2600 ft/s |
| Rifle / sight | Tigr (civilian SVD) · Sight Preset **PSO-1 Optics**: 65 mm, 0.5 ths clicks | AR-style .308 · Sight Preset **Standard Optics (High, Mil)**: 3 in, 0.1 mrad clicks |
| Zero | 300 m | 300 yd |
| Range / step | 1000 m / 100 m | 1000 yd / 100 yd |

Set **`View → Angular Units → Milliradian`** before recording. Every angular number below is mrad.

---

# Shared assets — produce once

## The cold open  (8 s, identical on every episode)

{ACTION: the application's main window, empty. Neutral wallpaper, no icons, no taskbar clutter.}

VOICE:
This is Ballistic Calculator Two. It is free, it is open source, and it runs on Windows, Linux and
macOS.

## The end card  (5 s)

{CARD, per episode:

```
Next  →  Episode N:  <title of the next episode>
Download:    github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
User guide:  nikolaygekht.github.io/ballistic.calculator.app.avalonia
```

On episode 8 the card reads instead:

```
Download:    github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
Source:      github.com/nikolaygekht/ballistic.calculator.app.avalonia
User guide:  nikolaygekht.github.io/ballistic.calculator.app.avalonia
```
}

---

# Episode 1 — Your first trajectory

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 1   ·   Your First Trajectory`}

{ACTION: the shared cold open clip.}

Remade 2026-11-04 in the episode 2+ style (voice on every action, values spoken as typed) with the ammunition
library added. As produced: `build/ep01/record.py`, clips in `build/ep01/audio`. Numbers: `demo-numbers.md` §1. The
take writes `Documents\Ballistic Calculator demo\Lapua Scenar-L 11.3 g.ammox`.

## Intro

{ACTION: the empty main window.}

VOICE:
In this episode we calculate a simple trajectory: one rifle, one load, and no wind. Let's start.

## Body, part one — a new trajectory and the load

{ACTION: `Trajectory` on "Trajectory", hover `New` on "New", then `Metric`.}

VOICE:
Everything starts here: Trajectory, New. First, the program asks: metric or imperial?

{ACTION: click **Metric**; the Shot Parameters dialog opens on the Ammunition tab.}

VOICE:
I choose metric. This setting is for this window only, not for the whole program. You can open another window in imperial at the same time, and you can switch a window to the other system later.

{ACTION: sweep the six tab headers, back to Ammunition.}

VOICE:
One dialog describes the whole shot. It has six tabs, but only the first one is required.

{ACTION: type `Lapua Scenar-L 11.3 g` in Name.}

VOICE:
On the Ammunition tab, first a name, so we can find this load later: Lapua Scenar L, eleven point three grams.

{ACTION: type `11.3` in Weight (g).}

VOICE:
The bullet weight is eleven point three grams.

{ACTION: type `0.253` in BC, pick `G7`.}

VOICE:
The ballistic coefficient shows how well the bullet keeps its speed. It is zero point two five three, on the G seven table.

{ACTION: type `790` in Muzzle Vel. (m/s).}

VOICE:
And the muzzle velocity, the speed of the bullet when it leaves the barrel: seven hundred ninety metres per second.

VOICE:
This is the only tab you can't skip.

## Body, part two — the ammunition library

{ACTION: point at **Load**, press it: `data/ammo` with its calibre folders; point at `.308 Win`.}

VOICE:
You don't want to type a load every time. The program keeps loads as files. Load opens a small library of sample loads that comes with the program, sorted by calibre.

{ACTION: **Cancel**.}

VOICE:
Here it is. We don't need one now. Cancel.

{ACTION: **Save**: the dialog opens in `data/ammo` with the name filled in; type `E:\ngeht\Documents\Ballistic Calculator demo\Lapua Scenar-L 11.3 g`, **Save**.}

VOICE:
Save keeps our load as a file. The program suggests its own folder, and the name of the load as the file name. I keep my own loads in my own folder, because a new version of the program can replace this one. Save.

## Body, part three — rifle, zero, range

{ACTION: **Rifle** tab, Sight Preset → `PSO-1 Optics`.}

VOICE:
On the Rifle tab we describe the scope. Our rifle is a Tigr, the civilian S V D, in three oh eight, with the P S O one scope. The program has a list of common sights, so I just pick it.

{ACTION: point at Sight Height (65 mm), then H Click and V Click (0.5 ths).}

VOICE:
It fills two things. Sight height, how high the scope is above the barrel: sixty-five millimetres. And the click value, how much one click of the turret moves the point of impact: half a thousandth.

VOICE:
Sight height matters at every distance. For your own rifle, measure it. Don't guess.

{ACTION: **Zero** tab; point at the suggested 100 m, type `300`.}

VOICE:
On the Zero tab we set the zero distance: the distance where the bullet hits the point of aim. The sight preset suggested one hundred metres. We use three hundred.

{ACTION: **Parameters** tab: `1000` in Max Range, `100` in Step, **OK**.}

VOICE:
On the Parameters tab: how far to calculate, one thousand metres, and the step between the lines of the table, one hundred. Now OK.

{ACTION: the *Confirm* box "Weather not filled. Use default values?"; **Yes** on "Yes".}

VOICE:
We did not fill the Weather tab, so the program offers the default weather: sea level, fifteen degrees Celsius, normal pressure, and seventy-eight percent humidity. That is fine for now. We will change the weather in episode three. Yes.

## Body, part four — the result

VOICE:
Here is the result. One line for every hundred metres.

{ACTION: point at the 300 m drop.}

VOICE:
At three hundred metres the drop is zero. This is our zero distance, so the bullet is exactly on the line of sight.

{ACTION: point at the 100 m drop.}

VOICE:
At one hundred metres the bullet is fifteen centimetres above the line of sight.

{ACTION: move down the Drop column to 1000 m (−1208.66 cm).}

VOICE:
And at one thousand metres the bullet is twelve metres below the line of sight. Twelve metres, after two seconds of flight.

{ACTION: the **Chart** tab (Drop).}

VOICE:
This is the same shot as a chart. You can show velocity, Mach number, drop, windage or energy.

{ACTION: point at the **Reticle** tab header; do not open it.}

VOICE:
There is one more view: the Reticle tab. It shows the trajectory through your scope. We will use it in the next episode.

## Next up

VOICE:
In the next episode: the same shot in imperial, the Reticle and Summary views, and the file we will use for the rest of the series.

{CARD 5 s: `Next → Episode 2: Table, chart, reticle, summary`}

---

# Episode 2 — Table, chart, reticle, summary

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 2   ·   Table, Chart, Reticle, Summary`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: the app starts empty. `Trajectory → Open` → `bc2-demo-metric` (episode 1's shot, saved off
camera into the demo folder). Hold 2.5 s on the table.}

VOICE:
In the first episode we built this: a Tigr with the P S O one scope, an eleven point three gram
bullet, a zero at three hundred metres, and twelve metres of drop at one thousand.

{ACTION, on the voice below: `Trajectory → New → Imperial`. A second dialog opens.}

VOICE:
Now the same load in imperial, on a different rifle: an A R style rifle in three oh eight, with a
standard scope on a high mount. The rest of the series uses yards and inches, so I want you to see
both.

## Body, part one — the same shot in imperial  (~1 min 10 s)

Each value is spoken **while it is typed** (a silent fill followed by narration about it put the words a
minute behind the picture). One short clip per field.

{ACTION: type `175` in Weight.}
VOICE: The bullet is one hundred seventy-five grains.

{ACTION: type `0.253`, pick `G7`, then type `2600` in Muzzle Velocity.}
VOICE: The same coefficient, and two thousand six hundred feet per second.

{ACTION: Rifle tab, open Sight Preset. On the voice: pick **Standard Optics (High, Mil)**, then point at
the sight height (3 in) and the clicks (0.1 mrad).}
VOICE: The scope is the standard one from the list: three inches above the barrel, zero point one milliradian per
click.

{ACTION: Zero tab; type `300`.}
VOICE: And the zero is at three hundred yards.

{FILL — silent: Parameters `1000` yd, step `100` yd. **OK**, accept the default weather.}

{HOLD 2.5 s on the table. Point at the last row's drop, then its hold.}

VOICE:
At one thousand yards the drop is three hundred sixty inches. That's thirty feet. The hold is ten
milliradians.

{ACTION: point at the Mach column, last row.}

VOICE:
Look at the Mach column: one point zero one. At one thousand yards this bullet is almost subsonic.
Around this point, accuracy usually gets worse.

## Body, part two — the summary and the reticle  (~1 min 50 s)

{ACTION, on the voice below: click the **Summary** tab; point at each value as it is named.}

VOICE:
The Summary view shows it exactly. Here you see the point blank range, the near and far zero, and the
distance where the bullet becomes subsonic. For this load, it's one thousand and fourteen yards.

{ACTION, on the voice below: click the **Reticle** tab; click **Mil-Dot** on "I click Mil-Dot"; select
**Far BDC** (Display group) on "choose Far B D C".}

VOICE:
And here is the third view: your scope.

I click Mil-Dot, which is built into the program, and choose Far B D C. The blue labels show the
distance for each mark of the reticle, for this load.

{ACTION: move the mouse slowly down the vertical stalk, dot by dot.}

VOICE:
The first dot below the centre is about four hundred and six yards. The second one is four hundred
ninety-nine. The third is five hundred eighty-two. The fourth is six hundred fifty-seven.

So the reticle works like a range card. Remember these numbers. In episode seven we come back to this
picture, and every one of them will change.

{ACTION: point at the thick bottom post.}

VOICE:
Below the fourth dot there is only the thick post. So past about seven hundred twenty-five yards, you
need the turret. It's good to know this before you go to the range.

The program comes with thirty-four reticles, and you can also create your own.

## Body, part three — save it  (~35 s)

VOICE:
One more thing before we finish. The rest of the series depends on it.

{ACTION, on the voice below: `Trajectory` on "Trajectory", `Save As` on "Save As", type `bc2-demo` and
save during the next sentence; point at the window title (`bc2-demo`) on "Every next episode opens this file".}

VOICE:
Trajectory, Save As. This saves the whole shot to a file: the ammunition, the rifle, the zero, the weather,
the wind, and all the settings on the six tabs.

Every next episode opens this file and adds one thing to it. So every number you see will be compared
with the table we just made.

## Next up

VOICE:
Next time: the two things that change every shot — the weather and the wind.

{CARD 5 s: `Next → Episode 3: Weather and wind`}

---

# Episode 3 — Weather and wind

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 3   ·   Weather and Wind`}

{ACTION: the shared cold open clip.}

Rules from episode 2: the voice starts when the action starts, one short clip per action, no silent
fill followed by narration. Numbers: `demo-numbers.md` §3 (read from the app).

## Recap  (~20 s)

{ACTION, on the voice below: the app starts empty. `Trajectory` on "Trajectory", `Open` on "Open", pick
`bc2-demo`, open it.}

VOICE:
Trajectory, Open. This is the file we saved in episode two: the A R style rifle in three oh eight, a one
hundred seventy-five grain bullet at two thousand six hundred feet per second, and a zero at three
hundred yards.

{ACTION: point at the drop in the last row.}

VOICE:
At one thousand yards the drop is three hundred sixty inches. But this was calculated in the default
weather: sea level and fifty-nine degrees Fahrenheit. Today we change the weather, and then we add wind.

## Body, part one — weather  (~1 min 30 s)

{ACTION, on the voice: `View` → `Edit Parameters`. The dialog opens. Click the **Weather** tab.}

VOICE:
View, Edit Parameters. This opens the same dialog again. The weather is on the Weather tab.

VOICE:
Let's take a cold day in February.

{ACTION: type `1000` in Altitude.}
VOICE: The range is one thousand feet above sea level.

{ACTION: type `28.9` in Pressure.}
VOICE: The pressure is twenty-eight point nine inches of mercury.

{ACTION: point at the Pressure field.}
VOICE:
This must be the pressure where you stand. A weather report usually gives the pressure corrected to sea
level. Don't put that number here.

{ACTION: type `40` in Temperature.}
VOICE: The temperature is forty degrees.

{ACTION: type `60` in Humidity, then press **OK**.}
VOICE: And the humidity is sixty percent. OK.

{HOLD 2 s on the table. On the voice: point at the drop in the last row.}

VOICE:
At one thousand yards the drop is now three hundred fifty-nine inches. Only about one inch less.

VOICE:
Why so small? Cold air is denser, so the bullet slows down faster. But the air at one thousand feet is
thinner, so it slows down less. Here the two almost cancel each other.

In episode five you will see a case where the weather changes a lot.

## Body, part two — wind  (~1 min 30 s)

{ACTION, on the voice: `View` → `Edit Parameters`, then the **Wind** tab.}

VOICE:
Now the wind. Edit Parameters again, and the Wind tab.

{ACTION: type `10` in Velocity.}
VOICE: Ten miles per hour.

{ACTION: type `90` in Direction; the dial turns. Point at the dial.}
VOICE:
It blows from the right. On this dial, zero degrees is a wind from behind you, and ninety degrees is a
wind from the right.

{ACTION: point at the Distance checkbox, then at **Add Wind**. Press **OK** at the end.}
VOICE:
We leave the distance empty, so this wind blows along the whole way. If the wind is different at
different distances, you can add more winds here. OK.

{HOLD 2 s on the table. On the voice: point at Windage at 600 yd, then at 1000 yd.}

VOICE:
Before, the Windage column was all zeros. Now, at six hundred yards, the bullet moves thirty-one inches
to the left. At one thousand yards, one hundred and four inches. That's almost nine feet.

{ACTION: point at the wind correction (Wnd.Adj.) and its clicks in the last row.}

VOICE:
To correct for it, you need two point nine milliradians. That's twenty-nine clicks.

VOICE:
That's why wind is the hardest part of long range shooting. You can calculate the drop. The wind you
have to read on the range.

{ACTION, on the voice: close the trajectory window. The program does not ask about saving.}

VOICE:
Now I close this window without saving. The file from episode two stays as it was, and we open it again
in the next episode.

## Next up

VOICE:
Next time: three small effects that matter only far away. Spin drift, aerodynamic jump, and the
Coriolis effect.

{CARD 5 s: `Next → Episode 4: The Effects That Only Matter Far Away`}

---

# Episode 4 — Spin drift, aerodynamic jump, Coriolis

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 4   ·   The Effects That Only Matter Far Away`}

{ACTION: the shared cold open clip.}

Rules from episode 2: the voice starts when the action starts, one short clip per action, no silent
fill followed by narration. Numbers: `demo-numbers.md` §4 (read from the app). Windage in the table is
signed, **positive is left**: −8.00 is eight inches to the right.

## Recap  (~20 s)

{ACTION, on the voice below: the app starts empty. `Trajectory` on "Trajectory", `Open` on "Open", pick
`bc2-demo`, open it.}

VOICE:
Trajectory, Open. The same file from episode two: standard weather, and no wind.

{ACTION: point at the drop in the last row, then at the Windage column.}

VOICE:
At one thousand yards the drop is three hundred sixty inches, and the Windage column is all zeros.
Today we add three small effects. You see them only far away.

## Body, part one — spin drift  (~1 min 20 s)

VOICE:
The first one is spin drift. A bullet spins, like a gyroscope. During a long flight it slowly moves to
the side, in the direction of the spin.

{ACTION, on the voice: `View` → `Edit Parameters`. The dialog opens on the Ammunition tab.}

VOICE:
View, Edit Parameters. For spin drift, the program needs the size of the bullet and the twist of the
barrel.

{ACTION: type `0.308` in Diameter.}
VOICE: The diameter is zero point three oh eight inches.

{ACTION: type `1.394` in Length.}
VOICE: The length is one point three nine four inches.

{ACTION: the **Rifle** tab, open Rifling, pick **Right**.}
VOICE: On the Rifle tab, the rifling is right hand.

{ACTION: type `10` in Twist Rate, then press **OK**.}
VOICE: And the twist is one turn in ten inches. OK.

{HOLD 2 s on the table. On the voice: point at Windage in the last row.}

VOICE:
There is still no wind. But now, at one thousand yards, the bullet moves eight inches to the right.
This is spin drift.

{ACTION: point at the drop in the last row.}

VOICE:
And the drop did not change at all. Three hundred sixty inches, the same as before.

## Body, part two — aerodynamic jump  (~1 min)

VOICE:
The second effect is aerodynamic jump. It happens only when the wind blows from the side. A side wind
turns the nose of a spinning bullet a little, and the bullet moves up or down. So for the jump, we need
wind.

{ACTION, on the voice: `View` → `Edit Parameters`, then the **Wind** tab.}

VOICE:
Edit Parameters, and the Wind tab.

{ACTION: type `10` in Velocity.}
VOICE: Ten miles per hour,

{ACTION: type `90` in Direction, then press **OK**.}
VOICE: from the right. OK.

{HOLD 2 s on the table. On the voice: point at the drop in the last row.}

VOICE:
Look at the drop. Three hundred fifty-six and a half inches. Before the wind it was three hundred
sixty. The bullet went four inches up, from a wind that blows from the side.

VOICE:
This is aerodynamic jump. The program needs all four: the diameter, the length, the twist, and a side
wind. Without any one of them, there is no jump.

## Body, part three — the Coriolis effect  (~1 min 30 s)

{ACTION, on the voice: close the trajectory window. The program does not ask about saving.}

VOICE:
Now I close this window without saving.

{ACTION, on the voice: `Trajectory` → `Open` → `bc2-demo` again.}

VOICE:
Trajectory, Open, the same file again. The last effect has nothing to do with the bullet or the wind.

VOICE:
The Earth turns while the bullet flies. This is the Coriolis effect. For it, the program needs two
things: the direction you shoot, and how far north or south you are.

{ACTION, on the voice: `View` → `Edit Parameters`, then the **Parameters** tab.}

VOICE:
Edit Parameters, and the Parameters tab.

{ACTION: tick **Calculate Coriolis Effect**.}
VOICE: I turn on Calculate Coriolis Effect.

{ACTION: type `90` in Azimuth; the dial turns. Point at the dial.}
VOICE: The azimuth is ninety degrees. We shoot to the east.

{ACTION: type `45` in Latitude (N is already selected), then press **OK**.}
VOICE: The latitude is forty-five degrees north. OK.

{HOLD 2 s on the table. On the voice: point at Windage in the last row.}

VOICE:
At one thousand yards, the bullet moves three inches to the right.

{ACTION: point at the drop in the last row.}

VOICE:
And it hits almost four inches higher. Three hundred fifty-seven inches, instead of three hundred sixty.

VOICE:
If you shoot to the west, it goes four inches lower instead. To the north or to the south, the drop
does not change.

VOICE:
Is it worth it? At one thousand yards, on a small target, yes. At three hundred yards, no. If you
leave Coriolis off, the program does not calculate it.

## Next up

VOICE:
Next time: two situations from the range. What the table looks like when you have already dialled the
turret. And a rifle zeroed in winter that you shoot in July.

{CARD 5 s: `Next → Episode 5: Dialled clicks, and a winter zero in July`}

---

# Episode 5 — Dialled clicks, and a winter zero in July

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 5   ·   Dialled Clicks, and a Winter Zero in July`}

{ACTION: the shared cold open clip.}

Rules from episode 2: the voice starts when the action starts, one short clip per action, no silent
fill followed by narration. Numbers: `demo-numbers.md` §5 (read from the app).

## Recap  (~15 s)

{ACTION, on the voice below: the app starts empty. `Trajectory` on "Trajectory", `Open` on "Open", pick
`bc2-demo`, open it.}

VOICE:
Trajectory, Open. The same file from episode two.

{ACTION: point at Hold in the 800 yd row, then at its Clicks.}

VOICE:
Look at the eight hundred yard row. The hold is six point one eight milliradians. One click of our
scope is zero point one milliradian, so this is sixty-two clicks. The program shows them in the Clicks
column.

## Body, part one — clicks already on the turret  (~1 min 15 s)

VOICE:
Many shooters do not hold over at long range. They dial the turret. So let's say you dialled these
sixty-two clicks up. The table does not know about it. It still thinks the turret is at zero.

{ACTION, on the voice: `View` → `Edit Parameters`, then the **Rifle** tab.}

VOICE:
View, Edit Parameters, and the Rifle tab.

{ACTION: point at V Click, then at H Click.}

VOICE:
The size of one click is set here. V Click is zero point one milliradian, and H Click is the same. They
came with the sight preset in episode two. Without them, the program cannot count clicks.

{ACTION, on the voice: the **Parameters** tab.}

VOICE:
And now the Parameters tab.

{ACTION: point at V-Clicks, then at H-Clicks.}

VOICE:
V-Clicks is the number of clicks that are already on the turret. H-Clicks is the same for windage.

{ACTION: type `62` in V-Clicks, then press **OK**.}
VOICE: Sixty-two clicks up, the number from the eight hundred yard row. OK.

{HOLD 2 s on the table. On the voice: point at the drop in the 800 yd row.}

VOICE:
Now, at eight hundred yards, the drop is almost zero. This is where the rifle points now.

{ACTION: point at the drop in the 400 yd row.}

VOICE:
Everything closer goes high. At four hundred yards, the bullet goes seventy-six inches above the
crosshair. That's more than six feet.

{ACTION: point at Hold in the last row.}

VOICE:
And at one thousand yards you still need to hold three point eight milliradians.

VOICE:
So after you dial, this table shows what is left to hold.

{ACTION, on the voice: close the trajectory window. The program does not ask about saving.}

VOICE:
Now I close this window without saving.

## Body, part two — zeroed in winter, shooting in July  (~2 min)

{ACTION, on the voice: `Trajectory` → `Open` → `bc2-demo` again.}

VOICE:
Trajectory, Open, the same file again.

VOICE:
Now the second situation. You zeroed this rifle in February, in the weather from episode three: one
thousand feet up, and forty degrees. Now it is July, on the same range, and it is one hundred degrees.

{ACTION, on the voice: `View` → `Edit Parameters`, then the **Weather** tab.}

VOICE:
Edit Parameters, and the Weather tab.

{ACTION: type `1000` in Altitude, then `28.9` in Pressure.}
VOICE: The altitude and the pressure are the same as in February: one thousand feet, and twenty-eight point nine.

{ACTION: type `100` in Temperature.}
VOICE: The temperature is one hundred degrees.

{ACTION: type `90` in Humidity, then press **OK**.}
VOICE: And the humidity is ninety percent. OK.

{HOLD 2 s on the table. On the voice: point at the drop in the last row.}

VOICE:
At one thousand yards the drop is now three hundred twenty-eight and a half inches. In February it was
three hundred fifty-nine. That's thirty inches less. Hot air is thinner, so the bullet slows down less.

VOICE:
But this table is for a rifle zeroed today, in July. We changed only today's weather, and the program
calculates the zero in the same air as the shot. Our rifle was zeroed in February. So let's tell the
program that.

{ACTION, on the voice: `View` → `Edit Parameters`, then the **Zero** tab, scroll down.}

VOICE:
Edit Parameters, and the Zero tab. The Zero tab has its own weather, down here.

{ACTION: tick **Other atmosphere for zero**.}
VOICE: I turn on Other atmosphere for zero. This is the air you zeroed the rifle in.

{ACTION: type `1000` in Altitude, then `28.9` in Pressure.}
VOICE: One thousand feet, and twenty-eight point nine, as before.

{ACTION: type `40` in Temperature.}
VOICE: Forty degrees.

{ACTION: type `40` in Humidity, then press **OK**.}
VOICE: And forty percent. Winter air is dry. OK.

{HOLD 2 s on the table. On the voice: point at the drop in the last row.}

VOICE:
At one thousand yards the drop is three hundred twenty-seven inches. In February it was three hundred
fifty-nine. So in July this rifle drops thirty-two inches less. And this is the number you need, because
you do not zero your rifle again every time the weather changes.

{ACTION: point at the drop in the 300 yd row.}

VOICE:
But look at three hundred yards. The drop is not zero any more. At the zero distance itself, the bullet
hits almost half an inch high, because the air is not the same as on the day of the zero.

## Next up

VOICE:
Next time: we stop using the ballistic coefficient. Instead, we use the measured drag curve of this
bullet.

{CARD 5 s: `Next → Episode 6: A measured drag curve instead of a BC`}

---


# Episode 6 — A measured drag curve instead of a BC

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 6   ·   A Measured Drag Curve`}

{ACTION: the shared cold open clip.}

As produced (`build/ep06/record.py`, clips in `build/ep06/audio`). Numbers: `demo-numbers.md` §6. Part-one cards are
overlaid full frame after assembly (`overlay_cards.py`); PRB pictures carry "Image: PrecisionRifleBlog.com" and the
article link goes into the YouTube description: https://precisionrifleblog.com/2019/06/09/g1-vs-g7-vs-custom-drag-models/

## Recap

{ACTION: `Trajectory` → `Open` → `bc2-demo` (its load is now named **Baseline G7**).}

VOICE:
Trajectory, Open. The same file from episode two, one more time.

{ACTION: point at the 1000 yd drop.}

VOICE:
At one thousand yards the drop is three hundred sixty inches. All the numbers in this series came from one number: the ballistic coefficient, zero point two five three, G seven.

## Body, part one — what a BC is, and where drag curves come from

{ACTION: CARD `card-prb-profiles.png`.}

VOICE:
What is this number? It compares your bullet to a standard bullet. G one is an old standard, a bullet with a flat base. G seven is a newer one: a long bullet with a boat tail, much closer to a modern long range bullet.

{ACTION: CARD `card-curves.png`.}

VOICE:
The drag of the standard bullet was measured at every speed. The coefficient takes this curve and only makes it bigger or smaller. Here is the G seven curve, made smaller for zero point two five three, and the G one curve for zero point five oh five.

VOICE:
And here, in blue, is the real drag of our bullet. Its shape is different. That's why one number cannot fit at every speed.

{ACTION: CARD `card-prb-hornady.png`.}

VOICE:
And it is not only our bullet. Every bullet has its own curve. Here are four Hornady bullets against G seven. None of them follows it exactly.

{ACTION: CARD `bc-along-flight.png`.}

VOICE:
Here is the real G seven coefficient of our bullet, along the whole flight. Near the muzzle it is zero point two five. At three hundred yards it falls to zero point two four. Near one thousand yards it goes up to almost zero point three. The box gives you one number for all of this.

{ACTION: CARD `card-prb-radar.png`.}

VOICE:
So where do these curves come from? The maker shoots the bullet and follows it with a Doppler radar. The radar sees how fast the bullet slows down at every speed. From that, they get the drag of this exact bullet, without any standard bullet at all.

VOICE:
Some of these pictures are from the Precision Rifle Blog. It has a very good article about G one, G seven and measured drag curves. The link is in the description.

{ACTION: the card goes away.}

VOICE:
Lapua measured its bullets this way. Some makers publish these curves, others keep them inside their own apps. You have to look for the file for your bullet yourself. I have the one for our bullet, the one hundred seventy-five grain Scenar L.

{ACTION: `Tools` → `Approximate Drag Table`, point at `From BC Curve…`, then `From Measured Velocities…`; close the menu.}

VOICE:
If you cannot find a file for your bullet, the program can build a curve for you. Tools, Approximate Drag Table. From BC Curve makes one from several ballistic coefficients for different speeds. From Measured Velocities makes one from velocities measured at different distances.

## Body, part two — a new trajectory on the radar curve

{ACTION: `Trajectory` → `New` → `Imperial` (the first window stays as it is).}

VOICE:
Now let's use the file. I keep this window as it is, and make a new trajectory. Trajectory, New, Imperial.

{ACTION: press **Browse…** beside Drag table.}

VOICE:
On the Ammunition tab, next to Drag table, I press Browse.

{ACTION: type the demo folder into the file name box, Enter, click the `.drg`, **Open**.}

VOICE:
I go to my folder. Here is the file. Open.

{ACTION: point at BC (`1.000 GC`), the table, **BC is Form Factor**, BC again.}

VOICE:
The coefficient is one, and the table is G C, a custom curve. And BC is Form Factor is on. So this number is not a ballistic coefficient any more. It is a form factor: how much more or less drag the bullet has than the curve. One means exactly as the curve says. If your bullets fly a little better than the curve, you can put zero point nine eight here.

{ACTION: point at Name, Weight, Diameter, Length, Weight.}

VOICE:
The program also took the name, the weight, the diameter and the length from the file. The file says eleven point three grams. That's one hundred seventy-four point four grains, not one seventy-five.

{ACTION: type `2600` in Muzzle Vel.}

VOICE:
The muzzle velocity is ours: two thousand six hundred feet per second. The file knows the bullet, not your rifle.

{ACTION: **Rifle** tab, Sight Preset → `Standard Optics (High, Mil)`.}

VOICE:
On the Rifle tab, the same sight as in episode two: Standard Optics, High, Mil.

{ACTION: **Zero** tab, `300`; **OK**; *Confirm* → **Yes** (standard weather).}

VOICE:
On the Zero tab, three hundred yards. OK, and standard weather, yes.

{ACTION: point at the 1000 yd drop.}

VOICE:
At one thousand yards the drop is now three hundred seventy-one inches. That's ten inches more than with the coefficient.

{ACTION: point at its Clicks.}

VOICE:
On the turret, that's three more clicks.

## Body, part three — compare the two

{ACTION: `View` → `Compare` → `Add` (the radar window).}

VOICE:
Now let's put both on one chart. View, Compare, Add.

{ACTION: `Windows` → `1 Baseline G7`; `View` → `Compare` → `Add`.}

VOICE:
Windows, our first trajectory. View, Compare, Add again.

{ACTION: point at ~500 yd, then the far end of the curves.}

VOICE:
Here are the two drop curves. Up to about five hundred yards they are almost the same. Further out they go apart, and at one thousand yards the gap is ten inches.

{ACTION: `View` → `Chart` → `Velocity`.}

VOICE:
View, Chart, Velocity. Here the radar bullet is slower all the way.

{ACTION: point at ~800 yd on the velocity chart.}

VOICE:
And at eight hundred yards the bullet is twenty-five feet per second slower. Remember the chart: in the first few hundred yards the real coefficient is worse than the box says. The bullet loses speed there, and it never gets it back.

VOICE:
And zero point two five three is a good number. It is the G seven number. With the G one number from the box, zero point five oh five, the error is sixteen inches.

VOICE:
Inside three hundred yards, a good coefficient is enough. Past eight hundred, the drag curve matters more than almost anything else, except your muzzle velocity.

## Body, part four — save the radar version

{ACTION: `Windows` → `2 .308 Lapua…`; `Trajectory` → `Save As`.}

VOICE:
This one I keep. Windows, the Lapua window. Trajectory, Save As.

{ACTION: type `bc2-demo-radar`, **Save**.}

VOICE:
A new name: b c two demo radar. Save. The first file stays as it was.

## Next up

VOICE:
Next time: we turn on everything at once, and look through the scope.

{CARD 5 s: `Next → Episode 7: Everything at once`}

---

# Episode 7 — Everything at once

**Produced 2026-11-04** — `build/ep07/ep07.mp4` (5:14). Text below is exactly what was voiced (`build/ep07/clips.json`); q06b/q06c carry full-frame cards laid over in post (`build/ep07/overlay_cards.py`).

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 7   ·   Everything At Once`}
{ACTION: the shared cold open clip.}

Numbers: `demo-numbers.md` §7 (read from the app 2026-11-04, `build/ep07/read_numbers.py`).

## Recap
r01 {Trajectory → Open → bc2-demo-radar}
Trajectory, Open. This time the file from episode six, with the measured drag curve.

r02 {point at the 1000 yd drop, then the windage}
At one thousand yards the drop is three hundred seventy-one inches, and there is no windage at all.

r03 {Reticle tab, Mil-Dot, Far BDC; point down the labels}
And the scope: the Reticle tab, Mil-Dot, Far B D C. The dots below the centre are four hundred five, four ninety-six, five seventy-eight and six fifty-one yards. Remember these numbers.

r04 {back to the Table tab}
Until now we added one effect at a time. Today we add them on top of each other, like on a real range day.

## Step 1 — spin drift
e01 {View → Edit Parameters → Rifle tab}
View, Edit Parameters, the Rifle tab.

e02 {Rifling → Right}
The drag file already gave us the diameter and the length of the bullet. So we only need the rifling: right hand,

e03 {Twist 10, OK}
one turn in ten inches. OK.

e04 {point at the 1000 yd windage, then drop}
Spin drift: eight inches to the right. The drop did not change.

## Step 2 — Coriolis
e05 {Edit Parameters → Parameters tab, tick Coriolis}
Edit Parameters, the Parameters tab. Calculate Coriolis Effect,

e06 {Azimuth 90}
we shoot to the east,

e07 {Latitude 45, OK}
at forty-five degrees north. OK.

e08 {point at the drop, then the windage}
The bullet hits almost four inches higher, and the drift to the right grows to eleven and a half inches.

## Step 3 — the weather from episode five
e09 {Edit Parameters → Weather tab; 1000, 28.9, 100, 90}
Now the weather from episode five. Edit Parameters, the Weather tab. July: one thousand feet, twenty-eight point nine, one hundred degrees, ninety percent.

e10 {Zero tab, scroll, tick Other atmosphere for zero}
And the Zero tab: the rifle was zeroed in February.

e11 {1000, 28.9, 40, 40, OK}
One thousand feet, twenty-eight point nine, forty degrees, forty percent. OK.

e12 {point at the 1000 yd drop}
Three hundred thirty-three inches: thirty-four inches less than before.

## Step 4 — the wind, and aerodynamic jump
e13 {Edit Parameters → Wind tab; 10, 90, OK}
And last, the wind. Edit Parameters, the Wind tab. Ten miles per hour, from the right. OK.

e14 {point at the 1000 yd windage}
The wind takes the bullet eighty-two inches to the left.

e15 {point at the 1000 yd drop}
And look at the drop: three hundred twenty-nine, four inches less than without the wind. This is aerodynamic jump from episode four. It needs everything together: the bullet's shape, the twist, and the wind.

e16 {point at the 300 yd drop}
And at three hundred yards, our zero distance, the bullet is now almost two inches high.

e17
Some effects add up, some cancel each other. You cannot guess that. You put the real numbers in.

## The scope
q01 {Reticle tab (still Mil-Dot, Far BDC); point down the labels}
The Reticle tab again. Four hundred five is now four hundred twenty-eight. Four ninety-six is five twenty-two. Five seventy-eight is six oh seven. And six fifty-one is six eighty-four. Every dot is worth twenty to thirty yards more.

q02 {Near BDC; point at the labels}
Near B D C shows the distances before the zero. The scope sits three inches above the barrel, so at very short range the bullet is still below the line of sight. At twenty-two yards you hold one milliradian high, at twelve yards four.

q03 {Target; size 20 × 20 in, distance 600 yd; point at the box}
Target puts a target into the scope. A twenty inch plate at six hundred yards. The box is where the bullet lands: low, and to the left, because of the wind. The plate is less than one milliradian wide, so the wind call must be right to a fraction of a milliradian.

q04 {Load… → VCOG-16-762-175gr; Far BDC}
The program comes with thirty-four reticles. Load. For example, the Trijicon V-COG for seven six two, made for a one hundred seventy-five grain bullet.

q05 {point at the etched 4, then 10}
Its marks say four to ten: hundreds of metres, for its own load and its own zero. With our zero at three hundred yards and today's air, the four is five hundred ninety-three yards, and the ten is one thousand two hundred sixty-three.

q06
An etched B D C is true only for the load, the zero and the air it was made for. The program labels it for yours.

q06b {CARD: Reticle Editor screenshot, docs/screenshots/reticleeditor.png}
The calculator also comes with a reticle editor, so you can draw your own reticles. And some of the reticles that come with it are quite fun.

q06c {CARD: the reticle-designer skill name and its address}
And if you use an AI assistant like Claude or Codex, there is a skill that makes a reticle file from a description, or even from a picture of a reticle. It is in the repository too. The link is in the description.

q07
The scope did not change. Only the day, the air and the wind did. A printed range card is a photo of one day. The calculator shows you the day you are standing in.

## Next up
n01
Next time, the first special case: one rifle and two different cartridges. We zero it with one and shoot the other. Three hundred Blackout.

{CARD 5 s: `Next → Episode 8: 300 Blackout — zero one load, shoot another`}

---

# Episode 8 — Zero one load, shoot another

As produced 2026-11-05 (`build/ep08/ep8-script.md`, `build/ep08/record.py`, `ep08.mp4` 5:05). Clip ids = files in `build/ep08/audio/`.

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

---

# Episode 9 — Shooting at an angle

As produced 2026-11-05 (`build/ep09/ep9-script.md`, `build/ep09/record.py`, `ep09.mp4` 3:42). Clip ids = files in `build/ep09/audio/`.

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

---

# Episode 10 — Your own drag table

As produced 2026-11-05 (`build/ep10/ep10-script.md`, `build/ep10/record.py`). Clip ids = files in `build/ep10/audio/`. Cards: `card-bc-by-speed.png` (b01), `card-warner-sheet.png` (v01), `card-csv.png` (v04, first 8.6 s), overlaid after assembly (`overlay_cards.py`).

{CARD 3 s: `Ballistic Calculator 2   ·   Episode 10   ·   Your Own Drag Table`}
{ACTION: the shared cold open clip.}

Numbers: read from the app 2026-11-05 (`build/ep10/probe.py`): 1000 yd hold 10.01 one BC / 10.28 five BCs / 10.30 radar
(drop 360.51 / 370.01 / 370.81 in); .338 at 1500 yd 10.36 (G7 0.484) / 10.40 (our curve, 6000 ft 78 °F).
Agreed with Nikolay 2026-11-05: part 1 builds the Scenar-L curve from several BCs taken off the ep. 6 radar curve
and checks it against that radar `.drg`; part 2 builds the Warner .338 Flatline 285 gr from its published
velocity table and shows why the air of the range (EMRTC, New Mexico Tech, Socorro NM) matters.

## Intro
i01
In episode six we used a drag curve measured with a radar. It is the best data you can have for a bullet, but not every maker publishes one.

i02
If you do not have the file, the program can build an approximate curve for you. There are two ways: from several ballistic coefficients, or from velocities measured down range. Today we try both, and check each one against real radar data.

## Part 1 — from several coefficients
b01 {CARD `card-bc-by-speed.png`: the G7 BC of the Scenar-L against velocity, five points marked}
Some makers give more than one coefficient: one for each speed range. For our bullet I take five points from the radar curve of episode six. At two thousand six hundred feet per second the G seven coefficient is zero point two five. At eleven hundred it is almost zero point three.

b02 {Tools → Approximate Drag Table → From BC Curve}
Tools, Approximate Drag Table, From BC Curve.

b03 {Name; Weight 175 gr; Diameter 0.308 in}
A name. The weight, one hundred seventy-five grains, and the diameter, point three oh eight. The program needs both: they turn the coefficients into a real drag curve.

b04 {Base table G7}
The base table is G seven, because our coefficients are G seven numbers.

b05 {BC 0.250, Velocity 2600, Add}
The first point: zero point two five oh, at two thousand six hundred feet per second. Add. The program turns the speed into a Mach number, for standard air.

b06 {four more points, Add after each}
Zero point two four oh at twenty-two hundred. Zero point two four four at eighteen hundred. Zero point two six three at fourteen hundred. And zero point two nine three at eleven hundred.

b07 {Save Drg → name → Save; Close}
Save Drg. I put it next to the radar file. Close.

b08 {Trajectory → Open → bc2-demo; point at the 1000 yd Hold}
Now let's check it. Trajectory, Open: our file from episode two, with one coefficient. At one thousand yards we hold ten point oh one.

b09 {Edit Parameters → Browse → the new file; Name → "Five G7 BCs"; OK; point at the 1000 yd Hold}
Edit Parameters, Browse, and the new file. A new name: five G seven BCs. OK. Now we hold ten point two eight.

b10 {Trajectory → Open → bc2-demo-radar; point at the 1000 yd Hold}
And the real radar curve from episode six: ten point three oh. Five coefficients came within one inch at one thousand yards. One coefficient was off by ten inches.

b11 {in the radar window: View → Compare → Add}
Let's put both on one chart. In the radar window: View, Compare, Add.

b12 {Windows → the bc2-demo window (five coefficients); View → Compare → Add; Chart tab}
Windows, our five coefficients. View, Compare, Add again. And the chart: the two curves lie on top of each other.

## Part 2 — from measured velocities
v01 {CARD `card-warner-sheet.png`: page 1 of Warner's data sheet + source line}
The second way is velocities: the speed of the bullet at different distances, measured with a big Doppler radar. Some makers publish them. Warner Tool publishes radar data sheets for its Flatline bullets. Here is the three thirty-eight, two hundred eighty-five grains: the speed every hundred yards, out to fifteen hundred.

v02 {Tools → Approximate Drag Table → From Measured Velocities}
Tools, Approximate Drag Table, From Measured Velocities.

v03 {Name; Weight 285 gr; Diameter 0.338 in}
The name, the weight, two hundred eighty-five grains, and the diameter, point three three eight.

v04 {CARD `card-csv.png` over the first two sentences: the file as plain text; then Load Csv → 338-flatline-velocities.csv → Open; the grid fills}
I typed the table into a small text file. Here it is: a header, then a distance and a speed on each line, each with its unit. Load Csv. Sixteen readings.

v05 {point at the status line: "standard atmosphere"}
Before I save, one more thing: the air. The program has to know the air these speeds were measured in. In thin air the bullet slows down less, so the same speeds mean a different drag curve.

v06
So where were they measured? Warner tests its bullets at E M R T C, the Energetic Materials Research and Testing Center of New Mexico Tech, near Socorro, New Mexico. The town is at forty-six hundred feet, and the ranges go up into the hills.

v07
The sheet does not say what the weather was. But it also has the drag for standard air, so we can check which air fits. It is about six thousand feet on a warm day, seventy-eight degrees. That air is as thin as at almost nine thousand feet on a standard day.

v08 {Set Atmosphere: 6000 ft, 23.98 inHg, 78 °F, 25 %; OK}
Set Atmosphere. Six thousand feet, twenty-three point nine eight inches of pressure, seventy-eight degrees, and dry air: twenty-five percent. OK.

v09 {Save Drg → "338 flatline" → Save; Close}
Save Drg. Close.

v10 {Trajectory → Open → 338-flatline (prepared); point at the 1500 yd Hold}
I prepared a trajectory for this bullet: three thousand seventy-nine feet per second, a zero at one hundred yards, out to fifteen hundred. It uses Warner's own G seven coefficient: zero point four eight four. At fifteen hundred yards we hold ten point three six.

v11 {Edit Parameters → Browse → 338 flatline.drg → OK; point at the Hold}
Now our curve: ten point four oh. Almost the same as Warner's number.

## Wrap-up
x01
Both ways give an approximation. A radar curve from the maker is still the best. But five good coefficients, or a good velocity table with the right air, come very close.

x02
This is the last episode of this series. The program can do more than we showed, and the user guide describes all of it. Thank you for watching.

{CARD 5 s: the final end card `end-10.png` — free and open source, links}

---

# Series outro — not used

Episode 10 ends with its own short x02 instead ("This is the last episode of this series…"); the end card
`end-10.png` carries the links. Kept for reference.

Written for whichever episode ends the series (episode 10 is planned now, so not episode 9).

{ACTION: optional silent montage, about five seconds each, no narration: two loads compared on one
chart · the hit probability dialog · export to C S V · the ammunition library · the drag table builder
under Tools.}

VOICE:
There is more than we had time for in eight episodes. You can compare several loads on one chart. You
can estimate hit probability. You can export the table, and build your own drag curves and your own
reticles.

The program is free and open source. There are builds for Windows, Linux and macOS, and there is a
written user guide if you prefer to read.

The links are below this video. Thank you for watching.

{CARD 5 s: `Downloads · source · user guide` + links}

---

## Running time

Body figures are voice plus fill and pauses; the wrapper adds the title card, cold open, recap,
next-up line and end card.

| Episode | Body | Wrapper | Total |
|---|---|---|---|
| 1 Your first trajectory | 3:00 | 0:54 | **3:54** |
| 2 Four views + save the baseline | 3:35 | 0:34 | **4:09** |
| 3 Weather and wind | 3:25 | 0:34 | **3:59** |
| 4 Spin drift, jump, Coriolis | 3:45 | 0:34 | **4:19** |
| 5 Dialled clicks, winter zero | 3:30 | 0:34 | **4:04** |
| 6 A measured drag curve | 2:45 | 0:34 | **3:19** |
| 7 Everything at once | 3:00 | 0:37 | **3:37** |
| 8 300 Blackout | 4:10 | 1:11 | **5:21** |
| **Playlist** | **27:10** | **5:32** | **~32:40** |

Episode 1's wrapper carries the "what this series is" block instead of a recap; episode 8's carries the
outro instead of a next-up line.

## Recording notes specific to the baseline structure

1. **Record episodes in order anyway.** Not because the state carries over — it does not — but because
   the narration refers forward ("remember those numbers", "the number I promised you in episode three").
2. **Never save from episodes 3, 4 or 5.** Close without saving, decline any prompt. If `bc2-demo.trajectory`
   picks up a stray change, every later episode's recap number is wrong. Keep a spare copy of the file.
3. **Episode 4 opens the baseline twice** — once for spin drift and jump, once for Coriolis. Spin drift is shown
   clean (no wind); the wind goes on only for the jump, and only the drop is discussed there. Episode 5
   likewise, once for clicks and once for the weather swap. That is deliberate: it is what keeps each
   effect measured against the baseline and nothing else.
4. **Episode 7's fill is long.** Six tab groups in one `{FILL}` — speed it up hard in the edit. If it
   still drags, save `bc2-demo-everything.trajectory` off camera beforehand and simply `Trajectory → Open` it;
   the numbers are identical either way.
5. **The reticle is never restored by a file.** Mil-Dot in episodes 2 and 7, and the V-COG in episode 8,
   are always re-selected on camera.

## YouTube metadata

**Channel description** (YouTube Studio → Customization → Basic info; draft 2026-11-05):
Ballistic Calculator 2 is a free, open-source ballistic calculator for Windows, Linux and macOS. The videos
here show how to use it, one step at a time: a first trajectory, weather and wind, spin drift, Coriolis, scope
reticles and BDC, measured drag curves, shooting at an angle, and building your own drag table. The
calculations come from the open-source BallisticCalculator library for .NET. It is a full model of the
bullet's flight, but still a model: always check it against your own shots.

Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
Source: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
Library: https://github.com/gehtsoft-usa/BallisticCalculator1

**Playlist title:** Ballistic Calculator 2 — build one shot, step by step

**Playlist description:** A free, open-source ballistic calculator for Windows, Linux and macOS. Ten
short episodes. We build one long-range shot, save it, and then add one thing at a time — weather, wind,
spin drift, aerodynamic jump, Coriolis, dialled clicks, a winter zero in summer air, and a radar-measured
drag curve — so you can see what each one is actually worth. Then we turn everything on at once. The last
three episodes are special cases: zeroing a rifle with one cartridge and shooting another, shooting uphill
and downhill, and building your own drag table from several BCs or from measured velocities.

**Episode 1 description** (the series overview lives here, not in the narration): A free, open-source
ballistic calculator for Windows, Linux and macOS. In this series we build one shot, step by step. This
first episode calculates a simple trajectory and shows how to keep a load in the ammunition library, so you never type it twice; then we save the shot, and every next episode opens the same file
and adds one more thing — weather, wind, and the small effects that only matter at long range. Then we turn
everything on together, and after that come the special cases: zeroing with one load and shooting another,
shooting uphill or downhill, and building your own drag table.

Per-episode titles, in playlist order:

```
1. Your first trajectory — Ballistic Calculator 2, ep. 1
2. Table, chart, reticle, summary — Ballistic Calculator 2, ep. 2
3. Weather and wind — Ballistic Calculator 2, ep. 3
4. Spin drift, aerodynamic jump and Coriolis — Ballistic Calculator 2, ep. 4
5. Dialled clicks, and a winter zero in July — Ballistic Calculator 2, ep. 5
6. A measured drag curve instead of a BC — Ballistic Calculator 2, ep. 6
7. Everything at once — Ballistic Calculator 2, ep. 7
8. 300 Blackout: zero one load, shoot another — Ballistic Calculator 2, ep. 8
9. Shooting at an angle — Ballistic Calculator 2, ep. 9
10. Your own drag table — Ballistic Calculator 2, ep. 10
```

In-episode chapter markers. Eps. 1–6 are timed from the produced videos (title card 3 s + `video0` + the cue
time in `build/epNN/timeline.json`); ep. 7 was checked against its take's `timeline.json` on 2026-11-05 (all ten marks match); eps. 8–10 are timed from their takes (2026-11-05). YouTube needs
the first one at 0:00, at least three, each at least 10 s long.

```
Episode 1
0:00 Intro
0:22 A new trajectory
0:49 The load
1:22 The ammunition library
1:51 Rifle, zero, range
3:05 The result

Episode 2              Episode 3          Episode 4                 Episode 5
0:00 Intro             0:00 Intro         0:00 Intro                0:00 Intro
0:40 Imperial          0:46 Weather       0:38 Spin drift           0:36 Dialled clicks
1:57 Summary, reticle  1:46 Wind          1:33 Aerodynamic jump     1:54 A winter zero in July
3:03 Save it                              2:24 Coriolis

Episode 6                          Episode 7
0:00 Intro                         0:00 Intro
0:34 What a BC really is           0:15 Recap: the radar file
1:45 Where drag curves come from   0:53 Spin drift
2:46 A trajectory on the radar curve 1:17 Coriolis
4:26 Compare the two               1:39 July air, February zero
5:33 Save it                       2:11 Wind and aerodynamic jump
                                   2:54 The scope: Far and Near BDC
                                   3:29 A target in the scope
                                   3:46 A real BDC reticle: Trijicon V-COG
                                   4:23 Reticle editor and the AI skill

Episode 8                                      Episode 9
0:00 Intro                                     0:00 Intro
0:53 The supersonic load                       0:28 On the flat
2:08 Its sight picture                         0:42 Two kinds of drop on the chart
2:30 The subsonic load, zeroed with the other  1:07 Uphill
3:37 What a shared zero costs                  2:26 Downhill
4:00 The reticle for the subsonic load         2:42 The rifleman's rule

Episode 10
0:00 Intro
0:40 From several ballistic coefficients
1:54 Checking it against the radar curve
2:44 From measured velocities
3:29 The air of the range
4:33 Checking it against Warner's BC
5:08 Wrap-up
```

Every description ends with the same block (the playlist link goes in once the playlist exists):

```
Download: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases
Source: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia
Playlist: https://www.youtube.com/playlist?list=PLLcWDVvDdkbU
```

Episodes 2 to 8 open by naming the previous episode, so a viewer who lands from search knows there is an
earlier one.

### Episode descriptions (produced episodes)

**Episode 2.** The same shot again, this time in imperial units: a
.308 Winchester with a 175 grain Lapua Scenar-L at 2600 ft/s, zeroed at 300 yards. Then the three other views
— the chart, the summary and the reticle with Mil-Dot holds — and we save the shot as the file every next
episode opens.

**Episode 3.** We open the saved shot and change the weather
to a cold February day at 1000 feet: almost nothing changes, and we see why. Then a 10 mph wind from the right:
104 inches of drift at 1000 yards, and the clicks to correct it.

**Episode 4.** Three small effects that only matter far away. Spin drift
from the bullet's size and the barrel twist (8 inches at 1000 yards, no wind needed), aerodynamic jump (only
with a side wind), and the Coriolis effect shooting east and west at 45 degrees north.

**Episode 5.** Two situations from the range.
What the table shows after you have already dialled 62 clicks up. And a rifle zeroed in February shot on a
hot, humid July day: 32 inches less drop at 1000 yards, and why the zero needs its own weather.

**Episode 6.** What a ballistic coefficient
really is — a ratio to a standard G1 or G7 bullet — and why one number cannot fit a real bullet at every
speed. Where measured drag curves come from (Doppler radar), how to build one in the program when you cannot
find one, and what Lapua's radar curve for the 175 grain Scenar-L does to the trajectory: 10 inches more drop
at 1000 yards. We compare both on one chart and save the radar version.

Pictures of the standard bullet profiles, the Hornady drag curves and the Doppler radar are from
PrecisionRifleBlog — a very good article about G1, G7 and custom drag models:
https://precisionrifleblog.com/2019/06/09/g1-vs-g7-vs-custom-drag-models/

**Episode 7.** Everything at once, on the radar-curve
shot: spin drift (1:10 right twist), Coriolis shooting east at 45 degrees north, a hot July day on a rifle zeroed
in February, and a 10 mph wind from the right — with aerodynamic jump showing up in the drop. Then the scope: every
Mil-Dot BDC label moves 20 to 30 yards, Near BDC for very short range, a 20 inch plate at 600 yards drawn in the
reticle, and the Trijicon V-COG 7.62 etched BDC relabelled for our load, zero and air.

The program comes with a reticle editor, and there is a skill for Claude Code, Codex and other AI assistants that
makes a reticle file from a description or from a picture of a reticle:
https://github.com/gehtsoft-usa/BallisticCalculator1/tree/main/SKILL/SKILLS/reticle-designer

**Episode 8.** The first special case: a rifle in .300 Blackout with a
Trijicon V-COG scope. One cartridge, two loads: a fast supersonic one, close to the 7.62x39, and a heavy
subsonic one for a suppressor, close to the 9x39. The rifle is zeroed with the supersonic load, so the subsonic
trajectory uses "Other ammunition for zero" — and shows what a shared zero costs: 42 cm low at the 100 m zero,
almost 2 m low at 200 m. Then the V-COG's BDC ladder for both loads; with Split at 50 m the subsonic marks read
55, 102 and 151 m.

**Episode 9.** Shooting uphill and downhill, on the .308 shot
from episode 2. At 1000 yards and 30 degrees the hold is 8.2 mrad instead of 10.0, uphill or downhill: only part
of gravity pulls the bullet away from a tilted line of sight. The chart shows why the program has two kinds of
drop — over muzzle level (where the bullet is in space) and over the line of sight (what you hold). And the
rifleman's rule: much better than nothing, but at 1000 yards almost one milliradian away from the full
calculation.

**Episode 10.** No radar drag file for your bullet? The program can
build an approximate one, in two ways. From several ballistic coefficients: five G7 numbers taken off Lapua's
radar curve for the 175 grain Scenar-L give a curve within one inch of the radar file at 1000 yards, where the
single box BC was off by ten inches. And from velocities measured down range: Warner Tool publishes Doppler radar
sheets for its Flatline bullets, and the .338 285 grain table, loaded from a small CSV file, rebuilds the curve —
as long as you enter the air it was measured in. Warner tests at EMRTC, New Mexico Tech, near Socorro, New
Mexico, high in the desert. The last episode of the series.

Warner Tool Doppler data sheets: https://www.warner-tool.com/doppler-data/

The CSV format for Load Csv is in the user guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/approximating-a-drag-table.html
