// Scene 6 — one step, semi-implicit Euler: v_new = v + a·Δt, r_new = r + v_new·Δt, with a demo Δt of 0.1 s. The
// change a·Δt (−47 m/s back, ≈1 m/s down) clicks onto the old velocity; the dot moves by v_new·Δt; then the loop
// runs over the whole flight (datagen's own Euler, 0.1 s steps, from the narrated 790 m/s).
import React from "react";
import { C, clamp01, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Formula, P, Tok, label, mono } from "../lib/draw";
import { ArrowA, Dot, EULER, G, Magnifier, Scene, Stopwatch, VecText, sub, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 6;
const T = {
  step: cue(N, "make a step"), stop: cue(N, "Stop"), lines: cue(N, "two lines"), delta: cue(N, "Delta"), tenth: cue(N, "tenth"),
  longer: cue(N, "much longer"), accel: cue(N, "acceleration"), oneSec: cue(N, "one second"), times: cue(N, "Times delta"),
  thisStep: cue(N, "changes", 2), drag: cue(N, "Drag takes"), gravity: cue(N, "Gravity adds"), add: cue(N, "Add this"),
  newV: cue(N, "new velocity arrow"), shorter: cue(N, "shorter"), turned: cue(N, "turned"), second: cue(N, "second line"),
  moves: cue(N, "moves the dot"), newV2: cue(N, "new velocity", 2), same: cue(N, "same delta"), far: cue(N, "how far"),
  each: cue(N, "Each line"), all3: cue(N, "all three"), again: cue(N, "again"), slower: cue(N, "slower"), smaller: cue(N, "smaller"),
  finds: cue(N, "finds"), next: cue(N, "next step"), all: cue(N, "That's all"),
};

const S = EULER.s100;
const E1 = S[1];
const DV = E1.vx - S[0].vx;                      // ≈ −47 m/s
const DVY = E1.vy;                                // ≈ −0.98 m/s
const VEX = 20;                                   // vertical exaggeration, the whole scene

const L1: Tok[] = [vec("v", "vn"), sub("new", "vn"), t_(" = "), vec("v", "v"), t_(" + "), vec("a", "a"), t_(" · "), t_("Δt", "dt")];
const L2: Tok[] = [vec("r", "rn"), sub("new", "rn"), t_(" = "), vec("r", "r"), t_(" + "), vec("v", "vn2"), sub("new", "vn2"), t_(" · "), t_("Δt", "dt2")];

// the velocity picture: 1 px per m/s, vertical ×20
const D0: P = [380, 560];
const vTip: P = [D0[0] + 790, D0[1]];
const nTip: P = [D0[0] + E1.vx, D0[1] - DVY * VEX];
// the flight strip: 1 px per metre, vertical ×20, from the same start
const sx = (x: number) => D0[0] + x, sy = (y: number) => D0[1] - y * VEX;
const LENS: P = [1170, 830], Z = 4, FOC: P = [vTip[0] - 23, vTip[1] + 10];

export const S06Step: React.FC = () => {
  const v = useVoiceTime();
  // the dot arrives and time stops
  const arrive = ramp(v, -0.6, T.stop - 0.4 + 0.6, ease);
  const handRun = v < T.stop ? (v + 1) * 0.6 : (T.stop + 1) * 0.6;
  // formulas
  const active1 =
    v >= T.add - 0.1 && v < T.newV + 0.6 ? "v" : v >= T.times - 0.1 && v < T.add ? "dt" : v >= T.accel - 0.1 && v < T.times ? "a" :
    v >= T.delta - 0.1 && v < T.accel ? "dt" : v >= T.newV + 0.6 && v < T.second ? "vn" : null;
  const active2 = v >= T.same - 0.1 && v < T.far ? "dt2" : v >= T.newV2 - 0.1 && v < T.same ? "vn2" : v >= T.far && v < T.each ? "rn" : null;

  // the change a·Δt, over by the stopwatch
  const SW: P = [1580, 540];
  const aP = ramp(v, T.accel, 0.8, ease);
  const shrink = ramp(v, T.times + 0.2, 1.2, ease);
  const aLen = lerp(470, 47, shrink);
  const split = ramp(v, T.drag - 0.2, 0.6);
  const gdown = ramp(v, T.gravity - 0.1, 0.6, ease);
  const fly = ramp(v, T.add + 0.2, 1.0, ease);
  // where the change sits: by the stopwatch, then on the tip of the old velocity
  const cX = lerp(SW[0] + 30, vTip[0], fly), cY = lerp(SW[1] - 170, vTip[1], fly);
  const newP = ramp(v, T.newV - 0.1, 0.8, ease);
  // the dot moves by v_new · Δt
  const dP = ramp(v, T.same, 0.6, ease), slide = ramp(v, T.far, 1.0, ease);
  const d1: P = [D0[0] + E1.x, D0[1] - E1.y * VEX];
  // the loop
  const loopO = ramp(v, T.again - 0.2, 0.6);
  const picO = 1 - loopO;
  const span = T.all + 0.4 - T.again;
  const k = clamp01((v - T.again) / span) ** 1.9 * (S.length - 1);
  const ki = Math.min(S.length - 2, Math.floor(k)), kf = k - ki;
  const cur = S[Math.min(S.length - 1, Math.round(k))];
  const dot: P = v < T.again ? (slide > 0 ? [lerp(D0[0], d1[0], slide), lerp(D0[1], d1[1], slide)] : D0)
    : [lerp(sx(S[ki].x), sx(S[ki + 1].x), kf), lerp(sy(S[ki].y), sy(S[ki + 1].y), kf)];
  const state = v < T.again ? (slide > 0.5 ? E1 : newP > 0.5 ? { ...S[0], vx: E1.vx, vy: E1.vy } : S[0]) : cur;
  const rolled = v >= T.again || slide > 0.5;
  const allHot = window01(v, T.all3 - 0.1, T.again, 0.3);

  return (
    <Scene n={N}>
      <Formula toks={L1} cx={960} y={110} size={54} reveal={ramp(v, T.lines - 0.3, 1.0)} active={active1} />
      <Formula toks={L2} cx={960} y={190} size={54} reveal={ramp(v, T.lines + 0.4, 1.0)} active={active2} />

      {/* the stopwatch: time is stopped; Δt */}
      <Stopwatch cx={SW[0]} cy={SW[1]} r={78} p={ramp(v, T.stop - 0.4, 0.8)} hand={v < T.times ? handRun : handRun + ramp(v, T.times + 0.2, 1.2) * 0.1} o={picO} />
      <g opacity={picO}>
        <text x={SW[0]} y={SW[1] + 130} textAnchor="middle" style={mono(ramp(v, T.tenth - 0.3, 0.5), 40, { fontWeight: 600 })}>Δt = 0.1 s</text>
        <text x={SW[0]} y={SW[1] + 172} textAnchor="middle" style={label(ramp(v, T.longer - 0.1, 0.5) * 0.85, { fontSize: 20 })}>MUCH LONGER THAN REAL</text>
        <text x={SW[0]} y={SW[1] - 116} textAnchor="middle" style={label(window01(v, T.stop - 0.2, T.delta, 0.3), { fontSize: 24 })}>STOP THE TIME</text>

        {/* the acceleration: per second; times Δt: per step */}
        {split < 1 && (
          <g opacity={aP * (1 - split)}>
            <ArrowA x={cX + aLen / 2} y={cY} a={Math.PI} len={aLen} w={5} head={20} glow />
            <VecText x={cX} y={cY - 34} s="a" size={38} anchor="middle" o={1 - shrink} />
            <text x={cX} y={cY + 52} textAnchor="middle" style={mono(1 - shrink, 26)}>470 m/s per second</text>
            <text x={cX} y={cY + 52} textAnchor="middle" style={mono(shrink, 26)}>change in one step</text>
          </g>
        )}
        {/* the change, split into its parts: drag back, gravity down */}
        <g opacity={split}>
          <ArrowA x={cX} y={cY} a={Math.PI} len={-DV} w={5} head={14} color={C.drag} glow />
          <ArrowA x={cX + DV} y={cY} a={Math.PI / 2} len={-DVY * VEX} p={gdown} w={4} head={10} />
          <text x={cX + DV / 2} y={cY - 22} textAnchor="middle" style={mono(1 - fly, 28, { fill: C.drag })}>−47 m/s</text>
          <text x={cX + DV - 14} y={cY + 26} textAnchor="end" style={mono(gdown * (1 - fly), 28)}>1 m/s ↓</text>
        </g>
        <text x={1500} y={SW[1] - 250} textAnchor="middle" style={label(window01(v, T.accel - 0.1, T.times, 0.3), { fontSize: 24 })}>CHANGE OF VELOCITY IN ONE SECOND</text>
        <text x={1500} y={SW[1] - 250} textAnchor="middle" style={label(window01(v, T.times + 0.2, T.add, 0.3), { fontSize: 24 })}>… AND IN THIS STEP</text>

        {/* the old velocity, the change on its tip, the new velocity closing the triangle */}
        <Arrow x1={D0[0]} y1={D0[1]} x2={vTip[0]} y2={vTip[1]} p={ramp(v, T.step - 0.2, 0.8, ease)} w={4} head={20} opacity={1 - 0.5 * newP} />
        <text x={(D0[0] + vTip[0]) / 2} y={D0[1] - 26} textAnchor="middle" style={mono(ramp(v, T.step, 0.5) * (1 - 0.5 * newP), 32)}>790 m/s</text>
        <g filter="url(#glow)">
          <Arrow x1={D0[0]} y1={D0[1]} x2={nTip[0]} y2={nTip[1]} p={newP} w={5} head={22} />
        </g>
        <text x={(D0[0] + nTip[0]) / 2} y={(D0[1] + nTip[1]) / 2 + 52} textAnchor="middle" style={mono(newP, 34, { fontWeight: 600 })}>
          {E1.vx.toFixed(0)} m/s
        </text>
        <text x={(D0[0] + nTip[0]) / 2} y={(D0[1] + nTip[1]) / 2 + 92} textAnchor="middle" style={label(window01(v, T.shorter - 0.2, T.second, 0.3) * 0.85, { fontSize: 22 })}>
          A LITTLE SHORTER · TURNED A LITTLE DOWN
        </text>
        {/* a magnifier on the tip: the change, clicked on, and the new arrow closing the triangle */}
        <Magnifier cx={LENS[0]} cy={LENS[1]} r={165} p={ramp(v, T.add + 1.0, 0.7) * (1 - ramp(v, T.second, 0.5))} id="tip">
          <g transform={`translate(${LENS[0]},${LENS[1]}) scale(${Z}) translate(${-FOC[0]},${-FOC[1]})`}>
            <line x1={D0[0]} y1={D0[1]} x2={vTip[0]} y2={vTip[1]} stroke={C.line} strokeWidth={3 / Z} opacity={0.6} />
            <line x1={vTip[0]} y1={vTip[1]} x2={vTip[0] + DV} y2={vTip[1]} stroke={C.drag} strokeWidth={5 / Z} />
            <line x1={vTip[0] + DV} y1={vTip[1]} x2={nTip[0]} y2={nTip[1]} stroke={C.line} strokeWidth={4 / Z} />
            <line x1={D0[0]} y1={D0[1]} x2={nTip[0]} y2={nTip[1]} stroke={C.line} strokeWidth={5 / Z} opacity={newP} />
          </g>
          <text x={LENS[0]} y={LENS[1] - 62} textAnchor="middle" style={mono(1, 28, { fill: C.drag })}>−47 m/s</text>
          <text x={LENS[0] - 80} y={LENS[1] + 16} style={mono(1, 26)}>1 m/s ↓</text>
          <text x={LENS[0] - 40} y={LENS[1] + 92} textAnchor="middle" style={mono(newP, 26, { fontWeight: 600 })}>new</text>
        </Magnifier>
        {/* v_new · Δt: one tenth of the new arrow, the way the dot goes */}
        <g opacity={dP * (1 - ramp(v, T.again - 0.6, 0.4))}>
          <Arrow x1={D0[0]} y1={D0[1]} x2={D0[0] + E1.vx * 0.1} y2={D0[1] - E1.vy * 0.1 * VEX} w={3} head={12} />
          <line x1={D0[0]} y1={D0[1]} x2={dot[0]} y2={dot[1]} stroke={C.line} strokeWidth={2} strokeDasharray="3 5" opacity={slide} />
          <text x={D0[0] + 40} y={D0[1] + 58} textAnchor="middle" style={mono(1, 26)}>× Δt</text>
        </g>
      </g>

      {/* the loop: the whole flight, step by step; each lap the drag is drawn again, a little shorter */}
      <g opacity={loopO}>
        <path d={"M" + S.slice(0, ki + 1).map((s) => `${sx(s.x)},${sy(s.y)}`).join(" L") + ` L${dot[0]},${dot[1]}`} stroke={C.line} strokeWidth={2.5} fill="none" />
        {S.slice(0, ki + 1).map((s, i) => <circle key={i} cx={sx(s.x)} cy={sy(s.y)} r={4} fill={C.line} opacity={0.7} />)}
        <ArrowA x={dot[0] - 14} y={dot[1]} a={Math.PI + Math.atan2(-S[ki].vy * VEX, S[ki].vx)} len={S[ki].D * 0.5} w={5} head={20} color={C.drag} glow />
        <text x={dot[0] - 20 - S[ki].D * 0.25} y={dot[1] - 26} textAnchor="middle" style={mono(1, 28, { fill: C.drag })}>drag {(S[ki].D / G).toFixed(0)} g</text>
        <text x={960} y={1000} textAnchor="middle" style={label(window01(v, T.slower - 0.2, T.finds, 0.3), { fontSize: 26 })}>SLOWER · SO A LITTLE LESS DRAG</text>
        <text x={960} y={1000} textAnchor="middle" style={label(window01(v, T.finds - 0.1, T.all, 0.3), { fontSize: 26 })}>FIND THE FORCES · MAKE THE STEP · AGAIN</text>
        <text x={960} y={1000} textAnchor="middle" style={label(ramp(v, T.all, 0.4), { fontSize: 30 })}>THAT'S ALL THERE IS</text>
      </g>
      <Dot x={lerp(-40, dot[0], arrive)} y={dot[1]} />

      {/* the six numbers roll */}
      <g opacity={ramp(v, T.newV, 0.5)} filter={allHot > 0.3 ? "url(#glow)" : undefined}>
        <VecText x={110} y={850} s="v" size={34} />
        <text x={140} y={850} style={mono(rolled || newP > 0.5 ? 1 : 0.8, 34)} xmlSpace="preserve">
          {` = ( ${state.vx.toFixed(1)}, ${state.vy.toFixed(2)}, 0 ) m/s`}
        </text>
        <VecText x={110} y={910} s="r" size={34} />
        <text x={140} y={910} style={mono(0.95, 34)} xmlSpace="preserve">
          {` = ( ${(rolled ? state.x : 0).toFixed(1)}, ${(rolled ? state.y : 0).toFixed(2)}, 0 ) m`}
        </text>
        <text x={110} y={960} style={mono(0.6, 26)}>t = {(rolled ? state.t : 0).toFixed(1)} s</text>
      </g>
      <text x={110} y={790} style={label(allHot, { fontSize: 22 })}>ALL THREE NUMBERS AT ONCE</text>
    </Scene>
  );
};
