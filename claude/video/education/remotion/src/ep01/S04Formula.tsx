// Scene 4 — the drag formula, term by term. Every term is acted out full screen on the bullet itself, then the
// picture shrinks back into its term. A and m fold into the sectional density at the end.
import React from "react";
import { D } from "../data";
import { C, clamp01, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Bracket, Bullet, FLAT, Formula, P, Pen, SMK, Tok, label, layoutFormula, mono, pathD, radius, seeded } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 4;
const T = {
  form: cue(N, "formula"), rho: cue(N, "Rho"), mount: cue(N, "mountains"), v: cue(N, "V is the velocity"), sq: cue(N, "square"),
  two: cue(N, "Two times"), four: cue(N, "four times"), more: cue(N, "four times", 2), a: cue(N, "A is the area"), turn: cue(N, "turn"),
  here: cue(N, "Here"), half: cue(N, "half"), wider: cue(N, "wider"), m: cue(N, "M is"), twoB: cue(N, "two bullets"),
  pushes: cue(N, "pushes"), heavy: cue(N, "heavy one"), sd: cue(N, "The area and"), sdn: cue(N, "sectional"),
  long: cue(N, "long"), cd: cue(N, "CD"), flat: cue(N, "flat"), mess: cue(N, "mess"), sharp: cue(N, "sharp"), end: cue(N, "small", 1, true),
};

const FY = 170, FS = 66;
const F1: Tok[] = [
  { s: "drag", color: C.drag }, { s: " = ½ " }, { s: "ρ", key: "rho" }, { s: " · " }, { s: "v²", key: "v" }, { s: " · " },
  { s: "Cd", key: "cd" }, { s: " · " }, { s: "A", key: "a" }, { s: " / " }, { s: "m", key: "m" },
];
const F2: Tok[] = [
  { s: "drag", color: C.drag }, { s: " = ½ " }, { s: "ρ", key: "rho" }, { s: " · " }, { s: "v²", key: "v" }, { s: " · " },
  { s: "Cd", key: "cd" }, { s: " / " }, { s: "SD", key: "sd" },
];
const L1 = layoutFormula(F1, 960, FS), L2 = layoutFormula(F2, 960, FS);
const tokX = (L: ReturnType<typeof layoutFormula>, key: string) => L.find((t) => t.key === key)!.cx;

/** A beat's picture: fades in at t0; from t1-0.55 it shrinks and flies into its formula term at (tx, FY). */
const Beat: React.FC<{ v: number; t0: number; t1: number; tx: number; children: React.ReactNode }> = ({ v, t0, t1, tx, children }) => {
  if (v < t0 - 0.5 || v > t1 + 0.1) return null;
  const kin = ramp(v, t0 - 0.3, 0.5), k = ramp(v, t1 - 0.55, 0.55);
  const s = lerp(1, 0.06, k), cx = 960, cy = 620;
  return (
    <g opacity={kin * (1 - ramp(v, t1 - 0.15, 0.15))}
      transform={`translate(${lerp(0, tx - cx, k)},${lerp(0, FY - 20 - cy, k)}) translate(${cx},${cy}) scale(${s}) translate(${-cx},${-cy})`}>
      {children}
    </g>
  );
};

const DOTS = (() => { const r = seeded(11); return Array.from({ length: 260 }, () => ({ x: r(), y: r(), u: r(), s: 1.6 + r() * 1.4 })); })();
const HITS = (() => { const r = seeded(5); return Array.from({ length: 90 }, () => ({ a: r() * Math.PI * 2, r: Math.sqrt(r()), t: r() * 1.2 })); })();

