// Episode 3, scene 2 — wind, looked at from the air. Top view, downrange up the screen, wind 4.5 m/s from the right. We ride a
// balloon: the air stands still and the ground slides right; the bullet moves forward at 790 and right at 4.5.
// v_air = v − w, tilted 0.33° (drawn ×20), and v_air is what goes into the drag formula.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Formula, P, Tok, label, mono, seeded } from "../lib/draw";
import { ArrowA, Dot, Scene, VecText, sub, t_, useVoiceTime, vec } from "../ep02/kit";
import { cue } from "./timing";

const N = 2;
const T = {
  wind: cue(N, "wind"), fourHalf: cue(N, "Four and a half"), dragComes: cue(N, "Drag comes"), through: cue(N, "through the air"),
  ground: cue(N, "over the ground"), look: cue(N, "look from the air"), balloon: cue(N, "balloon"), carries: cue(N, "carries"),
  still: cue(N, "stands still"), slides: cue(N, "slides"), bullet: cue(N, "And the bullet"), forward: cue(N, "forward"),
  forUs: cue(N, "for us", 2), alsoMoves: cue(N, "also moves"), together: cue(N, "together"), sideways: cue(N, "sideways"),
  formula: cue(N, "In the formula"), minus: cue(N, "minus"), turn: cue(N, "Turn"), endOf: cue(N, "end of the velocity"),
  newArrow: cue(N, "new arrow"), tilted: cue(N, "tilted"), third: cue(N, "third"), thisIs: cue(N, "this is the velocity"),
  dragF: cue(N, "drag formula"), everywhere: cue(N, "Everywhere"),
};

const WPX = 150;                                  // drawn wind speed, px per second
const SIDE = 20;                                  // side exaggeration
const VL = 600, SL = VL * (4.5 / 790) * SIDE;     // 790 m/s up = 600 px; 4.5 m/s ×20 = 68 px
const STREAKS = (() => { const r = seeded(909); return Array.from({ length: 90 }, () => ({ x: r() * 2200, y: 90 + r() * 960, l: 26 + r() * 40, a: 0.15 + r() * 0.3 })); })();
const TUFTS = (() => { const r = seeded(77); return Array.from({ length: 40 }, () => ({ x: r() * 2400, y: 120 + r() * 900 })); })();

/** Camera offset (px): zero, then accelerating over `d` s to the wind speed — we ride with the air. */
const cam = (t: number, t0: number, d = 1.2) => (t < t0 ? 0 : t < t0 + d ? (WPX * (t - t0) ** 2) / (2 * d) : WPX * (t - t0 - d / 2));

const FW: Tok[] = [vec("v", "va"), sub("air", "va"), t_(" = "), vec("v", "v"), t_(" − "), vec("w", "w")];
const FD1: Tok[] = [vec("a", "a", C.drag), sub("drag", "a", C.drag), t_(" = − D · "), vec("v"), t_(" / |"), vec("v"), t_("|")];
const FD2: Tok[] = [vec("a", "a", C.drag), sub("drag", "a", C.drag), t_(" = − D · "), vec("v", "air"), sub("air", "air"), t_(" / |"), vec("v", "air"), sub("air", "air"), t_("|")];
const FM: Tok[] = [t_("M = |"), vec("v", "air"), sub("air", "air"), t_("| / c")];

