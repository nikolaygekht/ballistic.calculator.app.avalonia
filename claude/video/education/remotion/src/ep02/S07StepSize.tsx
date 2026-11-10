// Scene 7 — how big a step. The two lines are named (faint: the real path, bright: the dot in 0.1 s steps). One step,
// drawn as a schematic (exaggerated): the dot flies straight with the same velocity, the real bullet bends away,
// the gap at the end is the error; the red lines on the plot show it for every step. Shorter steps close the gap.
// The program picks each step by distance (Δt = Δs / vx, 10–50 cm, < 1 ms at the muzzle). Euler's error at 1000 m:
// 0.1 s steps 2.8 m (datagen), ½ m 2 cm, 5 cm (the library's Euler mode) 2 mm; the app's RK2 at ½ m: < 0.1 mm
// (numbers/, NUMBERS.md).
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Formula, P, Tok, label, mono } from "../lib/draw";
import { EStep, EULER, Magnifier, REF, Scene, Stopwatch, refAtT, sub, t_, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 7;
const T = {
  paths: cue(N, "two paths"), faint: cue(N, "faint"), bright: cue(N, "bright"), huge: cue(N, "huge"), inside: cue(N, "Inside"),
  straight: cue(N, "straight"), real: cue(N, "real", 2), bends: cue(N, "bends"), off: cue(N, "off"), red: cue(N, "red lines"),
  fix: cue(N, "fix"), shorter: cue(N, "Shorter"), distance: cue(N, "distance"), ds: cue(N, "delta s"), cm: cue(N, "centimetres"),
  turn: cue(N, "To turn"), divides: cue(N, "divides"), muzzle: cue(N, "muzzle"), thousandth: cue(N, "thousandth"), thousands: cue(N, "thousands"),
  euler: cue(N, "Euler"), grows: cue(N, "grows"), almost: cue(N, "almost three"), onlyTwo: cue(N, "only two"), drop: cue(N, "drop"),
  already: cue(N, "already"), library: cue(N, "library uses"), five: cue(N, "five centimetres"), twoMm: cue(N, "two millimetres"),
  byDefault: cue(N, "by default"), smarter: cue(N, "smarter"), longer: cue(N, "ten times longer"), tenth: cue(N, "less than a tenth"),
  idea: cue(N, "idea"), story: cue(N, "story"),
};

// the plot: 1.5 px per metre downrange, 28 px per metre of fall
const px = (x: number) => 180 + x * 1.5, py = (y: number) => 280 - y * 28;
const pts = (s: EStep[]): P[] => s.map((e) => [px(e.x), py(e.y)] as P);
const REFP = pts(REF);
const RUNS: { s: EStep[]; name: string }[] = [
  { s: EULER.s100, name: "Δt = 0.1 s" }, { s: EULER.s050, name: "Δt = 0.05 s" }, { s: EULER.s020, name: "Δt = 0.02 s" }, { s: EULER.s005, name: "Δt = 0.005 s" },
];
// where the two lines are named, on the plot itself
const LBL_REAL = refAtT(EULER.s100[9].t), LBL_STEP = EULER.s100[12];

// one step, as a schematic (not to scale): from S the dot flies straight to E; the real bullet curves down to R
const SA: P = [520, 420], SE: P = [1300, 520];
const SX: P = [SE[0] + 0.35 * (SE[0] - SA[0]), SE[1] + 0.35 * (SE[1] - SA[1])];
const REAL_ARC: P[] = Array.from({ length: 41 }, (_, i) => {
  const u = i / 40;                                           // the same start and start direction, bending down, slowing
  const x = SA[0] + (SE[0] - SA[0]) * (u - 0.08 * u * u), y = SA[1] + (SE[1] - SA[1]) * u + 190 * u * u;
  return [x, y] as P;
});
const SR = REAL_ARC[REAL_ARC.length - 1];
const pathOf = (q: P[]) => "M" + q.map((p) => p.join(",")).join(" L");

const FD: Tok[] = [t_("Δt", "dt"), t_(" = "), t_("Δs", "ds"), t_(" / "), t_("v", "vx"), sub("x", "vx")];

export const S07StepSize: React.FC = () => {
  const v = useVoiceTime();
  // phase A/B: the plot with its two lines named, then one step as a schematic, then the steps shrink
  const schem = window01(v, T.inside - 0.3, T.red - 0.2, 0.5);
  const plotO = (1 - 0.85 * ramp(v, T.distance - 0.3, 0.6)) * (1 - 0.92 * schem) * (1 - ramp(v, T.euler - 0.5, 0.5));
  const run = v < T.shorter ? 0 : Math.min(3, Math.floor((v - T.shorter + 0.1) / 0.8));
  const S = RUNS[run].s;
  const drawP = ramp(v, -0.3, 2.6);
  const gapHot = window01(v, T.red - 0.3, T.fix + 0.4, 0.3);
  const realHot = window01(v, T.faint - 0.2, T.bright - 0.1, 0.3), dotHot = window01(v, T.bright - 0.2, T.huge, 0.3);
  const names = window01(v, T.faint - 0.3, T.distance - 0.3, 0.4);
  const segP = ramp(v, T.inside, 0.9, ease), extP = ramp(v, T.straight, 0.7, ease);
  const arcP = ramp(v, T.real - 0.1, 1.2, ease), gapP = ramp(v, T.off - 0.3, 0.5);
  // phase C: Δs, Δt
  const oC = window01(v, T.distance - 0.2, T.euler - 0.2, 0.4);
  const dsCm = lerp(10, 50, ramp(v, T.ds + 0.4, 1.6, ease));
  const RX = 460, CM = 20, RY = 560;
  const count = Math.round(2000 * ramp(v, T.thousands - 0.3, 1.8, ease));
  // phase D: Euler's error at 1000 m — a ladder of steps and errors; the magnifier for the centimetres
  const oD = window01(v, T.euler - 0.3, T.byDefault, 0.4);
  const err = lerp(1.95, 0.19, ramp(v, T.twoMm - 0.3, 1.0, ease));
  const lensP = ramp(v, T.onlyTwo - 0.3, 0.8);
  const LC: P = [1460, 640], PXCM = 64;
  const LADDER = [
    { t: T.almost, step: "0.1 s steps", off: "2.8 m off", note: "" },
    { t: T.onlyTwo, step: "½ m steps", off: "2 cm off", note: "" },
    { t: T.library, step: "5 cm steps", off: "2 mm off", note: "THE LIBRARY, IN EULER MODE" },
  ];
  // phase E: the app's method
  const oE = ramp(v, T.byDefault - 0.2, 0.6);
  return (
    <Scene n={N}>
      {/* ---------------- the plot */}
      <g opacity={plotO}>
        <line x1={px(0)} y1={py(0)} x2={px(1000)} y2={py(0)} stroke={C.faint} strokeWidth={1.5} strokeDasharray="6 8" />
        <path d={pathOf(REFP)} stroke={C.line} strokeOpacity={0.45 + 0.5 * realHot} strokeWidth={3 + 3 * realHot} fill="none"
          filter={realHot > 0.3 ? "url(#glow)" : undefined} />
        {/* the gaps: where each Euler step lands vs where the real path is at that moment */}
        {run === 0 && EULER.s100.slice(1).map((e, i) => {
          const r = refAtT(e.t);
          return <line key={i} x1={px(e.x)} y1={py(e.y)} x2={px(r.x)} y2={py(r.y)} stroke={C.drag} strokeWidth={2 + 3 * gapHot} opacity={0.35 + 0.65 * gapHot}
            filter={gapHot > 0.3 ? "url(#glow)" : undefined} />;
        })}
        <g filter={dotHot > 0.3 ? "url(#glow)" : undefined}>
          <path d={pathOf(pts(S))} stroke={C.line} strokeWidth={2.5 + 2 * dotHot} fill="none" strokeDasharray={drawP < 1 ? `${drawP * 3000} 4000` : undefined} />
          {S.length < 60 && pts(S).map((q, i) => (
            <circle key={i} cx={q[0]} cy={q[1]} r={4.5 + 2 * dotHot} fill={C.line} opacity={i / S.length <= drawP ? 1 : 0} />
          ))}
        </g>
        {/* the two lines, named */}
        <g opacity={names}>
          <text x={px(LBL_REAL.x) + 10} y={py(LBL_REAL.y) - 26} textAnchor="end" style={label(0.6 + 0.4 * realHot, { fontSize: 24 })}>THE REAL PATH</text>
          <text x={px(LBL_STEP.x) - 16} y={py(LBL_STEP.y) + 48} textAnchor="end" style={label(0.7 + 0.3 * dotHot, { fontSize: 24 })}>THE DOT, STEP BY STEP</text>
        </g>
        <text x={px(1000)} y={py(0) - 16} textAnchor="end" style={label(0.6, { fontSize: 20 })}>1000 m</text>
        <text x={960} y={120} textAnchor="middle" style={label(window01(v, T.huge - 0.3, T.inside, 0.3), { fontSize: 28 })}>0.1 s STEPS · HUGE, SO WE CAN SEE THE PROBLEM</text>
        <text x={960} y={120} textAnchor="middle" style={label(window01(v, T.red - 0.3, T.shorter, 0.3), { fontSize: 28, fill: C.drag })}>THE RED LINES: HOW FAR OFF, AFTER EACH STEP</text>
        <text x={960} y={120} textAnchor="middle" style={label(window01(v, T.shorter - 0.2, T.distance, 0.3), { fontSize: 30 })}>SHORTER STEPS · {RUNS[run].name}</text>
      </g>

      {/* ---------------- one step, as a schematic */}
      <g opacity={schem}>
        <text x={960} y={160} textAnchor="middle" style={label(0.9, { fontSize: 30 })}>ONE STEP, EXAGGERATED</text>
        {/* the dot: straight, with the same velocity, and the straight line runs on, dashed */}
        <line x1={SA[0]} y1={SA[1]} x2={lerp(SA[0], SE[0], segP)} y2={lerp(SA[1], SE[1], segP)} stroke={C.line} strokeWidth={5} filter="url(#glow)" />
        <line x1={SE[0]} y1={SE[1]} x2={lerp(SE[0], SX[0], extP)} y2={lerp(SE[1], SX[1], extP)} stroke={C.line} strokeWidth={2.5} strokeDasharray="12 10" opacity={extP > 0 ? 0.7 : 0} />
        <circle cx={SA[0]} cy={SA[1]} r={12} fill={C.line} />
        <circle cx={SE[0]} cy={SE[1]} r={12} fill={C.line} opacity={segP >= 1 ? 1 : 0} />
        <text x={SA[0] + 10} y={SA[1] - 44} style={mono(ramp(v, T.straight - 0.2, 0.5), 32, { fontWeight: 600 })}>
          the dot: straight, the same velocity
        </text>
        {/* the real bullet: from the same point, slowing and bending down */}
        <path d={pathOf(REAL_ARC.slice(0, Math.max(2, Math.round(REAL_ARC.length * arcP))))} stroke={C.line} strokeOpacity={0.55} strokeWidth={4} fill="none" opacity={arcP > 0 ? 1 : 0} />
        <circle cx={SR[0]} cy={SR[1]} r={10} fill="none" stroke={C.line} strokeWidth={3} opacity={arcP >= 1 ? 0.8 : 0} />
        <text x={lerp(SA[0], SR[0], 0.5) - 20} y={REAL_ARC[22][1] + 60} textAnchor="middle" style={mono(ramp(v, T.real, 0.5) * 0.85, 32)}>
          the real bullet: slows and bends
        </text>
        {/* the gap at the end of the step */}
        <line x1={SE[0]} y1={SE[1]} x2={SR[0]} y2={SR[1]} stroke={C.drag} strokeWidth={6} filter="url(#glow)" opacity={gapP} />
        <text x={(SE[0] + SR[0]) / 2 + 30} y={(SE[1] + SR[1]) / 2 + 12} style={mono(gapP, 38, { fill: C.drag, fontWeight: 700 })}>off</text>
      </g>

      {/* ---------------- Δt = Δs / vx */}
      <g opacity={oC}>
        <Formula toks={FD} cx={960} y={180} size={72} reveal={ramp(v, T.distance - 0.2, 1.0)}
          active={v >= T.divides - 0.1 && v < T.muzzle ? "vx" : v >= T.ds - 0.1 && v < T.turn ? "ds" : v >= T.turn - 0.1 ? "dt" : null} />
        {/* a ruler under one real step */}
        <g opacity={ramp(v, T.ds - 0.3, 0.5)}>
          <line x1={RX} y1={RY + 40} x2={RX + 50 * CM} y2={RY + 40} stroke={C.dim} strokeWidth={2} />
          {Array.from({ length: 51 }, (_, i) => (
            <line key={i} x1={RX + i * CM} y1={RY + 40} x2={RX + i * CM} y2={RY + 40 + (i % 10 === 0 ? 22 : i % 5 === 0 ? 14 : 7)} stroke={C.dim} strokeWidth={1.5} />
          ))}
          {[0, 10, 20, 30, 40, 50].map((c) => <text key={c} x={RX + c * CM} y={RY + 96} textAnchor="middle" style={mono(0.6, 22)}>{c}</text>)}
          <text x={RX + 50 * CM + 30} y={RY + 96} style={mono(0.6, 22)}>cm</text>
          <line x1={RX} y1={RY} x2={RX + dsCm * CM} y2={RY} stroke={C.line} strokeWidth={4} />
          <circle cx={RX} cy={RY} r={9} fill={C.line} />
          <circle cx={RX + dsCm * CM} cy={RY} r={12} fill={C.line} filter="url(#glowBig)" />
          <text x={RX + (dsCm * CM) / 2} y={RY - 30} textAnchor="middle" style={mono(1, 38, { fontWeight: 600 })}>Δs = {Math.round(dsCm)} cm</text>
          <text x={RX + 25 * CM} y={RY - 110} textAnchor="middle" style={label(ramp(v, T.cm - 0.3, 0.4), { fontSize: 26 })}>ONE STEP · 10 TO 50 cm</text>
        </g>
        <g opacity={ramp(v, T.muzzle - 0.4, 0.5)}>
          <Stopwatch cx={460} cy={850} r={60} p={ramp(v, T.muzzle - 0.4, 0.7)} hand={0.06} />
          <text x={560} y={840} style={mono(1, 36, { fontWeight: 600 })}>Δt = 50 cm ÷ 790 m/s = 0.63 ms</text>
          <text x={560} y={886} style={label(ramp(v, T.thousandth - 0.2, 0.4), { fontSize: 24 })}>LESS THAN A THOUSANDTH OF A SECOND, AT THE MUZZLE</text>
        </g>
        <g opacity={ramp(v, T.thousands - 0.3, 0.4)}>
          <text x={1690} y={500} textAnchor="middle" style={mono(1, 64, { fontWeight: 600 })}>{count.toLocaleString("en-US")}</text>
          <text x={1690} y={546} textAnchor="middle" style={label(0.85, { fontSize: 22 })}>STEPS TO 1 km</text>
          <text x={1690} y={578} textAnchor="middle" style={label(0.6, { fontSize: 20 })}>(½ m EACH)</text>
        </g>
      </g>

      {/* ---------------- Euler's error at 1000 m */}
      <g opacity={oD}>
        <text x={960} y={150} textAnchor="middle" style={label(1, { fontSize: 44, letterSpacing: "0.3em" })}>THE EULER METHOD</text>
        <text x={960} y={200} textAnchor="middle" style={label(ramp(v, T.grows - 0.2, 0.4) * 0.8, { fontSize: 24 })}>ITS ERROR GROWS WITH THE STEP · AT 1000 m</text>
        {/* the drop at 1000 m, for scale */}
        {(() => {
          const X = 250, Y0 = 290, Y1 = 290 + 13.62 * 42, hot = window01(v, T.drop - 1.2, T.already, 0.3);
          return (
            <g opacity={ramp(v, T.onlyTwo, 0.5)}>
              <line x1={X - 60} y1={Y0} x2={X + 60} y2={Y0} stroke={C.dim} strokeWidth={2} strokeDasharray="6 6" />
              <line x1={X} y1={Y0} x2={X} y2={Y1} stroke={C.line} strokeWidth={4 + 3 * hot} filter={hot > 0.3 ? "url(#glow)" : undefined} />
              <line x1={X - 20} y1={Y1} x2={X + 20} y2={Y1} stroke={C.line} strokeWidth={3} />
              <circle cx={X} cy={Y1} r={10} fill={C.line} filter="url(#glowBig)" />
              <text x={X + 30} y={Y1 - 50} style={mono(1, 38, { fontWeight: 600 })}>13.6 m</text>
              <text x={X + 30} y={Y1 - 14} style={label(0.7, { fontSize: 20 })}>DROP AT 1000 m</text>
            </g>
          );
        })()}
        {/* the ladder: step -> error */}
        {LADDER.map((r, i) => {
          const o = ramp(v, r.t - 0.3, 0.5), y = 330 + i * 110;
          const dim = i === 0 ? 1 - 0.5 * ramp(v, T.onlyTwo, 0.5) : i === 1 ? 1 - 0.4 * ramp(v, T.library, 0.5) : 1;
          return (
            <g key={i} opacity={o * dim}>
              <text x={540} y={y} style={mono(1, 40)}>{r.step}</text>
              <text x={900} y={y} style={mono(1, 40, { fill: C.drag, fontWeight: 700 })}>{r.off}</text>
              {r.note && <text x={540} y={y + 38} style={label(ramp(v, T.library, 0.5) * 0.8, { fontSize: 20 })}>{r.note}</text>}
            </g>
          );
        })}
        <g opacity={ramp(v, T.already - 0.2, 0.5) * (1 - 0.4 * ramp(v, T.library, 0.5))}>
          <text x={540} y={482} style={label(0.9, { fontSize: 22 })}>SMALLER THAN ANY REAL GROUP</text>
          <text x={540} y={512} style={label(0.6, { fontSize: 20 })}>(1 MOA AT 1000 m = 29 cm)</text>
        </g>
        <Magnifier cx={LC[0]} cy={LC[1]} r={220} p={lensP} id="err">
          {/* a centimetre ruler, the real position, and Euler's dot */}
          {Array.from({ length: 5 }, (_, i) => (
            <g key={i}>
              <line x1={LC[0] - 110} y1={LC[1] - 128 + i * PXCM} x2={LC[0] - 90} y2={LC[1] - 128 + i * PXCM} stroke={C.dim} strokeWidth={2} />
              <text x={LC[0] - 118} y={LC[1] - 121 + i * PXCM} textAnchor="end" style={mono(0.6, 18)}>{i - 2} cm</text>
            </g>
          ))}
          <circle cx={LC[0]} cy={LC[1]} r={13} fill={C.line} filter="url(#glowBig)" />
          <text x={LC[0] + 26} y={LC[1] - 14} style={mono(0.9, 24)}>real</text>
          <circle cx={LC[0]} cy={LC[1] + err * PXCM} r={11} fill="none" stroke={C.drag} strokeWidth={3.5} />
          <line x1={LC[0] + 34} y1={LC[1]} x2={LC[0] + 34} y2={LC[1] + err * PXCM} stroke={C.drag} strokeWidth={2.5} />
          <text x={LC[0] + 50} y={LC[1] + err * PXCM / 2 + 34} style={mono(1, 26, { fill: C.drag })}>Euler</text>
        </Magnifier>
      </g>

      {/* ---------------- the app's own method */}
      <g opacity={oE}>
        <text x={960} y={300} textAnchor="middle" style={label(1, { fontSize: 40 })}>BY DEFAULT: A SMARTER METHOD</text>
        <text x={960} y={350} textAnchor="middle" style={label(0.6, { fontSize: 22 })}>(RUNGE–KUTTA, SECOND ORDER)</text>
        <text x={960} y={430} textAnchor="middle" style={label(ramp(v, T.longer - 0.2, 0.4) * 0.85, { fontSize: 26 })}>STEPS TEN TIMES LONGER THAN EULER'S 5 cm: HALF A METRE</text>
        <g opacity={ramp(v, T.tenth - 0.6, 0.5)}>
          <text x={560} y={560} style={mono(0.55, 40)}>Euler · ½ m steps</text>
          <text x={1360} y={560} textAnchor="end" style={mono(0.55, 40, { fill: C.drag })}>2 cm</text>
          <text x={560} y={640} style={mono(1, 40, { fontWeight: 600 })} xmlSpace="preserve">RK2   · ½ m steps</text>
          <text x={1360} y={640} textAnchor="end" style={mono(ramp(v, T.tenth - 0.1, 0.4), 40, { fontWeight: 700 })} filter="url(#glow)">{"< 0.1 mm"}</text>
        </g>
        <text x={960} y={780} textAnchor="middle" style={label(ramp(v, T.idea - 0.2, 0.4), { fontSize: 32 })}>THE SAME IDEA: SMALL STEPS</text>
        <text x={960} y={860} textAnchor="middle" style={label(ramp(v, T.story - 0.2, 0.4) * 0.6, { fontSize: 22 })}>THIS METHOD: A STORY FOR ANOTHER VIDEO</text>
      </g>
    </Scene>
  );
};
