// Scene 9 — the series outro: the four motifs in a row (the drag, the dot and its steps, the wind, the spinning
// bullet), then the link to the documentation.
import React from "react";
import { C, ramp } from "../theme";
import { Bullet, label } from "../lib/draw";
import { Dot, Scene, SpinBullet, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 9;
const T = {
  inside: cue(N, "inside"), drag: cue(N, "drag"), dot: cue(N, "dot"), wind: cue(N, "wind"), fixes: cue(N, "few small fixes"),
  formulas: cue(N, "formulas"), link: cue(N, "link"),
};

export const S09Outro: React.FC = () => {
  const v = useVoiceTime();
  const m = (t: number) => ramp(v, t - 0.3, 0.5);
  const Y = 560;
  return (
    <Scene n={N}>
      <text x={960} y={200} textAnchor="middle" style={label(ramp(v, -0.3, 0.8), { fontSize: 54, letterSpacing: "0.22em", fontWeight: 200 })}>INSIDE THE CALCULATOR</text>
      <text x={960} y={256} textAnchor="middle" style={label(ramp(v, 0.2, 0.8) * 0.6, { fontSize: 22 })}>PHYSICS OF A BALLISTIC CALCULATOR · 1 – 4</text>

      {/* the drag */}
      <g opacity={m(T.drag)}>
        <Bullet x0={260} y={Y} cal={26} w={2.5} />
        <line x1={250} y1={Y} x2={150} y2={Y} stroke={C.drag} strokeWidth={6} />
        <path d={`M168,${Y - 14} L150,${Y} L168,${Y + 14}`} stroke={C.drag} strokeWidth={6} fill="none" />
        <text x={300} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>THE DRAG</text>
      </g>
      {/* the dot, step by step */}
      <g opacity={m(T.dot)}>
        {Array.from({ length: 7 }, (_, i) => <Dot key={i} x={620 + i * 40} y={Y - 30 + i * i * 2.4} r={i === 6 ? 10 : 5} />)}
        <text x={740} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>THE DOT</text>
      </g>
      {/* the wind */}
      <g opacity={m(T.wind)}>
        {Array.from({ length: 6 }, (_, i) => {
          const x = 1080 + ((((i * 53 - 90 * v) % 240) + 240) % 240);
          return <line key={i} x1={x} y1={Y - 50 + i * 20} x2={x + 46} y2={Y - 50 + i * 20} stroke={C.line} strokeOpacity={0.6} strokeWidth={2.5} strokeLinecap="round" />;
        })}
        <text x={1180} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>THE WIND</text>
      </g>
      {/* the spin, and the turning Earth */}
      <g opacity={m(T.fixes)}>
        <SpinBullet id="o" cx={1620} cy={Y} cal={30} spin={v * 1.4} marks={0.9} />
        <text x={1620} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>A FEW SMALL FIXES</text>
      </g>

      <text x={960} y={830} textAnchor="middle" style={label(m(T.formulas), { fontSize: 30 })}>ALL THE FORMULAS: IN THE DOCUMENTATION</text>
      <text x={960} y={890} textAnchor="middle" style={label(m(T.link) * 0.7, { fontSize: 24 })}>THE LINK IS IN THE DESCRIPTION</text>
    </Scene>
  );
};
