// Episode 3, scene 1 — hook: last time the dot flew its steps in still air; one line stayed empty, the side line
// (vz = 0, so az = 0, with its bookmark from episode 2). Then the air starts to move, the side line lights up, and the
// title: the wind.
import React from "react";
import { C, lerp, ramp, window01 } from "../theme";
import { P, Pen, label } from "../lib/draw";
import { Dot, FLIGHT, Scene, SideLines, WindStreaks, useVoiceTime } from "../ep02/kit";
import { cue } from "./timing";

const N = 1;
const T = {
  last: cue(N, "Last time"), still: cue(N, "still air"), six: cue(N, "Six numbers"), two: cue(N, "two forces"), steps: cue(N, "small steps"),
  one: cue(N, "One line"), side: cue(N, "sideline"), noVel: cue(N, "No side velocity"), today: cue(N, "Today"), wind: cue(N, "wind"),
};

// the level-bore flight, side view: 1.5 px per metre downrange, 14 px per metre of fall
const px = (x: number) => 210 + x * 1.5, py = (y: number) => 220 - y * 13;
const PATH: P[] = FLIGHT.map((r) => [px(r.x), py(r.y)] as P);
const STEPS = FLIGHT.filter((_, i) => i % 5 === 0);

export const S01Hook: React.FC = () => {
  const v = useVoiceTime();
  const fly = ramp(v, -0.3, Math.max(1, T.one - 0.3));
  const flightO = (1 - 0.75 * ramp(v, T.one - 0.3, 0.6)) * (1 - ramp(v, T.today - 0.2, 0.5));
  const head = Math.min(FLIGHT.length - 1, Math.floor(fly * (FLIGHT.length - 1)));
  const zHot = ramp(v, T.today + 0.2, 0.6);
  const titleK = ramp(v, T.wind - 0.1, 1.0);
  const words = [
    { t: T.six, s: "6 NUMBERS" }, { t: T.two, s: "2 FORCES" }, { t: T.steps, s: "A LOT OF SMALL STEPS" },
  ];

  return (
    <Scene n={N}>
      {/* last time: the dot, step by step, in still air */}
      <g opacity={flightO}>
        <line x1={px(0)} y1={py(0)} x2={px(1000)} y2={py(0)} stroke={C.faint} strokeWidth={1.5} strokeDasharray="6 8" />
        <Pen pts={PATH} p={fly} w={2.5} tip={false} opacity={0.7} />
        {STEPS.filter((r) => r.x <= FLIGHT[head].x).map((r, i) => <circle key={i} cx={px(r.x)} cy={py(r.y)} r={4.5} fill={C.line} />)}
        <Dot x={px(FLIGHT[head].x)} y={py(FLIGHT[head].y)} r={11} />
        <text x={960} y={110} textAnchor="middle" style={label(window01(v, T.still - 0.3, T.one, 0.3), { fontSize: 30 })}>STILL AIR</text>
        {words.map((w, i) => (
          <text key={i} x={560 + i * 400} y={500} textAnchor="middle" style={label(ramp(v, w.t - 0.2, 0.4) * 0.85, { fontSize: 26 })}>{w.s}</text>
        ))}
      </g>

      {/* one line stayed empty: the side line */}
      <SideLines cx={960} y={600} reveal={ramp(v, T.one - 0.1, 1.0)} zHot={zHot} note={ramp(v, T.noVel - 0.2, 0.5)} mark={ramp(v, T.side, 0.5)}
        o={ramp(v, T.one - 0.3, 0.5)} />

      {/* today: the wind */}
      <WindStreaks v={v} t0={T.today - 0.2} />
      <g opacity={ramp(v, T.today, 0.5)}>
        <clipPath id="t3hClip"><rect x={0} y={150} width={lerp(300, 1800, titleK)} height={200} /></clipPath>
        <text x={960} y={300} textAnchor="middle" clipPath="url(#t3hClip)" style={label(1, { fontSize: 100, letterSpacing: "0.32em", fontWeight: 200 })}>THE WIND</text>
        <text x={960} y={360} textAnchor="middle" style={label(ramp(v, T.wind + 0.6, 0.6) * 0.6, { fontSize: 22 })}>PHYSICS OF A BALLISTIC CALCULATOR · 3</text>
      </g>
    </Scene>
  );
};
