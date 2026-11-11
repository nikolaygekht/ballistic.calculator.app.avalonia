// Episode 4 kit: the spinning bullet, the "quarter turn" rule seen from behind, a formula line with fractions, the
// globe, the S_g gauge and the effect cards. The scene frame, the dot and the data come from episode 2's kit.
import React from "react";
import { C, clamp01, lerp } from "../theme";
import { Bullet, Formula, P, Pen, SMK, Tok, label, layoutFormula, mono, outline, pathD } from "../lib/draw";
import { FLIGHT } from "../ep02/kit";

export { Scene, useVoiceTime, Dot, ArrowA, VecText, FLIGHT, sub, t_, vec } from "../ep02/kit";

export const sup = (s: string, key?: string, color?: string): Tok => ({ s, sup: true, key, color });
export const k_ = (s: string, key: string, color?: string): Tok => ({ s, key, color });

// ---------------------------------------------------------------- the numbers of our shot
export const SG = 1.708;
export const L_CAL = 1.394 / 0.308;
/** Litz spin drift (m) at time t, net of the windage zero at 100 m, as the engine reports it (31 cm at 1000 m). */
const driftRaw = (t: number) => 0.0254 * 1.25 * (SG + 1.2) * Math.pow(t, 1.83);
const T100 = FLIGHT.find((r) => r.x >= 100)!.t;
export const drift = (x: number, t: number) => driftRaw(t) - (driftRaw(T100) * x) / 100;
/** Flight row at downrange distance x (m), interpolated. */
export const atX = (x: number) => {
  const j = FLIGHT.findIndex((r) => r.x > x), i = j < 0 ? FLIGHT.length - 2 : Math.max(0, j - 1);
  const a = FLIGHT[i], b = FLIGHT[i + 1], k = clamp01((x - a.x) / (b.x - a.x));
  return { x, t: lerp(a.t, b.t, k), y: lerp(a.y, b.y, k), v: lerp(a.v, b.v, k) };
};

// ---------------------------------------------------------------- the bullet, turned about its centre of mass
/** Side-view bullet pointing right, base at x0. CG at 42 % of its length from the base, CP at 72 %. */
export const BLEN = SMK.len;
export const CG_F = 0.42, CP_F = 0.72;

/** A bullet whose centre of mass sits at (cx, cy), turned by `deg` (positive = nose up), with rifling marks running
 *  around it at `spin` (turns, any real number). */
export const SpinBullet: React.FC<{ cx: number; cy: number; cal: number; deg?: number; spin?: number; marks?: number; o?: number; w?: number; id: string }> =
  ({ cx, cy, cal, deg = 0, spin = 0, marks = 0, o = 1, w = 3, id }) => {
    const x0 = cx - CG_F * BLEN * cal;
    const body = outline(SMK, x0, cy, cal);
    const xa = x0 + (SMK.btLen + 0.05) * cal, xb = x0 + (BLEN - SMK.noseLen * 0.55) * cal;
    const n = 9, ph = ((spin % 1) + 1) % 1;
    return (
      <g opacity={o} transform={`rotate(${-deg},${cx},${cy})`}>
        <Bullet x0={x0} y={cy} cal={cal} w={w} cannelure={false} />
        {marks > 0 && (
          <>
            <clipPath id={`rif-${id}`}><path d={pathD(body, true)} /></clipPath>
            <g clipPath={`url(#rif-${id})`} opacity={marks}>
              {Array.from({ length: n + 1 }, (_, i) => {
                const u = (i + ph) / n;                      // 0..1 around the bullet
                const yy = cy + 0.5 * cal * Math.cos(u * Math.PI * 2);
                const front = Math.sin(u * Math.PI * 2) > 0;   // only the half facing us
                return front ? <line key={i} x1={xa} y1={yy} x2={xb} y2={yy - 0.18 * cal} stroke={C.line} strokeWidth={1.6} opacity={0.55} /> : null;
              })}
            </g>
          </>
        )}
      </g>
    );
  };

// ---------------------------------------------------------------- the rule, seen from behind
const DIR: Record<"left" | "up" | "right" | "down", P> = { left: [-1, 0], up: [0, -1], right: [1, 0], down: [0, 1] };
const NEXT = { left: "up", up: "right", right: "down", down: "left" } as const;
/** The bullet seen from behind as a clock face. It spins clockwise. A push on the nose towards `push` (coral arrow,
 *  drawn by pushP) moves the nose a quarter turn further (drawn by noseP); `quarter` shows the 90° arc. */