// ---------------------------------------------------------------- ρ: thinner air up the mountain
const RhoBeat: React.FC<{ v: number }> = ({ v }) => {
  const h = ramp(v, T.mount - 0.2, 2.0);                         // 0..1 -> 0..3000 m
  const ratio = lerp(1, 0.742, h);                               // ISA 3000 m: 0.909 / 1.225
  const MT: P[] = [[1120, 900], [1300, 760], [1380, 800], [1520, 560], [1600, 420], [1690, 520], [1790, 470], [1880, 900]];
  const climb = (k: number): P => {                              // along the left slope up to the peak
    const path: P[] = MT.slice(0, 5);
    const lens = path.slice(1).map((q, i) => Math.hypot(q[0] - path[i][0], q[1] - path[i][1]));
    let left = k * lens.reduce((a, b) => a + b, 0);
    for (let i = 0; i < lens.length; i++) {
      if (left <= lens[i]) return [lerp(path[i][0], path[i + 1][0], left / lens[i]), lerp(path[i][1], path[i + 1][1], left / lens[i])];
      left -= lens[i];
    }
    return path[path.length - 1];
  };
  const mk = climb(h);
  return (
    <g>
      <Pen pts={MT} p={ramp(v, T.rho, 1.2)} color={C.dim} w={2.5} tip={false} />
      <circle cx={mk[0]} cy={mk[1] - 14} r={9} fill={C.line} filter="url(#glow)" opacity={ramp(v, T.mount - 0.4, 0.4)} />
      <text x={mk[0] - 24} y={mk[1] - 30} textAnchor="end" style={mono(ramp(v, T.mount - 0.4, 0.4), 28)}>{Math.round(h * 30) * 100} m</text>
      {/* a box of air around the bullet */}
      <rect x={180} y={380} width={860} height={480} rx={18} fill="none" stroke={C.faint} strokeWidth={1.5} />
      {DOTS.map((d, i) => (
        <circle key={i} cx={200 + d.x * 820} cy={400 + d.y * 440} r={d.s} fill={C.line}
          opacity={(d.u < ratio ? 0.75 : 0.75 * clamp01((ratio - d.u) * 20 + 1)) * ramp(v, T.rho + d.x * 0.6, 0.5)} />
      ))}
      <Bullet x0={520} y={620} cal={46} />
      <g filter="url(#glow)"><Arrow x1={500} y1={620} x2={500 - 260 * ratio} y2={620} color={C.drag} w={5} head={20} p={ramp(v, T.rho + 0.6, 0.6)} /></g>
      <text x={610} y={940} textAnchor="middle" style={mono(ramp(v, T.rho + 0.3, 0.5), 30)}>ρ = {(1.2201 * ratio).toFixed(2)} kg/m³</text>
      <text x={610} y={985} textAnchor="middle" style={label(ramp(v, T.mount + 1.6, 0.4), { fontSize: 24, fill: C.drag })}>
        THINNER AIR · {Math.round((ratio - 1) * 100)} % DRAG
      </text>
    </g>
  );
};

