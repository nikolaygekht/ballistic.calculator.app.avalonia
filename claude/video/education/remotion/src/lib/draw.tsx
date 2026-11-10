// Shared line-art drawing kit for the education episodes.
import React from "react";
import { C, FONT, MONO, clamp01, lerp } from "../theme";

export type P = [number, number];

export const segLen = (a: P, b: P) => Math.hypot(b[0] - a[0], b[1] - a[1]);
export const pathD = (pts: P[], close = false) =>
  pts.length ? "M" + pts.map((q) => `${q[0].toFixed(1)},${q[1].toFixed(1)}`).join(" L") + (close ? " Z" : "") : "";

/** The first `p` (0..1) of a polyline, and the pen tip at its end. */
export const partial = (pts: P[], p: number): { d: string; tip: P; pts: P[] } => {
  const total = pts.slice(1).reduce((s, q, i) => s + segLen(pts[i], q), 0);
  let left = clamp01(p) * total;
  const out: P[] = [pts[0]];
  for (let i = 1; i < pts.length; i++) {
    const l = segLen(pts[i - 1], pts[i]);
    if (left >= l) { out.push(pts[i]); left -= l; continue; }
    const k = l === 0 ? 0 : left / l;
    out.push([lerp(pts[i - 1][0], pts[i][0], k), lerp(pts[i - 1][1], pts[i][1], k)]);
    break;
  }
  return { d: pathD(out), tip: out[out.length - 1], pts: out };
};

/** A polyline drawn on by `p`, with a glowing pen tip while it is being drawn. */
export const Pen: React.FC<{ pts: P[]; p: number; color?: string; w?: number; tip?: boolean; dash?: string; opacity?: number }> =
  ({ pts, p, color = C.line, w = 3, tip = true, dash, opacity = 1 }) => {
    if (p <= 0 || pts.length < 2) return null;
    const r = partial(pts, p);
    return (
      <g opacity={opacity}>
        <path d={r.d} stroke={color} strokeWidth={w} fill="none" strokeLinecap="round" strokeLinejoin="round" strokeDasharray={dash} />
        {tip && p < 1 && <circle cx={r.tip[0]} cy={r.tip[1]} r={w + 3} fill={color} filter="url(#glow)" />}
      </g>
    );
  };

export const Arrow: React.FC<{ x1: number; y1: number; x2: number; y2: number; p?: number; color?: string; w?: number; head?: number; opacity?: number }> =
  ({ x1, y1, x2, y2, p = 1, color = C.line, w = 3, head = 16, opacity = 1 }) => {
    if (p <= 0 || (Math.abs(x2 - x1) < 0.5 && Math.abs(y2 - y1) < 0.5)) return null;
    const x = lerp(x1, x2, p), y = lerp(y1, y2, p);
    const ang = Math.atan2(y - y1, x - x1);
    const hl = Math.min(head, Math.hypot(x - x1, y - y1) * 0.6);
    const h = (s: number) => `${(x - hl * Math.cos(ang + s)).toFixed(1)},${(y - hl * Math.sin(ang + s)).toFixed(1)}`;
    return (
      <g stroke={color} strokeWidth={w} strokeLinecap="round" strokeLinejoin="round" fill="none" opacity={opacity}>
        <line x1={x1} y1={y1} x2={x} y2={y} />
        <polyline points={`${h(0.45)} ${x},${y} ${h(-0.45)}`} />
      </g>
    );
  };

export const label = (o: number, extra: React.CSSProperties = {}): React.CSSProperties => ({
  fontFamily: FONT, fontWeight: 300, letterSpacing: "0.18em", fill: C.line, opacity: o, ...extra,
});
export const mono = (o: number, size: number, extra: React.CSSProperties = {}): React.CSSProperties => ({
  fontFamily: MONO, fontSize: size, fill: C.line, opacity: o, ...extra,
});

/** Filters and gradients every scene uses. */
export const Defs: React.FC = () => (
  <defs>
    <filter id="glow" x="-50%" y="-50%" width="200%" height="200%">
      <feGaussianBlur stdDeviation="5" result="b" />
      <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
    </filter>
    <filter id="glowBig" x="-50%" y="-50%" width="200%" height="200%">
      <feGaussianBlur stdDeviation="10" result="b" />
      <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
    </filter>
    <radialGradient id="vig" cx="50%" cy="45%" r="75%">
      <stop offset="60%" stopColor="#000" stopOpacity="0" />
      <stop offset="100%" stopColor="#000" stopOpacity="0.55" />
    </radialGradient>
    <pattern id="hatch" width="12" height="12" patternUnits="userSpaceOnUse" patternTransform="rotate(45)">
      <line x1="0" y1="0" x2="0" y2="12" stroke={C.line} strokeWidth="2" strokeOpacity="0.5" />
    </pattern>
    <pattern id="hatchDrag" width="12" height="12" patternUnits="userSpaceOnUse" patternTransform="rotate(45)">
      <line x1="0" y1="0" x2="0" y2="12" stroke={C.drag} strokeWidth="2" strokeOpacity="0.45" />
    </pattern>
  </defs>
);

