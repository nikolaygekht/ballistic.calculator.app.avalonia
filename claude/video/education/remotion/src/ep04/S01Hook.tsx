// Scene 1 — hook: the dot grows a nose and starts to spin (3 100 turns per second). Four small effects from the first
// series' table: two from the spin, two from the turning Earth. A dot can't do any of it: the camera swings round and
// the dot's whole path turns out to stand in one upright plane. The program flies the dot first, then the four bend the
// path out of that plane, one by one; at the target, the four add up, tip to tail. Title.
import React from "react";
import { C, ease, easeInOut, lerp, ramp } from "../theme";
import { Arrow, P, label, mono } from "../lib/draw";
import { Card, Dot, EFFECTS, Scene, SpinBullet, useVoiceTime } from "./kit";
import { Cam, EFFECT_CM, Ground, Path3, SE, ScaleNote, Sheet, Target3, V3, effects, mixCam, pathAt, pr } from "./stage3d";
import { cue } from "./timing";

const N = 1;
const T = {
  dot: cue(N, "dot"), nose: cue(N, "nose"), spins: cue(N, "spins"), turns: cue(N, "3"), four: cue(N, "four small"),
  spin: cue(N, "Two of them"), earth: cue(N, "Two come"), cant: cue(N, "can't spin"), first: cue(N, "flies the dot"),
  adds: cue(N, "adds"), today: cue(N, "Today"), counts: cue(N, "counts"),
};

// the flat picture of episode 2 (1.6 px per metre across, 20 px per metre down), seen as a camera from the side
const SIDE: Cam = { yaw: 0, pitch: 0, dist: 3000, focal: 4800, target: [500, 0, 0], sx: 960, sy: 330 };
// from behind and above the shooter's left shoulder
const BACK: Cam = { yaw: 45, pitch: 16, dist: 1700, focal: 2150, target: [560, -100, 40], sx: 930, sy: 450 };
// facing the target
const FACE: Cam = { yaw: 90, pitch: 0, dist: 640, focal: 2050, target: [1000, pathAt(1000)[1] + 50, 85], sx: 960, sy: 590 };

