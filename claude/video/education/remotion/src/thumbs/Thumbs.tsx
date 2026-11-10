// YouTube thumbnails for the four episodes: the series' line-art look, but big — a title that reads at phone size
// on the left, one picture of the episode's idea on the right. Rendered by thumbs.mjs to ../thumbs/epNN.jpg.
import React from "react";
import { AbsoluteFill } from "remotion";
import { C, FONT, MONO } from "../theme";
import { Bullet, Defs, P, Pen, Vignette } from "../lib/draw";
import { Globe, SpinBullet, TurnArrow } from "../ep04/kit";

const TITLE: React.CSSProperties = { fontFamily: FONT, fontWeight: 700, letterSpacing: "0.02em", fill: C.line };
const SUB: React.CSSProperties = { fontFamily: FONT, fontWeight: 400, letterSpacing: "0.12em", fill: C.line, opacity: 0.8 };
const MONO_B = (size: number, fill = C.line): React.CSSProperties => ({ fontFamily: MONO, fontWeight: 700, fontSize: size, fill });

const Arrow: React.FC<{ a: P; b: P; color?: string; w?: number; head?: number; glow?: boolean }> = ({ a, b, color = C.line, w = 8, head = 34, glow }) => {
  const ang = Math.atan2(b[1] - a[1], b[0] - a[0]);
  const s: P = [b[0] - head * 0.6 * Math.cos(ang), b[1] - head * 0.6 * Math.sin(ang)];
  return (
    <g filter={glow ? "url(#glow)" : undefined}>
      <line x1={a[0]} y1={a[1]} x2={s[0]} y2={s[1]} stroke={color} strokeWidth={w} strokeLinecap="round" />
      <path d={`M${b[0]},${b[1]} L${b[0] - head * Math.cos(ang - 0.42)},${b[1] - head * Math.sin(ang - 0.42)} L${b[0] - head * Math.cos(ang + 0.42)},${b[1] - head * Math.sin(ang + 0.42)} Z`} fill={color} />
    </g>
  );
};

const Frame: React.FC<{ n: number; lines: string[]; size: number; sub: string; children: React.ReactNode }> = ({ n, lines, size, sub, children }) => {
  const y0 = 560 - ((lines.length - 1) * size * 1.02) / 2;
  return (
    <AbsoluteFill style={{ background: C.bg }}>
      <svg width={1920} height={1080} viewBox="0 0 1920 1080">
        <Defs />
        {children}
        <text x={110} y={150} style={{ fontFamily: FONT, fontWeight: 500, fontSize: 34, letterSpacing: "0.24em", fill: C.line, opacity: 0.7 }}>
          PHYSICS OF A BALLISTIC CALCULATOR · {n}
        </text>
        {lines.map((s, i) => (
          <text key={i} x={104} y={y0 + i * size * 1.02} style={{ ...TITLE, fontSize: size }} filter="url(#glow)">{s}</text>
        ))}
        <text x={110} y={y0 + (lines.length - 1) * size * 1.02 + 110} style={{ ...SUB, fontSize: 50 }}>{sub}</text>
        <Vignette />
      </svg>
    </AbsoluteFill>
  );
};

// 1 — drag: the bullet, its shock wave, and the drag force backwards
const T1: React.FC = () => {
  const cal = 104, x0 = 1290, y = 600, nose: P = [x0 + 4.526 * cal, y];
  return (
    <Frame n={1} lines={["DRAG"]} size={260} sub="WHERE THE VELOCITY GOES">
      {[-1, 1].map((s) => (
        <Pen key={s} pts={[nose, [nose[0] - 520, y + s * 330]]} p={1} w={4} tip={false} opacity={0.45} />
      ))}
      {[-150, -95, 95, 150].map((d) => (
        <line key={d} x1={1880} y1={y + d} x2={1180} y2={y + d * 1.9} stroke={C.line} strokeWidth={2} opacity={0.18} />
      ))}
      <Bullet x0={x0} y={y} cal={cal} w={6} />
      <Arrow a={[x0 - 10, y]} b={[x0 - 330, y]} color={C.drag} w={16} head={60} glow />
      <text x={x0 - 170} y={y - 60} textAnchor="middle" style={MONO_B(78, C.drag)}>48 g</text>
    </Frame>
  );
};

