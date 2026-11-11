// A hand-rolled 2.5D stage for episode 4: a camera that can swing from the flat side view of episode 2 into a view
// from behind and above the shooter, the dot's path standing in its vertical plane over a ground grid, and the four
// small effects bending that path out of the plane. Everything is projected by hand into the scene's SVG.
import React from "react";
import { C, lerp } from "../theme";
import { P, Pen, SMK, label, radius } from "../lib/draw";
import { CG_F, FLIGHT, atX, drift } from "./kit";

export type V3 = [number, number, number];   // x downrange (m), y up, z to the shooter's right (drawn units)

// ---------------------------------------------------------------- the camera
/** yaw 0: from the shooter's right side, downrange to the right of the screen (the flat picture of episode 2);
 *  yaw 90°: from behind the shooter, looking downrange. pitch: looking down by that many degrees. */
export type Cam = { yaw: number; pitch: number; dist: number; focal: number; target: V3; sx: number; sy: number };

export const mixCam = (a: Cam, b: Cam, k: number): Cam => ({
  yaw: lerp(a.yaw, b.yaw, k), pitch: lerp(a.pitch, b.pitch, k), dist: Math.exp(lerp(Math.log(a.dist), Math.log(b.dist), k)),
  focal: Math.exp(lerp(Math.log(a.focal), Math.log(b.focal), k)),
  target: [lerp(a.target[0], b.target[0], k), lerp(a.target[1], b.target[1], k), lerp(a.target[2], b.target[2], k)],
  sx: lerp(a.sx, b.sx, k), sy: lerp(a.sy, b.sy, k),
});

const NEAR = 40;
/** Screen point and depth; depth < NEAR means behind or too close to the camera. */
export const project = (c: Cam, p: V3): [number, number, number] => {
  const ps = (c.yaw * Math.PI) / 180, th = (c.pitch * Math.PI) / 180;
  const dx = p[0] - c.target[0], dy = p[1] - c.target[1], dz = p[2] - c.target[2];
  // right r = (cos ψ, 0, sin ψ); level forward f = (sin ψ, 0, −cos ψ); pitched forward f' = f cos θ − up sin θ
  const xr = dx * Math.cos(ps) + dz * Math.sin(ps);
  const fl = dx * Math.sin(ps) - dz * Math.cos(ps);
  const yc = dy * Math.cos(th) + fl * Math.sin(th);
  const zc = fl * Math.cos(th) - dy * Math.sin(th) + c.dist;
  const s = c.focal / Math.max(zc, 1e-3);
  return [c.sx + xr * s, c.sy - yc * s, zc];
};
export const pr = (c: Cam, p: V3): P => { const q = project(c, p); return [q[0], q[1]]; };
/** Project a polyline, dropping the points too close to the camera. */
export const prLine = (c: Cam, pts: V3[]): P[] => pts.map((p) => project(c, p)).filter((q) => q[2] > NEAR).map((q) => [q[0], q[1]] as P);

// ---------------------------------------------------------------- the shot in drawn units
export const SY = 12.5;          // the drop: 1 m = 12.5 units (as in scene 1's flat picture: 1.6 px/m across, 20 px/m down)
export const SE = 400;           // the four effects: 1 m = 400 units — drawn ~30 times bigger than the drop
export const GROUND = -15 * SY - 70;
const T1000 = atX(1000).t;
const OMEGA = 7.292e-5, LAT = Math.PI / 4;

/** The four effects in metres at distance x: [spin drift → right, aerodynamic jump ↑, Coriolis → right, east–west ↑]. */
export const effects = (x: number): [number, number, number, number] => {
  const t = atX(x).t;
  return [drift(x, t), 0.112 * (x / 1000), OMEGA * Math.sin(LAT) * x * t, 0.12 * (t / T1000) ** 2];
};
export const EFFECT_CM = [31, 11, 10, 12];
/** A point of the path at x, with the effects switched on by k = [drift, jump, coriolis, eastwest] (0..1 each). */
export const pathAt = (x: number, k: number[] = [0, 0, 0, 0]): V3 => {
  const e = effects(x), y = atX(x).y;
  return [x, y * SY + SE * (k[1] * e[1] + k[3] * e[3]), SE * (k[0] * e[0] + k[2] * e[2])];
};
export const XS = FLIGHT.map((r) => r.x);

