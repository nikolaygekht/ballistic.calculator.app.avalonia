// Scene 2 — six numbers: the position arrow r (x, y, z) as the diagonal of a box, the velocity arrow v with its
// three parts, and its length by Pythagoras in three directions. The generic velocity box is not to scale; at
// "Here" it collapses to the real shot, 790 m/s straight downrange.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Formula, P, Pen, Tok, label, layoutFormula, mono } from "../lib/draw";
import { Dot, Scene, VecText, sub, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 2;
const T = {
  six: cue(N, "six numbers"), pos: cue(N, "position"), little: cue(N, "little arrow"), down: cue(N, "downrange"), high: cue(N, "high"),
  side: cue(N, "side"), vel: cue(N, "the velocity"), parts: cue(N, "three parts"), down2: cue(N, "downrange", 2), up: cue(N, "up"),
  side2: cue(N, "side", 2), length: cue(N, "length"), box: cue(N, "box"), corner: cue(N, "corner"), pyth: cue(N, "Pythagoras"),
  square: cue(N, "Square"), add: cue(N, "add"), root: cue(N, "root"), here: cue(N, "Here", 2), six2: cue(N, "Six numbers", 2),
  degrees: cue(N, "degrees"), dirs: cue(N, "three directions", 2), vels: cue(N, "velocities"),
};

// oblique view: x downrange to the right, y up, z (to the side, away from us) up-right into the screen
const O: P = [300, 830];
const ZX = 0.6, ZY = -0.42;
const proj = (x: number, y: number, z: number, o: P = O): P => [o[0] + x + ZX * z, o[1] - y + ZY * z];
const PX = 700, PY = 230, PZ = 300;
const PD = proj(PX, PY, PZ);

/** The 12 edges of the box from `o` spanned by (a, b, c) in model units. */
const boxEdges = (a: number, b: number, c: number, o: P): [P, P][] => {
  const v = (i: number, j: number, k: number) => proj(i * a, j * b, k * c, o);
  const e: [P, P][] = [];
  for (const [i, j, k] of [[0, 0, 0], [1, 0, 0], [0, 1, 0], [0, 0, 1], [1, 1, 0], [1, 0, 1], [0, 1, 1], [1, 1, 1]] as const) {
    if (!i) e.push([v(i, j, k), v(1, j, k)]);
    if (!j) e.push([v(i, j, k), v(i, 1, k)]);
    if (!k) e.push([v(i, j, k), v(i, j, 1)]);
  }
  return e;
};

const FR: Tok[] = [vec("r", "r"), t_(" = ("), t_("x", "x"), t_(", "), t_("y", "y"), t_(", "), t_("z", "z"), t_(")")];
const FV: Tok[] = [vec("v", "v"), t_(" = ("), t_("v", "vx"), sub("x", "vx"), t_(", "), t_("v", "vy"), sub("y", "vy"), t_(", "), t_("v", "vz"), sub("z", "vz"), t_(")")];
const FL: Tok[] = [t_("|", "len"), vec("v", "len"), t_("| = √(", "len"), t_("v", "sx"), sub("x", "sx"), t_("²", "sx"), t_(" + ", "plus"), t_("v", "sy"), sub("y", "sy"), t_("²", "sy"),
  t_(" + ", "plus"), t_("v", "sz"), sub("z", "sz"), t_("²", "sz"), t_(")", "len")];
// "|v|" alone, sitting exactly where the full formula will start
const FLa: Tok[] = [t_("|", "len"), vec("v", "len"), t_("|", "len")];
const FLaX = layoutFormula(FL, 960, 50)[0].x + 45;

export const S02Six: React.FC = () => {
  const v = useVoiceTime();
  const axes = ramp(v, -0.5, 1.4);
  // the position: arrow, box, and the three edges on the axes
  const rP = ramp(v, T.pos, 1.0, ease);
  const boxP = ramp(v, T.down - 0.4, 0.8);
  const hot = (t0: number, t1: number) => window01(v, t0 - 0.15, t1, 0.25);
  // the velocity: generic proportions, then the real shot
  const real = ramp(v, T.here - 0.1, 1.2, ease);
  const VX = lerp(300, 430, real), VY = lerp(110, 0, real), VZ = lerp(170, 0, real);
  const vP = ramp(v, T.vel, 0.9, ease);
  const tip = proj(VX, VY, VZ, PD);
  const c1 = proj(VX, 0, 0, PD), c2 = proj(VX, VY, 0, PD);
  const partsP = [ramp(v, T.down2 - 0.1, 0.5), ramp(v, T.up - 0.1, 0.5), ramp(v, T.side2 - 0.1, 0.5)];
  const vBox = ramp(v, T.box - 0.1, 0.9) * (1 - real);
  const floor = proj(VX, 0, VZ, PD);
  const floorP = ramp(v, T.pyth, 0.8) * (1 - real);
  const diagHot = window01(v, T.root - 0.1, T.here + 0.4, 0.3);
  // formula activity
  const active =
    v >= T.square - 0.1 && v < T.add ? "sx" : v >= T.add && v < T.root ? "plus" : v >= T.root && v < T.six2 ? "len" :
    v >= T.length - 0.1 && v < T.square ? "len" :
    v >= T.side2 - 0.1 && v < T.length ? "vz" : v >= T.up - 0.1 && v < T.side2 ? "vy" : v >= T.down2 - 0.1 && v < T.up ? "vx" :
    v >= T.vel - 0.1 && v < T.down2 ? "v" :
    v >= T.side - 0.1 && v < T.vel ? "z" : v >= T.high - 0.1 && v < T.side ? "y" : v >= T.down - 0.1 && v < T.high ? "x" :
    v >= T.little - 0.1 && v < T.down ? "r" : null;
  const sqLit = new Set(["len", "plus", "vx", "vy", "vz", "x", "y", "z", "r", "v"]);
  if (v >= T.square + 0.2) { sqLit.add("sx"); sqLit.add("sy"); sqLit.add("sz"); }
  const allHot = window01(v, T.six2 - 0.1, T.vels - 0.1, 0.3);
  const velHot = window01(v, T.vels - 0.1, T.dirs - 0.1, 0.3);   // three of them are velocities: v boxed, r dimmed
  const dirs = ramp(v, T.dirs - 0.2, 0.6);

  return (
    <Scene n={N}>
      {/* formulas */}
      <Formula toks={FR} cx={560} y={120} size={50} reveal={ramp(v, T.pos - 0.1, 1.2)} active={allHot + velHot > 0.5 ? null : active} opacity={1 - 0.65 * velHot} />
      <Formula toks={FV} cx={1360} y={120} size={50} reveal={ramp(v, T.vel - 0.1, 1.4)} active={allHot + velHot > 0.5 ? null : active} />
      <Formula toks={FLa} cx={FLaX} y={215} size={50} reveal={ramp(v, T.length - 0.2, 0.5)} active={active} opacity={1 - ramp(v, T.square - 0.3, 0.1)} />
      <Formula toks={FL} cx={960} y={215} size={50} reveal={ramp(v, T.square - 0.3, 1.6)} active={allHot > 0.5 ? null : active} lit={sqLit}
        opacity={ramp(v, T.square - 0.3, 0.1)} />
      {allHot > 0 && (
        <g opacity={allHot}>
          <rect x={310} y={66} width={510} height={76} rx={12} fill="none" stroke={C.line} strokeWidth={2} filter="url(#glow)" />
          <rect x={1040} y={66} width={640} height={76} rx={12} fill="none" stroke={C.line} strokeWidth={2} filter="url(#glow)" />
        </g>
      )}
      {velHot > 0 && (
        <g opacity={velHot}>
          <rect x={1040} y={66} width={640} height={76} rx={12} fill="none" stroke={C.line} strokeWidth={3} filter="url(#glowBig)" />
          <text x={1360} y={48} textAnchor="middle" style={label(0.9, { fontSize: 22 })}>THREE VELOCITIES: HOW FAST x, y, z CHANGE</text>
        </g>
      )}
      <text x={960} y={300} textAnchor="middle" style={label(window01(v, T.six - 0.2, T.pos, 0.3), { fontSize: 30 })}>SIX NUMBERS</text>
      <text x={560} y={180} textAnchor="middle" style={label(window01(v, T.little - 0.1, T.down, 0.3) * 0.85, { fontSize: 22 })}>AN ARROW ON TOP = THREE NUMBERS</text>

      {/* axes */}
      <Pen pts={[O, [O[0] + 1500, O[1]]]} p={axes} color={C.dim} w={2} tip={false} />
      <Pen pts={[O, [O[0], O[1] - 560]]} p={axes} color={C.dim} w={2} tip={false} />
      <Pen pts={[O, proj(0, 0, 560)]} p={axes} color={C.dim} w={2} tip={false} />
      <g opacity={axes}>
        <text x={O[0] + 1500} y={O[1] + 40} textAnchor="end" style={label(0.7, { fontSize: 22 })}>x · DOWNRANGE</text>
        <text x={O[0] - 20} y={O[1] - 540} textAnchor="end" style={label(0.7, { fontSize: 22 })}>y · UP</text>
        <text x={proj(0, 0, 560)[0] + 20} y={proj(0, 0, 560)[1] - 8} style={label(0.7, { fontSize: 22 })}>z · SIDE</text>
        {/* the muzzle at the origin */}
        <path d={`M${O[0] - 150},${O[1] - 18} L${O[0] - 6},${O[1] - 18} L${O[0] - 6},${O[1] + 18} L${O[0] - 150},${O[1] + 18}`} stroke={C.line} strokeWidth={3} fill="none" />
      </g>

      {/* the position box and arrow */}
      <g opacity={boxP * (1 - 0.6 * ramp(v, T.vel, 0.6))}>
        {boxEdges(PX, PY, PZ, O).map(([a, b], i) => (
          <line key={i} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeOpacity={0.35} strokeWidth={1.5} strokeDasharray="6 7" />
        ))}
      </g>
      {[
        { a: O, b: proj(PX, 0, 0), t0: T.down, t1: T.high, s: "x", at: proj(PX / 2, 0, 0), dx: 0, dy: 40 },
        { a: O, b: proj(0, PY, 0), t0: T.high, t1: T.side, s: "y", at: proj(0, PY / 2, 0), dx: -30, dy: 10 },
        { a: O, b: proj(0, 0, PZ), t0: T.side, t1: T.vel, s: "z", at: proj(0, 0, PZ / 2), dx: -34, dy: -6 },
      ].map((e) => {
        const h = hot(e.t0, e.t1), o = ramp(v, e.t0 - 0.15, 0.4);
        return (
          <g key={e.s} opacity={o * (1 - 0.6 * ramp(v, T.vel, 0.6))}>
            <line x1={e.a[0]} y1={e.a[1]} x2={e.b[0]} y2={e.b[1]} stroke={C.line} strokeWidth={3 + 4 * h} filter={h > 0.3 ? "url(#glow)" : undefined} />
            <text x={e.at[0] + e.dx} y={e.at[1] + e.dy} textAnchor="middle" style={mono(1, 40, { fontWeight: h > 0.5 ? 700 : 400 })}>{e.s}</text>
          </g>
        );
      })}
      <g opacity={1 - 0.5 * ramp(v, T.vel, 0.6)}>
        <Arrow x1={O[0]} y1={O[1]} x2={PD[0]} y2={PD[1]} p={rP} w={4} head={20} />
        <VecText x={lerp(O[0], PD[0], 0.62) + 10} y={lerp(O[1], PD[1], 0.62) + 58} s="r" size={44} o={rP} />
      </g>
      <Dot x={PD[0]} y={PD[1]} o={ramp(v, -0.2, 0.6)} />

      {/* the velocity arrow, its three parts, its box */}
      <g opacity={vBox}>
        {boxEdges(VX, VY, VZ, PD).map(([a, b], i) => (
          <line key={i} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeOpacity={0.4} strokeWidth={1.5} strokeDasharray="6 7" />
        ))}
      </g>
      {[[PD, c1, "vx", partsP[0], T.down2, T.up], [c1, c2, "vy", partsP[1], T.up, T.side2], [c2, tip, "vz", partsP[2], T.side2, T.length]].map(([a, b, s, pp, t0, t1]) => {
        const A = a as P, B = b as P, h = hot(t0 as number, t1 as number);
        return (
          <g key={s as string} opacity={1 - real}>
            <Pen pts={[A, B]} p={pp as number} w={3 + 3 * h} tip={false} />
            <text x={(A[0] + B[0]) / 2 + (s === "vy" ? 22 : s === "vz" ? 26 : 0)} y={(A[1] + B[1]) / 2 + (s === "vx" ? 42 : 8)}
              style={mono((pp as number) * (0.7 + 0.3 * h), 32, { fontWeight: h > 0.5 ? 700 : 400 })}>
              v<tspan fontSize={20} dy={7}>{(s as string)[1]}</tspan>
            </text>
          </g>
        );
      })}
      {/* Pythagoras: the floor diagonal, then the diagonal through the box, which is the arrow */}
      <g opacity={floorP}>
        <line x1={PD[0]} y1={PD[1]} x2={floor[0]} y2={floor[1]} stroke={C.line} strokeWidth={2.5} strokeDasharray="10 6" />
        <line x1={floor[0]} y1={floor[1]} x2={tip[0]} y2={tip[1]} stroke={C.line} strokeWidth={2.5} strokeDasharray="10 6" />
      </g>
      {[PD, tip].map((q, i) => (
        <circle key={i} cx={q[0]} cy={q[1]} r={16} fill="none" stroke={C.line} strokeWidth={2.5} opacity={window01(v, T.corner - 0.1, T.pyth + 0.6, 0.3)} />
      ))}
      <g filter={diagHot > 0.2 ? "url(#glowBig)" : "url(#glow)"}>
        <Arrow x1={PD[0]} y1={PD[1]} x2={tip[0]} y2={tip[1]} p={vP} w={4 + 3 * diagHot} head={22} />
      </g>
      <VecText x={tip[0] + 16} y={tip[1] - 18} s="v" size={44} o={vP * (1 - real)} />
      <g opacity={real}>
        <text x={(PD[0] + tip[0]) / 2 - 64} y={tip[1] - 30} style={mono(1, 40, { fontWeight: 600 })}>|</text>
        <VecText x={(PD[0] + tip[0]) / 2 - 40} y={tip[1] - 30} s="v" size={40} weight={600} />
        <text x={(PD[0] + tip[0]) / 2 - 16} y={tip[1] - 30} style={mono(1, 40, { fontWeight: 600 })} xmlSpace="preserve">| = 790 m/s</text>
      </g>
      <text x={960} y={300} textAnchor="middle" style={label(window01(v, T.box - 0.1, T.pyth + 0.2, 0.3) * 0.85, { fontSize: 24 })}>FROM ONE CORNER TO THE OPPOSITE CORNER</text>
      <text x={960} y={300} textAnchor="middle" style={label(window01(v, T.pyth, T.here, 0.3) * 0.85, { fontSize: 24 })}>PYTHAGORAS, IN THREE DIRECTIONS</text>

      {/* three degrees of freedom */}
      <g opacity={dirs}>
        {[[proj(-90, 0, 0, PD), proj(90, 0, 0, PD)], [proj(0, -90, 0, PD), proj(0, 90, 0, PD)], [proj(0, 0, -120, PD), proj(0, 0, 120, PD)]].map(([a, b], i) => (
          <g key={i}>
            <Arrow x1={PD[0]} y1={PD[1]} x2={a[0]} y2={a[1]} head={14} w={2.5} />
            <Arrow x1={PD[0]} y1={PD[1]} x2={b[0]} y2={b[1]} head={14} w={2.5} />
          </g>
        ))}
      </g>
      <text x={960} y={1000} textAnchor="middle" style={label(ramp(v, T.degrees - 0.2, 0.5), { fontSize: 34 })}>THREE DEGREES OF FREEDOM</text>
      <text x={960} y={1000} textAnchor="middle" style={label(window01(v, T.six2 - 0.2, T.dirs - 0.2, 0.3), { fontSize: 34 })}>6 NUMBERS</text>
    </Scene>
  );
};
