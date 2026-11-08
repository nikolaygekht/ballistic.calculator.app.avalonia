// Pilot for episode 1 (drag): how drag slows a flying bullet. Deceleration only — gravity and the fall
// belong to episode 2 (3DOF), where the bullet moves along its trajectory.
// Every moving number is the library's: speed, Mach, Cd and the drag deceleration per 5 m (public/data.json).
import React from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import { D, G, ROWS, State, T_END, T_MACH1, stateAt } from "./data";
import { C, FONT, FPS, MONO, W, clamp01, ease, lerp, ramp, window01 } from "./theme";

export const PILOT_SECONDS = 22;
const LAST = ROWS[ROWS.length - 1];

// ---------------------------------------------------------------- film time -> flight time
// The intro holds the bullet frozen at the muzzle; the flight plays from F0 to F1 in slow motion,
// slower still around Mach 1, then the air drifts on while the end frame holds.
const PF0 = 6.2, PF1 = 18.2;
const TAU_GRID = (() => {
  const n = 2000, slow = (tau: number) => 1 + 1.8 * Math.exp(-(((tau - T_MACH1) / 0.22) ** 2));
  const s: number[] = [0];
  for (let i = 1; i <= n; i++) s.push(s[i - 1] + slow((i - 0.5) * T_END / n));
  return s.map((v) => v / s[n]);                         // normalised film progress at tau = i*T_END/n
})();
const gridTau = (f: number, F0: number, F1: number) => {
  if (f <= F0) return 0;
  if (f >= F1) return T_END + (f - F1) * 0.06;
  const p = (f - F0) / (F1 - F0);
  let lo = 0, hi = TAU_GRID.length - 1;
  while (hi - lo > 1) { const m = (lo + hi) >> 1; if (TAU_GRID[m] <= p) lo = m; else hi = m; }
  const k = (p - TAU_GRID[lo]) / (TAU_GRID[hi] - TAU_GRID[lo]);
  return ((lo + k) / (TAU_GRID.length - 1)) * T_END;
};
const gridFilm = (tau: number) => {                       // inverse, for cueing the Mach 1 moment
  const i = Math.round((tau / T_END) * (TAU_GRID.length - 1));
  return PF0 + TAU_GRID[i] * (PF1 - PF0);
};

/** Film timing. Without `fm` the flight plays on the pilot's slow-motion grid; with it, the bullet crosses
 *  Mach 1 exactly at film time `fm` (the episode keys it to the voice). `intro` shifts the intro beats. */
export type DecelTiming = { f0: number; f1: number; fm?: number; capMuzzle?: [number, number]; intro?: number };
const makeTau = (tm: DecelTiming) => (f: number) => {
  if (tm.fm === undefined) return gridTau(f, tm.f0, tm.f1);
  if (f <= tm.f0) return 0;
  if (f >= tm.f1) return T_END + (f - tm.f1) * 0.06;
  if (f <= tm.fm) return ((f - tm.f0) / (tm.fm - tm.f0)) * T_MACH1;
  return T_MACH1 + ((f - tm.fm) / (tm.f1 - tm.fm)) * (T_END - T_MACH1);
};

// ---------------------------------------------------------------- geometry helpers
type P = [number, number];
const segLen = (a: P, b: P) => Math.hypot(b[0] - a[0], b[1] - a[1]);
/** The first `p` (0..1) of a polyline, and the pen tip at its end. */
const partial = (pts: P[], p: number): { d: string; tip: P } => {
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
  return { d: "M" + out.map((q) => `${q[0].toFixed(1)},${q[1].toFixed(1)}`).join(" L"), tip: out[out.length - 1] };
};

