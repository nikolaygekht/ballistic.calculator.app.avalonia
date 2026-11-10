// Episodes 2–3 kit: the scene frame, the library data, and the small drawn props (dot, stopwatch, magnifier...).
import React, { useContext } from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import ep from "../../public/ep02.json";
import { C, FPS, clamp01, lerp } from "../theme";
import { Arrow, Defs, Formula, P, Pen, Tok, Vignette, label, layoutFormula, mono, seeded } from "../lib/draw";
import { LEAD, TimingContext } from "../lib/voiceTiming";

/** Voice time in seconds for the current scene (negative during the lead-in). */
export const useVoiceTime = () => useCurrentFrame() / FPS - LEAD;

/** One scene: the midnight ground, shared defs, a short fade at both ends, and the vignette on top. */
export const Scene: React.FC<{ n: number; children: React.ReactNode; fadeIn?: number; fadeOut?: number }> =
  ({ n, children, fadeIn = 0.35, fadeOut = 0.35 }) => {
    const tm = useContext(TimingContext);
    const t = useCurrentFrame() / FPS, total = tm ? tm.sceneSeconds(n) : Infinity;
    const o = Math.min(clamp01(t / fadeIn), clamp01((total - t) / fadeOut));
    return (
      <AbsoluteFill style={{ background: C.bg }}>
        <svg width={1920} height={1080} viewBox="0 0 1920 1080" style={{ opacity: o }}>
          <Defs />
          {children}
          <Vignette />
        </svg>
      </AbsoluteFill>
    );
  };

// ---------------------------------------------------------------- data (datagen/Program.cs -> public/ep02.json)
export const EP = ep;
export const G = 9.80665;
export const SOUND = ep.sound;
/** The level-bore flight from the library, 10 m rows: x, t, y (m, negative = below the bore), v, drag (m/s²), z (wind, left). */
export const FLIGHT = ep.flight;
export type Row = (typeof ep.flight)[number];
const lerpRow = (a: Row, b: Row, k: number): Row => ({
  x: lerp(a.x, b.x, k), t: lerp(a.t, b.t, k), y: lerp(a.y, b.y, k), v: lerp(a.v, b.v, k), drag: lerp(a.drag, b.drag, k), z: lerp(a.z, b.z, k), tw: lerp(a.tw, b.tw, k),
});
/** The library's flight state at flight time `tau` (s). */
export const flightAtT = (tau: number): Row => {
  const F = FLIGHT;
  if (tau <= 0) return F[0];
  for (let i = 1; i < F.length; i++) if (F[i].t >= tau) return lerpRow(F[i - 1], F[i], (tau - F[i - 1].t) / (F[i].t - F[i - 1].t));
  return F[F.length - 1];
};
export const T_FLIGHT = FLIGHT[FLIGHT.length - 1].t;

export type EStep = { t: number; x: number; y: number; vx: number; vy: number; ax: number; ay: number; D: number };
export const EULER: Record<"s100" | "s050" | "s020" | "s005", EStep[]> = ep.euler;
/** The same model with a tiny step (0.0002 s, kept every 0.01 s): the "real" path for the step pictures. */
export const REF: EStep[] = ep.reference;
export const refAtT = (tau: number): EStep => {
  for (let i = 1; i < REF.length; i++)
    if (REF[i].t >= tau) {
      const a = REF[i - 1], b = REF[i], k = (tau - a.t) / (b.t - a.t);
      return { t: tau, x: lerp(a.x, b.x, k), y: lerp(a.y, b.y, k), vx: lerp(a.vx, b.vx, k), vy: lerp(a.vy, b.vy, k), ax: lerp(a.ax, b.ax, k), ay: lerp(a.ay, b.ay, k), D: lerp(a.D, b.D, k) };
    }
  return REF[REF.length - 1];
};

// ---------------------------------------------------------------- tokens
export const vec = (s: string, key?: string, color?: string): Tok => ({ s, vec: true, key, color });
export const sub = (s: string, key?: string, color?: string): Tok => ({ s, sub: true, key, color });
export const t_ = (s: string, key?: string, color?: string): Tok => ({ s, key, color });

// ---------------------------------------------------------------- props
/** The point mass: a bright dot. */
export const Dot: React.FC<{ x: number; y: number; r?: number; o?: number; color?: string }> = ({ x, y, r = 12, o = 1, color = C.line }) =>
  o <= 0 ? null : <circle cx={x} cy={y} r={r} fill={color} filter="url(#glowBig)" opacity={o} />;