// ---------------------------------------------------------------- the props
/** The ground: a grid under the line of fire, from the muzzle to `x1`. */
export const Ground: React.FC<{ c: Cam; o: number; x1?: number; zw?: number }> = ({ c, o, x1 = 1000, zw = 300 }) => {
  const lines: React.ReactNode[] = [];
  for (let x = 0; x <= x1; x += 100) {
    const a = pr(c, [x, GROUND, -zw]), b = pr(c, [x, GROUND, zw]);
    lines.push(<line key={`x${x}`} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={1} opacity={0.18} />);
  }
  for (let z = -zw; z <= zw; z += 100) {
    const pts = prLine(c, [[0, GROUND, z], [x1 / 2, GROUND, z], [x1, GROUND, z]]);
    lines.push(<Pen key={`z${z}`} pts={pts} p={1} w={1} tip={false} opacity={z === 0 ? 0 : 0.18} />);
  }
  // the line of fire on the ground
  const lf = prLine(c, Array.from({ length: 21 }, (_, i) => [(x1 * i) / 20, GROUND, 0] as V3));
  return (
    <g opacity={o}>
      {lines}
      <Pen pts={lf} p={1} w={2} tip={false} dash="10 10" opacity={0.6} />
    </g>
  );
};

/** The dot's vertical plane: a pale sheet standing on the line of fire. */
export const Sheet: React.FC<{ c: Cam; o: number; top?: number }> = ({ c, o, top = 60 }) => {
  const q = [[0, GROUND, 0], [1000, GROUND, 0], [1000, top, 0], [0, top, 0]].map((p) => pr(c, p as V3));
  return <path d={`M${q.map((p) => p.join(",")).join(" L")} Z`} fill={C.line} fillOpacity={0.045 * o} stroke={C.line} strokeOpacity={0.22 * o} strokeWidth={1.2} />;
};

/** The path with its stakes (every 100 m, down to the ground) and its shadow on the ground. */
export const Path3: React.FC<{ c: Cam; k: number[]; p?: number; o?: number; w?: number; stakes?: number; shadow?: number; dash?: string; glow?: boolean }> =
  ({ c, k, p = 1, o = 1, w = 3.5, stakes = 0, shadow = 0, dash, glow }) => {
    const xs = XS.filter((x) => x <= 1000 * p + 1e-6);
    const pts = xs.map((x) => pathAt(x, k));
    const sh = xs.map((x) => { const q = pathAt(x, k); return [x, GROUND, q[2]] as V3; });
    return (
      <g opacity={o}>
        {shadow > 0 && <Pen pts={prLine(c, sh)} p={1} w={3} tip={false} opacity={0.55 * shadow} />}
        {stakes > 0 && xs.filter((x) => x > 0 && x % 100 === 0).map((x) => {
          const a = pr(c, pathAt(x, k)), b = pr(c, [x, GROUND, pathAt(x, k)[2]]);
          return <line key={x} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={1.2} opacity={0.35 * stakes} />;
        })}
        <g filter={glow ? "url(#glow)" : undefined}><Pen pts={prLine(c, pts)} p={1} w={w} tip={false} dash={dash} /></g>
      </g>
    );
  };

/** A target board standing across the line of fire at 1000 m, centred on the dot's hit. */
export const Target3: React.FC<{ c: Cam; o: number; half?: number; edge?: number }> = ({ c, o, half = 200, edge = 1 }) => {
  const hit = pathAt(1000);
  const ring = (r: number) => prLine(c, Array.from({ length: 49 }, (_, i) => { const a = (i / 48) * Math.PI * 2; return [1000, hit[1] + r * Math.sin(a), r * Math.cos(a)] as V3; }));
  const sq = [[-half, -half], [half, -half], [half, half], [-half, half], [-half, -half]].map(([z, y]) => [1000, hit[1] + y, z] as V3);
  return (
    <g opacity={o}>
      <Pen pts={prLine(c, sq)} p={1} w={2.5} tip={false} opacity={edge} />
      {[60, 120, 180].map((r) => <Pen key={r} pts={ring(r)} p={1} w={1.4} tip={false} opacity={0.4} />)}
    </g>
  );
};

// ---------------------------------------------------------------- a bullet in space
const norm = (a: V3): V3 => { const l = Math.hypot(a[0], a[1], a[2]) || 1; return [a[0] / l, a[1] / l, a[2] / l]; };
const cross = (a: V3, b: V3): V3 => [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];
const at3 = (p: V3, d: V3, k: number): V3 => [p[0] + d[0] * k, p[1] + d[1] * k, p[2] + d[2] * k];