export const ClockBack: React.FC<{
  cx: number; cy: number; r: number; spin: number; push?: keyof typeof DIR; pushP?: number; noseP?: number; quarter?: number; o?: number; nums?: boolean;
}> = ({ cx, cy, r, spin, push = "left", pushP = 0, noseP = 0, quarter = 0, o = 1, nums = true }) => {
  const pd = DIR[push], nd = DIR[NEXT[push]];
  const nose: P = [cx + nd[0] * r * 0.42 * noseP, cy + nd[1] * r * 0.42 * noseP];
  const a0 = spin * Math.PI * 2;
  const arc = (from: number, to: number, rr: number) => {
    const pts: P[] = [];
    for (let i = 0; i <= 24; i++) { const a = lerp(from, to, i / 24); pts.push([cx + rr * Math.cos(a), cy + rr * Math.sin(a)]); }
    return pts;
  };
  const sw = Math.max(1.5, r / 70);
  // the quarter arc: from the push direction to the result, clockwise on screen
  const ang = (d: P) => Math.atan2(d[1], d[0]);
  let qa = ang(pd), qb = ang(nd);
  if (qb < qa) qb += Math.PI * 2;
  return (
    <g opacity={o}>
      <circle cx={cx} cy={cy} r={r} fill="none" stroke={C.line} strokeWidth={sw * 1.4} />
      <circle cx={cx} cy={cy} r={r * 0.8} fill="none" stroke={C.line} strokeWidth={sw * 0.7} opacity={0.4} />
      {/* clockwise spin arrow, running */}
      <Pen pts={arc(a0, a0 + 1.6, r * 1.14)} p={1} w={sw * 1.2} tip={false} opacity={0.8} />
      {(() => {
        const a = a0 + 1.6, x = cx + r * 1.14 * Math.cos(a), y = cy + r * 1.14 * Math.sin(a), tx = -Math.sin(a), ty = Math.cos(a), h = r * 0.09;
        return <path d={`M${x - tx * h * 1.6 + ty * h},${y - ty * h * 1.6 - tx * h} L${x},${y} L${x - tx * h * 1.6 - ty * h},${y - ty * h * 1.6 + tx * h}`}
          stroke={C.line} strokeWidth={sw * 1.2} fill="none" opacity={0.8} strokeLinejoin="round" />;
      })()}
      {nums && (
        <g>
          {[["12", 0, -1], ["3", 1, 0], ["6", 0, 1], ["9", -1, 0]].map(([s, dx, dy]) => (
            <text key={s as string} x={cx + (dx as number) * r * 0.66} y={cy + (dy as number) * r * 0.66 + r * 0.07} textAnchor="middle"
              style={mono(0.5, r * 0.17)}>{s}</text>
          ))}
        </g>
      )}
      {quarter > 0 && <Pen pts={arc(qa, lerp(qa, qb, quarter), r * 0.92)} p={1} w={sw * 2.2} color={C.line} tip={false} />}
      {pushP > 0 && (
        <g filter="url(#glow)">
          <line x1={cx} y1={cy} x2={cx + pd[0] * r * 0.62 * pushP} y2={cy + pd[1] * r * 0.62 * pushP} stroke={C.drag} strokeWidth={sw * 2.2} strokeLinecap="round" />
          {pushP > 0.6 && (() => {
            const x = cx + pd[0] * r * 0.62 * pushP, y = cy + pd[1] * r * 0.62 * pushP, h = r * 0.09;
            return <path d={`M${x - pd[0] * h * 1.5 + pd[1] * h},${y - pd[1] * h * 1.5 - pd[0] * h} L${x},${y} L${x - pd[0] * h * 1.5 - pd[1] * h},${y - pd[1] * h * 1.5 + pd[0] * h}`}
              stroke={C.drag} strokeWidth={sw * 2.2} fill="none" strokeLinejoin="round" />;
          })()}
        </g>
      )}
      {noseP > 0 && <line x1={cx} y1={cy} x2={nose[0]} y2={nose[1]} stroke={C.line} strokeWidth={sw} strokeDasharray="4 6" opacity={0.6} />}
      <circle cx={nose[0]} cy={nose[1]} r={r * 0.075} fill={C.line} filter="url(#glowBig)" />
    </g>
  );
};