export const Vignette: React.FC = () => <rect x={0} y={0} width={1920} height={1080} fill="url(#vig)" pointerEvents="none" />;

// ---------------------------------------------------------------- bullets
export type Shape = { len: number; noseLen: number; btLen: number; rBase: number; meplat: number; flatNose?: number };
/** 175 gr Sierra MatchKing / Lapua Scenar-L proportions (4.53 calibres, tangent ogive, boat tail). */
export const SMK: Shape = { len: 1.394 / 0.308, noseLen: 2.53, btLen: 0.65, rBase: 0.4, meplat: 0.035 };
/** Standard G1: short, flat base. Standard G7: long secant-like ogive and a 7.5° boat tail. */
export const G1S: Shape = { len: 3.28, noseLen: 1.32, btLen: 0, rBase: 0.5, meplat: 0.05 };
export const G7S: Shape = { len: 4.23, noseLen: 2.18, btLen: 0.6, rBase: 0.42, meplat: 0.04 };
/** A blunt flat-nosed slug. */
export const FLAT: Shape = { len: 3.2, noseLen: 0.25, btLen: 0, rBase: 0.5, meplat: 0.42, flatNose: 1 };

/** Radius (calibres) at axial position x (calibres from the base). */
export const radius = (s: Shape, x: number) => {
  if (x < s.btLen) return lerp(s.rBase, 0.5, x / s.btLen);
  const x0 = s.len - s.noseLen;
  if (x < x0) return 0.5;
  const u = x - x0;
  if (s.flatNose) return lerp(0.5, s.meplat, Math.pow(u / s.noseLen, 2));
  const rho = (s.noseLen * s.noseLen + 0.25) / (2 * 0.5);
  return Math.max(s.meplat, Math.sqrt(Math.max(0, rho * rho - u * u)) - (rho - 0.5));
};
export const mixShape = (a: Shape, b: Shape, k: number): Shape => ({
  len: lerp(a.len, b.len, k), noseLen: lerp(a.noseLen, b.noseLen, k), btLen: lerp(a.btLen, b.btLen, k),
  rBase: lerp(a.rBase, b.rBase, k), meplat: lerp(a.meplat, b.meplat, k),
});
/** Radius of a blend of two shapes at the same fraction of their lengths. */
const radiusMix = (a: Shape, b: Shape, k: number, f: number) => lerp(radius(a, f * a.len), radius(b, f * b.len), k);

/** Closed outline, starting at the top of the base. `x0` is the base, `cal` px per calibre, pointing right. */
export const outline = (s: Shape, x0: number, y: number, cal: number, n = 120, morph?: { to: Shape; k: number }): P[] => {
  const len = morph ? lerp(s.len, morph.to.len, morph.k) : s.len;
  const r = (f: number) => (morph ? radiusMix(s, morph.to, morph.k, f) : radius(s, f * s.len));
  const fs: number[] = [];
  for (let i = 0; i <= n; i++) fs.push(i / n);
  const top = fs.map((f) => [x0 + f * len * cal, y - r(f) * cal] as P);
  const bot = fs.slice().reverse().map((f) => [x0 + f * len * cal, y + r(f) * cal] as P);
  return [...top, ...bot, top[0]];
};

/** A bullet drawn as one line. `turn` 0 = side view, 1 = looking straight at the nose (fake 3D). */
export const Bullet: React.FC<{
  s?: Shape; x0: number; y: number; cal: number; p?: number; turn?: number; color?: string; w?: number;
  fill?: string; fillLevel?: number; id?: string; morph?: { to: Shape; k: number }; cannelure?: boolean; opacity?: number;
}> = ({ s = SMK, x0, y, cal, p = 1, turn = 0, color = C.line, w = 3, fill, fillLevel = 0, id = "b", morph, cannelure = true, opacity = 1 }) => {
  const len = morph ? lerp(s.len, morph.to.len, morph.k) : s.len;
  const th = (turn * Math.PI) / 2, cs = Math.cos(th), sn = Math.sin(th);
  const mid = x0 + (len * cal) / 2;
  const pts = outline(s, x0, y, cal, 120, morph).map(([x, yy]) => [mid + (x - mid) * cs, yy] as P);
  const sideO = clamp01(1 - (turn - 0.75) / 0.2);
  const ring = (rCal: number, xCal: number, o: number) => (
    <ellipse cx={mid + (x0 + xCal * cal - mid) * cs} cy={y} rx={Math.max(0.01, rCal * cal * sn)} ry={rCal * cal}
      stroke={color} strokeWidth={w * 0.8} fill="none" opacity={o} />
  );
  return (
    <g opacity={opacity}>
      {fill && fillLevel > 0 && (
        <>
          <clipPath id={`fill-${id}`}>
            <rect x={x0 - 10} y={y + cal * 0.5 - fillLevel * cal} width={len * cal + 20} height={fillLevel * cal + 2} />
          </clipPath>
          <path d={pathD(pts, true)} fill={fill} clipPath={`url(#fill-${id})`} stroke="none" />
        </>
      )}
      <g opacity={sideO}><Pen pts={pts} p={p} color={color} w={w} /></g>
      {cannelure && p >= 1 && turn < 0.05 && !morph && (
        <line x1={x0 + 1.55 * cal} y1={y - 0.5 * cal} x2={x0 + 1.55 * cal} y2={y + 0.5 * cal} stroke={color} strokeWidth={1.5} opacity={0.6} />
      )}
      {turn > 0.02 && p >= 1 && (
        <>
          {ring(s.rBase, 0, clamp01(turn * 3) * 0.9)}
          {ring(0.5, s.btLen + 0.05, clamp01(turn * 3))}
          {ring(0.25, len - s.noseLen * 0.35, clamp01((turn - 0.4) * 3) * 0.5)}
          {turn > 0.9 && <circle cx={mid} cy={y} r={Math.max(2, s.meplat * cal)} fill={color} opacity={(turn - 0.9) * 10} />}
        </>
      )}
    </g>
  );
};

