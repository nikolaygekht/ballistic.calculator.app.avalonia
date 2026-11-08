// Scene 3 — the shock wave: sound rings from the nose fall behind a supersonic bullet and pile up into a cone;
// a faster bullet makes a narrower cone (half-angle asin 1/M); making the wave costs energy: wave drag.
import React from "react";
import { C, clamp01, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Bullet, P, Pen, SMK, label, mono } from "../lib/draw";
import { Scene, Streaks, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 3;
const T_WARN = cue(N, "no warning"), T_WALL = cue(N, "builds"), T_SHOCK = cue(N, "shock wave"), T_CONE = cue(N, "cone");
const T_FASTER = cue(N, "The faster"), T_NARROW = cue(N, "narrower"), T_MAKING = cue(N, "making"), T_DRAG = cue(N, "big part");

const AX = 560, CAL = 56, X0 = 1000, NOSE = X0 + SMK.len * CAL;
const CS = 160;                                                  // the speed of sound on screen, px/s

/** Mach number over voice time: slow first, past sound by "no warning", narrower cone at "narrower". */
const machAt = (v: number) => {
  let m = lerp(0.65, 2.3, ramp(v, 0.1, 2.6));
  m = lerp(m, 1.35, ramp(v, T_FASTER - 0.4, 0.6));
  m = lerp(m, 2.9, ramp(v, T_NARROW - 0.3, 1.4));
  return m;
};
const DT = 1 / 60, V0 = -1;
/** Distance flown (px) since V0. */
const flown = (() => {
  const acc: number[] = [0];
  for (let i = 1; i < 40 * 60; i++) acc.push(acc[i - 1] + machAt(V0 + (i - 0.5) * DT) * CS * DT);
  return (v: number) => {
    const k = (v - V0) / DT, i = Math.max(0, Math.min(acc.length - 2, Math.floor(k)));
    return lerp(acc[i], acc[i + 1], clamp01(k - i));
  };
})();

export const S03Shock: React.FC = () => {
  const v = useVoiceTime();
  const M = machAt(v);
  const X = flown(v);
  const mu = M > 1 ? Math.asin(1 / M) : Math.PI / 2;
  const coneO = clamp01((M - 1.05) / 0.25) * ramp(v, T_WALL - 0.3, 0.8);
  const coneHot = ramp(v, T_MAKING, 0.8);
  const ringsO = 1 - 0.55 * ramp(v, T_SHOCK, 1.0);

  // sound rings, one every 0.22 s, emitted at the nose
  const rings: { x: number; r: number; age: number }[] = [];
  for (let te = Math.floor(v / 0.22) * 0.22; te > v - 4.2 && te > V0; te -= 0.22) {
    const age = v - te;
    rings.push({ x: NOSE - (X - flown(te)), r: CS * age, age });
  }
  const LC = 720;
  const coneTop: P[] = [[NOSE, AX], [NOSE - LC * Math.cos(mu), AX - LC * Math.sin(mu)]];
  const coneBot: P[] = [[NOSE, AX], [NOSE - LC * Math.cos(mu), AX + LC * Math.sin(mu)]];
  const coneColor = coneHot > 0.5 ? C.drag : C.line;

  return (
    <Scene n={N}>
      <Streaks shift={X} y0={170} y1={950} gapY={AX} gap={40} len={60 + M * 40} o={0.6} />
      <g opacity={ringsO}>
        {rings.map((g, i) => (
          <circle key={i} cx={g.x} cy={AX} r={g.r} fill="none" stroke={C.line} strokeWidth={1.6} opacity={clamp01(1 - g.age / 4.2) * 0.7} />
        ))}
      </g>
      <g opacity={coneO} filter={coneHot > 0 ? "url(#glow)" : undefined}>
        <Pen pts={coneTop} p={ramp(v, T_WALL - 0.3, 1.0)} color={coneColor} w={3 + coneHot * 2} tip={false} />
        <Pen pts={coneBot} p={ramp(v, T_WALL - 0.3, 1.0)} color={coneColor} w={3 + coneHot * 2} tip={false} />
      </g>
      {/* the half-angle */}
      {(() => {
        const o = window01(v, T_CONE, T_MAKING, 0.4);
        const R = 330, pts: P[] = Array.from({ length: 16 }, (_, i) => {
          const a = Math.PI - (mu * i) / 15;
          return [NOSE + R * Math.cos(a), AX - R * Math.sin(a)];
        });
        return (
          <g opacity={o}>
            <line x1={NOSE} y1={AX} x2={NOSE - 420} y2={AX} stroke={C.dim} strokeWidth={1.2} strokeDasharray="5 7" />
            <polyline points={pts.map((q) => q.join(",")).join(" ")} stroke={C.line} strokeWidth={2} fill="none" />
            <text x={NOSE - R - 18} y={AX - 22} textAnchor="end" style={mono(1, 26)}>{((mu * 180) / Math.PI).toFixed(0)}°</text>
          </g>
        );
      })()}
      <Bullet x0={X0} y={AX} cal={CAL} p={ramp(v, -0.6, 1.0)} />
      <g filter="url(#glow)">
        <Arrow x1={X0 - 24} y1={AX} x2={X0 - 24 - 330} y2={AX} p={ramp(v, T_DRAG - 0.6, 0.9, ease)} color={C.drag} w={5} head={22} />
      </g>
      <text x={X0 - 190} y={AX + 52} textAnchor="middle" style={label(ramp(v, T_DRAG - 0.2, 0.5), { fontSize: 24, fill: C.drag })}>WAVE DRAG</text>

      {/* readouts */}
      <g opacity={ramp(v, 0, 0.6)}>
        <text x={1800} y={170} textAnchor="end" style={label(0.6, { fontSize: 18 })}>MACH</text>
        <text x={1800} y={222} textAnchor="end" style={mono(1, 46)}>{M.toFixed(2)}</text>
        <text x={1800} y={258} textAnchor="end" style={label(0.6, { fontSize: 16 })}>
          {M < 1 ? "SLOWER THAN SOUND" : "FASTER THAN SOUND"}
        </text>
      </g>
      <text x={160} y={170} style={label(window01(v, 0.3, T_WARN + 1.4, 0.4), { fontSize: 26 })}>EACH RING IS A SOUND — THE AIR'S WARNING</text>
      <text x={160} y={170} style={label(window01(v, T_SHOCK - 0.2, T_FASTER, 0.4), { fontSize: 26 })}>SHOCK WAVE: THE RINGS PILE UP INTO A WALL</text>
      <text x={160} y={170} style={label(window01(v, T_FASTER, T_MAKING, 0.4), { fontSize: 26 })}>FASTER → NARROWER CONE · sin μ = 1 / M</text>
    </Scene>
  );
};