/** The rule as a corner badge for scenes 4 and 5. */
export const CornerRule: React.FC<{ spin: number; push: "left" | "up"; pushP: number; noseP: number; o: number; hot?: number }> =
  ({ spin, push, pushP, noseP, o, hot = 0 }) => (
    <g opacity={o}>
      <ClockBack cx={1760} cy={170} r={lerp(62, 78, hot)} spin={spin} push={push} pushP={pushP} noseP={noseP} nums={false} />
      <text x={1760} y={290} textAnchor="middle" style={label(0.55 + 0.4 * hot, { fontSize: 16 })}>FROM BEHIND</text>
    </g>
  );

// ---------------------------------------------------------------- a formula line with fractions
export type Item = { toks: Tok[] } | { num: Tok[]; den: Tok[] };
const tokW = (toks: Tok[], size: number) => toks.reduce((a, t) => a + [...t.s].length * size * 0.6 * (t.sub || t.sup ? 0.6 : 1), 0);
const itemW = (it: Item, size: number) => ("toks" in it ? tokW(it.toks, size) : Math.max(tokW(it.num, size * 0.82), tokW(it.den, size * 0.82)) + size * 0.5);
export type MathRun = { toks: Tok[]; cx: number; y: number; size: number; x0: number; w: number };
/** Lay out the items centred at cx: plain runs on the line, fractions as numerator / bar / denominator. */
export const layoutMath = (items: Item[], cx: number, y: number, size: number) => {
  const total = items.reduce((a, it) => a + itemW(it, size), 0);
  let x = cx - total / 2;
  const runs: MathRun[] = [], bars: { x0: number; x1: number; y: number }[] = [];
  for (const it of items) {
    const w = itemW(it, size), mid = x + w / 2;
    if ("toks" in it) runs.push({ toks: it.toks, cx: mid, y, size, x0: x, w });
    else {
      const fs = size * 0.82;
      runs.push({ toks: it.num, cx: mid, y: y - size * 0.62, size: fs, x0: mid - tokW(it.num, fs) / 2, w: tokW(it.num, fs) });
      runs.push({ toks: it.den, cx: mid, y: y + size * 0.62, size: fs, x0: mid - tokW(it.den, fs) / 2, w: tokW(it.den, fs) });
      bars.push({ x0: x + size * 0.15, x1: x + w - size * 0.15, y: y - size * 0.3 });
    }
    x += w;
  }
  return { runs, bars, x0: cx - total / 2, x1: cx + total / 2 };
};
/** Centre x of the n-th token with `key` in a laid-out formula. */
export const keyPos = (items: Item[], cx: number, y: number, size: number, key: string, nth = 1): P => {
  const { runs } = layoutMath(items, cx, y, size);
  let seen = 0;
  for (const r of runs) for (const t of layoutFormula(r.toks, r.cx, r.size)) if (t.key === key && ++seen === nth) return [t.cx, r.y - r.size * 0.3];
  return [cx, y];
};
export const MathLine: React.FC<{ items: Item[]; cx: number; y: number; size: number; reveal: number; active?: string | null; lit?: Set<string>; opacity?: number }> =
  ({ items, cx, y, size, reveal, active, lit, opacity = 1 }) => {
    const { runs, bars, x0, x1 } = layoutMath(items, cx, y, size);
    const rx = lerp(x0, x1, clamp01(reveal));
    return (
      <g opacity={opacity}>
        {runs.map((r, i) => (
          <Formula key={i} toks={r.toks} cx={r.cx} y={r.y} size={r.size} reveal={reveal >= 1 ? 1 : clamp01((rx - r.x0) / Math.max(1, r.w))} active={active} lit={lit} />
        ))}
        {bars.map((b, i) => <line key={i} x1={b.x0} y1={b.y} x2={lerp(b.x0, b.x1, clamp01((rx - b.x0) / (b.x1 - b.x0)))} y2={b.y} stroke={C.line} strokeWidth={2.5} />)}
      </g>
    );
  };

// ---------------------------------------------------------------- what the letters of a formula mean
export type LegendRow = { sym: React.ReactNode; text: string; at: number };
/** The letters of a formula, one row each, appearing as the voice names them and staying; the newest row is bright. */
export const Legend: React.FC<{ x: number; y: number; rows: LegendRow[]; v: number; o: number; size?: number; gap?: number }> =
  ({ x, y, rows, v, o, size = 20, gap = 38 }) => {
    const shown = rows.filter((r) => v >= r.at - 0.2).length;
    return (
      <g opacity={o}>
        {rows.map((r, i) => {
          const k = clamp01((v - (r.at - 0.2)) / 0.4), hot = i === shown - 1 ? 1 : 0;
          return (
            <g key={i} opacity={k * (0.6 + 0.4 * hot)}>
              <text x={x} y={y + i * gap} style={mono(1, size * 1.45, { fontWeight: 700 })}>{r.sym}</text>
              <text x={x + size * 5.6} y={y + i * gap} style={label(1, { fontSize: size })}>{r.text}</text>
            </g>
          );
        })}
      </g>
    );
  };
