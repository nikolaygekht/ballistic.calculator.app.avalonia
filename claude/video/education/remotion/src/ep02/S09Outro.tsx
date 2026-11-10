// Scene 9 — outro: every formula of the episode on one screen, lit as it is named; then the three component lines
// come back with the side line still empty (its bookmark from scene 5), and the air starts to move: next time, the wind.
import React from "react";
import { C, lerp, ramp, window01 } from "../theme";
import { Formula, Tok, label } from "../lib/draw";
import { Scene, SideLines, WindStreaks, sub, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 9;
const T = {
  model: cue(N, "whole model"), six: cue(N, "Six numbers"), two: cue(N, "Two forces"), drag: cue(N, "drag against"), gravity: cue(N, "gravity"),
  steps: cue(N, "small steps"), far: cue(N, "so far"), still: cue(N, "stood still"), side: cue(N, "sideline"), empty: cue(N, "empty"),
  next: cue(N, "Next time"), wind: cue(N, "wind"),
};

const K = (k: string, toks: Tok[]): Tok[] => toks.map((t) => ({ ...t, key: k }));
const ROWS: { toks: Tok[]; y: number; cx: number }[] = [
  { toks: K("six", [vec("r"), t_(" = (x, y, z)")]), y: 230, cx: 640 },
  { toks: K("six", [vec("v"), t_(" = (v"), sub("x"), t_(", v"), sub("y"), t_(", v"), sub("z"), t_(")")]), y: 230, cx: 1280 },
  { toks: K("two", [vec("a"), t_(" = "), vec("F"), t_(" / m")]), y: 350, cx: 960 },
  { toks: K("drag", [vec("a", undefined, C.drag), sub("drag", undefined, C.drag), t_(" = − D · "), vec("v"), t_(" / |"), vec("v"), t_("|")]), y: 470, cx: 960 },
  { toks: K("g", [t_("+ "), vec("g"), t_(" = (0, −g, 0)")]), y: 580, cx: 960 },
  { toks: K("steps", [vec("v"), sub("new"), t_(" = "), vec("v"), t_(" + "), vec("a"), t_(" · Δt")]), y: 710, cx: 960 },
  { toks: K("steps", [vec("r"), sub("new"), t_(" = "), vec("r"), t_(" + "), vec("v"), sub("new"), t_(" · Δt")]), y: 800, cx: 960 },
];

export const S09Outro: React.FC = () => {
  const v = useVoiceTime();
  const fO = 1 - ramp(v, T.far - 0.4, 0.6);
  const active =
    v >= T.steps - 0.2 ? "steps" : v >= T.gravity - 0.2 ? "g" : v >= T.drag - 0.2 ? "drag" : v >= T.two - 0.2 ? "two" : v >= T.six - 0.2 ? "six" : null;
  const linesO = ramp(v, T.far - 0.2, 0.6) * (1 - 0.6 * ramp(v, T.next - 0.2, 0.6));
  const zHot = ramp(v, T.empty - 0.2, 0.5);
  const titleK = ramp(v, T.wind - 0.1, 1.0);

  return (
    <Scene n={N}>
      <g opacity={fO}>
        {ROWS.map((r, i) => (
          <Formula key={i} toks={r.toks} cx={r.cx} y={r.y} size={48} reveal={ramp(v, -0.4 + i * 0.22, 0.7)} active={active} lit={new Set([active ?? "", "six", "two", "drag", "g", "steps"])} />
        ))}
        <text x={960} y={120} textAnchor="middle" style={label(ramp(v, T.model - 0.2, 0.5) * 0.8, { fontSize: 26 })}>THE WHOLE MODEL</text>
        <text x={960} y={920} textAnchor="middle" style={label(window01(v, T.steps - 0.2, T.far, 0.3), { fontSize: 26 })}>… AND A LOT OF SMALL STEPS</text>
      </g>

      {/* so far: still air, and the side line stayed empty */}
      <SideLines cx={960} y={560} reveal={ramp(v, T.far - 0.1, 1.0)} zHot={zHot} note={ramp(v, T.side - 0.2, 0.5)} mark={ramp(v, T.far + 0.3, 0.5)} o={linesO} />
      <text x={960} y={430} textAnchor="middle" style={label(window01(v, T.still - 0.3, T.next, 0.3), { fontSize: 28 })}>SO FAR: STILL AIR</text>

      {/* next time: the wind */}
      <WindStreaks v={v} t0={T.next - 0.4} />
      <g opacity={ramp(v, T.next - 0.2, 0.5)}>
        <text x={960} y={190} textAnchor="middle" style={label(ramp(v, T.next, 0.5) * 0.7, { fontSize: 26 })}>NEXT TIME</text>
        <clipPath id="t3wClip"><rect x={0} y={200} width={lerp(300, 1800, titleK)} height={140} /></clipPath>
        <text x={960} y={300} textAnchor="middle" clipPath="url(#t3wClip)" style={label(1, { fontSize: 80, letterSpacing: "0.3em", fontWeight: 200 })}>THE WIND</text>
        <text x={960} y={352} textAnchor="middle" style={label(ramp(v, T.wind + 0.4, 0.6) * 0.6, { fontSize: 22 })}>PHYSICS OF A BALLISTIC CALCULATOR · 3</text>
      </g>
    </Scene>
  );
};
