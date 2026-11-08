// Scene 6 — Cd is not one number: the bullet's Cd against Mach, big; the three speed ranges; the hump where the
// shock waves are born; measuring a curve takes a radar and many shots.
import React from "react";
import { D } from "../data";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Bullet, P, Pen, label, mono, pathD } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 6;
const T_NOT = cue(N, "not one"), T_CHANGES = cue(N, "changes"), T_SLOW = cue(N, "Slower"), T_NEAR = cue(N, "Near");
const T_JUMPS = cue(N, "jumps"), T_BORN = cue(N, "born"), T_FAST = cue(N, "And faster"), T_DOWN = cue(N, "slowly");
const T_EVERY = cue(N, "Every"), T_RADAR = cue(N, "radar"), T_SHOTS = cue(N, "shots");

export const CH = { x0: 230, x1: 1720, y0: 860, y1: 300, mMin: 0.5, mMax: 2.7, cdMax: 0.45 };
export const chx = (m: number) => lerp(CH.x0, CH.x1, (m - CH.mMin) / (CH.mMax - CH.mMin));
export const chy = (cd: number) => lerp(CH.y0, CH.y1, cd / CH.cdMax);
const CURVE: P[] = D.cdCurve.map((p) => [chx(p.mach), chy(p.cd)] as P);
const cdAt = (m: number) => {
  const c = D.cdCurve;
  for (let i = 1; i < c.length; i++) if (c[i].mach >= m) return lerp(c[i - 1].cd, c[i].cd, (m - c[i - 1].mach) / (c[i].mach - c[i - 1].mach));
  return c[c.length - 1].cd;
};
const seg = (a: number, b: number): P[] => D.cdCurve.filter((p) => p.mach >= a && p.mach <= b).map((p) => [chx(p.mach), chy(p.cd)] as P);

export const ChartAxes: React.FC<{ p: number; yTicks?: number[] }> = ({ p, yTicks = [0.1, 0.2, 0.3, 0.4] }) => (
  <g>
    <Pen pts={[[CH.x0, CH.y1 - 20], [CH.x0, CH.y0], [CH.x1 + 20, CH.y0]]} p={p} color={C.dim} w={1.8} tip={false} />
    <g opacity={p}>
      {[0.5, 1, 1.5, 2, 2.5].map((m) => (
        <text key={m} x={chx(m)} y={CH.y0 + 34} textAnchor="middle" style={mono(0.6, 20)}>{m.toFixed(1)}</text>
      ))}
      {yTicks.map((v) => (
        <text key={v} x={CH.x0 - 16} y={chy(v) + 7} textAnchor="end" style={mono(0.6, 20)}>{v.toFixed(1)}</text>
      ))}
      <text x={CH.x1 + 20} y={CH.y0 - 16} textAnchor="end" style={label(0.7, { fontSize: 20 })}>MACH = VELOCITY ÷ SPEED OF SOUND</text>
      <text x={CH.x0 - 16} y={CH.y1 - 30} textAnchor="end" style={label(0.7, { fontSize: 20 })}>Cd</text>
      <line x1={chx(1)} y1={CH.y0} x2={chx(1)} y2={CH.y1} stroke={C.dim} strokeWidth={1.2} strokeDasharray="6 8" />
    </g>
  </g>
);