/** A subscript inside a legend symbol. */
export const Sub: React.FC<{ s: string }> = ({ s }) => <tspan fontSize="0.6em" dy="0.3em">{s}</tspan>;

// ---------------------------------------------------------------- the S_g gauge
export const Gauge: React.FC<{ cx: number; cy: number; r: number; value: number; o: number; p?: number }> = ({ cx, cy, r, value, o, p = 1 }) => {
  const MAX = 2.5, a = (v: number) => Math.PI * (1 - Math.min(v, MAX) / MAX);
  const pt = (v: number, rr: number): P => [cx + rr * Math.cos(a(v)), cy - rr * Math.sin(a(v))];
  const arc: P[] = Array.from({ length: 41 }, (_, i) => pt((MAX * i) / 40, r));
  const good: P[] = Array.from({ length: 21 }, (_, i) => pt(lerp(1.4, MAX, i / 20), r));
  return (
    <g opacity={o}>
      <Pen pts={arc} p={p} w={3} tip={false} opacity={0.6} />
      <Pen pts={good} p={p} w={7} tip={false} />
      {[1, 1.4].map((m) => (
        <g key={m}>
          <line x1={pt(m, r - 16)[0]} y1={pt(m, r - 16)[1]} x2={pt(m, r + 16)[0]} y2={pt(m, r + 16)[1]} stroke={C.line} strokeWidth={3} />
          <text x={pt(m, r + 44)[0]} y={pt(m, r + 44)[1] + 8} textAnchor="middle" style={mono(0.8, 26)}>{m}</text>
        </g>
      ))}
      <line x1={cx} y1={cy} x2={pt(value, r - 30)[0]} y2={pt(value, r - 30)[1]} stroke={C.line} strokeWidth={5} strokeLinecap="round" filter="url(#glow)" />
      <circle cx={cx} cy={cy} r={10} fill={C.line} />
      <text x={cx} y={cy + 56} textAnchor="middle" style={mono(1, 44, { fontWeight: 700 })}>S<tspan fontSize={26} dy={8}>g</tspan><tspan dy={-8}> = {value.toFixed(1)}</tspan></text>
    </g>
  );
};

// ---------------------------------------------------------------- the effect cards
/** A small bullet lying nose-right with a ring arrow around its body: "it spins about its long axis". */
export const SpinIcon: React.FC<{ x: number; y: number; o: number; s?: number }> = ({ x, y, o, s = 1 }) => {
  const ring: P[] = Array.from({ length: 25 }, (_, i) => { const a = lerp(-Math.PI / 2, Math.PI * 1.1, i / 24); return [-5 + 5 * Math.cos(a), 12 * Math.sin(a)]; });
  const e = ring[24], d = ring[22], ang = Math.atan2(e[1] - d[1], e[0] - d[0]), h = 5;
  return (
    <g transform={`translate(${x},${y}) scale(${s})`} opacity={o}>
      <path d="M-18,-5 L2,-5 Q13,-4 19,0 Q13,4 2,5 L-18,5 Z" fill="none" stroke={C.line} strokeWidth={1.8} strokeLinejoin="round" />
      <path d={`M${ring.map((p) => p.join(",")).join(" L")}`} fill="none" stroke={C.line} strokeWidth={1.8} />
      <path d={`M${e[0] - h * Math.cos(ang - 0.55)},${e[1] - h * Math.sin(ang - 0.55)} L${e[0]},${e[1]} L${e[0] - h * Math.cos(ang + 0.55)},${e[1] - h * Math.sin(ang + 0.55)}`}
        fill="none" stroke={C.line} strokeWidth={1.8} strokeLinejoin="round" />
    </g>
  );
};
export const EFFECTS = ["SPIN DRIFT", "AERODYNAMIC JUMP", "CORIOLIS", "EAST – WEST"];
export const Card: React.FC<{ x: number; y: number; w: number; text: string; o: number; hot?: number; icon?: "spin" | "earth" }> = ({ x, y, w, text, o, hot = 0, icon }) => (
  <g opacity={o}>
    <rect x={x - w / 2} y={y - 34} width={w} height={68} rx={10} fill={C.bg} stroke={C.line} strokeWidth={2 + 1.5 * hot} filter={hot > 0.3 ? "url(#glow)" : undefined} />
    <text x={x + (icon ? 18 : 0)} y={y + 8} textAnchor="middle" style={label(0.7 + 0.3 * hot, { fontSize: 22 })}>{text}</text>
    {icon === "spin" && <SpinIcon x={x - w / 2 + 34} y={y} o={0.5 + 0.5 * hot} />}
    {icon === "earth" && (
      <g transform={`translate(${x - w / 2 + 30},${y})`} opacity={0.5 + 0.5 * hot}>
        <circle r={16} fill="none" stroke={C.line} strokeWidth={2} />
        <ellipse rx={7} ry={16} fill="none" stroke={C.line} strokeWidth={1.5} />
        <line x1={-16} y1={0} x2={16} y2={0} stroke={C.line} strokeWidth={1.5} />
      </g>
    )}
  </g>
);

