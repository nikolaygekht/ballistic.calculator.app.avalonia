# Episode 10 — Your own drag table

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