export const S01Hook: React.FC = () => {
  const v = useVoiceTime();
  const swing = ramp(v, T.cant + 0.2, 3.4, easeInOut);
  const face = ramp(v, T.today - 0.1, 1.2, easeInOut);
  const cam = face > 0 ? mixCam(BACK, FACE, face) : mixCam(SIDE, BACK, swing);

  // the bullet's ride: slowly to 380 m while it is shown, then the dot runs to the end
  const xB = lerp(0, 380, ramp(v, -0.3, T.cant + 0.5));
  const xD = lerp(xB, 1000, ramp(v, T.first - 0.2, 1.8));
  const x = v < T.first - 0.2 ? xB : xD;
  const kE = [0, 1, 2, 3].map((i) => ramp(v, T.adds - 0.25 + i * 0.7, 0.75, ease));
  const head = pr(cam, pathAt(x, kE));
  const ahead = pr(cam, pathAt(Math.min(1000, x + 8))), behind = pr(cam, pathAt(Math.max(0, x - 8)));
  const slope = (-Math.atan2(ahead[1] - behind[1], ahead[0] - behind[0]) * 180) / Math.PI;
  const grow = ramp(v, T.nose - 0.3, 0.6) * (1 - ramp(v, T.cant - 0.1, 0.6));
  const marks = ramp(v, T.spins - 0.1, 0.5);
  const spin = Math.max(0, v - T.spins) * 1.4;
  const pathO = 0.25 + 0.75 * ramp(v, T.first - 0.2, 0.5);

  const anyE = kE[0];
  const stageO = 1 - face;                             // the path and the ground step back while we look at the target
  const titleO = ramp(v, T.counts + 0.4, 0.8);
  const cardsO = 1 - ramp(v, T.today - 0.3, 0.6);

  // tip to tail at the target: drift →, jump ↑, Coriolis →, east–west ↑
  const e = effects(1000);
  const steps: V3[] = [pathAt(1000)];
  [[0, 0, e[0]], [0, e[1], 0], [0, 0, e[2]], [0, e[3], 0]].forEach(([, dy, dz]) => {
    const l = steps[steps.length - 1];
    steps.push([1000, l[1] + SE * dy, l[2] + SE * dz]);
  });
  const tip = (i: number) => ramp(v, T.today + 1.1 + i * 0.4, 0.4, ease);

  return (
    <Scene n={N}>
      <g opacity={1 - 0.6 * titleO}>
        {/* the ground, the plane, the target: they come with the swing */}
        <Ground c={cam} o={swing * stageO} />
        <Sheet c={cam} o={swing * (1 - 0.6 * anyE) * stageO} top={120} />
        <Target3 c={cam} o={Math.max(0, (swing - 0.6) / 0.4)} edge={1 - face} />
        <text x={pr(cam, [40, 120, 0])[0]} y={pr(cam, [40, 120, 0])[1] - 14} style={label(ramp(v, T.cant + 2.2, 0.6) * (1 - anyE) * 0.8, { fontSize: 20 })}>
          THE DOT: ALL IN ONE UPRIGHT PLANE
        </text>

        {/* the dot's path: before the effects it stays as a dashed ghost */}
        {anyE > 0 && <Path3 c={cam} k={[0, 0, 0, 0]} o={0.45 * stageO} w={2} dash="8 8" />}
        <Path3 c={cam} k={kE} p={x / 1000} o={pathO * stageO} w={anyE > 0 ? 4 : 2.5} stakes={swing * ramp(v, T.first, 0.8)} shadow={swing * ramp(v, T.first, 0.8)} glow={anyE > 0.5} />

        {/* the dot that grows a nose and spins */}
        <Dot x={head[0]} y={head[1]} r={lerp(11, 4, grow)} o={(1 - grow) * (1 - face)} />
        <g transform={`translate(${head[0]},${head[1]}) scale(${lerp(0.2, 1, grow)}) translate(${-head[0]},${-head[1]})`}>
          <SpinBullet id="h1" cx={head[0]} cy={head[1]} cal={34} deg={slope} spin={spin} marks={marks} o={grow} />
        </g>
        <g opacity={ramp(v, T.turns - 0.2, 0.5) * (1 - ramp(v, T.four - 0.4, 0.5))}>
          <text x={head[0]} y={head[1] - 70} textAnchor="middle" style={mono(1, 40, { fontWeight: 700 })}>3 100</text>
          <text x={head[0]} y={head[1] - 38} textAnchor="middle" style={label(0.75, { fontSize: 20 })}>TURNS PER SECOND</text>
        </g>
        <ScaleNote x={1880} y={760} anchor="end" o={anyE * (1 - face)} />

        {/* at the target: the dot's hit, and the four added tip to tail */}
        <g opacity={face}>
          {(() => {
            const h0 = pr(cam, steps[0]);
            return (
              <>
                <circle cx={h0[0]} cy={h0[1]} r={14} fill="none" stroke={C.line} strokeWidth={2.5} strokeDasharray="5 5" />
                <text x={h0[0] - 24} y={h0[1] + 8} textAnchor="end" style={label(0.8, { fontSize: 20 })}>THE DOT'S HIT</text>
              </>
            );
          })()}
          {EFFECTS.map((s, i) => {
            const a = pr(cam, steps[i]), b = pr(cam, steps[i + 1]), k = tip(i), up = i % 2 === 1;
            return (
              <g key={s} opacity={k > 0 ? 1 : 0}>
                <Arrow x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} p={k} w={4} head={14} />
                <text x={up ? a[0] - 20 : a[0] + 16} y={up ? (a[1] + b[1]) / 2 + 7 : a[1] + 36} textAnchor={up ? "end" : "start"}
                  style={label(ramp(v, T.today + 1.3 + i * 0.4, 0.3) * 0.9, { fontSize: 18 })}>
                  {s} · {EFFECT_CM[i]} cm
                </text>
              </g>
            );
          })}
          {(() => {
            const h = pr(cam, steps[4]);
            return (
              <g opacity={ramp(v, T.today + 2.8, 0.4)}>
                <circle cx={h[0]} cy={h[1]} r={13} fill={C.line} filter="url(#glow)" />
                <text x={h[0] + 26} y={h[1] - 16} style={label(1, { fontSize: 22 })}>THE REAL HIT</text>
              </g>
            );
          })()}
          <ScaleNote x={960} y={1010} anchor="middle" o={1} />
        </g>
      </g>

      {/* the four effects: from the spin, from the turning Earth; each lights while it bends the path */}
      {EFFECTS.map((s, i) => {
        const xy: P = [330 + i * 420, 860];
        const told = i < 2 ? ramp(v, T.spin - 0.1, 0.4) * (1 - ramp(v, T.earth - 0.1, 0.4)) : ramp(v, T.earth - 0.1, 0.4) * (1 - ramp(v, T.cant, 0.4));
        const bend = kE[i] * (1 - ramp(v, T.adds - 0.25 + i * 0.7 + 0.9, 0.4));
        return (
          <Card key={i} x={xy[0]} y={xy[1]} w={380} text={s} o={ramp(v, T.four - 0.2 + i * 0.25, 0.4) * cardsO * (1 - 0.7 * titleO)} hot={Math.max(told, bend)} icon={i < 2 ? "spin" : "earth"} />
        );
      })}
      <text x={540} y={950} textAnchor="middle" style={label(ramp(v, T.spin, 0.4) * (1 - ramp(v, T.cant, 0.4)) * 0.8, { fontSize: 22 })}>FROM THE SPIN</text>
      <text x={1380} y={950} textAnchor="middle" style={label(ramp(v, T.earth, 0.4) * (1 - ramp(v, T.cant, 0.4)) * 0.8, { fontSize: 22 })}>FROM THE TURNING EARTH</text>
      <text x={960} y={110} textAnchor="middle" style={label(ramp(v, T.cant - 0.2, 0.4) * (1 - ramp(v, T.first - 0.5, 0.4)) * 0.85, { fontSize: 28 })}>
        A DOT CAN'T SPIN · IT DOESN'T KNOW THE EARTH TURNS
      </text>
      <text x={960} y={110} textAnchor="middle" style={label(ramp(v, T.first - 0.2, 0.4) * (1 - ramp(v, T.today - 0.4, 0.4)) * 0.85, { fontSize: 28 })}>
        FIRST THE DOT<tspan opacity={ramp(v, T.adds - 0.2, 0.4)}> · THEN THE FOUR, ON TOP</tspan>
      </text>
      <text x={960} y={110} textAnchor="middle" style={label(ramp(v, T.today, 0.4) * (1 - titleO) * 0.85, { fontSize: 28 })}>AT THE TARGET, THE FOUR ADD UP</text>

      <g opacity={titleO}>
        <text x={960} y={190} textAnchor="middle" style={label(1, { fontSize: 76, letterSpacing: "0.22em", fontWeight: 200 })}>WHAT A DOT CAN'T DO</text>
        <text x={960} y={250} textAnchor="middle" style={label(0.6, { fontSize: 22 })}>PHYSICS OF A BALLISTIC CALCULATOR · 4</text>
      </g>
    </Scene>
  );
};
