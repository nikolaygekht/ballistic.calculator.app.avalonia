// Episode 3, scene 4 — outro: every formula of the episode on one screen, lit as it is named; then the dot grows back into a
// spinning bullet over a turning planet — next time, what a dot can't do.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Bullet, Formula, P, SMK, Tok, label, outline, pathD } from "../lib/draw";
import { Dot, Scene, sub, t_, useVoiceTime, vec } from "../ep02/kit";
import { cue } from "./timing";

const N = 4;
const T = {
  model: cue(N, "whole model"), six: cue(N, "six"), two: cue(N, "two forces"), drag: cue(N, "drag against"), gravity: cue(N, "gravity"),
  steps: cue(N, "small steps"), notDot: cue(N, "not a dot"), spins: cue(N, "spins"), nose: cue(N, "nose"), planet: cue(N, "planet"),
  next: cue(N, "Next time"), cant: cue(N, "can't"), fixes: cue(N, "fixes"),
};

const K = (k: string, toks: Tok[]): Tok[] => toks.map((t) => ({ ...t, key: k }));
const ROWS: { toks: Tok[]; y: number; cx: number }[] = [
  { toks: K("six", [vec("r"), t_(" = (x, y, z)")]), y: 230, cx: 640 },
  { toks: K("six", [vec("v"), t_(" = (v"), sub("x"), t_(", v"), sub("y"), t_(", v"), sub("z"), t_(")")]), y: 230, cx: 1280 },
  { toks: K("two", [vec("a"), t_(" = "), vec("F"), t_(" / m")]), y: 350, cx: 960 },
  { toks: K("drag", [vec("a", undefined, C.drag), sub("drag", undefined, C.drag), t_(" = − D · "), vec("v"), sub("air"), t_(" / |"), vec("v"), sub("air"), t_("|")]), y: 470, cx: 960 },
  { toks: K("g", [t_("+ "), vec("g"), t_(" = (0, −g, 0)")]), y: 580, cx: 960 },
  { toks: K("steps", [vec("v"), sub("new"), t_(" = "), vec("v"), t_(" + "), vec("a"), t_(" · Δt")]), y: 710, cx: 960 },
  { toks: K("steps", [vec("r"), sub("new"), t_(" = "), vec("r"), t_(" + "), vec("v"), sub("new"), t_(" · Δt")]), y: 800, cx: 960 },
];