// 2 — the point mass: a path of steps, the dot, gravity down and drag back
const T2: React.FC = () => {
  const path = (x: number): P => { const k = (x - 1300) / 560; return [x, 520 - 110 * k + 380 * k * k]; };
  const dots = Array.from({ length: 6 }, (_, i) => path(1300 + i * 52));
  const d = path(1620), b = path(1590), ang = Math.atan2(d[1] - b[1], d[0] - b[0]);
  return (
    <Frame n={2} lines={["HOW A DOT", "FLIES"]} size={170} sub="THE POINT-MASS MODEL">
      <Pen pts={Array.from({ length: 41 }, (_, i) => path(1300 + i * 14))} p={1} w={3} tip={false} dash="10 12" opacity={0.45} />
      {dots.map((p, i) => <circle key={i} cx={p[0]} cy={p[1]} r={9} fill={C.line} opacity={0.25 + i * 0.08} />)}
      <circle cx={d[0]} cy={d[1]} r={30} fill={C.line} filter="url(#glow)" />
      <Arrow a={[d[0], d[1] + 40]} b={[d[0], d[1] + 260]} w={10} head={44} />
      <text x={d[0] + 34} y={d[1] + 230} style={MONO_B(64)}>g</text>
      <Arrow a={[d[0] - 40 * Math.cos(ang), d[1] - 40 * Math.sin(ang)]} b={[d[0] - 290 * Math.cos(ang), d[1] - 290 * Math.sin(ang)]} color={C.drag} w={14} head={54} glow />
      <text x={d[0] - 200} y={d[1] + 30} textAnchor="end" style={MONO_B(64, C.drag)}>drag</text>
    </Frame>
  );
};

// 3 — the wind, from above: not the 9 m a naive guess gives, but 3.35 m
const T3: React.FC = () => {
  const a: P = [1060, 380], L = 760, real = 120, naive = 324;
  const curve = Array.from({ length: 41 }, (_, i) => { const k = i / 40; return [a[0] + L * k, a[1] + real * k * k] as P; });
  return (
    <Frame n={3} lines={["THE WIND"]} size={200} sub="HOW THE AIR MOVES THE DOT">
      {[1220, 1440, 1660].map((x) => <Arrow key={x} a={[x, 150]} b={[x, 290]} w={6} head={30} />)}
      <text x={1880} y={240} textAnchor="end" style={MONO_B(46)}>4.5 m/s</text>
      <line x1={a[0]} y1={a[1]} x2={a[0] + L} y2={a[1]} stroke={C.line} strokeWidth={3} strokeDasharray="12 12" opacity={0.5} />
      <Pen pts={curve} p={1} w={9} color={C.drag} tip={false} />
      <circle cx={a[0]} cy={a[1]} r={14} fill={C.line} />
      {/* the naive end, struck out */}
      <g opacity={0.6}>
        <circle cx={a[0] + L} cy={a[1] + naive} r={18} fill="none" stroke={C.line} strokeWidth={4} strokeDasharray="6 6" />
        <text x={a[0] + L - 60} y={a[1] + naive + 24} textAnchor="end" style={MONO_B(74)}>9 m?</text>
        <line x1={a[0] + L - 270} y1={a[1] + naive} x2={a[0] + L - 50} y2={a[1] + naive - 4} stroke={C.drag} strokeWidth={8} />
      </g>
      <circle cx={a[0] + L} cy={a[1] + real} r={22} fill={C.drag} filter="url(#glow)" />
      <text x={a[0] + L - 40} y={a[1] + real + 110} textAnchor="end" style={MONO_B(96, C.drag)}>3.35 m</text>
      <text x={a[0] + L} y={a[1] - 40} textAnchor="end" style={{ ...SUB, fontSize: 30, opacity: 0.6 }}>1000 m</text>
    </Frame>
  );
};

// 4 — what a dot can't do: a spinning bullet and the turning Earth
const T4: React.FC = () => {
  const cx = 1580, cy = 420, cal = 92, rx = cal * 0.3, ry = cal * 0.95, ex = cx - 70;
  const arc: P[] = Array.from({ length: 41 }, (_, i) => { const a = -Math.PI / 2 + 0.35 + (i / 40) * Math.PI * 1.75; return [ex + rx * Math.cos(a), cy + ry * Math.sin(a)]; });
  const e = arc[40], q = arc[38], ang = Math.atan2(e[1] - q[1], e[0] - q[0]), h = 30;
  return (
    <Frame n={4} lines={["WHAT A DOT", "CAN'T DO"]} size={160} sub="SPIN AND THE TURNING EARTH">
      <SpinBullet id="th" cx={cx} cy={cy} cal={cal} deg={4} spin={0.35} marks={1} w={6} />
      <Pen pts={arc} p={1} w={7} tip={false} />
      <path d={`M${e[0]},${e[1]} L${e[0] - h * Math.cos(ang - 0.45)},${e[1] - h * Math.sin(ang - 0.45)} L${e[0] - h * Math.cos(ang + 0.45)},${e[1] - h * Math.sin(ang + 0.45)} Z`} fill={C.line} />
      <text x={cx} y={250} textAnchor="middle" style={MONO_B(56)}>3 100 turns/s</text>
      <Globe cx={1640} cy={850} r={170} rot={0.12} />
      <TurnArrow x={1640} y={640} rx={120} k={1} ph={0.1} w={6} />
    </Frame>
  );
};

export const THUMBS = [T1, T2, T3, T4];
export const Thumb: React.FC<{ ep: number }> = ({ ep }) => {
  const T = THUMBS[ep - 1];
  return <T />;
};