// ---------------------------------------------------------------- the bullet (175 gr SMK proportions)
const AXIS = 380, CAL = 72, BASE_X = 610;
const LEN = 1.394 / 0.308;                                 // 4.53 calibres
const NOSE_X = BASE_X + LEN * CAL;
const BULLET: P[] = (() => {
  const og = 2.53, r0 = 0.5, rog = (og * og + r0 * r0) / (2 * r0), x0 = LEN - og;
  const r = (x: number) => {
    if (x < 0.65) return lerp(0.4, 0.5, x / 0.65);         // boat tail
    if (x < x0) return 0.5;                                // shank
    const u = x - x0;                                      // tangent ogive
    return Math.max(0.035, Math.sqrt(rog * rog - u * u) - (rog - r0));
  };
  const xs: number[] = [];
  for (let i = 0; i <= 120; i++) xs.push((LEN * i) / 120);
  const top = xs.map((x) => [BASE_X + x * CAL, AXIS - r(x) * CAL] as P);
  const bot = xs.slice().reverse().map((x) => [BASE_X + x * CAL, AXIS + r(x) * CAL] as P);
  return [...top, [NOSE_X, AXIS + 0.035 * CAL], ...bot.slice(1), top[0]];
})();
const CANNELURE_X = BASE_X + 1.55 * CAL;

// ---------------------------------------------------------------- the air (seeded, world-fixed streaks)
const rnd = (() => { let s = 20261107; return () => ((s = (s * 1664525 + 1013904223) >>> 0) / 4294967296); })();
const STREAKS = Array.from({ length: 54 }, () => {
  let y = 160 + rnd() * 440;
  if (Math.abs(y - AXIS) < 52) y += y < AXIS ? -52 : 52;
  return { y, x: rnd() * 2600, w: 0.5 + rnd() * 0.9, a: 0.12 + rnd() * 0.3, d: rnd() * 0.8 };
});
const PX_PER_M = 6;

// ---------------------------------------------------------------- arrows
const Arrow: React.FC<{ x1: number; y1: number; x2: number; y2: number; p: number; color: string; w?: number; head?: number }> =
  ({ x1, y1, x2, y2, p, color, w = 3, head = 16 }) => {
    if (p <= 0) return null;
    const x = lerp(x1, x2, p), y = lerp(y1, y2, p);
    const ang = Math.atan2(y - y1, x - x1);
    const h = (s: number) => `${(x - head * Math.cos(ang + s)).toFixed(1)},${(y - head * Math.sin(ang + s)).toFixed(1)}`;
    return (
      <g stroke={color} strokeWidth={w} strokeLinecap="round" strokeLinejoin="round" fill="none">
        <line x1={x1} y1={y1} x2={x} y2={y} />
        <polyline points={`${h(0.45)} ${x},${y} ${h(-0.45)}`} />
      </g>
    );
  };

// ---------------------------------------------------------------- charts
const SPEED = { x0: 140, x1: 860, y0: 1000, y1: 815, xMax: 1600, yMax: 850 };
const sx = (m: number) => lerp(SPEED.x0, SPEED.x1, m / SPEED.xMax);
const sy = (v: number) => lerp(SPEED.y0, SPEED.y1, v / SPEED.yMax);
const CDC = { x0: 1060, x1: 1780, y0: 1000, y1: 815, mMin: 0.5, mMax: 2.6, cdMax: 0.45 };
const cx = (m: number) => lerp(CDC.x0, CDC.x1, (m - CDC.mMin) / (CDC.mMax - CDC.mMin));
const cy = (cd: number) => lerp(CDC.y0, CDC.y1, cd / CDC.cdMax);
const CD_PTS: P[] = D.cdCurve.map((p) => [cx(p.mach), cy(p.cd)] as P);

const Axes: React.FC<{ x0: number; x1: number; y0: number; y1: number; p: number }> = ({ x0, x1, y0, y1, p }) => (
  <g stroke={C.dim} strokeWidth={1.5} fill="none" strokeLinecap="round">
    <path d={partial([[x0, y1 - 10], [x0, y0], [x1 + 10, y0]], p).d} />
  </g>
);