// ---------------------------------------------------------------- v²: the arrow folds into a square
const VBeat: React.FC<{ v: number }> = ({ v }) => {
  // left: the bullet with its velocity arrow on the nose (and the drag on its tail); right, apart from it: the
  // square with side v, its edges labelled, so it reads as v × v and not as an axis. At "two times" the side
  // doubles to 2v and the square splits into four of the old v² squares.
  const AX = 720, X0 = 330, CAL = 44, NOSE = X0 + SMK.len * CAL, L = 170;
  const dbl = ramp(v, T.two, 0.9, ease);
  const side = L * (1 + dbl);
  const sqX = 1160, sqY = 880;
  const sqP = ramp(v, T.sq - 0.3, 0.9);
  const cells = [0, 1, 2, 3].map((i) => ramp(v, T.four + i * 0.18, 0.3));
  const drag = lerp(50, 200, ramp(v, T.more, 0.8, ease));
  const vName = dbl > 0.5 ? "2v" : "v";
  return (
    <g>
      <Bullet x0={X0} y={AX} cal={CAL} />
      <Arrow x1={NOSE + 16} y1={AX} x2={NOSE + 16 + side} y2={AX} p={ramp(v, T.v + 0.2, 0.6, ease)} w={4} head={18} />
      <text x={NOSE + 16 + side / 2} y={AX - 22} textAnchor="middle" style={mono(ramp(v, T.v + 0.4, 0.4), 34)}>{vName}</text>
      <text x={NOSE + 16 + side / 2} y={AX + 44} textAnchor="middle" style={label(ramp(v, T.v + 0.6, 0.4) * 0.7, { fontSize: 20 })}>VELOCITY</text>
      <g filter="url(#glow)"><Arrow x1={X0 - 20} y1={AX} x2={X0 - 20 - drag} y2={AX} color={C.drag} w={5} head={20} p={ramp(v, T.v + 0.6, 0.6)} /></g>
      <text x={X0 - 20} y={AX + 44} textAnchor="end" style={label(ramp(v, T.more, 0.4), { fontSize: 26, fill: C.drag })}>DRAG × 4</text>
      {/* the square v × v, with its own edge labels */}
      <Pen pts={[[sqX, sqY], [sqX, sqY - side], [sqX + side, sqY - side], [sqX + side, sqY]]} p={sqP} w={3} />
      <line x1={sqX} y1={sqY} x2={sqX + side} y2={sqY} stroke={C.line} strokeWidth={3} opacity={sqP >= 1 ? 1 : 0} />
      <g opacity={ramp(v, T.sq + 0.3, 0.4)}>
        <text x={sqX + side / 2} y={sqY + 44} textAnchor="middle" style={mono(0.85, 30)}>{vName}</text>
        <text x={sqX - 18} y={sqY - side / 2 + 10} textAnchor="end" style={mono(0.85, 30)}>{vName}</text>
      </g>
      {dbl > 0 && (
        <g opacity={dbl}>
          <line x1={sqX + side / 2} y1={sqY} x2={sqX + side / 2} y2={sqY - side} stroke={C.dim} strokeWidth={2} strokeDasharray="6 6" />
          <line x1={sqX} y1={sqY - side / 2} x2={sqX + side} y2={sqY - side / 2} stroke={C.dim} strokeWidth={2} strokeDasharray="6 6" />
        </g>
      )}
      {/* four of the old squares fit in the new one: each one is a v² */}
      {cells.map((c, i) => {
        const cx = sqX + (i % 2) * side / 2, cy = sqY - side + Math.floor(i / 2) * side / 2;
        return (
          <g key={i} opacity={c}>
            <rect x={cx + 4} y={cy + 4} width={side / 2 - 8} height={side / 2 - 8} fill="url(#hatch)" />
            <text x={cx + side / 4} y={cy + side / 4 + 11} textAnchor="middle" style={mono(1, 32)}>v²</text>
          </g>
        );
      })}
      <text x={sqX + side + 30} y={sqY - side / 2 + 14} style={mono(sqP, 44)}>{dbl > 0.5 ? "(2v)²" : "v²"}</text>
      <text x={sqX + side + 30} y={sqY - side / 2 + 56} style={label(window01(v, T.sq, T.two, 0.3), { fontSize: 22 })}>v × v</text>
      <text x={sqX + side + 30} y={sqY - side / 2 + 56} style={label(ramp(v, T.four + 0.8, 0.4), { fontSize: 22 })}>FOUR SQUARES OF v²</text>
    </g>
  );
};

// ---------------------------------------------------------------- A: turn the bullet, look at its front
const ABeat: React.FC<{ v: number }> = ({ v }) => {
  const turn = ramp(v, T.turn, 1.3);
  const zoom = ramp(v, T.here - 0.3, 1.0, ease);
  const cal = lerp(70, 300, zoom), AX = 620;
  const x0 = 960 - (SMK.len * cal) / 2;
  const R = 0.5 * cal;
  const area = ramp(v, cue(N, "circle") - 0.2, 0.6);
  const t = v - T.here - 0.6;
  const wide = ramp(v, T.wider - 0.2, 0.8, ease);
  const R50 = R * (12.95 / 7.82);
  const hitsIn = (rMax: number, rMin: number, color: string, k: number) => HITS.map((h, i) => {
    const ph = ((t * 1.3 + h.t) % 1.2) / 1.2;
    if (t < h.t || k <= 0) return null;
    const rr = rMin + (rMax - rMin) * h.r;
    return <circle key={i} cx={960 + rr * Math.cos(h.a)} cy={AX + rr * Math.sin(h.a)} r={3 + ph * 10} fill="none" stroke={color} strokeWidth={2} opacity={(1 - ph) * k} />;
  });
  return (
    <g>
      {wide > 0 && (
        <g opacity={wide}>
          <circle cx={960} cy={AX} r={R50} fill="url(#hatchDrag)" stroke={C.drag} strokeWidth={2.5} strokeDasharray="8 8" />
          {hitsIn(R50, R, C.drag, wide)}
          <text x={960 + R50 + 30} y={AX - R50 + 50} style={mono(1, 30, { fill: C.drag })}>.50 BMG · 1.32 cm²</text>
        </g>
      )}
      <Bullet x0={x0} y={AX} cal={cal} turn={turn} cannelure={turn < 0.05} />
      {area > 0 && <circle cx={960} cy={AX} r={R} fill="url(#hatch)" opacity={area} />}
      {hitsIn(R, 0, C.line, area * (1 - wide * 0.3))}
      <g opacity={area}>
        <Arrow x1={960 - R} y1={AX + R50 + 40} x2={960 + R} y2={AX + R50 + 40} head={12} w={2} />
        <Arrow x1={960 + R} y1={AX + R50 + 40} x2={960 - R} y2={AX + R50 + 40} head={12} w={2} />
        <line x1={960 - R} y1={AX} x2={960 - R} y2={AX + R50 + 50} stroke={C.dim} strokeWidth={1.2} strokeDasharray="4 6" />
        <line x1={960 + R} y1={AX} x2={960 + R} y2={AX + R50 + 50} stroke={C.dim} strokeWidth={1.2} strokeDasharray="4 6" />
        <text x={960} y={AX + R50 + 85} textAnchor="middle" style={mono(1, 30)}>d = 7.8 mm</text>
        <text x={960 - R50 - 50} y={AX - R + 30} textAnchor="end" style={mono(ramp(v, T.half - 0.3, 0.5), 40, { fontWeight: 600 })}>A = 0.48 cm²</text>
      </g>
    </g>
  );
};

