// Scene 1 — hook: the dot grows a nose and starts to spin (3 100 turns per second). Four small effects from the first
// series' table: two from the spin, two from the turning Earth. A dot can't do any of it, so the program flies the dot
// first and puts the four on top. Title.
import React from "react";
import { C, lerp, ramp } from "../theme";
import { P, Pen, label, mono } from "../lib/draw";
import { Card, Dot, EFFECTS, FLIGHT, Scene, SpinBullet, atX, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 1;
const T = {
  dot: cue(N, "dot"), nose: cue(N, "nose"), spins: cue(N, "spins"), turns: cue(N, "3"), four: cue(N, "four small"),
  spin: cue(N, "Two of them"), earth: cue(N, "Two come"), cant: cue(N, "can't spin"), first: cue(N, "flies the dot"),
  adds: cue(N, "adds"), today: cue(N, "Today"),
};

const px = (x: number) => 160 + x * 1.6, py = (y: number) => 330 - y * 20;
const PATH: P[] = FLIGHT.map((r) => [px(r.x), py(r.y)] as P);
const at = (x: number): P => { const r = atX(x); return [px(x), py(r.y)]; };
const slopeDeg = (x: number) => { const a = at(Math.max(0, x - 5)), b = at(Math.min(1000, x + 5)); return (-Math.atan2(b[1] - a[1], b[0] - a[0]) * 180) / Math.PI; };

export const S01Hook: React.FC = () => {
  const v = useVoiceTime();
  // the bullet's ride: slowly to 380 m while it is shown, then the dot runs to the end
  const xB = lerp(0, 380, ramp(v, -0.3, T.cant + 0.5));
  const xD = lerp(xB, 1000, ramp(v, T.first - 0.2, 2.4));
  const x = v < T.first - 0.2 ? xB : xD;
  const head = at(x);
  const grow = ramp(v, T.nose - 0.3, 0.6) * (1 - ramp(v, T.cant - 0.1, 0.6));
  const marks = ramp(v, T.spins - 0.1, 0.5);
  const spin = Math.max(0, v - T.spins) * 1.4;
  const pathO = 0.25 + 0.75 * ramp(v, T.first - 0.2, 0.5);
  const toEnd = ramp(v, T.adds - 0.1, 1.0);
  const titleO = ramp(v, T.today - 0.2, 0.8);
  const end = at(1000);

  return (
    <Scene n={N}>
      <g opacity={1 - 0.6 * titleO}>
        <line x1={px(0)} y1={py(0)} x2={px(1000)} y2={py(0)} stroke={C.faint} strokeWidth={1.5} strokeDasharray="6 8" />
        <Pen pts={PATH} p={x / 1000} w={2.5} tip={false} opacity={pathO} />
        <Dot x={head[0]} y={head[1]} r={lerp(11, 4, grow)} o={1 - grow} />
        <g transform={`translate(${head[0]},${head[1]}) scale(${lerp(0.2, 1, grow)}) translate(${-head[0]},${-head[1]})`}>
          <SpinBullet id="h1" cx={head[0]} cy={head[1]} cal={34} deg={slopeDeg(x)} spin={spin} marks={marks} o={grow} />
        </g>
        <g opacity={ramp(v, T.turns - 0.2, 0.5) * (1 - ramp(v, T.four - 0.4, 0.5))}>
          <text x={head[0]} y={head[1] - 70} textAnchor="middle" style={mono(1, 40, { fontWeight: 700 })}>3 100</text>
          <text x={head[0]} y={head[1] - 38} textAnchor="middle" style={label(0.75, { fontSize: 20 })}>TURNS PER SECOND</text>
        </g>
      </g>

      {/* the four effects: from the spin, from the turning Earth; then onto the end of the dot's path */}
      {EFFECTS.map((s, i) => {
        const from: P = [330 + i * 420, 820], to: P = [end[0] - 220, 700 + i * 80];
        const xy: P = [lerp(from[0], to[0], toEnd), lerp(from[1], to[1], toEnd)];
        const hot = i < 2 ? ramp(v, T.spin - 0.1, 0.4) * (1 - ramp(v, T.earth - 0.1, 0.4)) : ramp(v, T.earth - 0.1, 0.4) * (1 - ramp(v, T.cant, 0.4));
        return (
          <g key={i} opacity={1 - 0.7 * titleO}>
            <Card x={xy[0]} y={xy[1]} w={380} text={s} o={ramp(v, T.four - 0.2 + i * 0.25, 0.4)} hot={hot} icon={i < 2 ? "spin" : "earth"} />
            {toEnd > 0.95 && <line x1={end[0]} y1={end[1]} x2={xy[0] + 190} y2={xy[1]} stroke={C.line} strokeWidth={1.5} opacity={0.4} />}
          </g>
        );
      })}
      <text x={540} y={910} textAnchor="middle" style={label(ramp(v, T.spin, 0.4) * (1 - ramp(v, T.cant, 0.4)) * 0.8, { fontSize: 22 })}>FROM THE SPIN</text>
      <text x={1380} y={910} textAnchor="middle" style={label(ramp(v, T.earth, 0.4) * (1 - ramp(v, T.cant, 0.4)) * 0.8, { fontSize: 22 })}>FROM THE TURNING EARTH</text>
      <text x={960} y={130} textAnchor="middle" style={label(ramp(v, T.cant - 0.2, 0.4) * (1 - ramp(v, T.today - 0.4, 0.4)) * 0.85, { fontSize: 28 })}>
        A DOT CAN'T SPIN · IT DOESN'T KNOW THE EARTH TURNS
      </text>
      <text x={960} y={180} textAnchor="middle" style={label(ramp(v, T.first - 0.2, 0.4) * (1 - ramp(v, T.today - 0.4, 0.4)) * 0.7, { fontSize: 22 })}>
        FIRST THE DOT · THEN THE FOUR, ON TOP
      </text>

      <g opacity={titleO}>
        <text x={960} y={190} textAnchor="middle" style={label(1, { fontSize: 76, letterSpacing: "0.22em", fontWeight: 200 })}>WHAT A DOT CAN'T DO</text>
        <text x={960} y={250} textAnchor="middle" style={label(0.6, { fontSize: 22 })}>PHYSICS OF A BALLISTIC CALCULATOR · 4</text>
      </g>
    </Scene>
  );
};
