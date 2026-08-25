# Ballistic Calculator 2 — video demo scripts

A **playlist of eight episodes**, three to five minutes each, about thirty-two minutes in total.
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
| Cold open | 8 s | **one audio file and one clip, reused verbatim on all eight** |
| Recap | ~10 s | footage of `File → Open` and the restored table; narration per episode |
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
| Sight height | 50 mm | 2 in |
| Click value | 0.1 mrad | 0.1 mrad |
| Zero | 300 m | 300 yd |
| Range / step | 1000 m / 100 m | 1000 yd / 100 yd |

Set **`View → Angular Units → Milliradian`** before recording. Every angular number below is mrad.

---

# Shared assets — produce once

## The cold open  (8 s, identical on all eight episodes)

{ACTION: the application's main window, empty. Neutral wallpaper, no icons, no taskbar clutter.}

VOICE:
This is Ballistic Calculator Two. It is free, it is open source, and it runs on Windows, Linux and
macOS.

## The end card  (5 s)

{CARD, per episode:

```
Next  →  Episode N:  <title of the next episode>
Downloads and the user guide: <link>
```

On episode 8 the card reads instead:

```
Downloads · source · user guide
<links>
```
}

---

# Episode 1 — Your first trajectory

{CARD 3 s: `Ballistic Calculator 2   ·   1 of 8   ·   Your First Trajectory`}

{ACTION: the shared cold open clip.}

## What the series does  (~30 s)

{ACTION: stay on the empty main window.}

VOICE:
In this series we build one shot, step by step. We start with the smallest possible trajectory, in this
episode. Then we save it, and after that every episode takes that same saved shot and adds exactly one
thing to it. Weather. Wind. The small effects that only matter far away.

At the end we turn everything on at once, and then we do something most free calculators cannot do. We
zero a rifle with one cartridge, and shoot a completely different one.

Eight short episodes. Let us start.

## Body — the smallest trajectory, in metric  (~3 min)

{ACTION: `Trajectory → New → Metric`. The Shot Parameters dialog opens on the Ammunition tab.}

VOICE:
Everything begins here. Trajectory, New. The application asks one question first: metric or imperial.

I will choose metric. This choice belongs to the window, not to the program. You can open a second
window in imperial at the same time. And you can convert a finished window later.

{HOLD 2 s on the six tab headers.}

VOICE:
One dialog describes the whole shot. Six tabs. Only the first one is required. The rest have sensible
defaults.

{FILL — silence. Ammunition tab: Weight `11.3` g · Ballistic coefficient `0.253`, table `G7` ·
Muzzle velocity `790` m/s. Leave the name, diameter and length empty for now.}

VOICE:
The Ammunition tab needs three numbers. The weight of the bullet. Its ballistic coefficient, which
describes how well it cuts the air. And the speed it leaves the barrel.

Eleven point three grams. Zero point two five three, on the G seven drag table. Seven hundred ninety
metres per second.

That is the only tab you cannot skip.

{ACTION: click the **Rifle** tab.}

{FILL — silence. Rifle tab: Sight height `50` mm · Vertical click `0.1` mrad · Horizontal click
`0.1` mrad. Leave the barrel group empty.}

VOICE:
The Rifle tab describes the sight. How high the scope sits above the barrel: fifty millimetres. And
how much one click of the turret moves the point of impact: zero point one milliradian.

Sight height matters at every distance. Please do not guess it.

{ACTION: click the **Zero** tab.}

{FILL — silence. Zero tab: Distance `300` m.}

VOICE:
The Zero tab asks where the rifle is sighted in. Three hundred metres.

Notice what it does not ask for. It never asks for the angle of your scope. It calculates that angle
from the zero you give it.

{ACTION: click the **Parameters** tab.}

{FILL — silence. Parameters tab: Maximum distance `1000` m · Step `100` m.}

VOICE:
The Parameters tab asks how far to compute, and how often to print a line. One thousand metres, in
steps of one hundred.

Now press OK.

{ACTION: press **OK**. If the dialog offers defaults for the untouched Weather and Wind tabs, accept
them. The trajectory window opens on the Table view.}

{HOLD 4 s on the full table.}

VOICE:
There is the answer. One line for every hundred metres.

At three hundred metres the drop is zero. That is our zero, so the bullet is exactly on the line of
sight.

At one hundred metres the bullet is sixteen centimetres high. This surprises people. The bullet is
still climbing towards the sight line, so a close target is above your crosshair, not below it.

{ACTION: move the mouse slowly down the Drop column to the last row.}

VOICE:
And at one thousand metres, the bullet has fallen twelve metres below the line of sight. Twelve
metres. Two seconds of flight.

{ACTION: `Ctrl+C` or `View → Show → Chart`. Choose **Drop** in the variable selector.}

{HOLD 3 s.}

VOICE:
The same shot as a curve. You can chart velocity, Mach number, drop, windage or energy.

The curve is not a parabola. It gets steeper and steeper, because the bullet keeps losing speed.

## Next up

VOICE:
Next, the same shot in imperial — the other three ways this program can show you a trajectory, and the
file we are going to reuse for the rest of the series.

{CARD 5 s: `Next → Episode 2: Table, chart, reticle, summary`}

---

# Episode 2 — Table, chart, reticle, summary

{CARD 3 s: `Ballistic Calculator 2   ·   2 of 8   ·   Table, Chart, Reticle, Summary`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: the metric window from episode 1, on the Table view. Hold 3 s.}

VOICE:
In the first episode we built this. A three oh eight rifle, an eleven point three gram bullet, zeroed at
three hundred metres, and twelve metres of drop at one thousand.

Now the same load again, in imperial. I want you to see both, because the numbers in the rest of this
series are in yards and inches.

## Body, part one — the same shot in imperial  (~1 min 10 s)

{ACTION: `Trajectory → New → Imperial`. A second dialog opens.}

{FILL — silence, fast. Ammunition: `175` gr · `0.253` `G7` · `2600` ft/s. Rifle: sight `2` in,
clicks `0.1` mrad. Zero: `300` yd. Parameters: `1000` yd, step `100` yd. Press **OK** and accept the
default tabs.}

{HOLD 3 s on the table.}

VOICE:
One hundred seventy-five grains. Two thousand six hundred feet per second. A three hundred yard zero.

At one thousand yards this bullet falls three hundred sixty-three inches. That is about thirty feet.
The hold is ten point zero eight milliradians.

{ACTION: point at the Mach column, last row.}

VOICE:
Look at the Mach column. One point zero one. This bullet is about to go subsonic at one thousand
yards. That is the distance where groups usually open up.

## Body, part two — the other three views  (~1 min 50 s)

{ACTION: `View → Show → Summary`.}

{HOLD 3 s.}

VOICE:
The Summary view says it exactly. It gives the point blank range, the near and far zero, and the
distance where the bullet drops below the speed of sound. For this load, one thousand and seventeen
yards.

{ACTION: `Ctrl+R` or `View → Show → Reticle`. Click the **Mil-Dot** button. Select **Far BDC** in the
Display group.}

{HOLD 4 s on the sight picture.}

VOICE:
And this is the third view. Your scope.

I clicked Mil-Dot, which is built in. Then I chose Far B D C. The blue labels tell you what each mark
in your reticle is worth, for this load.

{ACTION: move the mouse slowly down the vertical stalk, dot by dot.}

VOICE:
The first dot below the centre is about four hundred and five yards. The second dot is four hundred
ninety-eight. The third is five hundred eighty. The fourth is six hundred fifty-six. The fifth is
seven hundred twenty-five.

That is your reticle turned into a range card. Remember those numbers — in episode seven we come back
to this exact picture, and every one of them will have moved.

{ACTION: point at the bottom post.}

VOICE:
And it stops. This reticle runs out at about seven hundred eighty-seven yards. Past that you must use
the turret. It is better to know that at home than on the hill.

The application ships with thirty-four reticles, and you can draw your own.

## Body, part three — save it  (~35 s)

{ACTION: `File → Save As…`, type `bc2-demo`, save. Hold 2 s on the window title now showing the file
name.}

VOICE:
One more thing before we finish, and it is the reason the rest of this series works.

File, Save As. This writes the whole shot to a file: the ammunition, the rifle, the zero, the weather,
the wind, every parameter on all six tabs.

Every episode from here on opens this same file and adds exactly one thing to it. So every number you
see will be a comparison against the table we just built, and nothing else.

## Next up

VOICE:
Next, the two things that change every shot you take: the weather, and the wind.

{CARD 5 s: `Next → Episode 3: Weather and wind`}

---

# Episode 3 — Weather and wind

{CARD 3 s: `Ballistic Calculator 2   ·   3 of 8   ·   Weather and Wind`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: `File → Open` → `bc2-demo.trajectory`. Hold 3 s on the restored table.}

VOICE:
File, Open. This is the baseline we saved in episode two. A one hundred seventy-five grain Scenar-L,
two thousand six hundred feet per second, zeroed at three hundred yards. Three hundred sixty-three
inches of drop at one thousand.

And so far, this bullet has been flying in perfectly standard air. Nobody shoots in standard air.

## Body, part one — weather  (~1 min 45 s)

{ACTION: `Ctrl+E` → **Weather** tab.}

VOICE:
The standard atmosphere means sea level and fifty-nine degrees Fahrenheit. Your air is almost never
that.

This is a February range day. One thousand feet above the sea, and forty degrees.

{FILL — silence. Weather tab: Altitude `1000` ft · Pressure `28.9` inHg (station) · Temperature
`40` °F · Humidity `60` %. Press **OK**.}

{HOLD 3 s on the table.}

VOICE:
Two things about this tab cause more wrong answers than anything else in the program.

First, the pressure must be the pressure where you are standing. Not the sea level pressure from the
weather report. There is a switch for that, so read it carefully.

Second, altitude and pressure are not two ways of saying the same thing. Give it both.

{ACTION: point at the 1000 yd row.}

VOICE:
Now here is the honest answer. The drop at one thousand yards went from three hundred sixty-three
inches to three hundred sixty-one and a half. One and a half inches.

That is not a disappointment, it is the physics. This air is colder than standard, which makes it
denser, and it is a thousand feet higher, which makes it thinner. The two nearly cancel.

Weather matters enormously. But it matters when the temperature and the altitude pull the *same* way —
and you will see exactly that in episode five.

## Body, part two — wind  (~1 min 40 s)

{ACTION: `Ctrl+E` → **Wind** tab.}

VOICE:
Now wind. Ten miles per hour, straight across, from the right.

{ACTION: on the wind dial, set direction to 90 degrees. Pause on the dial so the viewer sees it turn.}

VOICE:
Watch the dial. Zero degrees is a wind that blows towards the target, from behind you. Ninety degrees
blows from the right, across your path. Get this convention right before you trust any wind number.

{FILL — silence. Wind: Velocity `10` mph · Direction `90` deg · leave the distance empty so it applies
to the whole flight. Press **OK**.}

{HOLD 3 s on the Windage column.}

VOICE:
The Windage column was full of zeros. Now look at it.

At six hundred yards the wind pushes the bullet thirty-one inches sideways. At one thousand yards, one
hundred and four inches. Almost nine feet.

Wind is the hardest problem in long range shooting, and this is why. The drop you can look up. The
wind you have to read.

You can also add several wind zones along the flight path, each starting at its own distance.

{ACTION: close the window **without saving**. If prompted, decline.}

VOICE:
And now I close this without saving. The baseline file stays exactly as it was, ready for the next
episode.

## Next up

VOICE:
Next, the small effects. A spinning bullet slowly walks sideways. A pure sideways wind moves the impact
up. And the Earth turns underneath the whole flight.

{CARD 5 s: `Next → Episode 4: Spin drift, aerodynamic jump, Coriolis`}

---

# Episode 4 — Spin drift, aerodynamic jump, Coriolis

{CARD 3 s: `Ballistic Calculator 2   ·   4 of 8   ·   The Effects That Only Matter Far Away`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: `File → Open` → `bc2-demo.trajectory`. Hold 3 s.}

VOICE:
The same baseline again, straight from the file. Standard air, no wind, three hundred sixty-three inches
of drop at one thousand yards.

This episode adds the three effects most free calculators leave out entirely.

## Body, part one — spin drift, and why wind has to go back on  (~2 min 15 s)

{ACTION: `Ctrl+E` → **Ammunition** tab.}

VOICE:
A bullet is a spinning gyroscope. Over a long flight it slowly walks sideways, in the direction of the
spin. That is called spin drift.

To calculate it, the program needs the shape of the bullet and the twist of your barrel.

{FILL — silence. Ammunition tab: Bullet diameter `0.308` in · Bullet length `1.394` in. Then the
**Rifle** tab: Rifling twist `10` in per turn, direction **Right**. Press **OK**.}

{HOLD 3 s on the Windage column.}

VOICE:
Three tenths of an inch across. One point three nine four inches long. A one in ten right hand twist.

There was no wind in this file, so the Windage column was all zeros. Now, with no wind at all, the
bullet lands eight inches to the right at one thousand yards. That is pure spin drift.

{ACTION: point at the Drop column.}

VOICE:
And look at the Drop column. It has not changed. Not by a tenth of an inch.

That is worth knowing, because there is a second effect hiding here, and it needs wind before it can
exist at all.

{ACTION: `Ctrl+E` → **Wind** tab.}

{FILL — silence. Wind: Velocity `10` mph · Direction `90` deg, no distance limit. Press **OK**.}

{HOLD 4 s.}

VOICE:
So I put the same ten mile per hour crosswind back on.

The wind alone would push this bullet one hundred and four inches left. Now it goes ninety-seven,
because the spin drift is pushing right and partly cancelling it.

And now watch the drop. Three hundred fifty-nine inches, instead of three hundred sixty-three. The
bullet climbed four inches — from a wind blowing purely sideways.

That is aerodynamic jump. The crosswind tips the nose at the muzzle, and a spinning bullet responds by
moving vertically. It needs the twist, the diameter, the length *and* a crosswind. Take any one of them
away and the program shows you nothing, because there is nothing to show.

## Body, part two — the Coriolis effect  (~1 min 30 s)

{ACTION: close without saving. `File → Open` → `bc2-demo.trajectory` again. Then `Ctrl+E` →
**Parameters** tab.}

VOICE:
Back to the clean baseline one more time, because the last effect has nothing to do with the wind.

The Earth turns under the bullet while it flies. That is the Coriolis effect. It needs two things the
program cannot know: which way your barrel points, and how far north or south you are.

{FILL — silence. Parameters tab, Coriolis group: Barrel azimuth `90` deg (East) · Latitude `45` deg.
Press **OK**.}

{HOLD 3 s.}

VOICE:
Ninety degrees, so we are shooting east. Latitude forty-five north.

At one thousand yards the bullet moves three inches to the right, and just under four inches up.

Shooting east lifts the impact. Shooting west lowers it by the same amount. North and south change
nothing vertically at all.

Is four inches worth the trouble? At one thousand yards, on a small target, yes. At three hundred
yards, no. Leave the two fields empty and the effect is simply not there.

## Next up

VOICE:
Next, two situations straight off the range. What your table looks like after you have already dialled
the turret. And what happens when you zeroed the rifle in winter and you are shooting in July.

{CARD 5 s: `Next → Episode 5: Dialled clicks, and a winter zero in July`}

---

# Episode 5 — Dialled clicks, and a winter zero in July

{CARD 3 s: `Ballistic Calculator 2   ·   5 of 8   ·   Dialled Clicks, and a Winter Zero in July`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: `File → Open` → `bc2-demo.trajectory`. Hold 3 s, then point at the 800 yd row.}

VOICE:
The baseline once more. And this time look at the eight hundred yard row: the hold is six point two
four milliradians.

## Body, part one — clicks already on the turret  (~1 min 30 s)

{ACTION: `Ctrl+E` → **Parameters** tab.}

VOICE:
Our scope moves zero point one milliradian per click. Six point two four milliradians is sixty-two
clicks up.

Suppose you have already dialled those sixty-two clicks. Now your whole table is wrong, because it
still assumes the turret is at zero.

{FILL — silence. Parameters tab: Shot drop adjustment `6.2` mrad — or enter 62 clicks, whichever the
field expects. Press **OK**.}

{HOLD 4 s on the table.}

VOICE:
Tell the program what is on the turret, and the table changes to match.

Now the eight hundred yard row reads zero. That is where the rifle is pointing.

Everything closer is high. At four hundred yards the bullet is seventy-five inches above your
crosshair — more than six feet. At three hundred yards, sixty-seven inches. Everything further is low.

This is the table you actually want after you dial. And it is a good reminder of how much of the sky is
above your crosshair when the turret is wound up.

## Body, part two — zeroed in winter, shooting in summer  (~2 min)

{ACTION: close without saving. `File → Open` → `bc2-demo.trajectory`. Then `Ctrl+E` → **Weather** tab.}

VOICE:
Clean baseline again. And now the number I promised you in episode three.

You zeroed this rifle in February, at forty degrees, a thousand feet up. Today is July, same range, and
it is one hundred degrees. The rifle has not moved. The zero has not moved. But the air has.

{FILL — silence. Weather tab: Altitude `1000` ft · Pressure `28.9` inHg · Temperature `100` °F ·
Humidity `30` %. Press **OK**.}

{HOLD 3 s.}

VOICE:
First, today's air. One hundred degrees at a thousand feet.

The drop at one thousand yards is three hundred thirty-four inches. In February it was three hundred
sixty-one and a half. Twenty-eight inches less.

That is what weather is worth when the temperature and the altitude pull the same way. Hot air is thin
air, and the bullet flies further before it falls.

{ACTION: `Ctrl+E` → **Zero** tab. Open the zero atmosphere group.}

VOICE:
But there is a second half to this, and almost every calculator gets it wrong.

The Zero tab has its own weather. Not today's air — the air the rifle was *zeroed* in. February.

{FILL — silence. Zero tab, zero atmosphere: Altitude `1000` ft · Pressure `28.9` inHg · Temperature
`40` °F · Humidity `60` %. Press **OK**.}

{HOLD 4 s.}

VOICE:
Now the sight angle is computed in February air, and the flight is computed in July air, which is what
actually happened.

It is worth one and a quarter inches at one thousand yards. I am telling you the small number honestly,
because most of that twenty-eight inches came from today's air, not from the air you zeroed in.

{ACTION: point at the 300 yd row.}

VOICE:
But look at the three hundred yard row. It is no longer zero. It reads four tenths of an inch high.

The rifle is not quite zeroed at its own zero distance any more, because the day changed. That is a
real thing that happens to real rifles, and this is the only free calculator I know that will show it
to you.

And the further out you zero, the more it is worth: at a six hundred yard zero, the same swap is worth
six and a half inches.

## Next up

VOICE:
Next, the feature this program was really built for. We throw the ballistic coefficient away and use
the bullet's own measured drag curve instead.

{CARD 5 s: `Next → Episode 6: A measured drag curve instead of a BC`}

---

# Episode 6 — A measured drag curve instead of a BC

{CARD 3 s: `Ballistic Calculator 2   ·   6 of 8   ·   A Measured Drag Curve`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: `File → Open` → `bc2-demo.trajectory`. Hold 3 s on the Ammunition tab via `Ctrl+E`, pointing at
the ballistic coefficient field, then OK back to the table.}

VOICE:
The clean baseline for the last time. Standard air, no wind, three hundred sixty-three inches at one
thousand yards.

Every number in this whole series has come out of one single field on the Ammunition tab: zero point
two five three, G seven. In this episode we get rid of it.

## Body — the measured curve  (~2 min 20 s)

{ACTION: `Ctrl+E` → **Ammunition** tab.}

VOICE:
A ballistic coefficient is not a property of your bullet. It is a number that says how much your bullet
resembles a standard shape from nineteen forty. G one and G seven are two of those shapes.

Your bullet is not that shape. So the coefficient is only approximately right, and the error changes as
the bullet slows down.

The alternative is your bullet's own measured drag curve. Lapua publish radar measurements for their
bullets, and this program ships them.

{FILL — silence. Ammunition tab: change the drag table selector from `G7` to `GC` (custom), then load
`data/drg/Lapua/30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg`. Press **OK**.}

{HOLD 4 s.}

VOICE:
This is the same bullet. The same weight, the same speed, the same rifle, the same air. The only thing
that changed is how the drag is described.

{ACTION: point at the 1000 yd drop cell, then the 800 yd velocity cell.}

VOICE:
Three hundred seventy-one inches instead of three hundred sixty-three. Nearly nine inches more drop at
one thousand yards, and at eight hundred yards the bullet is arriving twenty feet per second slower.

Nine inches does not sound like much. On a small target at one thousand yards, it is the difference
between a hit and a miss.

And notice the direction of the error. The G seven coefficient was optimistic. If you had used the G one
number from the box instead — zero point five oh five — you would be fourteen inches optimistic.

The program supports all the standard curves — G one, G two, G five, G six, G seven, G eight, G I, G S
and R A four. It also builds a curve for you from radar data, or from a set of coefficients at
different speeds. Those tools are under the Tools menu.

## Body, part two — save the radar version  (~25 s)

{ACTION: `File → Save As…`, type `bc2-demo-radar`, save.}

VOICE:
This one I am going to keep. Save As, a new name, next to the original.

Now there are two files: the baseline with a coefficient, and this one with the measured curve. The
last episode of the calculator half of this series opens this one.

## Next up

VOICE:
Because so far you have seen every effect on its own. Next, we turn all of them on at once — and look
through the scope.

{CARD 5 s: `Next → Episode 7: Everything at once`}

---

# Episode 7 — Everything at once

{CARD 3 s: `Ballistic Calculator 2   ·   7 of 8   ·   Everything At Once`}

{ACTION: the shared cold open clip.}

## Recap  (~15 s)

{ACTION: `File → Open` → `bc2-demo-radar.trajectory`. Hold 3 s.}

VOICE:
Five episodes, five features, each one on its own against the same baseline. That was to show you what
each one does. It is not how a range day works — out there they all arrive at once.

So this is the radar-curve file from episode six, and now I am going to put everything else on top of
it.

## Body, part one — build the real shot  (~1 min 20 s)

{FILL — silence, sped up in editing. `Ctrl+E`, then, tab by tab:
· **Weather**: Altitude `1000` ft · Pressure `28.9` inHg · Temperature `100` °F · Humidity `30` %
· **Wind**: `10` mph · `90` deg, no distance limit
· **Ammunition**: Bullet diameter `0.308` in · Bullet length `1.394` in
· **Rifle**: Rifling `10` in, **Right**
· **Parameters**: Barrel azimuth `90` deg · Latitude `45` deg
· **Zero**: zero atmosphere `1000` ft · `28.9` inHg · `40` °F · `60` %
Leave the shot drop adjustment empty — no clicks dialled. Press **OK**.}

{HOLD 5 s on the table.}

VOICE:
July air on a February zero. A ten mile per hour crosswind. The bullet's real shape and a one in ten
twist. Shooting east at forty-five degrees north. And the measured drag curve.

That is every feature in this series except the dialled clicks, in one shot.

{ACTION: point at the 1000 yd drop cell.}

VOICE:
Three hundred thirty-four inches of drop at one thousand yards.

And here is the thing I want you to notice. Back in episode five, July air on its own gave us three
hundred thirty-four inches too.

That is not the same answer arrived at twice. The radar curve added nine inches of drop. Aerodynamic
jump and the Coriolis effect took about nine inches back off. They very nearly cancelled — this time,
at this distance, in this wind, shooting in this direction.

Turn the wind around, or shoot west instead of east, and they add up instead. This is exactly why you
put the real numbers in rather than trusting a rule of thumb.

{ACTION: point at the 300 yd row, then the windage column.}

VOICE:
Two more things. At three hundred yards the bullet is now nearly two inches high — the rifle is not
zeroed where it thinks it is. And the wind is worth eighty-three inches instead of one hundred and
four, because the spin drift is fighting it.

## Body, part two — the same reticle, re-labelled  (~1 min 40 s)

{ACTION: `Ctrl+R` → Reticle view → **Mil-Dot** → **Far BDC**.}

{HOLD 5 s on the sight picture.}

VOICE:
And now the part that makes all of this concrete. The same Mil-Dot reticle from episode two. The same
scope. The same glass.

{ACTION: move the mouse slowly down the vertical stalk, dot by dot.}

VOICE:
In episode two, the first dot below the centre was four hundred and five yards. Now it is about four
hundred and twenty-five.

The second dot was four hundred ninety-eight. Now about five hundred seventeen. The third was five
hundred eighty, now about six hundred. And the bottom of the reticle, which ran out at seven hundred
eighty-seven yards, now reaches past eight hundred and fifteen.

{HOLD 3 s.}

VOICE:
Every mark in your scope is worth twenty to thirty yards more than it was, and not one thing about the
scope changed. Only the day, the air, the wind and an honest drag curve.

That is the argument for a ballistic calculator, in one picture. A printed range card is a photograph of
one afternoon. This is the afternoon you are actually standing in.

## Next up

VOICE:
One episode left, and it is the thing I most wanted to show you: a rifle zeroed with one cartridge,
shooting a completely different one.

{CARD 5 s: `Next → Episode 8: 300 Blackout — zero one load, shoot another`}

---

# Episode 8 — 300 Blackout: zero one load, shoot another

{CARD 3 s: `Ballistic Calculator 2   ·   8 of 8   ·   Zero One Load, Shoot Another`}

{ACTION: the shared cold open clip.}

## Recap  (~12 s)

{ACTION: the episode 7 window still open on the reticle view. Hold 3 s.}

VOICE:
Everything so far has been one rifle and one cartridge, refined seven times. This last episode is a
different rifle, a different problem, and a real scope.

And this one I build from nothing, in clean standard air — because the point of it is a comparison
against a manufacturer's own published data.

## Body, part one — zeroing with a different cartridge  (~2 min 20 s)

{ACTION: `Trajectory → New → Metric`. A fresh window — new rifle, new scope.}

VOICE:
Three hundred Blackout is really two cartridges in one chamber. A fast supersonic load, and a heavy,
slow, subsonic one. Most people zero with the supersonic load, and then shoot the subsonic one out of
the same rifle without touching the scope.

So the shot and the zero use different ammunition. Almost no free calculator can describe that.

{FILL — silence. Ammunition tab — the SUBSONIC load: Weight `208` gr · BC `0.648` `G1` · Muzzle
velocity `1010` ft/s · Diameter `0.308` in · Length `1.44` in. Rifle tab: Sight height `2.9` in ·
clicks `0.1` mrad · Rifling `8` in, **Right**. Parameters: `200` m, step `25` m.}

VOICE:
The shot is the heavy subsonic load. Two hundred and eight grains, at one thousand and ten feet per
second. Slower than the speed of sound.

{ACTION: click the **Zero** tab. Set Distance `100` m. Then open the zero ammunition group.}

{FILL — silence. Zero tab, zero ammunition: Weight `115` gr · BC `0.290` `G1` · Muzzle velocity
`2330` ft/s. Press **OK**.}

VOICE:
And here is the Zero tab again. One hundred metres. But the ammunition used for that zero is the
*other* load. One hundred fifteen grains, at two thousand three hundred thirty feet per second.

{HOLD 4 s on the table.}

VOICE:
Now look what that costs you.

At one hundred metres — the distance the rifle is zeroed at — the subsonic bullet is forty-two
centimetres low. Almost half a metre, at one hundred metres.

At two hundred metres it is one metre eighty-eight below the line of sight.

That is the price of a shared zero. It is not a mistake. It is simply what happens, and now you can
see it before you shoot.

## Body, part two — the sight picture  (~1 min 50 s)

{ACTION: `Ctrl+R` → Reticle view → **Load…** → `VCOG16-300BLK.reticle`.}

{HOLD 4 s on the reticle, before any overlay.}

VOICE:
And this is why that matters.

This is the Trijicon V-COG, one to six by twenty-four, in three hundred Blackout. A real reticle, drawn
from the manufacturer's own sheet.

Look at it. There is a ladder of horizontal bars going down. Those are the supersonic holds — two
hundred, three hundred, four hundred, five hundred, six hundred metres.

And there are three small diamonds. Those belong to the subsonic load. Trijicon etched them into the
glass for exactly the situation we just calculated.

{ACTION: select **Far BDC**. Set **Split at** to something small, e.g. `25` m, so the subsonic holds
are labelled.}

{HOLD 5 s.}

VOICE:
Now I turn on the labels, and the program marks where our subsonic bullet actually goes.

Fifty metres. One hundred metres. One hundred fifty metres.

{ACTION: slowly move the mouse from each computed label to the diamond it sits on.}

VOICE:
Every one of them lands on a diamond.

We did not copy those positions. We described a rifle, two cartridges and a zero, and the program
computed where the bullet goes. Trijicon cut their glass from the same physics.

That is a good way to check that a ballistic calculator is telling you the truth.

## Outro  (~45 s)

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
3. **Episode 4 opens the baseline twice** — once for spin drift and jump, once for Coriolis. Episode 5
   likewise, once for clicks and once for the weather swap. That is deliberate: it is what keeps each
   effect measured against the baseline and nothing else.
4. **Episode 7's fill is long.** Six tab groups in one `{FILL}` — speed it up hard in the edit. If it
   still drags, save `bc2-demo-everything.trajectory` off camera beforehand and simply `File → Open` it;
   the numbers are identical either way.
5. **The reticle is never restored by a file.** Mil-Dot in episodes 2 and 7, and the V-COG in episode 8,
   are always re-selected on camera.

## YouTube metadata

**Playlist title:** Ballistic Calculator 2 — build one shot, step by step

**Playlist description:** A free, open-source ballistic calculator for Windows, Linux and macOS. Eight
short episodes. We build one long-range shot, save it, and then add one thing at a time — weather, wind,
spin drift, aerodynamic jump, Coriolis, dialled clicks, a winter zero in summer air, and a radar-measured
drag curve — so you can see what each one is actually worth. Then we turn everything on at once, and
finish by zeroing a rifle with one cartridge and shooting another.

Per-episode titles, in playlist order:

```
1. Your first trajectory — Ballistic Calculator 2 (1/8)
2. Table, chart, reticle, summary — Ballistic Calculator 2 (2/8)
3. Weather and wind — Ballistic Calculator 2 (3/8)
4. Spin drift, aerodynamic jump and Coriolis — Ballistic Calculator 2 (4/8)
5. Dialled clicks, and a winter zero in July — Ballistic Calculator 2 (5/8)
6. A measured drag curve instead of a BC — Ballistic Calculator 2 (6/8)
7. Everything at once — Ballistic Calculator 2 (7/8)
8. 300 Blackout: zero one load, shoot another — Ballistic Calculator 2 (8/8)
```

In-episode chapter markers, for the episodes with more than one part. Re-time after the edit.

```
Episode 2          Episode 3        Episode 4         Episode 5
0:00 Intro         0:00 Intro       0:00 Intro        0:00 Intro
0:23 Imperial      0:23 Weather     0:23 Spin drift   0:23 Dialled clicks
1:33 Three views   2:08 Wind        2:38 Coriolis     1:53 Winter zero
3:23 Save it

Episode 6          Episode 7        Episode 8
0:00 Intro         0:00 Intro       0:00 Intro
0:23 The curve     0:26 Everything  0:23 Two cartridges
2:43 Save it       1:46 The reticle 2:43 The sight picture
                                    4:33 More
```

Each description ends with the same block: downloads, source repository, user guide, and a link to the
playlist. Episodes 2 to 8 additionally open by naming the previous episode, so a viewer who lands from
search knows there is an earlier one.
