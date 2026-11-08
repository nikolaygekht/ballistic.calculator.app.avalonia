// Scene 9 — where one number breaks: the own-Cd curves from G1 0.505, G7 0.253 and Lapua's radar; one number fits
// at one speed only. Then the race: the same bullet, three drag models, hits at 300 m and 1000 m.
import React from "react";
import { D } from "../data";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, mono, pathD } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";
import { CH, ChartAxes, chx, chy } from "./S06CdCurve";

const N = 9;
const T = {
  shape: cue(N, "same shape"), real: cue(N, "This one"), lapua: cue(N, "Lapua"), fit: cue(N, "One number"), not: cue(N, "not at"),
  fire: cue(N, "So let's fire"), models: cue(N, "models"), m300: cue(N, "300"), within: cue(N, "within"), km: cue(N, "one kilometer"),
  g1: cue(N, "G1"), n57: cue(N, "57"), g7: cue(N, "With G7"), n28: cue(N, "28"),
};
const cv = (ys: number[]): P[] => D.curves.mach.map((m, i) => [chx(m), chy(ys[i])] as P);
const R = D.race, at = (arr: number[], m: number) => arr[Math.round(m / R.stepM)];

const Target: React.FC<{ cx: number; cy: number; r: number; cmPerR: number; rings: number[]; dist: string; o: number; hits: { dy: number; name: string; color: string; o: number; lx?: number }[] }> =
  ({ cx, cy, r, cmPerR, rings, dist, o, hits }) => (
    <g opacity={o}>
      {rings.map((cm, i) => <circle key={i} cx={cx} cy={cy} r={(cm / cmPerR) * r} fill="none" stroke={C.dim} strokeWidth={1.5} />)}
      <line x1={cx - r - 20} y1={cy} x2={cx + r + 20} y2={cy} stroke={C.faint} strokeWidth={1.2} />
      <line x1={cx} y1={cy - r - 20} x2={cx} y2={cy + r + 20} stroke={C.faint} strokeWidth={1.2} />
      <text x={cx} y={cy + r + 60} textAnchor="middle" style={label(0.9, { fontSize: 26 })}>{dist}</text>
      {hits.map((h, i) => {
        const y = cy - (h.dy / cmPerR) * r;
        return (
          <g key={i} opacity={h.o}>
            <circle cx={cx} cy={y} r={9} fill={h.color} filter="url(#glow)" />
            {h.lx !== undefined && (
              <text x={cx + h.lx} y={y + 8} textAnchor={h.lx < 0 ? "end" : "start"} style={mono(1, 24, { fill: h.color })}>{h.name}</text>
            )}
          </g>
        );
      })}
    </g>
  );