export const S02WindAir: React.FC = () => {
  const v = useVoiceTime();
  const tRide = T.carries - 0.3;
  const cx = cam(v, tRide);
  const bgO = 1 - 0.55 * ramp(v, T.bullet + 0.4, 0.8);
  const span = 2200;
  const wrap = (x: number) => ((x % span) + span) % span - 140;
  // the balloon
  const bO = ramp(v, T.balloon - 0.3, 0.5);
  const bx = 1380 - WPX * Math.max(0, v - (T.balloon - 0.3)) + cx, by = 420;
  // a bullet flying past, then the frozen diagram
  // the bullet's pass, in the air's frame: up the range, and right with the ground at the ground's drawn speed (WPX).
  // Up at WPX·790/4.5/SIDE px/s keeps the side drift at the diagram's ×20; the trail stays put in the air: the path
  // through the air is tilted.
  const tP = T.bullet - 0.1, VY = (WPX * 790) / 4.5 / SIDE, P0: P = [420, 1100];
  const kP = Math.max(0, Math.min(v - tP, (P0[1] + 40) / VY));
  const pHead: P = [P0[0] + WPX * kP, P0[1] - VY * kP];
  const trailO = window01(v, tP, T.formula, 0.4);
  const trailHot = window01(v, T.alsoMoves - 0.2, T.formula - 0.3, 0.4);
  const D0: P = [860, 900];
  const diagO = ramp(v, T.forward - 0.3, 0.5);
  const vP = ramp(v, T.forward - 0.2, 0.7, ease);
  const sideP = ramp(v, T.alsoMoves - 0.1, 0.6, ease) * (1 - ramp(v, T.formula, 0.5));
  // v − w
  const wO = ramp(v, T.minus - 0.3, 0.5);
  const flip = ramp(v, T.turn, 0.8, ease), move = ramp(v, T.endOf - 0.3, 0.9, ease);
  const wAng = Math.PI * (1 - flip);                                         // left, then right
  const W0: P = [1260, 560], vTip: P = [D0[0], D0[1] - VL];
  const wx = lerp(W0[0], vTip[0], move), wy = lerp(W0[1], vTip[1], move);
  const vaP = ramp(v, T.newArrow - 0.1, 0.8, ease);
  const angO = ramp(v, T.tilted - 0.2, 0.5);
  // the drag formula, v -> v_air
  const fO = ramp(v, T.thisIs - 0.2, 0.6);
  const swap = ramp(v, T.everywhere - 0.2, 0.7);

  return (
    <Scene n={N}>
      {/* ---------------- the world from above: ground (slides with the camera), air (streaks) */}
      <g opacity={bgO}>
        {Array.from({ length: 22 }, (_, i) => {
          const x = wrap(i * 110 + cx);
          return <line key={i} x1={x} y1={80} x2={x} y2={1060} stroke={C.line} strokeOpacity={0.07} strokeWidth={1.5} />;
        })}
        {TUFTS.map((k, i) => {
          const x = ((((k.x + cx) % 2400) + 2400) % 2400) - 240;
          return <path key={i} d={`M${x - 8},${k.y} L${x + 8},${k.y} M${x},${k.y - 8} L${x},${k.y + 8}`} stroke={C.line} strokeOpacity={0.22} strokeWidth={2} />;
        })}
        {STREAKS.map((k, i) => {
          const x = wrap(k.x - WPX * v + cx);
          return <line key={i} x1={x} y1={k.y} x2={x + k.l} y2={k.y} stroke={C.line} strokeOpacity={k.a} strokeWidth={2} strokeLinecap="round" />;
        })}
        {/* the rifle on the ground, pointing downrange (up) */}
        <g transform={`translate(${960 + cx},0)`} opacity={1 - ramp(v, T.bullet - 0.6, 0.5)}>
          <path d="M-14,1060 L-14,980 L14,980 L14,1060" stroke={C.line} strokeWidth={3} fill="none" />
        </g>
      </g>
      <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>TOP VIEW · DOWNRANGE IS UP</text>
      <g opacity={window01(v, T.wind - 0.3, T.bullet, 0.4)}>
        <ArrowA x={1820} y={300} a={Math.PI} len={300} w={5} head={22} />
        <text x={1820} y={270} textAnchor="end" style={label(ramp(v, T.fourHalf - 0.2, 0.4), { fontSize: 26 })}>WIND 4.5 m/s · FROM THE RIGHT</text>
      </g>
      <text x={960} y={140} textAnchor="middle" style={label(window01(v, T.dragComes - 0.2, T.through, 0.3), { fontSize: 28 })}>DRAG = THE AIR HITTING THE BULLET</text>
      <text x={960} y={140} textAnchor="middle" style={label(window01(v, T.through - 0.2, T.look, 0.3), { fontSize: 28 })}>WHAT MATTERS: THROUGH THE AIR, NOT OVER THE GROUND</text>

      {/* the balloon, seen from above */}
      <g opacity={bO * (1 - 0.85 * ramp(v, T.bullet + 0.4, 0.8))}>
        <circle cx={bx} cy={by} r={80} fill="none" stroke={C.line} strokeWidth={3} />
        {[0, 1, 2, 3].map((i) => <line key={i} x1={bx} y1={by} x2={bx + 80 * Math.cos(i * Math.PI / 4)} y2={by + 80 * Math.sin(i * Math.PI / 4)} stroke={C.line} strokeWidth={1.5} opacity={0.5} />)}
        {[0, 1, 2, 3].map((i) => <line key={`b${i}`} x1={bx} y1={by} x2={bx - 80 * Math.cos(i * Math.PI / 4)} y2={by - 80 * Math.sin(i * Math.PI / 4)} stroke={C.line} strokeWidth={1.5} opacity={0.5} />)}
        <rect x={bx - 10} y={by - 10} width={20} height={20} fill={C.bg} stroke={C.line} strokeWidth={2} />
        <text x={bx} y={by - 104} textAnchor="middle" style={label(1, { fontSize: 22 })}>US</text>
      </g>
      <text x={960} y={140} textAnchor="middle" style={label(window01(v, T.still - 0.2, T.slides, 0.3), { fontSize: 30 })}>FOR US, THE AIR STANDS STILL</text>
      <g opacity={window01(v, T.slides - 0.2, T.bullet + 0.4, 0.3)}>
        <text x={960} y={140} textAnchor="middle" style={label(1, { fontSize: 30 })}>… AND THE GROUND SLIDES RIGHT</text>
        <ArrowA x={760} y={190} a={0} len={400} w={3} head={16} />
      </g>

      {/* a bullet flies past */}
      <g opacity={trailO}>
        <line x1={P0[0]} y1={P0[1]} x2={P0[0]} y2={60} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
        <text x={P0[0] - 16} y={240} textAnchor="end" style={label(0.6, { fontSize: 18 })}>STRAIGHT DOWNRANGE</text>
        <line x1={P0[0]} y1={P0[1]} x2={pHead[0]} y2={pHead[1]} stroke={C.line} strokeWidth={3 + 2 * trailHot} strokeOpacity={0.55 + 0.45 * trailHot}
          filter={trailHot > 0.3 ? "url(#glow)" : undefined} />
        {v - tP < (P0[1] + 40) / VY && <Dot x={pHead[0]} y={pHead[1]} r={9} />}
        <g opacity={ramp(v, tP + 0.9, 0.4)}>
          <text x={pHead[0] + 18} y={180} style={label(0.85, { fontSize: 20 })}>THE BULLET'S PATH</text>
          <text x={pHead[0] + 18} y={206} style={label(0.85, { fontSize: 20 })}>THROUGH THE AIR</text>
          <text x={pHead[0] + 18} y={236} style={label(0.55, { fontSize: 18 })}>SIDEWAYS DRAWN ×20</text>
        </g>
      </g>

      {/* ---------------- the diagram: forward 790, right 4.5 (drawn ×20) */}
      <g opacity={diagO}>
        <g filter={vaP > 0 ? undefined : "url(#glow)"}>
          <Arrow x1={D0[0]} y1={D0[1]} x2={vTip[0]} y2={vTip[1]} p={vP} w={4} head={20} opacity={1 - 0.45 * vaP} />
        </g>
        <VecText x={D0[0] - 30} y={(D0[1] + vTip[1]) / 2} s="v" size={40} anchor="end" />
        <text x={D0[0] - 30} y={(D0[1] + vTip[1]) / 2 + 44} textAnchor="end" style={mono(0.8, 28)}>790 m/s</text>
        <Arrow x1={D0[0]} y1={D0[1]} x2={D0[0] + SL} y2={D0[1]} p={sideP} w={4} head={14} />
        <text x={D0[0] + SL + 16} y={D0[1] + 10} style={mono(sideP, 28)}>4.5 m/s · with the ground</text>
        <text x={960} y={140} textAnchor="middle" style={label(window01(v, T.sideways - 0.3, T.formula, 0.3), { fontSize: 28 })}>THROUGH THE AIR: A LITTLE SIDEWAYS</text>
      </g>
      {/* v − w: turn the wind arrow around and put it on the tip */}
      <g opacity={wO}>
        <Formula toks={FW} cx={960} y={130} size={64} reveal={ramp(v, T.formula - 0.1, 1.1)}
          active={v >= T.minus - 0.1 && v < T.newArrow ? "w" : v >= T.newArrow - 0.1 && v < T.thisIs ? "va" : null} />
        <ArrowA x={wx} y={wy} a={wAng} len={SL} w={4} head={14} glow={flip > 0 && flip < 1} />
        <g opacity={1 - move}>
          <VecText x={W0[0] - SL / 2} y={W0[1] - 26} s="w" size={36} anchor="middle" o={1 - flip} />
          <text x={W0[0] + (flip > 0.5 ? SL / 2 : -SL / 2)} y={W0[1] - 26} textAnchor="middle" style={mono(flip, 36)}>−w</text>
          <text x={W0[0] - SL / 2} y={W0[1] + 44} textAnchor="middle" style={label(1 - flip, { fontSize: 20 })}>THE WIND</text>
        </g>
        <g filter="url(#glow)">
          <Arrow x1={D0[0]} y1={D0[1]} x2={vTip[0] + SL} y2={vTip[1]} p={vaP} w={5} head={22} />
        </g>
        <g opacity={vaP}>
          <VecText x={vTip[0] + SL + 20} y={vTip[1] + 60} s="v" subS="air" size={40} />
          <text x={vTip[0] + SL + 20} y={vTip[1] + 104} style={label(0.85, { fontSize: 20 })}>THROUGH THE AIR</text>
        </g>
        <g opacity={angO}>
          <path d={`M${D0[0]},${D0[1] - 260} A260,260 0 0 1 ${D0[0] + 260 * Math.sin(Math.atan(SL / VL))},${D0[1] - 260 * Math.cos(Math.atan(SL / VL))}`}
            stroke={C.line} strokeWidth={2} fill="none" />
          <text x={D0[0] + 70} y={D0[1] - 236} style={mono(1, 32, { fontWeight: 600 })}>0.33°</text>
          <text x={D0[0] + 70} y={D0[1] - 200} style={label(0.7, { fontSize: 18 })}>DRAWN ×20</text>
        </g>
      </g>
      {/* this velocity goes into the drag formula */}
      <g opacity={fO}>
        <Formula toks={FD1} cx={1440} y={700} size={46} reveal={ramp(v, T.thisIs, 0.8)} opacity={1 - swap} />
        <Formula toks={FD2} cx={1440} y={700} size={46} reveal={1} opacity={swap} active={v >= T.everywhere && v < T.everywhere + 2.2 ? "air" : null} />
        <Formula toks={FM} cx={1440} y={790} size={46} reveal={ramp(v, T.everywhere + 0.6, 0.6)} active={v >= T.everywhere + 0.6 && v < T.everywhere + 2.5 ? "air" : null} />
      </g>
      <Dot x={D0[0]} y={D0[1]} o={diagO} />
    </Scene>
  );
};
