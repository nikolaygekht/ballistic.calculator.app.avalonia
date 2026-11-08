// Scene 1 — hook: the bullet leaves the barrel at 790 m/s; two seconds later it is 1 km away at less than half.
import React from "react";
import { ROWS, stateAt } from "../data";
import { C, clamp01, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Bullet, Pen, label, mono } from "../lib/draw";
import { Scene, Streaks, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 1;
const T_TWO = cue(N, "Two"), T_790 = cue(N, "790"), T_HALF = cue(N, "half"), T_SO = cue(N, "So where");
const T_AIR = cue(N, "The air took"), T_TOOK = cue(N, "took"), T_TODAY = cue(N, "Today");
const T1000 = ROWS.find((r) => r.x >= 1000)!.t;
const V1000 = ROWS.find((r) => r.x >= 1000)!.v;

export const S01Hook: React.FC = () => {
  const v = useVoiceTime();
  const AX = 470, CAL = 54;

  // part A: muzzle exit, bullet glides to the centre
  const oA = 1 - ramp(v, T_TWO - 0.3, 0.5);
  const exit = ramp(v, -0.4, 2.2, ease);
  const bx = lerp(250, 760, exit);
  const flow = (v + 1) * 900;
  // part B: the 1 km ruler, real time
  const oB = window01(v, T_TWO - 0.2, T_SO + 0.2, 0.4);
  const tau = clamp01((v - (T_TWO + 0.3)) / T1000) * T1000;
  const s = stateAt(tau);
  const RX0 = 180, RX1 = 1740, rx = (m: number) => lerp(RX0, RX1, m / 1000);
  // part C: back to the big bullet, the air takes the speed, the title
  const oC = ramp(v, T_SO, 0.5);
  const drag = ramp(v, T_TOOK, 0.8, ease);

  return (
    <Scene n={N}>
      {/* ---------------- A */}
      <g opacity={oA}>
        <Streaks shift={flow} y0={250} y1={700} gapY={AX} gap={46} len={170} o={ramp(v, 0.6, 1)} />
        {/* the barrel's muzzle */}
        <g stroke={C.line} strokeWidth={3} fill="none" strokeLinecap="round">
          <Pen pts={[[-20, AX - 44], [200, AX - 44], [200, AX + 44], [-20, AX + 44]]} p={ramp(v, -0.7, 0.6)} tip={false} />
          <line x1={200} y1={AX - 30} x2={200} y2={AX + 30} stroke={C.bg} strokeWidth={6} opacity={ramp(v, -0.2, 0.1)} />
        </g>
        {/* muzzle blast, a few lines */}
        {[-40, -20, 0, 20, 40].map((a, i) => {
          const k = ramp(v, -0.25, 0.5), o = 1 - ramp(v, 0.1, 0.6);
          const r = (a * Math.PI) / 180;
          return <line key={i} x1={210 + 30 * k * Math.cos(r)} y1={AX + 30 * k * Math.sin(r)} x2={210 + 110 * k * Math.cos(r)} y2={AX + 110 * k * Math.sin(r)}
            stroke={C.line} strokeWidth={2} opacity={o * 0.7} strokeLinecap="round" />;
        })}
        <Bullet x0={bx} y={AX} cal={CAL} p={1} opacity={ramp(v, -0.35, 0.15)} />
        <Arrow x1={bx + 4.53 * CAL + 24} y1={AX} x2={bx + 4.53 * CAL + 24 + 300} y2={AX} p={ramp(v, T_790 - 0.6, 0.8, ease)} />
        <text x={bx + 4.53 * CAL + 24} y={AX - 34} style={mono(ramp(v, T_790 - 0.2, 0.5), 54, { fontWeight: 600 })}>
          790 m/s
        </text>
      </g>

      {/* ---------------- B: one kilometre in two seconds */}
      <g opacity={oB}>
        <line x1={RX0} y1={600} x2={RX1} y2={600} stroke={C.dim} strokeWidth={1.5} />
        {Array.from({ length: 11 }, (_, i) => i * 100).map((m) => (
          <g key={m}>
            <line x1={rx(m)} y1={600} x2={rx(m)} y2={m % 500 === 0 ? 620 : 610} stroke={C.dim} strokeWidth={1.5} />
            {m % 500 === 0 && <text x={rx(m)} y={652} textAnchor="middle" style={mono(0.6, 22)}>{m === 1000 ? "1 km" : `${m} m`}</text>}
          </g>
        ))}
        <Bullet x0={rx(s.x) - 4.53 * 9} y={580} cal={9} w={2} cannelure={false} />
        <text x={rx(s.x)} y={540} textAnchor="middle" style={mono(1, 30)}>{tau.toFixed(1)} s</text>

        {/* speed bars: muzzle vs now, with the half mark */}
        {(() => {
          const BX = 360, sc = 1.25, k = ramp(v, T_TWO, 0.6);
          const half = window01(v, T_HALF - 0.2, T_SO + 0.4, 0.3);
          return (
            <g>
              <text x={BX - 24} y={808} textAnchor="end" style={label(0.6, { fontSize: 20 })}>MUZZLE</text>
              <line x1={BX} y1={800} x2={BX + 792 * sc * k} y2={800} stroke={C.dim} strokeWidth={14} strokeLinecap="round" />
              <text x={BX + 792 * sc + 24} y={808} style={mono(0.6 * k, 26)}>790 m/s</text>
              <text x={BX - 24} y={878} textAnchor="end" style={label(0.9, { fontSize: 20 })}>NOW</text>
              <line x1={BX} y1={870} x2={BX + s.v * sc * k} y2={870} stroke={C.line} strokeWidth={14} strokeLinecap="round" />
              <text x={BX + 792 * sc + 24} y={878} style={mono(k, 26)}>{Math.round(Math.min(790, s.v))} m/s</text>
              <line x1={BX + 396 * sc} y1={770} x2={BX + 396 * sc} y2={900} stroke={C.drag} strokeWidth={2} strokeDasharray="6 6" opacity={half} />
              <text x={BX + 396 * sc} y={758} textAnchor="middle" style={label(half, { fontSize: 22, fill: C.drag })}>HALF</text>
            </g>
          );
        })()}
        <text x={960} y={300} textAnchor="middle" style={label(ramp(v, T_TWO + 0.3 + T1000, 0.4) * oB, { fontSize: 28 })}>
          1 km · {T1000.toFixed(1)} s · {Math.round(V1000)} m/s
        </text>
      </g>

      {/* ---------------- C: the air took it */}
      <g opacity={oC}>
        <Streaks shift={(v - T_SO) * 300} y0={250} y1={720} gapY={AX} gap={46} len={90} o={ramp(v, T_SO + 0.2, 1.2)} />
        <Bullet x0={760} y={AX} cal={CAL} opacity={1 - 0.55 * ramp(v, T_TODAY, 0.6)} />
        <g filter="url(#glow)" opacity={1 - 0.55 * ramp(v, T_TODAY, 0.6)}>
          <Arrow x1={736} y1={AX} x2={736 - 330} y2={AX} p={drag} color={C.drag} w={5} head={22} />
        </g>
        <text x={736} y={AX - 34} textAnchor="end" style={label(drag * (1 - ramp(v, T_TODAY, 0.4)), { fontSize: 24, fill: C.drag })}>THE AIR</text>
        <text x={960} y={250} textAnchor="middle" style={label(window01(v, T_SO, T_AIR + 0.1, 0.3), { fontSize: 40 })}>WHERE DID THE VELOCITY GO?</text>
        {/* the title */}
        <g opacity={ramp(v, T_TODAY, 0.4)}>
          <text x={960} y={780} textAnchor="middle" style={label(1, { fontSize: 120, letterSpacing: "0.5em", fontWeight: 200 })}
            clipPath="url(#titleClip)">DRAG</text>
          <clipPath id="titleClip"><rect x={0} y={600} width={lerp(560, 1500, ramp(v, T_TODAY, 1.2))} height={300} /></clipPath>
          <text x={960} y={840} textAnchor="middle" style={label(ramp(v, T_TODAY + 0.8, 0.6) * 0.6, { fontSize: 22 })}>
            PHYSICS OF A BALLISTIC CALCULATOR · 1
          </text>
        </g>
      </g>
    </Scene>
  );
};