// ---------------------------------------------------------------- m: two bullets, same push, different slowing
const MBeat: React.FC<{ v: number }> = ({ v }) => {
  const CAL = 62, Y1 = 500, Y2 = 800, X0 = 700;
  const f1 = ramp(v, T.m + 0.2, 1.2), f2 = ramp(v, T.twoB, 1.2);
  const push = ramp(v, T.pushes - 0.2, 0.7, ease), slow = ramp(v, T.heavy - 0.2, 0.7, ease);
  const row = (y: number, fill: number, fillCol: string, name: string, decel: number, id: string, o: number) => (
    <g opacity={o}>
      <Bullet x0={X0} y={y} cal={CAL} fill={fillCol} fillLevel={fill} id={id} />
      <text x={X0 + SMK.len * CAL / 2} y={y - 50} textAnchor="middle" style={mono(fill, 30)}>{name}</text>
      <g filter="url(#glow)"><Arrow x1={X0 - 20} y1={y} x2={X0 - 240} y2={y} p={push} color={C.drag} w={5} head={20} /></g>
      <Arrow x1={X0 + SMK.len * CAL + 40} y1={y + 70} x2={X0 + SMK.len * CAL + 40 - decel} y2={y + 70} p={slow} w={3} head={14} />
    </g>
  );
  return (
    <g>
      {row(Y1, f1, "rgba(233,215,165,0.22)", "11.3 g", 260, "m1", 1)}
      {row(Y2, f2, "rgba(233,215,165,0.75)", "heavier", 140, "m2", ramp(v, T.twoB - 0.3, 0.5))}
      <text x={X0 - 130} y={Y1 - 40} textAnchor="middle" style={label(push, { fontSize: 22, fill: C.drag })}>SAME PUSH</text>
      <text x={X0 + SMK.len * CAL + 40} y={Y1 + 115} textAnchor="end" style={label(slow, { fontSize: 22 })}>SLOWS DOWN</text>
      <text x={X0 + SMK.len * CAL + 40} y={Y2 + 115} textAnchor="end" style={label(slow, { fontSize: 22 })}>SLOWS DOWN LESS</text>
    </g>
  );
};

