// The end card of each episode: 6 silent seconds after the outro, for YouTube's end screen. Two empty frames sit
// exactly where the end-screen elements go (as in the app series: 620 × 349 at (125, 424) and (1175, 424)), each
// with a label above it saying what the viewer will find there.
import React from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import { C, FPS, MONO, ramp } from "./theme";
import { Defs, Vignette, label } from "./lib/draw";

export const END_SECONDS = 6;
export const SERIES = "HOW BALLISTIC CALCULATOR 2 WORKS";
const NAMES = ["DRAG", "HOW A DOT FLIES", "THE WIND", "WHAT A DOT CAN'T DO"];
const SLOT = { w: 620, h: 349, y: 424, x: [125, 1175] };

type Slot = { head: string; name: string };
const slots = (ep: number): [Slot, Slot] => [
  ep > 1 ? { head: `PREVIOUS · EPISODE ${ep - 1}`, name: NAMES[ep - 2] } : { head: "THE SERIES", name: "ALL FOUR EPISODES" },
  ep < 4 ? { head: `NEXT · EPISODE ${ep + 1}`, name: NAMES[ep] } : { head: "THE PROGRAM ITSELF", name: "THE VIDEO TUTORIALS" },
];

export const EndCard: React.FC<{ ep: number }> = ({ ep }) => {
  const t = useCurrentFrame() / FPS;
  const o = (a: number) => ramp(t, a, 0.7);
  return (
    <AbsoluteFill style={{ background: C.bg }}>
      <svg width={1920} height={1080} viewBox="0 0 1920 1080">
        <Defs />
        <text x={960} y={190} textAnchor="middle" style={label(o(0.1), { fontSize: 54, letterSpacing: "0.22em", fontWeight: 200 })}>{SERIES}</text>
        <text x={960} y={250} textAnchor="middle" style={label(o(0.3) * 0.6, { fontSize: 24 })}>EPISODE {ep} OF 4 · {NAMES[ep - 1]}</text>
        {slots(ep).map((s, i) => {
          const x = SLOT.x[i];
          return (
            <g key={i} opacity={o(0.5 + 0.2 * i)}>
              <text x={x} y={SLOT.y - 58} style={label(0.6, { fontSize: 20 })}>{s.head}</text>
              <text x={x} y={SLOT.y - 20} style={label(0.95, { fontSize: 30, fontWeight: 400 })}>{s.name}</text>
              <rect x={x} y={SLOT.y} width={SLOT.w} height={SLOT.h} rx={10} fill="none" stroke={C.line} strokeWidth={1.5} strokeDasharray="8 10" opacity={0.35} />
            </g>
          );
        })}
        <text x={960} y={900} textAnchor="middle" style={label(o(1.0) * 0.6, { fontSize: 22 })}>ALL THE FORMULAS: IN THE DOCUMENTATION</text>
        <text x={960} y={950} textAnchor="middle" style={{ fontFamily: MONO, fontSize: 28, fill: C.line, opacity: o(1.0) * 0.85 }}>
          nikolaygekht.github.io/ballistic.calculator.app.avalonia
        </text>
        <Vignette />
      </svg>
    </AbsoluteFill>
  );
};