export const S04Outro: React.FC = () => {
  const v = useVoiceTime();
  const fO = 1 - ramp(v, T.notDot - 0.3, 0.7);
  const active =
    v >= T.steps - 0.2 ? "steps" : v >= T.gravity - 0.2 ? "g" : v >= T.drag - 0.2 ? "drag" : v >= T.two - 0.2 ? "two" : v >= T.six - 0.2 ? "six" : null;
  // the dot, a spiral, the bullet
  const CX = 960, CY = 520;
  const helix = ramp(v, T.notDot + 0.2, 1.0);
  const grow = ramp(v, T.spins - 0.3, 1.3, ease);
  const cal = lerp(2, 92, grow);
  const x0 = CX - (SMK.len * cal) / 2;
  const phase = Math.max(0, v - T.spins) * 2.4;
  const bodyA = x0 + SMK.btLen * cal, bodyB = x0 + (SMK.len - SMK.noseLen) * cal;
  const noseHot = window01(v, T.nose - 0.15, T.planet, 0.3);
  const OUT = outline(SMK, x0, CY, cal, 120);
  const planetP = ramp(v, T.planet - 0.3, 1.0);
  const spinG = Math.max(0, v - T.planet) * 0.12;
  const titleK = ramp(v, T.next, 1.2);

  return (
    <Scene n={N}>
      <g opacity={fO}>
        {ROWS.map((r, i) => (
          <Formula key={i} toks={r.toks} cx={r.cx} y={r.y} size={48} reveal={ramp(v, -0.4 + i * 0.22, 0.7)} active={active} lit={new Set([active ?? "", "six", "two", "drag", "g", "steps"])} />
        ))}
        <text x={960} y={120} textAnchor="middle" style={label(ramp(v, T.model - 0.2, 0.5) * 0.8, { fontSize: 26 })}>THE WHOLE MODEL</text>
        <text x={960} y={920} textAnchor="middle" style={label(window01(v, T.steps - 0.2, T.notDot, 0.3), { fontSize: 26 })}>… AND A LOT OF SMALL STEPS</text>
      </g>

      {/* a real bullet is not a dot */}
      <g opacity={ramp(v, T.notDot - 0.3, 0.6)}>
        {/* a thin spiral line winds around the dot */}
        {helix > 0 && grow < 1 && (
          <path d={pathD(Array.from({ length: 121 }, (_, i) => {
            const u = i / 120;
            return [CX - 220 + 440 * u, CY + (30 + 30 * grow) * Math.sin(u * Math.PI * 10 + phase * 3)] as P;
          }).slice(0, Math.max(2, Math.round(121 * helix))))} stroke={C.line} strokeWidth={2} fill="none" opacity={0.6 * (1 - grow)} />
        )}
        {grow < 0.15 && <Dot x={CX} y={CY} r={12} />}
        {grow > 0 && (
          <g opacity={ramp(v, T.spins - 0.3, 0.4)}>
            <Bullet x0={x0} y={CY} cal={cal} cannelure={false} />
            {/* the spin: engraving lines running around the body */}
            <clipPath id="body11"><rect x={bodyA} y={CY - cal * 0.5} width={bodyB - bodyA} height={cal} /></clipPath>
            <g clipPath="url(#body11)" opacity={0.55 * grow}>
              {Array.from({ length: 8 }, (_, i) => {
                const span = bodyB - bodyA, x = bodyA + ((((i / 8) * span + phase * span * 0.35) % span) + span) % span;
                return <line key={i} x1={x - cal * 0.12} y1={CY - cal * 0.5} x2={x + cal * 0.12} y2={CY + cal * 0.5} stroke={C.line} strokeWidth={2} />;
              })}
            </g>
            <path d={pathD(OUT.filter(([x]) => x >= bodyB))} stroke={C.line} strokeWidth={7} fill="none" filter="url(#glow)" opacity={noseHot} />
          </g>
        )}
        <text x={CX} y={CY - cal * 0.5 - 50} textAnchor="middle" style={label(window01(v, T.spins - 0.1, T.nose, 0.3), { fontSize: 28 })}>IT SPINS</text>
        <text x={CX + (SMK.len * cal) / 2 - 40} y={CY - cal * 0.5 - 50} textAnchor="middle" style={label(noseHot, { fontSize: 28 })}>IT HAS A NOSE</text>
        {/* a planet that turns */}
        <g opacity={planetP * (1 - 0.4 * ramp(v, T.next, 0.6))}>
          <path d={pathD(Array.from({ length: 81 }, (_, i) => { const a = Math.PI * (1.18 + 0.64 * (i / 80)); return [CX + 1300 * Math.cos(a), 2120 + 1300 * Math.sin(a)] as P; }))}
            stroke={C.line} strokeWidth={2.5} fill="none" />
          {Array.from({ length: 9 }, (_, i) => {
            const lon = ((i / 9 + spinG) % 1) * Math.PI - Math.PI / 2;           // meridians sliding: the planet turns
            const x = CX + 1250 * Math.sin(lon), top = 2120 - Math.sqrt(Math.max(0, 1300 * 1300 - (x - CX) ** 2));
            return <line key={i} x1={x} y1={top} x2={CX + (x - CX) * 0.85} y2={1080} stroke={C.line} strokeWidth={1.5} opacity={0.3 * Math.cos(lon)} />;
          })}
          <text x={CX} y={1010} textAnchor="middle" style={label(ramp(v, T.planet, 0.5) * (1 - ramp(v, T.next, 0.5)), { fontSize: 24 })}>OVER A PLANET THAT TURNS</text>
        </g>
      </g>

      {/* next time */}
      <g opacity={ramp(v, T.next - 0.2, 0.5)}>
        <text x={960} y={190} textAnchor="middle" style={label(ramp(v, T.next, 0.5) * 0.7, { fontSize: 26 })}>NEXT TIME</text>
        <clipPath id="t3Clip"><rect x={0} y={200} width={lerp(300, 1800, titleK)} height={140} /></clipPath>
        <text x={960} y={300} textAnchor="middle" clipPath="url(#t3Clip)" style={label(1, { fontSize: 70, letterSpacing: "0.26em", fontWeight: 200 })}>WHAT A DOT CAN'T DO</text>
        <text x={960} y={352} textAnchor="middle" style={label(ramp(v, T.fixes - 0.4, 0.6) * 0.6, { fontSize: 22 })}>PHYSICS OF A BALLISTIC CALCULATOR · 4</text>
      </g>
    </Scene>
  );
};