// ---------------------------------------------------------------- SD: A / m fold into one number
const SDBeat: React.FC<{ v: number }> = ({ v }) => {
  const fat = { ...SMK, len: 3.0, noseLen: 1.5, btLen: 0.4 }, slim = { ...SMK, len: 5.9, noseLen: 3.1, btLen: 0.8 };
  const hl = ramp(v, T.long - 0.2, 0.6);
  const bar = (y: number, len: number, o: number) => (
    <g opacity={o * ramp(v, T.sdn, 0.2)}><line x1={1250} y1={y} x2={1250 + len * ramp(v, T.sdn, 0.8, ease)} y2={y} stroke={C.line} strokeWidth={14} strokeLinecap="round" /></g>
  );
  return (
    <g>
      <text x={960} y={380} textAnchor="middle" style={mono(ramp(v, T.sdn - 0.3, 0.6), 40)}>SD = m / d²</text>
      <text x={960} y={428} textAnchor="middle" style={label(ramp(v, T.sdn, 0.6) * 0.7, { fontSize: 22 })}>SECTIONAL DENSITY · SAME WEIGHT, TWO SHAPES</text>
      <Bullet s={fat} x0={300} y={560} cal={72} cannelure={false} opacity={1 - 0.5 * hl} />
      <circle cx={1060} cy={560} r={36} fill="url(#hatch)" stroke={C.line} strokeWidth={2.5} opacity={1 - 0.5 * hl} />
      {bar(560, 260, 1 - 0.5 * hl)}
      <g filter={hl > 0.5 ? "url(#glow)" : undefined}>
        <Bullet s={slim} x0={300} y={800} cal={51} cannelure={false} />
      </g>
      <circle cx={1060} cy={800} r={25.5} fill="url(#hatch)" stroke={C.line} strokeWidth={2.5} />
      {bar(800, 510, 1)}
      <text x={1250} y={530} style={label(ramp(v, T.sdn, 0.6) * 0.7, { fontSize: 20 })}>SD</text>
      <text x={1250 + 510 + 30} y={810} style={label(hl, { fontSize: 22 })}>BIG SD · LESS DRAG</text>
    </g>
  );
};

// ---------------------------------------------------------------- Cd: flat nose -> pointed bullet
const CdBeat: React.FC<{ v: number }> = ({ v }) => {
  const k = ramp(v, T.sharp - 0.1, 1.6, ease);                     // 0 flat .. 1 our bullet
  const CAL = 74, AX = 640;
  const len = lerp(FLAT.len, SMK.len, k), x0 = 960 - (len * CAL) / 2, nose = x0 + len * CAL;
  const blunt = 1 - k;
  const mess = ramp(v, T.flat - 1.0, 1.0);
  const cdv = lerp(1.0, D.rows[0].cd, k);
  const rAt = (x: number) => {
    const f = (x - x0) / (len * CAL);
    if (f >= 0 && f <= 1) return lerp(radius(FLAT, f * FLAT.len), radius(SMK, f * SMK.len), k) * CAL;
    if (f > 1) return (lerp(0.5, 0.05, k) * CAL + 50 * blunt) * Math.exp(-(x - nose) / lerp(120, 30, k));
    const w = lerp(0.5, 0.4, k) * CAL * (1 + 0.9 * blunt * mess);
    return w * Math.exp(-(x0 - x) / lerp(420, 160, k));
  };
  const lines = [36, 80, 130, 190, 260];
  const flow = (v * 260) % 40;
  // the bow wave: a wide curve for the flat nose, folding back into a thin cone
  const mu = Math.asin(1 / D.rows[0].mach);
  const bow: P[] = Array.from({ length: 31 }, (_, i) => {
    const y = -300 + i * 20;
    const xBlunt = nose + 70 - (Math.abs(y) ** 1.6) / 60;
    const xCone = nose - Math.abs(y) / Math.tan(mu);
    return [lerp(xBlunt, xCone, k), AX + y];
  });
  return (
    <g>
      {lines.flatMap((d) => [-1, 1].map((sg) => {
        const pts: P[] = [];
        for (let x = 140; x <= 1800; x += 12) pts.push([x, AX + sg * (d + rAt(x) * Math.exp(-d / 220))]);
        return <path key={`${d}${sg}`} d={pathD(pts)} stroke={C.line} strokeWidth={1.6} fill="none" opacity={0.45}
          strokeDasharray="22 18" strokeDashoffset={flow} />;
      }))}
      <path d={pathD(bow)} stroke={C.drag} strokeWidth={2.5} fill="none" opacity={0.8} />
      {/* the mess behind: swirls that shrink as the tail gets a boat tail */}
      {Array.from({ length: 9 }, (_, i) => {
        const big = (1 - k * 0.8) * mess;
        const cx = x0 - 50 - i * 70 * (1 - 0.5 * k), cy = AX + (i % 2 ? 1 : -1) * (12 + 24 * big), r = (46 - i * 3.5) * big;
        if (r < 2) return null;
        const a0 = v * (3 + i * 0.3) * (i % 2 ? 1 : -1) + i;
        const pts: P[] = Array.from({ length: 30 }, (_, j) => {
          const a = a0 + (j / 30) * Math.PI * 1.7, rr = r * (0.3 + (0.7 * j) / 30);
          return [cx + rr * Math.cos(a), cy + rr * Math.sin(a)];
        });
        return <path key={i} d={pathD(pts)} stroke={C.drag} strokeWidth={2.6} fill="none" opacity={0.9} />;
      })}
      <Bullet s={FLAT} morph={{ to: SMK, k }} x0={x0} y={AX} cal={CAL} cannelure={false} />
      <text x={960} y={930} textAnchor="middle" style={mono(ramp(v, T.cd + 0.3, 0.5), 56, { fontWeight: 600 })}>Cd ≈ {cdv.toFixed(2)}</text>
      <text x={960} y={980} textAnchor="middle" style={label(window01(v, T.mess - 0.3, T.sharp + 0.2, 0.3), { fontSize: 24, fill: C.drag })}>A MESS BEHIND A FLAT NOSE</text>
      <text x={960} y={980} textAnchor="middle" style={label(ramp(v, T.sharp + 1.4, 0.4), { fontSize: 24 })}>SHARP NOSE · BOAT TAIL</text>
    </g>
  );
};

