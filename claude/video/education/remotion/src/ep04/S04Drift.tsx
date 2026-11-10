// Scene 4 — spin drift. Side view: the path bends down, the stiff axis follows late, the nose stays above the path
// (×20); the air pushes the nose up; by the rule, the nose goes right. Top view: the nose a little right of the path,
// the air pushes the bullet right like a small wing. A dot has no nose: Litz's formula, its two terms, the power 1.83,
// and the drift for our shot: 4.7 cm at 500 m, 31 cm at 1000 m.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, mono } from "../lib/draw";
import { CornerRule, Dot, FLIGHT, Item, MathLine, Scene, SpinBullet, Title, atX, drift, k_, sub, sup, t_, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 4;
const T = {
  first: cue(N, "spin drift"), gravity: cue(N, "Gravity"), keep: cue(N, "keep its axis"), late: cue(N, "a little late"), above: cue(N, "nose stays"),
  push: cue(N, "pushes the nose up"), rule: cue(N, "now the rule"), pushUp: cue(N, "Push up"), right: cue(N, "goes right"), looks: cue(N, "looks a little"),
  whole: cue(N, "whole bullet"), wing: cue(N, "small wing"), noNose: cue(N, "dot has no"), litz: cue(N, "Litz"), fitted: cue(N, "fitted"),
  two: cue(N, "two things"), stab: cue(N, "stability"), more: cue(N, "more stable"), bigger: cue(N, "drift is bigger"), time: cue(N, "time of flight"),
  power: cue(N, "power"), fasterT: cue(N, "faster than the time"), itself: cue(N, "time itself"), m500: cue(N, "500"), km: cue(N, "kilometre"), n31: cue(N, "31"),
};

const FX = 960, FY = 210, FS = 64;
const LITZ: Item[] = [{ toks: [t_("Δz = 1.25 · ("), k_("S", "sg"), sub("g", "sg"), t_(" + 1.2) · "), k_("t", "t"), sup("1.83", "p")] }];

// side view: a strongly bent path, so the lag can be seen
const sideP = (u: number): P => [220 + 1480 * u, 400 + 360 * u * u];
const sideA = (u: number) => (-Math.atan2(720 * u, 1480) * 180) / Math.PI;      // path angle, degrees (negative = down)
const SIDE: P[] = Array.from({ length: 61 }, (_, i) => sideP(i / 60));

export const S04Drift: React.FC = () => {
  const v = useVoiceTime();
  // ---- side view
  const sideO = ramp(v, T.gravity - 0.5, 0.5) * (1 - ramp(v, T.looks - 0.4, 0.5));
  const u = lerp(0.05, 0.55, ramp(v, T.gravity, T.late + 0.4 - T.gravity, ease));
  const lag = lerp(0, 9, ramp(v, T.keep, 2.5));
  const B = sideP(u), pathDeg = sideA(u), axisDeg = pathDeg + lag;
  const upP = ramp(v, T.push - 0.1, 0.5);
  // ---- the rule in the corner
  const ruleHot = window01(v, T.rule - 0.2, T.looks, 0.3);
  // ---- top view
  const topO = ramp(v, T.looks - 0.3, 0.5) * (1 - ramp(v, T.litz - 0.6, 0.5));
  const toDot = ramp(v, T.noNose - 0.1, 0.6);
  const sideK = ramp(v, T.whole - 0.2, 1.4, ease);
  // ---- formula
  const fO = ramp(v, T.litz - 0.3, 0.5);
  const active = v >= T.m500 - 0.3 ? null : v >= T.power - 0.1 ? "p" : v >= T.time - 0.1 ? "t" : v >= T.stab - 0.2 ? "sg" : null;
  const cmpO = window01(v, T.more - 0.3, T.time - 0.2, 0.4);
  const plotO = window01(v, T.fasterT - 0.4, T.m500 - 0.4, 0.4);
  const resO = ramp(v, T.m500 - 0.4, 0.6);

  return (
    <Scene n={N}>
      <Title text="SPIN DRIFT" o={ramp(v, -0.5, 0.5) * (1 - ramp(v, T.gravity - 0.4, 0.4))} />
      <CornerRule spin={v * 0.8} push="up" pushP={ramp(v, T.pushUp - 0.1, 0.5)} noseP={ramp(v, T.right - 0.2, 0.7)} o={ramp(v, T.first, 0.6) * (1 - ramp(v, T.litz - 0.6, 0.5))} hot={ruleHot} />

      {/* ---------------- side view: the nose stays above the path */}
      <g opacity={sideO}>
        <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>SIDE VIEW</text>
        <Pen pts={SIDE} p={1} w={2.5} tip={false} opacity={0.5} />
        <text x={1700} y={740} textAnchor="end" style={label(ramp(v, T.gravity, 0.5) * 0.7, { fontSize: 20 })}>GRAVITY BENDS THE PATH DOWN</text>
        {/* the velocity: along the path */}
        <g transform={`rotate(${-pathDeg},${B[0]},${B[1]})`} opacity={ramp(v, T.keep, 0.5)}>
          <line x1={B[0] - 260} y1={B[1]} x2={B[0] + 300} y2={B[1]} stroke={C.dim} strokeWidth={2} strokeDasharray="8 8" />
          <text x={B[0] + 300} y={B[1] + 34} textAnchor="end" style={label(0.6, { fontSize: 18 })}>THE PATH</text>
        </g>
        <SpinBullet id="d" cx={B[0]} cy={B[1]} cal={44} deg={axisDeg} spin={v * 1.2} marks={0.8} />
        {/* the angle between them */}
        <g opacity={ramp(v, T.above - 0.2, 0.5)}>
          {(() => {
            const r = 230, a0 = (-pathDeg * Math.PI) / 180, a1 = (-axisDeg * Math.PI) / 180;
            const pts: P[] = Array.from({ length: 13 }, (_, i) => { const a = lerp(a0, a1, i / 12); return [B[0] + r * Math.cos(a), B[1] + r * Math.sin(a)]; });
            return <Pen pts={pts} p={1} w={3} tip={false} />;
          })()}
          <text x={B[0] + 250} y={B[1] - 110} style={label(1, { fontSize: 22 })}>THE NOSE: A LITTLE ABOVE THE PATH</text>
          <text x={B[0] + 250} y={B[1] - 82} style={label(0.6, { fontSize: 18 })}>DRAWN ×20</text>
        </g>
        {/* the air pushes the nose up */}
        {upP > 0 && (() => {
          const a = (-axisDeg * Math.PI) / 180, nx = B[0] + 100 * Math.cos(a), ny = B[1] + 100 * Math.sin(a);
          return (
            <g opacity={upP} filter="url(#glow)">
              <line x1={nx} y1={ny} x2={nx} y2={ny - 120 * upP} stroke={C.drag} strokeWidth={6} strokeLinecap="round" />
              <path d={`M${nx - 14},${ny - 120 * upP + 18} L${nx},${ny - 120 * upP} L${nx + 14},${ny - 120 * upP + 18}`} stroke={C.drag} strokeWidth={6} fill="none" />
            </g>
          );
        })()}
        <text x={B[0]} y={B[1] + 140} textAnchor="middle" style={label(ramp(v, T.push, 0.4), { fontSize: 22, fill: C.drag })}>THE AIR PUSHES THE NOSE UP</text>
        <text x={1500} y={330} textAnchor="middle" style={label(ramp(v, T.right - 0.2, 0.4), { fontSize: 26 })}>PUSH UP → THE NOSE GOES RIGHT</text>
      </g>

      {/* ---------------- top view: the nose right of the path, a push to the right */}
      <g opacity={topO}>
        <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>TOP VIEW · DOWNRANGE IS UP</text>
        {(() => {
          const pts: P[] = Array.from({ length: 41 }, (_, i) => { const k = i / 40; return [960 + 160 * sideK * k * k, 1000 - 860 * k]; });
          const k = 0.5, bx = 960 + 160 * sideK * k * k, by = 1000 - 860 * k;
          return (
            <>
              <line x1={960} y1={1000} x2={960} y2={140} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
              <Pen pts={pts} p={1} w={3} tip={false} opacity={0.7} />
              <g opacity={1 - toDot}>
                <SpinBullet id="t" cx={bx} cy={by} cal={44} deg={90 - 7} spin={v * 1.2} marks={0.8} />
                <g opacity={ramp(v, T.whole - 0.3, 0.5)} filter="url(#glow)">
                  <line x1={bx + 20} y1={by} x2={bx + 150} y2={by} stroke={C.drag} strokeWidth={6} strokeLinecap="round" />
                  <path d={`M${bx + 132},${by - 14} L${bx + 150},${by} L${bx + 132},${by + 14}`} stroke={C.drag} strokeWidth={6} fill="none" />
                </g>
              </g>
              <Dot x={bx} y={by} r={12} o={toDot} />
              <text x={bx + 180} y={by - 70} style={label(ramp(v, T.looks, 0.4) * (1 - toDot), { fontSize: 24 })}>THE NOSE: A LITTLE TO THE RIGHT</text>
              <text x={bx + 180} y={by + 10} style={label(ramp(v, T.wing - 0.2, 0.4) * (1 - toDot), { fontSize: 24, fill: C.drag })}>THE AIR PUSHES IT RIGHT, LIKE A WING</text>
              <text x={bx + 40} y={by + 70} style={label(toDot, { fontSize: 26 })}>A DOT HAS NO NOSE</text>
            </>
          );
        })()}
      </g>

      {/* ---------------- Litz's formula */}
      <g opacity={fO}>
        <MathLine items={LITZ} cx={FX} y={FY} size={FS} reveal={ramp(v, T.litz, 1.4)} active={active} />
        <text x={960} y={300} textAnchor="middle" style={label(window01(v, T.litz + 0.6, T.two, 0.4) * 0.8, { fontSize: 22 })}>BRYAN LITZ · FITTED TO MEASURED SHOTS</text>
        <text x={960} y={300} textAnchor="middle" style={label(window01(v, T.two, T.m500 - 0.4, 0.4) * 0.6, { fontSize: 20 })}>Δz — INCHES · t — TIME OF FLIGHT, SECONDS</text>
      </g>
      {/* a more stable bullet: a bigger lag, a bigger push */}
      <g opacity={cmpO}>
        {[{ sg: "1.2", lag: 5, x: 600 }, { sg: "2.5", lag: 12, x: 1320 }].map((c, i) => (
          <g key={i}>
            <line x1={c.x - 220} y1={620} x2={c.x + 260} y2={620} stroke={C.dim} strokeWidth={2} strokeDasharray="8 8" />
            <SpinBullet id={`c${i}`} cx={c.x} cy={620} cal={44} deg={c.lag} spin={v * 1.2} marks={0.8} />
            <text x={c.x} y={760} textAnchor="middle" style={mono(1, 34, { fontWeight: 600 })}>S<tspan fontSize={20} dy={7}>g</tspan><tspan dy={-7}> = {c.sg}</tspan></text>
            <line x1={c.x + 120} y1={540} x2={c.x + 120 + (i ? 120 : 50)} y2={540} stroke={C.drag} strokeWidth={6} strokeLinecap="round" opacity={ramp(v, T.bigger - 0.3, 0.5)} />
          </g>
        ))}
        <text x={960} y={880} textAnchor="middle" style={label(0.85, { fontSize: 24 })}>MORE STABLE → HOLDS ITS AXIS HARDER → THE NOSE HIGHER → MORE DRIFT</text>
      </g>
      {/* the time: t^1.83 grows faster than t; t grows faster than the distance */}
      <g opacity={plotO}>
        {(() => {
          const W = 520, H = 340, ax = (x0: number, y0: number) => (
            <>
              <line x1={x0} y1={y0} x2={x0 + W} y2={y0} stroke={C.line} strokeWidth={2} opacity={0.6} />
              <line x1={x0} y1={y0} x2={x0} y2={y0 - H} stroke={C.line} strokeWidth={2} opacity={0.6} />
            </>
          );
          const A = [330, 820], Bp = [1070, 820];
          const lin: P[] = [[A[0], A[1]], [A[0] + W, A[1] - H * 0.55]];
          const pw: P[] = Array.from({ length: 31 }, (_, i) => { const k = i / 30; return [A[0] + W * k, A[1] - H * 0.55 * Math.pow(k, 1.83) * (1 / 1) * 1.8]; });
          const tx: P[] = FLIGHT.map((r) => [Bp[0] + (W * r.x) / 1000, Bp[1] - (H * r.t) / 2.1] as P);
          const linB: P[] = [[Bp[0], Bp[1]], [Bp[0] + W, Bp[1] - (H * (1000 / 790)) / 2.1]];
          return (
            <>
              {ax(A[0], A[1])}
              <Pen pts={lin} p={1} w={2} tip={false} dash="8 8" opacity={0.6} />
              <Pen pts={pw.filter((q) => q[1] >= A[1] - H)} p={ramp(v, T.fasterT - 0.2, 1.0)} w={4} tip={false} />
              <text x={A[0] + W} y={A[1] + 36} textAnchor="end" style={label(0.7, { fontSize: 18 })}>TIME</text>
              <text x={A[0] + 10} y={A[1] - H - 14} style={label(0.9, { fontSize: 20 })}>DRIFT ∝ t<tspan fontSize={13} dy={-8}>1.83</tspan></text>
              <g opacity={ramp(v, T.itself - 0.3, 0.5)}>
                {ax(Bp[0], Bp[1])}
                <Pen pts={linB} p={1} w={2} tip={false} dash="8 8" opacity={0.6} />
                <Pen pts={tx} p={ramp(v, T.itself, 1.0)} w={4} tip={false} />
                <text x={Bp[0] + W} y={Bp[1] + 36} textAnchor="end" style={label(0.7, { fontSize: 18 })}>DISTANCE</text>
                <text x={Bp[0] + 10} y={Bp[1] - H - 14} style={label(0.9, { fontSize: 20 })}>TIME: THE BULLET SLOWS DOWN</text>
              </g>
            </>
          );
        })()}
      </g>
      {/* our shot */}
      <g opacity={resO}>
        {(() => {
          const X0 = 760, Y0 = 960, Y1 = 380, SX = 1100;     // 1 m sideways = 1100 px
          const pts: P[] = Array.from({ length: 51 }, (_, i) => { const x = (1000 * i) / 50, r = atX(x); return [X0 + SX * Math.max(0, drift(x, r.t)), Y0 - ((Y0 - Y1) * x) / 1000]; });
          const at = (x: number): P => { const r = atX(x); return [X0 + SX * drift(x, r.t), Y0 - ((Y0 - Y1) * x) / 1000]; };
          const p5 = at(500), p10 = at(1000);
          return (
            <>
              <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>TOP VIEW · DOWNRANGE IS UP</text>
              <line x1={X0} y1={Y0} x2={X0} y2={Y1 - 20} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
              <Pen pts={pts} p={ramp(v, T.m500 - 0.3, 3.0)} w={4} tip={false} />
              <g opacity={ramp(v, T.m500, 0.4)}>
                <circle cx={p5[0]} cy={p5[1]} r={8} fill={C.line} />
                <text x={p5[0] + 24} y={p5[1] + 10} style={mono(1, 32, { fontWeight: 600 })}>4.7 cm · 500 m</text>
              </g>
              <g opacity={ramp(v, T.n31 - 0.2, 0.4)}>
                <circle cx={p10[0]} cy={p10[1]} r={10} fill={C.line} filter="url(#glow)" />
                <text x={p10[0] + 24} y={p10[1] + 12} style={mono(1, 40, { fontWeight: 700 })}>31 cm · 1000 m</text>
              </g>
              <text x={X0 - 20} y={Y0 - 10} textAnchor="end" style={label(0.55, { fontSize: 18 })}>SIDEWAYS: STRETCHED</text>
            </>
          );
        })()}
      </g>
    </Scene>
  );
};
