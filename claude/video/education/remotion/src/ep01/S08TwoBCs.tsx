// Scene 8 — one bullet, two BCs: 0.505 on G1 and 0.253 on G7 are the same bullet read on two rulers.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Bullet, G1S, G7S, Pen, SMK, label, mono } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 8;
const T = {
  box: cue(N, "box"), b1: cue(N, "0"), b2: cue(N, "0", 2), right: cue(N, "both"), rulers: cue(N, "rulers"),
  same: cue(N, "The bullet"), only: cue(N, "only"), why: cue(N, "That's why"), nothing: cue(N, "nothing"),
};
const PX = 960, R0 = 300;
const rulerA = (b: number) => R0 + (b / 0.505) * (PX - R0);
const rulerB = (b: number) => R0 + (b / 0.253) * (PX - R0);

export const S08TwoBCs: React.FC = () => {
  const v = useVoiceTime();
  const rul = ramp(v, T.rulers - 0.6, 1.0);
  const cal = 80, X0 = PX - (SMK.len * cal) / 2;
  const fin = ramp(v, T.why - 0.2, 0.6);
  const Ruler = ({ y, f, max, step, name, val, o }: { y: number; f: (b: number) => number; max: number; step: number; name: string; val: string; o: number }) => (
    <g opacity={o}>
      <Pen pts={[[R0 - 20, y], [f(max) + 20, y]]} p={rul} color={C.line} w={2} tip={false} />
      {Array.from({ length: Math.round(max / step) + 1 }, (_, i) => i * step).map((b, i) => (
        <g key={i} opacity={ramp(v, T.rulers - 0.4 + i * 0.03, 0.3)}>
          <line x1={f(b)} y1={y} x2={f(b)} y2={y + (i % 2 ? 12 : 22)} stroke={C.line} strokeWidth={1.6} />
          {i % 2 === 0 && <text x={f(b)} y={y + 50} textAnchor="middle" style={mono(0.6, 18)}>{b.toFixed(2).replace(/^0/, "")}</text>}
        </g>
      ))}
      <text x={R0 - 40} y={y + 8} textAnchor="end" style={mono(0.9 * rul, 26)}>{name}</text>
      <text x={PX + 16} y={y - 14} style={mono(ramp(v, T.only - 0.4, 0.5), 30, { fill: C.drag, fontWeight: 600 })}>{val}</text>
    </g>
  );
  return (
    <Scene n={N}>
      <text x={960} y={140} textAnchor="middle" style={label(ramp(v, 0, 0.6) * (1 - fin), { fontSize: 30 })}>ONE BULLET · TWO BCs ON THE BOX</text>
      <Bullet x0={X0} y={360} cal={cal} p={ramp(v, -0.5, 1.2)} opacity={1 - 0.6 * fin} />
      <g opacity={ramp(v, T.b1 - 0.2, 0.5) * (1 - fin)}>
        <Bullet s={G1S} x0={130} y={360} cal={42} cannelure={false} color={C.dim} w={2} />
        <text x={130 + G1S.len * 21} y={450} textAnchor="middle" style={mono(1, 34)}>G1 · 0.505</text>
      </g>
      <g opacity={ramp(v, T.b2 - 0.2, 0.5) * (1 - fin)}>
        <Bullet s={G7S} x0={1520} y={360} cal={42} cannelure={false} color={C.dim} w={2} />
        <text x={1520 + G7S.len * 21} y={450} textAnchor="middle" style={mono(1, 34)}>G7 · 0.253</text>
      </g>
      {[130 + G1S.len * 21, 1520 + G7S.len * 21].map((x, i) => (
        <path key={i} d={`M${x - 16},500 l12,14 l24,-30`} stroke={C.line} strokeWidth={4} fill="none" strokeLinecap="round" strokeLinejoin="round"
          opacity={ramp(v, T.right + i * 0.2, 0.4) * (1 - fin)} />
      ))}
      <g opacity={1 - fin}>
        <Ruler y={660} f={rulerA} max={1.0} step={0.05} name="G1" val="0.505" o={1} />
        <Ruler y={820} f={rulerB} max={0.5} step={0.025} name="G7" val="0.253" o={1} />
        <line x1={PX} y1={420} x2={PX} y2={lerp(420, 860, ramp(v, T.same - 0.3, 0.8, ease))} stroke={C.drag} strokeWidth={2.5} strokeDasharray="8 6"
          opacity={ramp(v, T.same - 0.3, 0.3)} />
        <text x={960} y={960} textAnchor="middle" style={label(ramp(v, T.only, 0.5) * 0.85, { fontSize: 24 })}>SAME BULLET · DIFFERENT STANDARD</text>
      </g>
      {/* without its table */}
      <g opacity={fin}>
        <text x={960} y={640} textAnchor="middle" style={mono(1, 120, { fontWeight: 600 })}>0.253</text>
        <text x={1210} y={640} style={mono(ramp(v, T.why + 0.4, 0.4), 120, { fill: C.drag })}>?</text>
        <text x={960} y={760} textAnchor="middle" style={label(ramp(v, T.nothing - 0.6, 0.5), { fontSize: 30 })}>A BC WITHOUT ITS TABLE MEANS NOTHING</text>
      </g>
    </Scene>
  );
};
