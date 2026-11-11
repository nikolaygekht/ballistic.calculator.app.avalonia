// Scene 8 — the honest part. The program flies the dot (the 2.5D stage of scene 1), then puts the four effects on top,
// all at once; facing the target: the hit moves, the
// velocity and the time stay. A 4DOF model follows the nose. But all four are smaller than a wrong drag curve:
// G1 against the radar curve, 57 cm at 1000 m.
import React from "react";
import { C, ease, easeInOut, ramp, window01 } from "../theme";
import { Arrow, P, Pen, label, mono } from "../lib/draw";
import { Card, Dot, EFFECTS, Scene, SpinBullet, useVoiceTime } from "./kit";
import { Cam, Ground, Path3, ScaleNote, Sheet, Target3, mixCam, pathAt, pr } from "./stage3d";
import { cue } from "./timing";

const N = 8;
const T = {
  really: cue(N, "doesn't really"), dot: cue(N, "flies the dot"), adds: cue(N, "adds"), hits: cue(N, "where the bullet hits"), vel: cue(N, "velocity"),
  tof: cue(N, "time of flight"), models: cue(N, "models"), four: cue(N, "four degrees"), angle: cue(N, "angle"), itself: cue(N, "by itself"),
  sizes: cue(N, "sizes"), smaller: cue(N, "smaller"), remember: cue(N, "Remember"), n57: cue(N, "57"), good: cue(N, "good drag curve"),
};

const BACK: Cam = { yaw: 45, pitch: 16, dist: 1700, focal: 2150, target: [560, -100, 40], sx: 930, sy: 450 };
const FACE: Cam = { yaw: 90, pitch: 0, dist: 640, focal: 2050, target: [1000, pathAt(1000)[1] + 50, 85], sx: 960, sy: 560 };
const BARS = [["SPIN DRIFT", 31], ["AERODYNAMIC JUMP", 11], ["CORIOLIS", 10], ["EAST – WEST", 12]] as const;