export const S04Formula: React.FC = () => {
  const v = useVoiceTime();
  const fold = ramp(v, T.sdn - 0.4, 0.8, ease);
  const active =
    v >= T.cd - 0.2 ? "cd" : v >= T.sdn - 0.4 ? "sd" : v >= T.sd ? null : v >= T.m - 0.2 ? "m" : v >= T.a - 0.2 ? "a" : v >= T.v - 0.2 ? "v" : v >= T.rho - 0.2 ? "rho" : null;
  const order = ["rho", "v", "a", "m", "sd", "cd"], starts = [T.rho, T.v, T.a, T.m, T.sdn, T.cd];
  const lit = new Set(order.filter((_, i) => v >= starts[i] - 0.2));
  if (v >= T.sd) { lit.add("a"); lit.add("m"); }
  const aX0 = L1.find((t) => t.key === "a")!.x, mT = L1.find((t) => t.key === "m")!;
  const result = ramp(v, T.end + 0.2, 0.6);

  return (
    <Scene n={N}>
      <text x={960} y={FY - 92} textAnchor="middle" style={label(window01(v, T.form, T.rho, 0.4) * 0.7, { fontSize: 22 })}>HOW MUCH THE AIR SLOWS THE BULLET</text>
      <Formula toks={F1} cx={960} y={FY} size={FS} reveal={ramp(v, T.form - 0.2, 2.0)} active={active} lit={lit} opacity={1 - fold} />
      <Formula toks={F2} cx={960} y={FY} size={FS} reveal={1} active={active} lit={lit} opacity={fold} />
      <g opacity={window01(v, T.sd - 0.1, T.sdn + 0.4, 0.3)}>
        <Bracket x0={aX0 - 6} x1={mT.x + mT.w + 6} y={FY + 22} p={ramp(v, T.sd, 0.6)} />
      </g>
      <text x={L2[L2.length - 1].x + L2[L2.length - 1].w + 30} y={FY} style={mono(result, FS, { fill: C.drag, fontWeight: 700 })} filter="url(#glow)">
        = {Math.round(D.rows[0].drag / D.g)} g
      </text>

      <Beat v={v} t0={T.rho} t1={T.v} tx={tokX(L1, "rho")}><RhoBeat v={v} /></Beat>
      <Beat v={v} t0={T.v} t1={T.a} tx={tokX(L1, "v")}><VBeat v={v} /></Beat>
      <Beat v={v} t0={T.a} t1={T.m} tx={tokX(L1, "a")}><ABeat v={v} /></Beat>
      <Beat v={v} t0={T.m} t1={T.sd} tx={tokX(L1, "m")}><MBeat v={v} /></Beat>
      <Beat v={v} t0={T.sd} t1={T.cd} tx={tokX(L2, "sd")}><SDBeat v={v} /></Beat>
      <Beat v={v} t0={T.cd} t1={T.end + 1.4} tx={tokX(L2, "cd")}><CdBeat v={v} /></Beat>
    </Scene>
  );
};