// ---------------------------------------------------------------- seeded random
export const seeded = (seed: number) => {
  let s = seed >>> 0;
  return () => ((s = (s * 1664525 + 1013904223) >>> 0) / 4294967296);
};

// ---------------------------------------------------------------- formula, written term by term
/** `vec` draws a small arrow over the token: the letter is an arrow (three numbers), not one number. */
export type Tok = { s: string; key?: string; color?: string; bold?: boolean; sub?: boolean; sup?: boolean; vec?: boolean };
/** Monospace formula: every token sits at a computed x, so connectors and brackets can find it. */
export const layoutFormula = (toks: Tok[], cx: number, size: number) => {
  const cw = size * 0.6;
  const wOf = (t: Tok) => [...t.s].length * cw * (t.sub || t.sup ? 0.6 : 1);
  const n = toks.reduce((a, t) => a + wOf(t), 0);
  let x = cx - n / 2;
  return toks.map((t) => {
    const w = wOf(t);
    const r = { ...t, x, w, cx: x + w / 2 };
    x += w;
    return r;
  });
};
export const Formula: React.FC<{
  toks: Tok[]; cx: number; y: number; size: number; reveal: number; active?: string | null; lit?: Set<string>; opacity?: number;
}> = ({ toks, cx, y, size, reveal, active, lit, opacity = 1 }) => {
  const L = layoutFormula(toks, cx, size);
  const x0 = L[0].x, x1 = L[L.length - 1].x + L[L.length - 1].w;
  const id = `fx-${Math.round(cx)}-${Math.round(y)}-${[...toks.map((t) => t.s).join("")].reduce((h, c) => (h * 31 + c.charCodeAt(0)) >>> 0, 7)}`;
  const rx = lerp(x0 - 4, x1 + 4, clamp01(reveal));
  return (
    <g opacity={opacity}>
      <clipPath id={id}><rect x={x0 - 20} y={y - size * 1.4} width={rx - x0 + 20} height={size * 2} /></clipPath>
      <g clipPath={`url(#${id})`}>
        {L.map((t, i) => {
          const isA = !!t.key && t.key === active;
          const isLit = !t.key || (lit?.has(t.key) ?? true);
          const o = isA ? 1 : isLit ? 0.95 : 0.4, fs = t.sub || t.sup ? size * 0.6 : size, ty = t.sub ? y + size * 0.22 : t.sup ? y - size * 0.42 : y;
          const ay = ty - fs * 0.9, ax0 = t.x + fs * 0.06, ax1 = t.x + t.w - fs * 0.04, hh = fs * 0.1;
          return (
            <g key={i} filter={isA ? "url(#glow)" : undefined}>
              <text x={t.x} y={ty} style={mono(o, fs, { fill: t.color ?? C.line, fontWeight: t.bold || isA ? 700 : 400 })} xmlSpace="preserve">{t.s}</text>
              {t.vec && (
                <path d={`M${ax0},${ay} L${ax1},${ay} M${ax1 - hh * 1.3},${ay - hh} L${ax1},${ay} L${ax1 - hh * 1.3},${ay + hh}`}
                  stroke={t.color ?? C.line} strokeWidth={Math.max(1.5, fs * (isA ? 0.06 : 0.045))} fill="none" strokeLinecap="round" strokeLinejoin="round" opacity={o} />
              )}
            </g>
          );
        })}
      </g>
      {reveal > 0 && reveal < 1 && <circle cx={rx} cy={y - size * 0.35} r={6} fill={C.line} filter="url(#glow)" />}
    </g>
  );
};

/** A soft bracket under [x0, x1] at y, drawn on by p. */
export const Bracket: React.FC<{ x0: number; x1: number; y: number; p: number; color?: string; down?: number }> =
  ({ x0, x1, y, p, color = C.line, down = 14 }) => (
    <Pen pts={[[x0, y], [x0, y + down * 0.6], [(x0 + x1) / 2 - 10, y + down * 0.6], [(x0 + x1) / 2, y + down * 1.3], [(x0 + x1) / 2 + 10, y + down * 0.6], [x1, y + down * 0.6], [x1, y]]}
      p={p} color={color} w={2} tip={false} />
  );