/** A stopwatch: dial drawn on by `p`, the hand at `hand` turns (0..1 = one lap). */
export const Stopwatch: React.FC<{ cx: number; cy: number; r: number; p: number; hand: number; o?: number }> = ({ cx, cy, r, p, hand, o = 1 }) => {
  const circ: P[] = Array.from({ length: 61 }, (_, i) => [cx + r * Math.sin((i / 60) * Math.PI * 2), cy - r * Math.cos((i / 60) * Math.PI * 2)]);
  const a = hand * Math.PI * 2;
  return (
    <g opacity={o}>
      <Pen pts={circ} p={p} w={3} tip={false} />
      <g opacity={clamp01(p * 2 - 1)}>
        <line x1={cx} y1={cy - r} x2={cx} y2={cy - r - 16} stroke={C.line} strokeWidth={3} />
        <line x1={cx - 16} y1={cy - r - 18} x2={cx + 16} y2={cy - r - 18} stroke={C.line} strokeWidth={4} strokeLinecap="round" />
        {Array.from({ length: 12 }, (_, i) => {
          const b = (i / 12) * Math.PI * 2;
          return <line key={i} x1={cx + r * 0.82 * Math.sin(b)} y1={cy - r * 0.82 * Math.cos(b)} x2={cx + r * 0.92 * Math.sin(b)} y2={cy - r * 0.92 * Math.cos(b)} stroke={C.dim} strokeWidth={2} />;
        })}
        <line x1={cx} y1={cy} x2={cx + r * 0.75 * Math.sin(a)} y2={cy - r * 0.75 * Math.cos(a)} stroke={C.line} strokeWidth={3} strokeLinecap="round" />
        <circle cx={cx} cy={cy} r={5} fill={C.line} />
      </g>
    </g>
  );
};

/** A magnifier: a lens circle with a handle. Children are drawn inside the lens (clipped). */
export const Magnifier: React.FC<{ cx: number; cy: number; r: number; p: number; id: string; children?: React.ReactNode }> = ({ cx, cy, r, p, id, children }) => {
  const circ: P[] = Array.from({ length: 61 }, (_, i) => [cx + r * Math.cos((i / 60) * Math.PI * 2 - Math.PI * 0.75), cy + r * Math.sin((i / 60) * Math.PI * 2 - Math.PI * 0.75)]);
  return (
    <g>
      <clipPath id={`mag-${id}`}><circle cx={cx} cy={cy} r={r - 3} /></clipPath>
      {p > 0 && <circle cx={cx} cy={cy} r={r} fill={C.bg} opacity={0.92 * clamp01(p * 2)} />}
      <g clipPath={`url(#mag-${id})`} opacity={clamp01(p * 2 - 0.6)}>{children}</g>
      <Pen pts={circ} p={p} w={3.5} tip={false} />
      {p > 0.5 && <line x1={cx + r * 0.71} y1={cy + r * 0.71} x2={cx + r * 0.71 + 70 * clamp01(p * 2 - 1)} y2={cy + r * 0.71 + 70 * clamp01(p * 2 - 1)}
        stroke={C.line} strokeWidth={9} strokeLinecap="round" />}
    </g>
  );
};

/** A small bookmark ribbon: "we come back to this line". */
export const Bookmark: React.FC<{ x: number; y: number; o: number; s?: number }> = ({ x, y, o, s = 1 }) => (
  <path d={`M${x},${y - 26 * s} L${x + 22 * s},${y - 26 * s} L${x + 22 * s},${y + 14 * s} L${x + 11 * s},${y + 4 * s} L${x},${y + 14 * s} Z`}
    fill="none" stroke={C.line} strokeWidth={2.5} strokeLinejoin="round" opacity={o} />
);

/** An exaggeration tag ("VERTICAL ×20"): the series says on screen when a picture is not to scale. */
export const Tag: React.FC<{ x: number; y: number; o: number; text: string; anchor?: "start" | "middle" | "end" }> = ({ x, y, o, text, anchor = "end" }) => (
  <text x={x} y={y} textAnchor={anchor} style={label(o * 0.75, { fontSize: 20 })}>{text}</text>
);

/** An arrow from (x, y) along angle `a` (radians, screen: 0 = right, +π/2 = down) with length `len`. */
export const ArrowA: React.FC<{ x: number; y: number; a: number; len: number; p?: number; color?: string; w?: number; head?: number; opacity?: number; glow?: boolean }> =
  ({ x, y, a, len, p = 1, color = C.line, w = 3, head = 16, opacity = 1, glow }) => (
    <g filter={glow ? "url(#glow)" : undefined}>
      <Arrow x1={x} y1={y} x2={x + len * Math.cos(a)} y2={y + len * Math.sin(a)} p={p} color={color} w={w} head={head} opacity={opacity} />
    </g>
  );

/** A three-number readout "(a, b, c)" in mono, with a name in front. */
export const Triple: React.FC<{ x: number; y: number; name: React.ReactNode; vals: string[]; o: number; size?: number; hot?: number[] }> =
  ({ x, y, name, vals, o, size = 32, hot = [] }) => (
    <g opacity={o}>
      {name}
      <text x={x} y={y} style={mono(0.95, size)} xmlSpace="preserve">
        {"( "}
        {vals.map((s, i) => (
          <tspan key={i} style={{ fontWeight: hot.includes(i) ? 700 : 400 }} fill={hot.includes(i) ? C.line : undefined}>{s}{i < vals.length - 1 ? ", " : ""}</tspan>
        ))}
        {" )"}
      </text>
    </g>
  );