export const S09Breaks: React.FC = () => {
  const v = useVoiceTime();
  const part2 = ramp(v, T.fire - 0.3, 0.7);
  const fitO = window01(v, T.fit - 0.2, T.fire, 0.3), gapO = window01(v, T.not - 0.2, T.fire, 0.3);
  // gap between the radar curve and the G7 curve, subsonic and transonic
  const gap: P[] = (() => {
    const idx = D.curves.mach.map((m, i) => i).filter((i) => D.curves.mach[i] <= 1.6);
    return [...idx.map((i) => [chx(D.curves.mach[i]), chy(D.curves.radar[i])] as P), ...idx.reverse().map((i) => [chx(D.curves.mach[i]), chy(D.curves.fromG7[i])] as P)];
  })();

  // the race: three dots fly 0..1000 m in 2.6 s of film, drop drawn to scale
  const fly = ramp(v, T.fire + 0.2, 2.6, ease) * 1000;
  const SX = (m: number) => lerp(200, 1720, m / 1000), SY = (cm: number) => 230 - cm * 0.13;
  const trail = (arr: number[]): P[] => arr.slice(0, Math.floor(fly / R.stepM) + 1).map((cm, i) => [SX(i * R.stepM), SY(cm)] as P);
  const rd300 = at(R.radar, 300), rd1000 = at(R.radar, 1000);
  const o300 = ramp(v, T.m300 - 0.3, 0.6), o1000 = ramp(v, T.km - 0.3, 0.6);

  return (
    <Scene n={N}>
      {/* ---- part 1: three curves for one bullet */}
      <g opacity={1 - part2}>
        <ChartAxes p={ramp(v, -0.5, 0.8)} />
        <Pen pts={cv(D.curves.fromG1)} p={ramp(v, 0.2, 1.4)} color={C.dim} w={2.5} tip={false} />
        <Pen pts={cv(D.curves.fromG7)} p={ramp(v, 0.6, 1.4)} color={C.line} w={2.5} tip={false} />
        <text x={chx(1.45)} y={chy(0.25)} style={mono(0.7, 22)}>from G1 0.505</text>
        <text x={chx(1.1)} y={chy(0.42) - 12} style={mono(0.95, 22)}>from G7 0.253</text>
        <path d={pathD(gap, true)} fill="url(#hatchDrag)" opacity={gapO} />
        <path d={pathD(gap, true)} fill={C.drag} opacity={gapO * 0.16} />
        <g filter="url(#glow)">
          <Pen pts={cv(D.curves.radar)} p={ramp(v, T.real - 0.2, 1.6)} color={C.drag} w={4} />
        </g>
        <text x={chx(0.55)} y={chy(0.207) - 18} style={label(ramp(v, T.lapua - 0.3, 0.5), { fontSize: 22, fill: C.drag })}>LAPUA · RADAR</text>
        <g opacity={fitO}>
          <circle cx={chx(2.33)} cy={chy(0.291)} r={30} fill="none" stroke={C.line} strokeWidth={2.5} />
          <text x={chx(2.33)} y={chy(0.291) - 46} textAnchor="middle" style={label(1, { fontSize: 22 })}>FITS AT ONE VELOCITY</text>
        </g>
        <text x={chx(0.75)} y={chy(0.06)} textAnchor="middle" style={label(gapO, { fontSize: 26, fill: C.drag })}>NOT AT EVERY VELOCITY</text>
        <text x={960} y={190} textAnchor="middle" style={label(window01(v, 0.2, T.real, 0.4), { fontSize: 30 })}>OUR BULLET'S CURVE HAS ITS OWN SHAPE</text>
      </g>

      {/* ---- part 2: the race */}
      <g opacity={part2}>
        <text x={960} y={110} textAnchor="middle" style={label(ramp(v, T.fire, 0.5), { fontSize: 28 })}>THE SAME BULLET · THREE DRAG MODELS</text>
        <line x1={200} y1={230} x2={1720} y2={230} stroke={C.faint} strokeWidth={1.2} strokeDasharray="6 8" />
        {[0, 300, 1000].map((m) => (
          <g key={m}>
            <line x1={SX(m)} y1={222} x2={SX(m)} y2={238} stroke={C.dim} strokeWidth={1.5} />
            <text x={SX(m)} y={212} textAnchor="middle" style={mono(0.6, 18)}>{m === 1000 ? "1 km" : `${m} m`}</text>
          </g>
        ))}
        {([[R.g1, C.dim, "G1"], [R.g7, C.line, "G7"], [R.radar, C.drag, "radar"]] as [number[], string, string][]).map(([arr, col, name], i) => {
          const tr = trail(arr);
          const tip = tr[tr.length - 1];
          return (
            <g key={i}>
              <path d={pathD(tr)} stroke={col} strokeWidth={2.5} fill="none" />
              {fly > 0 && <circle cx={tip[0]} cy={tip[1]} r={6} fill={col} filter="url(#glow)" />}
              {fly >= 1000 && <text x={tip[0] + 18} y={tip[1] + (i - 1) * 26 + 8} style={mono(0.9, 20, { fill: col })}>{name}</text>}
            </g>
          );
        })}
        <Target cx={560} cy={700} r={210} cmPerR={2} rings={[0.5, 1, 1.5, 2]} dist="300 m" o={o300}
          hits={[
            { dy: at(R.g1, 300) - rd300, name: "", color: C.dim, o: o300 },
            { dy: at(R.g7, 300) - rd300, name: "", color: C.line, o: o300 },
            { dy: 0, name: "", color: C.drag, o: o300 },
          ]} />
        <text x={560} y={460} textAnchor="middle" style={label(ramp(v, T.within, 0.5), { fontSize: 26 })}>ALL WITHIN 3 mm</text>
        <Target cx={1360} cy={690} r={250} cmPerR={80} rings={[20, 40, 60, 80]} dist="1 km" o={o1000}
          hits={[
            { dy: 0, name: "radar", color: C.drag, o: o1000, lx: 26 },
            { dy: at(R.g7, 1000) - rd1000, name: `G7 +${Math.round(at(R.g7, 1000) - rd1000)} cm`, color: C.line, o: ramp(v, T.g7 - 0.2, 0.5), lx: 26 },
            { dy: at(R.g1, 1000) - rd1000, name: `G1 +${Math.round(at(R.g1, 1000) - rd1000)} cm`, color: C.line, o: ramp(v, T.g1 - 0.2, 0.5), lx: 26 },
          ]} />
        <text x={1360 + 270} y={690 - 230} style={label(ramp(v, T.km, 0.5) * 0.7, { fontSize: 20 })}>RINGS EVERY 20 cm</text>
      </g>
    </Scene>
  );
};