export const S08Honest: React.FC = () => {
  const v = useVoiceTime();
  const aO = 1 - ramp(v, T.models - 0.5, 0.5);
  const stage = ramp(v, -0.3, 0.8);
  const fly = ramp(v, T.dot - 0.6, 1.8);
  const all = ramp(v, T.adds - 0.1, 1.4, ease);
  const k = [all, all, all, all];
  const face = ramp(v, T.hits - 0.4, 1.2, easeInOut);
  const cam = mixCam(BACK, FACE, face);
  const head = pr(cam, pathAt(1000 * fly, k));
  const h0 = pr(cam, pathAt(1000)), h1 = pr(cam, pathAt(1000, [1, 1, 1, 1]));
  const bO = window01(v, T.models - 0.3, T.sizes - 0.2, 0.4);
  const cO = ramp(v, T.sizes - 0.3, 0.5);

  return (
    <Scene n={N}>
      {/* ---------------- the dot first, then the four on top, all at once; then the hit moves, the velocity and the time stay */}
      <g opacity={aO}>
        <text x={960} y={110} textAnchor="middle" style={label(ramp(v, T.really - 0.2, 0.4) * (1 - ramp(v, T.hits - 0.6, 0.4)) * 0.85, { fontSize: 28 })}>
          NO SPINNING BULLET INSIDE · THE DOT FIRST<tspan opacity={ramp(v, T.adds - 0.2, 0.4)}> · THEN THE FOUR</tspan>
        </text>
        <g opacity={stage * (1 - face)}>
          <Ground c={cam} o={1} />
          <Sheet c={cam} o={1 - 0.6 * all} top={120} />
          {all > 0 && <Path3 c={cam} k={[0, 0, 0, 0]} o={0.45} w={2} dash="8 8" />}
          <Path3 c={cam} k={k} p={fly} w={all > 0 ? 4 : 2.5} stakes={ramp(v, T.dot, 0.8)} shadow={ramp(v, T.dot, 0.8)} glow={all > 0.5} />
          <Dot x={head[0]} y={head[1]} r={11} o={fly > 0 ? 1 : 0} />
          <ScaleNote x={1880} y={760} anchor="end" o={all} />
        </g>
        <Target3 c={cam} o={stage} edge={1 - face} />
        {EFFECTS.map((s, i) => (
          <Card key={i} x={330 + i * 420} y={860} w={380} text={s} o={ramp(v, T.adds - 0.3, 0.5) * (1 - face)} hot={all * (1 - ramp(v, T.adds + 1.6, 0.6))} icon={i < 2 ? "spin" : "earth"} />
        ))}
        {/* at the target: the hit moves */}
        <g opacity={face}>
          <circle cx={h0[0]} cy={h0[1]} r={14} fill="none" stroke={C.line} strokeWidth={2.5} strokeDasharray="5 5" />
          <text x={h0[0] - 24} y={h0[1] + 8} textAnchor="end" style={label(0.8, { fontSize: 20 })}>THE DOT'S HIT</text>
          <Arrow x1={h0[0]} y1={h0[1]} x2={h1[0]} y2={h1[1]} p={ramp(v, T.hits + 0.6, 0.6, ease)} w={4} head={16} />
          <g opacity={ramp(v, T.hits + 1.0, 0.4)}>
            <circle cx={h1[0]} cy={h1[1]} r={13} fill={C.line} filter="url(#glow)" />
            <text x={h1[0] + 26} y={h1[1] - 16} style={label(1, { fontSize: 22 })}>THE HIT MOVES</text>
          </g>
          <ScaleNote x={960} y={1010} anchor="middle" o={1} />
        </g>
        <g opacity={ramp(v, T.vel - 0.3, 0.5)}>
          <text x={260} y={200} style={mono(1, 40)}>v = 322 m/s</text>
          <text x={260} y={260} style={mono(ramp(v, T.tof - 0.3, 0.5), 40)}>t = 2.01 s</text>
          <text x={260} y={310} style={label(ramp(v, T.tof, 0.5) * 0.8, { fontSize: 22 })}>AT 1000 m · UNCHANGED</text>
        </g>
      </g>

      {/* ---------------- a 4DOF model follows the nose */}
      <g opacity={bO}>
        <line x1={420} y1={600} x2={1500} y2={600} stroke={C.dim} strokeWidth={2} strokeDasharray="8 8" />
        <text x={1500} y={640} textAnchor="end" style={label(0.6, { fontSize: 18 })}>THE PATH</text>
        <SpinBullet id="h" cx={960} cy={600} cal={60} deg={8} spin={v * 1.2} marks={0.8} />
        {(() => {
          const r = 260, pts: P[] = Array.from({ length: 11 }, (_, i) => { const a = (-8 * Math.PI) / 180 * (i / 10); return [960 + r * Math.cos(a), 600 + r * Math.sin(a)]; });
          return <g opacity={ramp(v, T.angle - 0.2, 0.4)}><Pen pts={pts} p={1} w={3} tip={false} /><text x={1240} y={560} style={label(0.9, { fontSize: 20 })}>THE NOSE'S ANGLE</text></g>;
        })()}
        <text x={960} y={330} textAnchor="middle" style={label(ramp(v, T.four - 0.2, 0.4), { fontSize: 28 })}>A MODEL WITH FOUR DEGREES OF FREEDOM FOLLOWS THE NOSE</text>
        <text x={960} y={830} textAnchor="middle" style={label(ramp(v, T.itself - 0.3, 0.4) * 0.85, { fontSize: 26 })}>IT GETS SPIN DRIFT BY ITSELF</text>
      </g>

      {/* ---------------- the sizes */}
      <g opacity={cO}>
        <text x={960} y={160} textAnchor="middle" style={label(0.85, { fontSize: 28 })}>AT 1000 m</text>
        {BARS.map(([s, cm], i) => {
          const y = 300 + i * 110, w = cm * 16 * ramp(v, T.sizes + i * 0.2, 0.6, ease);
          return (
            <g key={s}>
              <text x={520} y={y + 10} textAnchor="end" style={label(0.85, { fontSize: 22 })}>{s}</text>
              <rect x={560} y={y - 22} width={w} height={44} fill={C.line} opacity={0.75} />
              <text x={560 + w + 18} y={y + 12} style={mono(1, 32, { fontWeight: 600 })}>{cm} cm</text>
            </g>
          );
        })}
        <g opacity={ramp(v, T.remember - 0.2, 0.4)}>
          {(() => {
            const y = 300 + 4 * 110 + 30, w = 57 * 16 * ramp(v, T.n57 - 0.6, 0.8, ease);
            return (
              <>
                <text x={520} y={y + 10} textAnchor="end" style={label(1, { fontSize: 22, fill: C.drag })}>G1 vs THE RADAR CURVE</text>
                <rect x={560} y={y - 26} width={w} height={52} fill={C.drag} filter="url(#glow)" />
                <text x={560 + w + 18} y={y + 14} style={mono(ramp(v, T.n57 - 0.2, 0.4), 40, { fontWeight: 700, fill: C.drag })}>57 cm</text>
              </>
            );
          })()}
        </g>
        <text x={960} y={980} textAnchor="middle" style={label(ramp(v, T.good - 0.2, 0.5), { fontSize: 32 })}>FOR A LONG SHOT: A GOOD DRAG CURVE FIRST</text>
      </g>
    </Scene>
  );
};