/** The letter v with a vector arrow over it, as plain SVG (for labels outside a Formula). */
export const VecText: React.FC<{ x: number; y: number; s: string; size: number; o?: number; color?: string; subS?: string; anchor?: "start" | "middle" | "end"; weight?: number }> =
  ({ x, y, s, size, o = 1, color = C.line, subS, anchor = "start", weight = 400 }) => {
    const cw = size * 0.6, w = s.length * cw + (subS ? subS.length * cw * 0.6 : 0);
    const x0 = anchor === "start" ? x : anchor === "middle" ? x - w / 2 : x - w;
    const ay = y - size * 0.9, ax1 = x0 + s.length * cw - size * 0.04, hh = size * 0.1;
    return (
      <g opacity={o}>
        <text x={x0} y={y} style={mono(1, size, { fill: color, fontWeight: weight })}>{s}</text>
        {subS && <text x={x0 + s.length * cw} y={y + size * 0.22} style={mono(1, size * 0.6, { fill: color })}>{subS}</text>}
        <path d={`M${x0 + size * 0.06},${ay} L${ax1},${ay} M${ax1 - hh * 1.3},${ay - hh} L${ax1},${ay} L${ax1 - hh * 1.3},${ay + hh}`}
          stroke={color} strokeWidth={Math.max(1.5, size * 0.05)} fill="none" strokeLinecap="round" strokeLinejoin="round" />
      </g>
    );
  };

/** A ghost of a small text that pops softly: opacity helper with a tiny rise. */
export const rise = (k: number, dy = 14) => `translate(0,${(1 - k) * dy})`;

// ---------------------------------------------------------------- the three component lines (episode 2 scene 5)
const SL_SZ = 50;
const sideLine = (ax: string, extra: boolean): Tok[] => [
  t_("a"), sub(ax), t_(" = − D · v"), sub(ax), t_(" / |"), vec("v"), t_("|"), ...(extra ? [t_(" − g")] : []),
];
const SL = [sideLine("x", false), sideLine("y", true), sideLine("z", false)];

/** a_x, a_y, a_z one per line; the side line dim with its bookmark ("vz = 0 → az = 0") until `zHot` lights it. */
export const SideLines: React.FC<{ cx: number; y: number; reveal: number; zHot: number; note: number; mark: number; o?: number }> =
  ({ cx, y, reveal, zHot, note, mark, o = 1 }) => {
    const ys = [y, y + 90, y + 180], shiftY = (4 * SL_SZ * 0.6) / 2;
    // the z line's real extent: the glow box hugs it, the bookmark and the note sit outside the box
    const LZ = layoutFormula(SL[2], cx, SL_SZ), zx0 = LZ[0].x - 26, zx1 = LZ[LZ.length - 1].x + LZ[LZ.length - 1].w + 26;
    return (
      <g opacity={o}>
        {SL.map((L, i) => (
          <g key={i} opacity={i === 2 ? 0.45 + 0.55 * zHot : 1}>
            <Formula toks={L} cx={cx + (i === 1 ? shiftY : 0)} y={ys[i]} size={SL_SZ} reveal={clamp01(reveal * 3 - i)} />
          </g>
        ))}
        {zHot > 0 && (
          <rect x={zx0} y={ys[2] - 58} width={zx1 - zx0} height={82} rx={12} fill="none" stroke={C.line} strokeWidth={2.5} filter="url(#glow)" opacity={zHot} />
        )}
        <text x={zx1 + 34} y={ys[2]} style={mono(note, 34)}>
          v<tspan fontSize={20} dy={7}>z</tspan><tspan dy={-7}> = 0  →  a</tspan><tspan fontSize={20} dy={7}>z</tspan><tspan dy={-7}> = 0</tspan>
        </text>
        <Bookmark x={zx0 - 52} y={ys[2] - 4} o={mark} s={1.3} />
      </g>
    );
  };

// ---------------------------------------------------------------- the wind: air streaks drifting right to left
const WIND = (() => { const r = seeded(909); return Array.from({ length: 90 }, () => ({ x: r() * 2200, y: 90 + r() * 960, l: 26 + r() * 40, a: 0.15 + r() * 0.3 })); })();

/** Air streaks drifting from right to left at `px` per second, from voice time `t0` (fading in over 0.8 s). */
export const WindStreaks: React.FC<{ v: number; t0: number; px?: number; o?: number }> = ({ v, t0, px = 150, o = 1 }) => {
  const k = clamp01((v - t0) / 0.8);
  if (k <= 0) return null;
  const span = 2200, wrap = (x: number) => ((x % span) + span) % span - 140;
  return (
    <g opacity={k * o}>
      {WIND.map((w, i) => {
        const x = wrap(w.x - px * (v - t0));
        return <line key={i} x1={x} y1={w.y} x2={x + w.l} y2={w.y} stroke={C.line} strokeOpacity={w.a} strokeWidth={2} strokeLinecap="round" />;
      })}
    </g>
  );
};