const label = (o: number, extra: React.CSSProperties = {}): React.CSSProperties => ({
  fontFamily: FONT, fontWeight: 300, letterSpacing: "0.18em", fill: C.line, opacity: o, ...extra,
});

// ---------------------------------------------------------------- the film
export const DecelPilot: React.FC = () => <Decel f0={PF0} f1={PF1} />;

export const Decel: React.FC<DecelTiming> = (tm) => {
  const F0 = tm.f0, F1 = tm.f1, F_MACH1 = tm.fm ?? gridFilm(T_MACH1);
  const cap = tm.capMuzzle ?? [4.5, 6.6];
  const f = useCurrentFrame() / FPS - (tm.intro ?? 0);
  const tau = makeTau(tm)(f + (tm.intro ?? 0));
  const s: State = stateAt(tau);
  const live = f + (tm.intro ?? 0) >= F0;
  const g = s.drag / G;

  // intro choreography
  const pBullet = ramp(f, 0.2, 1.7);
  const pRuler = ramp(f, 0.7, 1.4);
  const pVel = ramp(f, 2.2, 0.8, ease);
  const pDrag = ramp(f, 3.0, 0.9, ease);
  const pCone = ramp(f, 3.4, 0.8);
  const pAxes = ramp(f, 4.6, 1.0);
  const pCd = ramp(f, 5.0, 1.0);
  const oText = (a: number, d = 0.5) => ramp(f, a, d);

  // the shock cone: angle asin(1/M); it stands off and lets go as the bullet nears Mach 1
  const mu = s.mach > 1 ? Math.asin(1 / s.mach) : Math.PI / 2;
  const stand = clamp01((1.1 - s.mach) / 0.1) * 70;
  const coneO = clamp01((s.mach - 0.985) / 0.08);
  const LC = 560;
  const cone = (x: number, y: number, r: number, k: number): P[] => [
    [x - LC * k * Math.cos(mu), y - r - LC * k * Math.sin(mu)], [x, y - r], [x, y + r], [x - LC * k * Math.cos(mu), y + r + LC * k * Math.sin(mu)],
  ];
  const bow = cone(NOSE_X + stand, AXIS, 0, 1);
  const tail = cone(BASE_X, AXIS, 0.4 * CAL, 0.8);

  // the Mach 1 ring
  const fa = f + (tm.intro ?? 0);
  const ringP = clamp01((fa - F_MACH1) / 1.1);
  const ringO = fa >= F_MACH1 ? (1 - ringP) * 0.9 : 0;

  // arrow lengths are proportional to the real numbers
  const velLen = s.v * 0.62;
  const dragLen = g * 11;                                  // 1 g = 11 px
  const travelled = ROWS.filter((r) => r.x <= s.x + 0.01);
  const speedPts: P[] = travelled.map((r) => [sx(r.x), sy(r.v)] as P);
  if (live && s.x > 0 && tau < T_END) speedPts.push([sx(s.x), sy(s.v)]);
  const cdDone: P[] = [[cx(s.mach), cy(s.cd)] as P].concat(
    D.cdCurve.filter((p) => p.mach > s.mach && p.mach < ROWS[0].mach).map((p) => [cx(p.mach), cy(p.cd)] as P),
    [[cx(ROWS[0].mach), cy(ROWS[0].cd)] as P]);

  const readouts: [string, string][] = [
    ["TIME", `${tau.toFixed(3)} s`],
    ["DISTANCE", `${Math.round(Math.min(s.x, ROWS[ROWS.length - 1].x))} m`],
    ["VELOCITY", `${Math.round(s.v)} m/s`],
    ["MACH", s.mach.toFixed(2)],
    ["DRAG", `${g.toFixed(1)} g`],
  ];

  return (
    <AbsoluteFill style={{ background: C.bg }}>
      <svg width={W} height={1080} viewBox="0 0 1920 1080">
        <defs>
          <filter id="glow" x="-50%" y="-50%" width="200%" height="200%">
            <feGaussianBlur stdDeviation="5" result="b" />
            <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
          </filter>
          <radialGradient id="vig" cx="50%" cy="45%" r="75%">
            <stop offset="60%" stopColor="#000" stopOpacity="0" />
            <stop offset="100%" stopColor="#000" stopOpacity="0.55" />
          </radialGradient>
          <clipPath id="stage"><rect x="0" y="120" width="1920" height="510" /></clipPath>
        </defs>

        {/* ---- the air: still air, so it streams past at the bullet's own speed */}
        <g clipPath="url(#stage)" strokeLinecap="round">
          {STREAKS.map((k, i) => {
            const span = 2600;
            const x = ((((k.x - s.x * PX_PER_M) % span) + span) % span) - 300;
            const len = (40 + s.v * 0.26) * (0.6 + k.w * 0.5) * ramp(f, 1.0 + k.d, 0.8);
            return <line key={i} x1={x} y1={k.y} x2={x + len} y2={k.y} stroke={C.line} strokeOpacity={k.a} strokeWidth={k.w * 1.6} />;
          })}
        </g>

        {/* ---- shock waves: bow from the nose, a weaker one from the base */}
        <g fill="none" strokeLinecap="round" strokeLinejoin="round" opacity={coneO} clipPath="url(#stage)">
          <path d={partial(bow.slice(0, 2), pCone).d} stroke={C.line} strokeWidth={2} />
          <path d={partial([bow[3], bow[2]], pCone).d} stroke={C.line} strokeWidth={2} />
          <path d={partial(tail.slice(0, 2), pCone).d} stroke={C.dim} strokeWidth={1.5} />
          <path d={partial([tail[3], tail[2]], pCone).d} stroke={C.dim} strokeWidth={1.5} />
        </g>
        {ringO > 0 && (
          <circle cx={NOSE_X} cy={AXIS} r={40 + ringP * 520} fill="none" stroke={C.drag} strokeWidth={2} opacity={ringO} />
        )}

        {/* ---- the bullet, drawn as one line */}
        <g fill="none" stroke={C.line} strokeWidth={3} strokeLinecap="round" strokeLinejoin="round">
          <path d={partial(BULLET, pBullet).d} />
          <line x1={CANNELURE_X} y1={AXIS - 0.5 * CAL} x2={CANNELURE_X} y2={lerp(AXIS - 0.5 * CAL, AXIS + 0.5 * CAL, ramp(f, 1.9, 0.4))} strokeWidth={1.5} opacity={0.6} />
        </g>
        {pBullet > 0 && pBullet < 1 && (
          <circle cx={partial(BULLET, pBullet).tip[0]} cy={partial(BULLET, pBullet).tip[1]} r={6} fill={C.line} filter="url(#glow)" />
        )}

        {/* ---- forces and velocity, to scale */}
        <Arrow x1={NOSE_X + 24} y1={AXIS} x2={NOSE_X + 24 + velLen} y2={AXIS} p={pVel} color={C.line} />
        <text x={NOSE_X + 24 + velLen * pVel} y={AXIS - 26} textAnchor="end" style={label(oText(2.8), { fontSize: 22 })}>
          VELOCITY · {Math.round(s.v)} m/s
        </text>
        <g filter="url(#glow)">
          <Arrow x1={BASE_X - 24} y1={AXIS} x2={BASE_X - 24 - dragLen} y2={AXIS} p={pDrag} color={C.drag} w={4} head={20} />
        </g>
        <text x={BASE_X - 30} y={AXIS - 26} textAnchor="end" style={label(oText(3.6), { fontSize: 22, fill: C.drag })}>
          DRAG · {g.toFixed(1)} g
        </text>

        {/* ---- the ground: a distance ruler sliding under the bullet */}
        <g opacity={pRuler}>
          <line x1={0} y1={640} x2={W * pRuler} y2={640} stroke={C.dim} strokeWidth={1.5} />
          {Array.from({ length: 80 }, (_, i) => i * 50).map((m) => {
            const x = NOSE_X + (m - s.x) * PX_PER_M;
            if (x < -60 || x > W + 60) return null;
            const big = m % 100 === 0;
            return (
              <g key={m}>
                <line x1={x} y1={640} x2={x} y2={big ? 656 : 648} stroke={C.dim} strokeWidth={1.5} />
                {big && <text x={x} y={684} textAnchor="middle" style={{ fontFamily: MONO, fontSize: 18, fill: C.dim }}>{m} m</text>}
              </g>
            );
          })}
          <polyline points={`${NOSE_X - 9},628 ${NOSE_X},638 ${NOSE_X + 9},628`} stroke={C.line} strokeWidth={2} fill="none" />
        </g>

        {/* ---- the formula, linked to the two charts */}
        <g opacity={ramp(f, 4.4, 0.8)}>
          <text x={960} y={738} textAnchor="middle" style={{ fontFamily: MONO, fontSize: 30, fill: C.line }}>
            <tspan fill={C.drag}>drag</tspan> = ½ ρ · <tspan fontWeight={700}>v²</tspan> · <tspan fill={C.drag} fontWeight={700}>Cd</tspan> · A / m
          </text>
          <path d={partial([[930, 750], [930, 760], [440, 760], [440, 768]], ramp(f, 5.0, 0.8)).d} stroke={C.faint} strokeWidth={1.5} fill="none" />
          <path d={partial([[1034, 750], [1034, 760], [1240, 760], [1240, 768]], ramp(f, 5.0, 0.8)).d} stroke={C.dragDim} strokeWidth={1.5} fill="none" />
        </g>

        {/* ---- speed vs distance, drawn as it happens */}
        <Axes x0={SPEED.x0} x1={SPEED.x1} y0={SPEED.y0} y1={SPEED.y1} p={pAxes} />
        <g opacity={pAxes}>
          <text x={SPEED.x0} y={SPEED.y1 - 22} style={label(1, { fontSize: 17 })}>VELOCITY, m/s · DISTANCE, m</text>
          <line x1={SPEED.x0} y1={sy(D.air.sound)} x2={SPEED.x1} y2={sy(D.air.sound)} stroke={C.dim} strokeWidth={1.2} strokeDasharray="6 8" />
          <text x={SPEED.x1} y={sy(D.air.sound) - 10} textAnchor="end" style={label(0.7, { fontSize: 15 })}>SPEED OF SOUND</text>
          {[0, 400, 800, 1200, 1600].map((m) => (
            <text key={m} x={sx(m)} y={SPEED.y0 + 26} textAnchor="middle" style={{ fontFamily: MONO, fontSize: 15, fill: C.dim }}>{m}</text>
          ))}
          {[200, 400, 600, 800].map((v) => (
            <text key={v} x={SPEED.x0 - 12} y={sy(v) + 5} textAnchor="end" style={{ fontFamily: MONO, fontSize: 15, fill: C.dim }}>{v}</text>
          ))}
        </g>
        {speedPts.length > 1 && (
          <path d={"M" + speedPts.map((q) => `${q[0].toFixed(1)},${q[1].toFixed(1)}`).join(" L")} stroke={C.line} strokeWidth={2.5} fill="none" strokeLinejoin="round" />
        )}
        {pAxes > 0.5 && <circle cx={sx(Math.min(s.x, 1600))} cy={sy(s.v)} r={6} fill={C.line} filter="url(#glow)" opacity={pAxes} />}

        {/* ---- the bullet's Cd against Mach; the dot rides the curve as the bullet slows */}
        <Axes x0={CDC.x0} x1={CDC.x1} y0={CDC.y0} y1={CDC.y1} p={pAxes} />
        <g opacity={pAxes}>
          <text x={CDC.x0} y={CDC.y1 - 22} style={label(1, { fontSize: 17 })}>Cd · MACH</text>
          <line x1={cx(1)} y1={CDC.y0} x2={cx(1)} y2={CDC.y1} stroke={C.dim} strokeWidth={1.2} strokeDasharray="6 8" />
          <text x={cx(1) + 8} y={CDC.y1 + 4} style={label(0.7, { fontSize: 15 })}>MACH 1</text>
          {[0.5, 1, 1.5, 2, 2.5].map((m) => (
            <text key={m} x={cx(m)} y={CDC.y0 + 26} textAnchor="middle" style={{ fontFamily: MONO, fontSize: 15, fill: C.dim }}>{m.toFixed(1)}</text>
          ))}
          {[0.1, 0.2, 0.3, 0.4].map((v) => (
            <text key={v} x={CDC.x0 - 12} y={cy(v) + 5} textAnchor="end" style={{ fontFamily: MONO, fontSize: 15, fill: C.dim }}>{v.toFixed(1)}</text>
          ))}
        </g>
        <path d={partial(CD_PTS, pCd).d} stroke={C.dragDim} strokeWidth={2} fill="none" strokeLinejoin="round" />
        {live && cdDone.length > 1 && (
          <path d={"M" + cdDone.map((q) => `${q[0].toFixed(1)},${q[1].toFixed(1)}`).join(" L")} stroke={C.drag} strokeWidth={3} fill="none" strokeLinejoin="round" />
        )}
        {pCd > 0.9 && <circle cx={cx(s.mach)} cy={cy(s.cd)} r={7} fill={C.drag} filter="url(#glow)" />}

        {/* ---- readouts and title */}
        <g opacity={oText(2.0, 0.8)}>
          {readouts.map(([k, v], i) => (
            <g key={k}>
              <text x={1560} y={262 + i * 38} textAnchor="end" style={label(0.6, { fontSize: 16 })}>{k}</text>
              <text x={1800} y={262 + i * 38} textAnchor="end" style={{ fontFamily: MONO, fontSize: 25, fill: k === "DRAG" ? C.drag : C.line }}>{v}</text>
            </g>
          ))}
        </g>
        <g opacity={oText(0.3, 1.0)}>
          <text x={1800} y={170} textAnchor="end" style={label(1, { fontSize: 34, letterSpacing: "0.32em" })}>DRAG</text>
          <text x={1800} y={204} textAnchor="end" style={label(0.6, { fontSize: 17 })}>{D.load.name.toUpperCase()} · STANDARD AIR</text>
        </g>

        {/* ---- captions */}
        <text x={960} y={600} textAnchor="middle" style={label(window01(f + (tm.intro ?? 0), cap[0], cap[1]), { fontSize: 24 })}>
          AT THE MUZZLE THE BULLET IS LOSING {Math.round(ROWS[0].drag * 0.1)} m/s EVERY TENTH OF A SECOND
        </text>
        <text x={960} y={600} textAnchor="middle" style={label(window01(f + (tm.intro ?? 0), F_MACH1 - 0.1, F_MACH1 + 2.6), { fontSize: 24, fill: C.drag })}>
          BELOW THE SPEED OF SOUND THE SHOCK WAVE IS GONE — DRAG DROPS
        </text>
        <g style={label(ramp(f + (tm.intro ?? 0), F1 + 0.4, 0.8), { fontSize: 24 })}>
          <text x={960} y={578} textAnchor="middle" style={{ fill: C.drag }}>
            {(ROWS[0].drag / G).toFixed(0)} g AT THE MUZZLE → {(LAST.drag / G).toFixed(1)} g AT {LAST.x} m
          </text>
          <text x={960} y={614} textAnchor="middle">
            THE BULLET SLOWED {Math.round(ROWS[0].v / LAST.v)} TIMES — DRAG FELL {Math.round(ROWS[0].drag / LAST.drag)} TIMES
          </text>
        </g>

        <rect x={0} y={0} width={1920} height={1080} fill="url(#vig)" pointerEvents="none" />
      </svg>
    </AbsoluteFill>
  );
};