/** The bullet as a body of revolution, projected like everything else: its outline, a few rings across it, and
 *  three stripes on its side that run round as it spins. `cg` is its centre of mass, `axis` points to the nose,
 *  `cal` drawn units per calibre. */
export const Bullet3: React.FC<{ c: Cam; cg: V3; axis: V3; cal: number; spin?: number; o?: number }> = ({ c, cg, axis, cal, spin = 0, o = 1 }) => {
  const ax = norm(axis), L = SMK.len * cal;
  const ref: V3 = Math.abs(ax[1]) > 0.9 ? [1, 0, 0] : [0, 1, 0];
  const u = norm(cross(ax, ref)), w = cross(u, ax);
  const base = at3(cg, ax, -CG_F * L);
  const ringAt = (f: number, m = 28): V3[] => {
    const ctr = at3(base, ax, f * L), r = radius(SMK, f * SMK.len) * cal;
    return Array.from({ length: m + 1 }, (_, i) => { const a = (i / m) * Math.PI * 2; return [ctr[0] + r * (Math.cos(a) * u[0] + Math.sin(a) * w[0]), ctr[1] + r * (Math.cos(a) * u[1] + Math.sin(a) * w[1]), ctr[2] + r * (Math.cos(a) * u[2] + Math.sin(a) * w[2])] as V3; });
  };
  const b2 = pr(c, base), t2 = pr(c, at3(base, ax, L));
  const dl = Math.hypot(t2[0] - b2[0], t2[1] - b2[1]) || 1, n2: P = [-(t2[1] - b2[1]) / dl, (t2[0] - b2[0]) / dl];
  // the outline: on every station, the two ring points furthest out to each side of the axis on screen
  const top: P[] = [], bot: P[] = [];
  for (let i = 0; i <= 30; i++) {
    const pts = ringAt(i / 30).map((p) => pr(c, p));
    let lo = pts[0], hi = pts[0], dlo = Infinity, dhi = -Infinity;
    for (const p of pts) { const d = (p[0] - b2[0]) * n2[0] + (p[1] - b2[1]) * n2[1]; if (d < dlo) { dlo = d; lo = p; } if (d > dhi) { dhi = d; hi = p; } }
    top.push(hi); bot.push(lo);
  }
  const outlineD = `M${[...top, ...bot.reverse()].map((p) => p.join(",")).join(" L")} Z`;
  const rings = [0, SMK.btLen / SMK.len, (SMK.len - SMK.noseLen) / SMK.len, (SMK.len - SMK.noseLen * 0.55) / SMK.len];
  const ogive = (SMK.len - SMK.noseLen) / SMK.len;
  const stripes = [0, 1, 2].map((k) => {
    const a = spin * Math.PI * 2 + (k * Math.PI * 2) / 3, dir: V3 = [Math.cos(a) * u[0] + Math.sin(a) * w[0], Math.cos(a) * u[1] + Math.sin(a) * w[1], Math.cos(a) * u[2] + Math.sin(a) * w[2]];
    const f0 = SMK.btLen / SMK.len + 0.04, f1 = ogive + 0.12;
    const p0 = at3(at3(base, ax, f0 * L), dir, 0.5 * cal), p1 = at3(at3(base, ax, f1 * L), dir, radius(SMK, f1 * SMK.len) * cal);
    const front = project(c, at3(base, ax, 0.5 * L))[2] > project(c, at3(at3(base, ax, 0.5 * L), dir, 0.5 * cal))[2];
    return front ? [pr(c, p0), pr(c, p1)] : null;
  });
  return (
    <g opacity={o}>
      <path d={outlineD} fill={C.bg} stroke={C.line} strokeWidth={2.5} strokeLinejoin="round" />
      {rings.map((f, i) => <Pen key={i} pts={ringAt(f).map((p) => pr(c, p))} p={1} w={1.4} tip={false} opacity={0.5} />)}
      {stripes.map((s, i) => s && <line key={i} x1={s[0][0]} y1={s[0][1]} x2={s[1][0]} y2={s[1][1]} stroke={C.line} strokeWidth={1.6} opacity={0.6} />)}
    </g>
  );
};

/** "Drawn bigger" note, placed beside the picture. */
export const ScaleNote: React.FC<{ x: number; y: number; o: number; anchor?: "start" | "middle" | "end" }> = ({ x, y, o, anchor = "start" }) => (
  <text x={x} y={y} textAnchor={anchor} style={label(0.6 * o, { fontSize: 18 })}>THE FOUR EFFECTS: DRAWN ABOUT 400 TIMES BIGGER</text>
);