// ---------------------------------------------------------------- the globe, side view, turning
/** A line globe; `rot` turns its meridians (turns). `tiltDeg` tips the north pole towards us. */
export const Globe: React.FC<{ cx: number; cy: number; r: number; rot: number; o?: number; lats?: boolean }> = ({ cx, cy, r, rot, o = 1, lats = true }) => (
  <g opacity={o}>
    <circle cx={cx} cy={cy} r={r} fill="none" stroke={C.line} strokeWidth={3} />
    {lats && [-60, -30, 0, 30, 60].map((d) => {
      const a = (d * Math.PI) / 180;
      return <ellipse key={d} cx={cx} cy={cy - r * Math.sin(a)} rx={r * Math.cos(a)} ry={r * Math.cos(a) * 0.16} fill="none" stroke={C.line} strokeWidth={d === 0 ? 2 : 1.2} opacity={d === 0 ? 0.6 : 0.3} />;
    })}
    {Array.from({ length: 6 }, (_, i) => {
      const ph = (((rot + i / 6) % 1) + 1) % 1;               // 0..1 around
      const rx = r * Math.abs(Math.cos(ph * Math.PI * 2));
      const front = Math.sin(ph * Math.PI * 2) > 0;
      return <ellipse key={i} cx={cx} cy={cy} rx={Math.max(0.5, rx)} ry={r} fill="none" stroke={C.line} strokeWidth={1.2} opacity={front ? 0.4 : 0.12} />;
    })}
    <line x1={cx} y1={cy - r * 1.18} x2={cx} y2={cy + r * 1.18} stroke={C.line} strokeWidth={1.5} strokeDasharray="6 8" opacity={0.5} />
  </g>
);

/** A small curved "turning" arrow around a vertical axis at (x, y), its width `k` (0..1), running at phase `ph`. */
export const TurnArrow: React.FC<{ x: number; y: number; rx: number; k: number; ph: number; o?: number; w?: number }> = ({ x, y, rx, k, ph, o = 1, w = 3 }) => {
  if (k < 0.02) return <g opacity={o}><circle cx={x} cy={y} r={4} fill={C.line} /></g>;
  const ry = rx * 0.32, pts: P[] = [];
  for (let i = 0; i <= 30; i++) { const a = ph * Math.PI * 2 + (i / 30) * Math.PI * 1.5; pts.push([x + rx * k * Math.cos(a), y + ry * Math.sin(a)]); }
  const e = pts[pts.length - 1], d = pts[pts.length - 2], ang = Math.atan2(e[1] - d[1], e[0] - d[0]), h = 10;
  return (
    <g opacity={o}>
      <Pen pts={pts} p={1} w={w} tip={false} />
      <path d={`M${e[0] - h * Math.cos(ang - 0.5)},${e[1] - h * Math.sin(ang - 0.5)} L${e[0]},${e[1]} L${e[0] - h * Math.cos(ang + 0.5)},${e[1] - h * Math.sin(ang + 0.5)}`}
        stroke={C.line} strokeWidth={w} fill="none" />
    </g>
  );
};

export const Title: React.FC<{ text: string; o: number; y?: number; size?: number }> = ({ text, o, y = 120, size = 34 }) => (
  <text x={960} y={y} textAnchor="middle" style={label(o, { fontSize: size, letterSpacing: "0.3em" })}>{text}</text>
);