export const S06CdCurve: React.FC = () => {
  const v = useVoiceTime();
  const run = ramp(v, 0.4, T_SLOW - 0.6, ease);
  let m = lerp(D.rows[0].mach, 0.55, run);
  m = lerp(m, 1.05, ramp(v, T_NEAR - 0.4, 1.2, ease));
  m = lerp(m, 2.5, ramp(v, T_FAST - 0.2, 2.6, ease));
  const brk = (a: number, b: number, t0: number, t1: number, text: string, color: string = C.line) => {
    const o = ramp(v, t0 - 0.2, 0.5), hot = window01(v, t0 - 0.2, t1, 0.3);
    return (
      <g opacity={o}>
        <Pen pts={[[chx(a) + 6, 920], [chx(a) + 6, 932], [chx(b) - 6, 932], [chx(b) - 6, 920]]} p={ramp(v, t0 - 0.2, 0.6)} color={color} w={2} tip={false} />
        <text x={(chx(a) + chx(b)) / 2} y={968} textAnchor="middle" style={label(0.5 + 0.5 * hot, { fontSize: 20, fill: color })}>{text}</text>
      </g>
    );
  };
  return (
    <Scene n={N}>
      <ChartAxes p={ramp(v, -0.4, 0.8)} />
      <path d={pathD(CURVE)} stroke={C.drag} strokeWidth={3} fill="none" strokeLinejoin="round" opacity={0.85} />
      {/* the three ranges glow in turn */}
      <g filter="url(#glow)">
        <path d={pathD(seg(0.5, 0.9))} stroke={C.drag} strokeWidth={6} fill="none" opacity={window01(v, T_SLOW, T_NEAR, 0.3)} />
        <path d={pathD(seg(0.88, 1.25))} stroke={C.drag} strokeWidth={6} fill="none" opacity={window01(v, T_NEAR, T_FAST, 0.3)} />
        <path d={pathD(seg(1.2, 2.7))} stroke={C.drag} strokeWidth={6} fill="none" opacity={window01(v, T_FAST, T_EVERY, 0.3)} />
      </g>
      <circle cx={chx(m)} cy={chy(cdAt(m))} r={11} fill={C.drag} filter="url(#glow)" opacity={1 - ramp(v, T_EVERY, 0.5)} />
      {brk(0.5, 0.9, T_SLOW, T_NEAR, "SLOWER THAN SOUND")}
      {brk(0.9, 1.25, T_NEAR, T_FAST, "NEAR SOUND", C.drag)}
      {brk(1.25, 2.7, T_FAST, T_EVERY, "FASTER THAN SOUND")}
      {/* where shock waves are born: a little cone pops at the hump */}
      {(() => {
        const o = window01(v, T_JUMPS, T_FAST + 0.3, 0.3), x = chx(1.25), y = chy(cdAt(1.1)) - 110, k = ramp(v, T_BORN - 0.6, 0.8);
        return (
          <g opacity={o}>
            <path d={`M${x + 70},${y} L${x - 40 - 110 * k},${y - 90 * k} M${x + 70},${y} L${x - 40 - 110 * k},${y + 90 * k}`} stroke={C.line} strokeWidth={3} fill="none" />
            <Bullet x0={x - 40} y={y} cal={24} cannelure={false} w={2.5} />
            <text x={x + 110} y={y + 8} style={label(ramp(v, T_BORN - 0.6, 0.5), { fontSize: 24 })}>SHOCK WAVES ARE BORN</text>
          </g>
        );
      })()}
      <text x={960} y={190} textAnchor="middle" style={label(window01(v, T_NOT - 0.2, T_SLOW, 0.4), { fontSize: 34 })}>Cd IS NOT ONE NUMBER</text>
      <text x={960} y={236} textAnchor="middle" style={label(window01(v, T_CHANGES - 0.1, T_SLOW, 0.4) * 0.7, { fontSize: 22 })}>IT CHANGES WITH THE VELOCITY</text>
      {/* the radar: a dish, a beam, a stream of shots */}
      {(() => {
        const o = ramp(v, T_EVERY - 0.2, 0.6), bx = 1480, by = 210, beam = ramp(v, T_RADAR - 0.4, 0.8);
        return (
          <g opacity={o}>
            <text x={960} y={190} textAnchor="middle" style={label(ramp(v, T_EVERY, 0.4), { fontSize: 30 })}>EVERY BULLET HAS ITS OWN CURVE</text>
            <g opacity={beam}>
              <path d={`M${bx - 40},${by + 50} Q${bx},${by - 30} ${bx + 40},${by + 50}`} stroke={C.line} strokeWidth={3} fill="none" />
              <line x1={bx} y1={by + 30} x2={bx} y2={by + 80} stroke={C.line} strokeWidth={3} />
              <line x1={bx - 26} y1={by + 80} x2={bx + 26} y2={by + 80} stroke={C.line} strokeWidth={3} />
              {[0, 1, 2].map((i) => (
                <path key={i} d={`M${bx + 30 + i * 22},${by - 10 - i * 10} q12,22 0,44`} stroke={C.line} strokeWidth={2} fill="none"
                  opacity={0.4 + 0.6 * (((v * 2 + i * 0.3) % 1))} />
              ))}
              <text x={bx + 140} y={by + 40} style={label(1, { fontSize: 22 })}>RADAR</text>
            </g>
            {Array.from({ length: 14 }, (_, i) => (
              <line key={i} x1={300 + i * 70} y1={by + 50} x2={330 + i * 70} y2={by + 50} stroke={C.line} strokeWidth={3} strokeLinecap="round"
                opacity={ramp(v, T_SHOTS - 0.8 + i * 0.06, 0.2)} />
            ))}
            <text x={300} y={by + 100} style={label(ramp(v, T_SHOTS - 0.4, 0.4) * 0.8, { fontSize: 22 })}>× A LOT OF SHOTS</text>
          </g>
        );
      })()}
    </Scene>
  );
};
