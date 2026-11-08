// Scene 10 — outro: a measured curve beats any single number; several BCs can be joined into a curve; next time
// the bullet becomes a dot and flies, step by step.
import React from "react";
import { D } from "../data";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Bullet, P, Pen, SMK, label, mono } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";
import { ChartAxes, chx, chy } from "./S06CdCurve";

const N = 10;
const T = { measured: cue(N, "measured"), program: cue(N, "The program"), build: cue(N, "build"), several: cue(N, "several"), next: cue(N, "Next"), flies: cue(N, "flies"), step: cue(N, "step") };
const cv = (ys: number[]): P[] => D.curves.mach.map((m, i) => [chx(m), chy(ys[i])] as P);
const radarAt = (m: number) => {
  const ms = D.curves.mach, ys = D.curves.radar;
  for (let i = 1; i < ms.length; i++) if (ms[i] >= m) return lerp(ys[i - 1], ys[i], (m - ms[i - 1]) / (ms[i] - ms[i - 1]));
  return ys[ys.length - 1];
};

export const S10Outro: React.FC = () => {
  const v = useVoiceTime();
  const chartO = 1 - ramp(v, T.next - 0.4, 0.6);
  const multi = ramp(v, T.build - 0.3, 0.6);
  const pts = [2.4, 2.0, 1.6, 1.25, 1.05, 0.9, 0.7];
  const join = ramp(v, T.several, 1.2);
  // the dot: the bullet shrinks to a point and flies an arc (episode 2)
  const shrink = ramp(v, T.next + 0.2, 1.2, ease);
  const flyK = ramp(v, T.flies - 0.4, 3.2);
  const arc = (k: number): P => [lerp(260, 1660, k), 640 - 260 * Math.sin(Math.PI * k * 0.92) + 120 * k];
  const arcPts: P[] = Array.from({ length: 61 }, (_, i) => arc((i / 60) * flyK));
  const steps = Array.from({ length: 13 }, (_, i) => i / 12).filter((k) => k <= flyK);
  const dot = arc(flyK);
  return (
    <Scene n={N}>
      <g opacity={chartO}>
        <ChartAxes p={1} />
        <Pen pts={cv(D.curves.fromG7)} p={1} color={C.dim} w={2.5} tip={false} opacity={1 - 0.5 * multi} />
        <text x={chx(1.1)} y={chy(0.42) - 12} style={mono(0.7 * (1 - multi), 22)}>ONE NUMBER</text>
        <g filter="url(#glow)" opacity={1 - multi}><Pen pts={cv(D.curves.radar)} p={ramp(v, 0.2, 1.2)} color={C.drag} w={4} /></g>
        <text x={chx(0.55)} y={chy(0.26)} style={label(ramp(v, T.measured - 0.2, 0.5) * (1 - multi), { fontSize: 22, fill: C.drag })}>MEASURED CURVE · BETTER</text>
        {/* several BCs, each true at one speed, joined into a curve */}
        <g opacity={multi}>
          {pts.map((m, i) => (
            <g key={i} opacity={ramp(v, T.build + i * 0.12, 0.3)}>
              <circle cx={chx(m)} cy={chy(radarAt(m))} r={9} fill={C.line} />
              <text x={chx(m)} y={chy(radarAt(m)) - 22} textAnchor="middle" style={mono(0.7, 18)}>BC</text>
            </g>
          ))}
          <g filter="url(#glow)"><Pen pts={cv(D.curves.radar).filter(([x]) => x >= chx(0.7) && x <= chx(2.4))} p={join} color={C.drag} w={3.5} /></g>
          <text x={960} y={200} textAnchor="middle" style={label(ramp(v, T.several, 0.5), { fontSize: 28 })}>SEVERAL BCs → ONE CURVE</text>
        </g>
        <text x={960} y={200} textAnchor="middle" style={label(window01(v, 0, T.build - 0.3, 0.4), { fontSize: 28 })}>SHOOTING FAR? USE A MEASURED CURVE</text>
      </g>
      {/* next time */}
      <g opacity={ramp(v, T.next - 0.3, 0.6)}>
        <g opacity={1 - shrink}>
          <Bullet x0={260 - SMK.len * lerp(60, 2, shrink)} y={640} cal={lerp(60, 2, shrink)} />
        </g>
        {shrink > 0.6 && (
          <>
            <path d={"M" + arcPts.map((q) => q.join(",")).join(" L")} stroke={C.dim} strokeWidth={2} fill="none" strokeDasharray="4 8" />
            {steps.map((k, i) => { const q = arc(k); return <circle key={i} cx={q[0]} cy={q[1]} r={4} fill={C.line} opacity={0.6} />; })}
            <circle cx={dot[0]} cy={dot[1]} r={11} fill={C.line} filter="url(#glowBig)" />
          </>
        )}
        <text x={960} y={920} textAnchor="middle" style={label(ramp(v, T.next + 0.6, 0.6), { fontSize: 26 })}>NEXT TIME</text>
        <text x={960} y={980} textAnchor="middle" style={label(ramp(v, T.flies - 0.2, 0.6), { fontSize: 46, letterSpacing: "0.3em" })}>HOW A DOT FLIES</text>
      </g>
    </Scene>
  );
};
