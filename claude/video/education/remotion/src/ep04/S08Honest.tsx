// Scene 8 — the honest part. The program flies the dot, then puts the four effects on top: the hit moves, the
// velocity and the time stay. A 4DOF model follows the nose. But all four are smaller than a wrong drag curve:
// G1 against the radar curve, 57 cm at 1000 m.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, mono } from "../lib/draw";
import { Card, Dot, EFFECTS, FLIGHT, Scene, SpinBullet, atX, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 8;
const T = {
  really: cue(N, "doesn't really"), dot: cue(N, "flies the dot"), adds: cue(N, "adds"), hits: cue(N, "where the bullet hits"), vel: cue(N, "velocity"),
  tof: cue(N, "time of flight"), models: cue(N, "models"), four: cue(N, "four degrees"), angle: cue(N, "angle"), itself: cue(N, "by itself"),
  sizes: cue(N, "sizes"), smaller: cue(N, "smaller"), remember: cue(N, "Remember"), n57: cue(N, "57"), good: cue(N, "good drag curve"),
};

const px = (x: number) => 160 + x * 1.5, py = (y: number) => 300 - y * 18;
const PATH: P[] = FLIGHT.map((r) => [px(r.x), py(r.y)] as P);
const BARS = [["SPIN DRIFT", 31], ["AERODYNAMIC JUMP", 11], ["CORIOLIS", 10], ["EAST – WEST", 12]] as const;

export const S08Honest: React.FC = () => {
  const v = useVoiceTime();
  const aO = 1 - ramp(v, T.models - 0.5, 0.5);
  const fly = ramp(v, -0.2, Math.max(1, T.adds - 0.4), ease);
  const head = atX(1000 * fly);
  const end: P = [px(1000), py(atX(1000).y)];
  const snap = ramp(v, T.adds - 0.1, 1.2, ease);
  const shift = ramp(v, T.hits - 0.2, 0.8, ease);
  const bO = window01(v, T.models - 0.3, T.sizes - 0.2, 0.4);
  const cO = ramp(v, T.sizes - 0.3, 0.5);

  return (
    <Scene n={N}>
      {/* ---------------- the dot first, then the four on top */}
      <g opacity={aO}>
        <text x={960} y={110} textAnchor="middle" style={label(ramp(v, T.really - 0.2, 0.4) * 0.85, { fontSize: 28 })}>NO SPINNING BULLET INSIDE · THE DOT FIRST</text>
        <Pen pts={PATH} p={fly} w={3} tip={false} />
        <Dot x={px(head.x)} y={py(head.y)} r={11} />
        {EFFECTS.map((s, i) => {
          const from: P = [960, 1150], to: P = [end[0] - 240, 640 + i * 78];
          return <Card key={i} x={lerp(from[0], to[0], snap)} y={lerp(from[1], to[1], snap)} w={380} text={s} o={snap} icon={i < 2 ? "spin" : "earth"} />;
        })}
        {/* the hit moves; the velocity and the time stay */}
        <g opacity={shift}>
          <circle cx={end[0]} cy={end[1]} r={11} fill="none" stroke={C.line} strokeWidth={2} strokeDasharray="4 4" />
          <circle cx={end[0] + 4} cy={end[1] - 26 * shift} r={11} fill={C.line} filter="url(#glow)" />
          <text x={end[0] + 28} y={end[1] - 34} style={label(1, { fontSize: 20 })}>THE HIT MOVES</text>
        </g>
        <g opacity={ramp(v, T.vel - 0.3, 0.5)}>
          <text x={700} y={500} style={mono(1, 36)}>v = 322 m/s</text>
          <text x={700} y={560} style={mono(ramp(v, T.tof - 0.3, 0.5), 36)}>t = 2.01 s</text>
          <text x={700} y={610} style={label(ramp(v, T.tof, 0.5) * 0.75, { fontSize: 20 })}>AT 1000 m · UNCHANGED</text>
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
