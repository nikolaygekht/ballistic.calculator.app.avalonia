# Plan: Hit Probability dialog (UX first)

> **Status 2026-07-27 — built, 841 tests green, not yet smoke-tested by hand.** `HitProbabilityCalculator`
> (+ `HitProbabilityInputs` / `HitProbabilityEstimate` / `ShootingPosition`) in `BallisticCalculator.Types`,
> `HitProbabilityPanel` in `BallisticCalculator.Panels`, `HitProbabilityDialog` shell, and **Tools → Hit
> Probability…** enabled only with an active trajectory window. 46 new tests.
>
> Two implementation notes worth keeping:
> - **ScottPlot works under Avalonia.Headless**, including `Axes.SquareUnits()` — verified with a throwaway
>   probe before designing the panel around it. So the plot lives in the panel and is covered by panel tests,
>   rather than being pushed into the untested dialog shell.
> - **`TextBox.TextChanged` does not fire for a programmatic text set in headless Avalonia** (the same trap the
>   `.drg` work hit with `MeasurementControl.Changed`). The position ⇄ spread-multiplier sync therefore happens
>   inside `Recalculate()` — the point where the values are consumed — instead of in a text-changed handler.
>   That also removed the combo/field feedback loop by construction: the preset writes the fields only when they
>   differ and never recomputes, and the sync selects a preset without writing back.
>
> Departure from the sketch below: the panel has **no `ScrollViewer`** in its shell — the plot must take the
> space it is given, and the input column is fixed-height.
>
> **Review round 1 (2026-07-27), five changes:**
> 1. `Tools → Hit Probability` needed `IsEnabled="False"` in the markup — `UpdateMenus()` only runs once the
>    active child changes, so at startup the item kept the XAML default and clicking it did nothing.
> 2. **The estimate no longer runs automatically.** An **Estimate** button (also the dialog's default button)
>    runs it; nothing is computed on open or on edit. The cost argument for live recompute answered the wrong
>    question: a probability derived from untouched defaults lends those defaults an authority they have not
>    earned. Once shown, a result **stays untouched** when inputs change, so two set-ups can be compared.
>️ 3. **Every plain number is a `NumericUpDown`** — spreads (0.1–20 step 0.1), range and wind percentages
>    (0–100 step 1), MV deviation (0–10 step 0.1), Shots (1000–50 000 step 1000) and Seed (step 1, and empty
>    means re-roll, which the status line then says). Note that Avalonia's `NumericUpDown` **does not clip to
>    Minimum/Maximum** — `ClipValueToMinMax` defaults to false — and it is left off deliberately: it matches the
>    project's "Min/Max is for increment/decrement, never for rejecting input" rule, so an out-of-range shot
>    count is reported ("must be between 1000 and 50000") rather than silently rewritten.
> 4. **Muzzle velocity is not an estimation error**, it is the ammunition's velocity deviation. It moved out of
>    the "Estimation error" group (which now holds range and wind only) into its own **Ammunition** group, and
>    `MuzzleVelocityErrorPercent` was renamed to `MuzzleVelocityDeviationPercent` — which is what the library
>    called it all along.
> 5. **Default distance is a fixed 300 yd / 300 m**, not the shot's maximum distance: a table run out to 1000 yd
>    is not a statement that anyone means to shoot that far. Written outright per system rather than converted,
>    so a metric user sees 300 m and not 274.32 m.

Design for **2026-07-27**. Implements Feature 3 of [`07-25-plan.md`](07-25-plan.md). This file settles the
**UX**; the wrapper and test sketch at the end are deliberately thin until the layout is agreed.

## What the library actually does (read before designing)

`Tools.HitProbability.Estimate` is **not** a per-shot simulation of trajectories. It runs **exactly three**
trajectory calculations — with wind, without wind, and with muzzle velocity +5% — and derives the drop and
windage sensitivities from them. The Monte-Carlo loop is then pure arithmetic: five Gaussian draws and four
linear interpolations per shot.

Measured (.308 168gr G7 0.223 at 600 yd, 1 MOA group, prone, range 2% / wind 30% / MV 0.7%, seed 1):

| Shots | Time | Hit probability |
|---|---|---|
| 1 000 | 6 ms | 21.00% |
| 10 000 | 28 ms | 20.94% |
| 100 000 | 143 ms | 20.70% |
| 1 000 000 | 447 ms | 20.74% |

The ~5 ms floor is the three trajectory runs. **Consequences for the UX:**

- **No progress bar, and none needed.** *(This was originally "no Calculate button" — reversed in review: the
  estimate is cheap enough to run live, but the inputs are guesses, so it waits for the button. See banner.)*
- **`Shots` is bounded 1 000…50 000** (default 10 000). At 1 000 000 the UI thread stalls ~450 ms on every
  keystroke; 50 000 costs ~70 ms, which stays smooth under live typing. Below 1 000 the Monte-Carlo noise
  (±1.3% at 1 000) swamps the answer. Outside the range the panel refuses with a sentence.
- Monte-Carlo noise is real (±1.3% at 1 000 shots, ±0.4% at 10 000), which is why the default seed is fixed.

Note also that the model's own `dropDial`/`windDial` terms mean the *come-up is modelled internally* — the
dialog does not need to dial the scope to the target distance. It only needs the rifle's zero applied.

## Confirmed decisions (2026-07-27)

| Question | Decision |
|---|---|
| Window shape | **Modal dialog off Tools**, panel + thin `Window` shell, per the pattern in [`Archive/07-26-drg-plan.md`](Archive/07-26-drg-plan.md) §1d. Needs an active `ITrajectoryChildWindow`. |
| `Shots` / `Seed` | **Exposed as ordinary fields** (defaults 10 000 / 1), against the recommendation to hide them. `Shots` is bounded **1 000…50 000**. An empty seed is allowed and means "reroll on every recompute". |
| Position multipliers | The combo fills them, but **H and V are always-visible, always-editable `NumericUpDown` fields** beneath it — not read-only text behind a Custom unlock. Editing either one switches the combo to Custom. |
| When it runs | **On the Estimate button only** — never on open, never on edit. A shown result persists unchanged until the next press. |
| Probability-vs-distance curve | **Not in the first cut.** Single distance + scatter plot only. |
| Error budget entry | **Plain percent fields** with the defaults proposed below — no scenario presets. |

## Layout

```
┌─ Hit Probability — 308win 168gr ───────────────────────────────────────┐
│ Target                            │  Hit probability                   │
│  Distance    [ 300       yd ]     │        62.4 %                      │
│  Vital zone  [ 20        in ]     │    single shot                     │
│                                   │                                    │
│ Shooter                           │  Shots for a first hit             │
│  Group (1σ)  [ 1.00    MOA ]      │    50%  75%  90%  95%  98%         │
│  Position    [ Supported  ▾ ]     │     1    2    3    3    4          │
│  Spread H ×  [ 1      ▲▼ ]        │                                    │
│  Spread V ×  [ 1      ▲▼ ]        │                                    │
│                                   │  ┌──────────────────────────────┐  │
│ Estimation error (1σ, %)          │  │         ·   ·                │  │
│  Range       [ 2      ▲▼ ]        │  │      ·  ((⬤))  ·             │  │
│  Wind        [ 30     ▲▼ ]        │  │         ·    ·               │  │
│                                   │  │                              │  │
│ Ammunition                        │  └──────────────────────────────┘  │
│  MV deviation (1σ, %) [ 0.7 ▲▼ ]  │  mean miss 3.4 in · 90% within 6 in│
│                                   │                                    │
│ Simulation                        │                                    │
│  Shots       [ 10000  ▲▼ ]        │                                    │
│  Seed        [ 1      ▲▼ ]        │                                    │
│                                   │                                    │
│ ⚠ Group size is a 1σ per-axis figure — about a quarter of the extreme   │
│   spread of a large group. Assumes your come-up and wind hold are       │
│   correct for the range and wind you estimated.                         │
│                                          [ Estimate ]  [ Close ]       │
└────────────────────────────────────────────────────────────────────────┘
```

- Inputs left, results right; roughly **820×560**, `CanResize`, scrolling shell like the other Tools dialogs.
- The title **names the shot** (file name or ammunition + target distance). Unlike every other Tools window,
  this one is *about* a specific shot, and a user with three trajectory windows open must not have to guess.
- Section headers `FontWeight="SemiBold"`; label column `Auto` in one grid per column so labels cannot clip
  (the lesson from `BcConverterPanel`).
- One status/error line above the buttons, doubling as the refusal surface (bad shot count, missing input),
  exactly as the other panels do.

## Inputs

### From the active trajectory window (not editable here)
Ammunition, rifle + zero, atmosphere, wind, and the custom drag table when the BC is GC. Assembled through
`ZeroingCalculator.BuildInputs` so the zero matches the rest of the app. The `ShotParameters` handed to the
library is the active shot's, with `MaximumDistance` = the target distance, `Apply(zero)` applied.

### Entered here

| Field | Control | Default | Note |
|---|---|---|---|
| Distance | `MeasurementControl<DistanceUnit>` | **300 yd / 300 m** | the target range; deliberately not the shot's maximum distance |
| Vital zone | `MeasurementControl<DistanceUnit>` | `SummaryController.TargetSize` — 500 mm / 20 in | diameter of a **circular** zone, matching what the Summary panel already shows |
| Group (1σ) | `MeasurementControl<AngularUnit>` | 1 MOA | per-axis SD from a supported position |
| Position | `ComboBox` | Supported | fills the two multipliers |
| Spread H × | `NumericUpDown` | 1 | set by the preset, editable; 0.1…20 step 0.1 |
| Spread V × | `NumericUpDown` | 1 | as above |
| Range error | `NumericUpDown` (percent) | 2 | 1σ, percent of range; 0…100 step 1 |
| Wind error | `NumericUpDown` (percent) | 30 | 1σ, percent of wind speed; 0…100 step 1 |
| MV deviation | `NumericUpDown` (percent) | 0.7 | 1σ, percent of MV — **ammunition quality, not an estimation error**; 0…10 step 0.1 |
| Shots | `NumericUpDown` | 10 000 | 1 000…50 000 step 1000; out of range is reported, not clipped |
| Seed | `NumericUpDown` (optional) | 1 | empty = re-roll, and the status says so |

**Position presets** use the library's own documented values, so the numbers are its claim, not ours:

| Position | H | V |
|---|---|---|
| Supported | 1 | 1 |
| Prone | 2 | 2 |
| Kneeling | 4 | 3 |
| Standing | 5 | 4 |
| Custom | — | — |

**The two multipliers are ordinary editable fields under the combo.** Choosing a preset writes them; typing in
either one selects **Custom** and writes nothing back, so the preset is a shortcut rather than a mode and there
is no feedback loop between the combo and the fields.

**The proposed error defaults are a judgement call and easy to change** — say the word and I will use yours:

- **Muzzle velocity 0.7%** — ~19 fps SD on 2700 fps. Good handloads run nearer 0.4%, ordinary factory ammo
  0.8–1.0%.
- **Range 2%** — 12 yd at 600. A laser rangefinder is far better (~0.5%); an eyeball estimate is 10% or worse.
- **Wind 30%** — wind is usually the dominant error; 25–50% of the wind speed is the range commonly quoted.

**Group size 1 MOA** is likewise a choice: the library's docs call 4 MOA "an ordinary shooter and rifle" and
1 MOA "a precision setup". 1 MOA suits this app's users, but it is the flattering end.

## Outputs

1. **Single-shot hit probability**, large, as a percentage with one decimal. This is the answer; it gets the
   typographic weight.
2. **Shots for a first hit** at 50/75/90/95/98% — a compact five-column readout. A `null` (hit impossible,
   p = 0) shows as `—`, not a blank.
3. **Scatter plot** of the impacts with the vital zone drawn as a circle to scale, ScottPlot, in the panel's
   current linear unit.
   - **Equal axis scaling is mandatory** (`SquareUnits()`): stretched axes turn a round group into an ellipse
     and misrepresent the only thing the plot exists to show.
   - Plot at most **2 000** points (subsample when `Shots` is larger) and say so, so a 50 000-shot run does not
     try to render 50 000 markers.
   - Small translucent markers; the circle stroked, not filled.
4. **Mean radial miss** and the **90% radius** as a one-line summary under the plot — cheap to compute from
   the impacts and more informative than the plot alone at a glance.

## Honesty (stated in the dialog, not just here)

The same principle as the BC converter's transonic warning: the tool must not imply precision it lacks.

- **Group size is 1σ per axis, not extreme spread.** A user who types their 1 MOA extreme spread as 1 MOA is
  wrong by ~4× in the input that dominates the result. This warning sits next to the input.
- **A correct come-up and wind hold for the estimated range and wind is assumed.** Nominal conditions land
  dead centre by construction; this models estimation error and dispersion, *not* a bad zero, a wrong drag
  curve, or cant.
- **The vital zone is a circle.** Real targets are not.
- With an empty seed the answer moves slightly on every recompute — that is the Monte-Carlo noise, and seeing
  it is the point of allowing it.

## Implementation sketch (fill in after the layout is agreed)

- `Common/BallisticCalculator.Types/HitProbabilityCalculator.cs` — takes `ShotData`, the target distance and a
  `HitProbabilityParameters`; assembles the library call (via `ZeroingCalculator.BuildInputs`, threading the GC
  drag table like `ShotTrajectoryCalculator` does); validates with user-facing messages (positive target size,
  shots in 1 000…50 000, non-negative percentages, positive multipliers) and returns the library result plus the
  derived mean/90% radii. Pure and testable.
- `Common/BallisticCalculator.Panels/Panels/HitProbabilityPanel.axaml(.cs)` — the layout above; `Recalculate()`
  wired to every input's `Changed`, plus called directly by tests (programmatic `SetValue` raises no event
  headless).
- `Desktop/BallisticCalculator/Views/Dialogs/HitProbabilityDialog.axaml(.cs)` — thin shell, ctor takes
  `(MeasurementSystem, ShotData, string title)`.
- Menu: `Tools → Hit _Probability...`, enabled only with an active `ITrajectoryChildWindow` (`UpdateMenus`).
- **Tests:** calculator with a fixed seed — probability in [0,1]; bigger target ⇒ higher; tighter group ⇒
  higher; standing ⇒ lower than supported; identical inputs ⇒ identical result; each refusal message. Panel —
  defaults from the active shot, a position preset fills the multipliers, editing a multiplier selects Custom,
  subsampling caps the plotted points, refusals clear the result.

## Open items

- The error defaults and the 1 MOA group default (above) want your numbers.
- Whether the shots-to-hit row should also show the probability of at least one hit in *N* shots for a user's
  own *N* (a sixth field) — currently no.
- Whether to offer CEP rings (50%/90%) on the plot as well as the numbers — currently numbers only.
