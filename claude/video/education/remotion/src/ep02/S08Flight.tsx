// Scene 8 — the whole flight from a level barrel (the library's level-bore trajectory): almost straight at first,
// bending at the end. No air: 1.27 s to 1000 m, ½gt² = 7.9 m. With air: 2.0 s; ½gt² would give 19.8 m, but the
// bullet falls 14.9 m, because the drag's vertical part (the y line from scene 5) brakes the fall.
import React from "react";
import { C, ease, ramp, window01 } from "../theme";
import { Arrow, Formula, P, Pen, Tok, label, mono } from "../lib/draw";
import { ArrowA, Dot, FLIGHT, G, Scene, T_FLIGHT, flightAtT, sub, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 8;
const T = {
  flight: cue(N, "whole flight"), barrel: cue(N, "barrel"), start: cue(N, "At the start"), straight: cue(N, "straight"), end: cue(N, "At the end"),
  bends: cue(N, "bends"), why: cue(N, "Why"), falls: cue(N, "falls the whole"), without: cue(N, "Without air"), rule: cue(N, "simple rule"),
  half: cue(N, "One half"), reaches: cue(N, "reaches"), almost: cue(N, "falls almost"), dashed: cue(N, "dashed"), withAir: cue(N, "With air"),
  twoSec: cue(N, "2 seconds of"), only: cue(N, "only"), less: cue(N, "Why less"), remember: cue(N, "Remember"), downward: cue(N, "downward"),
  pushes: cue(N, "pushes"), slows: cue(N, "slows"), two: cue(N, "two things"), longer: cue(N, "longer"), more: cue(N, "falls more"), breaks: cue(N, "breaks"),
};

const X0 = 260, KX = 1.24, Y0 = 300, KY = 28;
const px = (x: number) => X0 + x * KX, py = (y: number) => Y0 - y * KY;
const VEX = Math.round(KY / KX);
const PATH: P[] = FLIGHT.map((r) => [px(r.x), py(r.y)] as P);
const T_NA = 1000 / 790, FALL_NA = 0.5 * G * T_NA * T_NA;            // 1.266 s, 7.86 m
const NOAIR: P[] = Array.from({ length: 41 }, (_, i) => { const t = (i / 40) * T_NA; return [px(790 * t), py(-0.5 * G * t * t)] as P; });
const END = FLIGHT[FLIGHT.length - 1];
const GHOST = 0.5 * G * END.t * END.t;                                 // 19.8 m
const XNA = px(1000) + 50, XA = px(1000) + 180;

const FF: Tok[] = [t_("fall = ½ "), t_("g", "g"), t_(" "), t_("t²", "t")];
const FY: Tok[] = [t_("a"), sub("y"), t_(" = "), t_("− D · v", "dy", C.drag), sub("y", "dy", C.drag), t_(" / |", "dy", C.drag), vec("v", "dy", C.drag), t_("|", "dy", C.drag), t_(" − g")];

export const S08Flight: React.FC = () => {
  const v = useVoiceTime();
  const tau = Math.min(T_FLIGHT, ramp(v, T.flight - 0.2, T.end + 2.4 - T.flight, (k) => k) * T_FLIGHT);
  const s = flightAtT(tau);
  const i = FLIGHT.findIndex((r) => r.t >= tau);
  const trail: P[] = [...PATH.slice(0, Math.max(1, i)), [px(s.x), py(s.y)]];
  const prev = flightAtT(Math.max(0, tau - 0.02));
  const ang = Math.atan2(py(s.y) - py(prev.y), px(s.x) - px(prev.x) + 1e-6);   // screen angle of the path
  const naP = ramp(v, T.without, 1.4);
  const ghostP = ramp(v, T.twoSec - 0.1, 0.9, ease);
  const realP = ramp(v, T.only - 0.3, 0.9, ease);
  const upHot = ramp(v, T.pushes - 0.3, 0.5);

  return (
    <Scene n={N}>
      {/* the barrel, held level, and its line */}
      <g opacity={ramp(v, -0.5, 0.6)}>
        <path d={`M${X0 - 220},${Y0 - 16} L${X0 - 4},${Y0 - 16} L${X0 - 4},${Y0 + 16} L${X0 - 220},${Y0 + 16}`} stroke={C.line} strokeWidth={3} fill="none" />
        <line x1={X0} y1={Y0} x2={px(1000) + 260} y2={Y0} stroke={C.faint} strokeWidth={1.5} strokeDasharray="8 8" />
        <line x1={px(1000)} y1={Y0 - 30} x2={px(1000)} y2={py(-20)} stroke={C.faint} strokeWidth={1.5} />
        <text x={px(1000)} y={py(-20) + 34} textAnchor="middle" style={mono(0.6, 22)}>1000 m</text>
        {[250, 500, 750].map((m) => <text key={m} x={px(m)} y={py(-20) + 34} textAnchor="middle" style={mono(0.4, 20)}>{m}</text>)}
      </g>
      <text x={X0 - 112} y={Y0 - 40} textAnchor="middle" style={label(ramp(v, T.barrel - 0.2, 0.5), { fontSize: 24 })}>BARREL LEVEL</text>

      {/* the flight, step by step, with the drag arrow shrinking on the way */}
      <path d={"M" + trail.map((q) => q.join(",")).join(" L")} stroke={C.line} strokeWidth={3} fill="none" />
      {FLIGHT.filter((r, k) => k % 5 === 0 && r.t <= tau).map((r, k) => <circle key={k} cx={px(r.x)} cy={py(r.y)} r={3.5} fill={C.line} opacity={0.6} />)}
      <g opacity={1 - ramp(v, T.without - 0.4, 0.5) * 0.7}>
        <ArrowA x={px(s.x) - 14 * Math.cos(ang)} y={py(s.y) - 14 * Math.sin(ang)} a={ang + Math.PI} len={s.drag * 0.45} w={5} head={20} color={C.drag}
          glow opacity={ramp(v, T.flight, 0.4)} />
      </g>
      <text x={px(250)} y={py(-0.4) - 40} textAnchor="middle" style={label(window01(v, T.start, T.end + 0.4, 0.3), { fontSize: 24 })}>FAST · ALMOST STRAIGHT</text>
      <text x={px(830)} y={py(-12) + 10} textAnchor="end" style={label(window01(v, T.end, T.without, 0.3), { fontSize: 24 })}>SLOW · BENDS DOWN QUICKLY</text>
      <text x={560} y={150} textAnchor="middle" style={label(window01(v, T.falls - 0.3, T.without + 1, 0.3), { fontSize: 30 })}>IT FALLS THE WHOLE TIME IT FLIES</text>

      {/* no air: the dashed line, ½ g t² */}
      <Pen pts={NOAIR} p={naP} w={2.5} dash="10 9" tip={false} />
      <Formula toks={FF} cx={1180} y={180} size={56} reveal={ramp(v, T.half - 0.2, 1.0)} opacity={1 - ramp(v, T.two - 0.4, 0.4)} active={v >= T.only - 0.2 && v < T.remember ? "t" : null} />
      <text x={1180} y={232} textAnchor="middle" style={label(window01(v, T.rule - 0.2, T.withAir, 0.3) * 0.75, { fontSize: 20 })}>WITHOUT AIR</text>
      <g opacity={ramp(v, T.reaches - 0.2, 0.5)}>
        <text x={XNA} y={Y0 - 26} textAnchor="middle" style={mono(0.9, 28)}>1.27 s</text>
        <text x={XNA} y={Y0 - 60} textAnchor="middle" style={label(0.6, { fontSize: 18 })}>NO AIR</text>
      </g>
      <g opacity={ramp(v, T.almost - 0.3, 0.4)}>
        <line x1={XNA} y1={Y0} x2={XNA} y2={py(-FALL_NA * ramp(v, T.almost - 0.3, 0.8, ease))} stroke={C.line} strokeWidth={4} strokeDasharray="10 7" />
        <line x1={XNA - 14} y1={py(-FALL_NA)} x2={XNA + 14} y2={py(-FALL_NA)} stroke={C.line} strokeWidth={3} />
        <text x={XNA + 18} y={py(-FALL_NA) + 10} style={mono(1, 32, { fontWeight: 600 })}>7.9 m</text>
      </g>

      {/* with air: 2 s; ½ g t² would be 19.8 m, the bullet falls 14.9 m */}
      <g opacity={ramp(v, T.withAir - 0.2, 0.5)}>
        <text x={XA} y={Y0 - 26} textAnchor="middle" style={mono(1, 28, { fontWeight: 600 })}>{END.t.toFixed(1)} s</text>
        <text x={XA} y={Y0 - 60} textAnchor="middle" style={label(0.6, { fontSize: 18 })}>WITH AIR</text>
      </g>
      <g opacity={ghostP}>
        <line x1={XA - 18} y1={Y0} x2={XA - 18} y2={py(-GHOST * ghostP)} stroke={C.line} strokeOpacity={0.7} strokeWidth={4} strokeDasharray="4 8" />
        <line x1={XA - 32} y1={py(-GHOST)} x2={XA - 4} y2={py(-GHOST)} stroke={C.line} strokeOpacity={0.5} strokeWidth={3} />
        <text x={XA + 2} y={py(-GHOST) + 10} style={mono(0.6, 28)}>½gt² = {GHOST.toFixed(1)} m</text>
      </g>
      <g opacity={realP}>
        <line x1={XA + 18} y1={Y0} x2={XA + 18} y2={py(END.y * realP)} stroke={C.line} strokeWidth={5} />
        <line x1={XA + 4} y1={py(END.y)} x2={XA + 32} y2={py(END.y)} stroke={C.line} strokeWidth={3} />
        <text x={XA + 38} y={py(END.y) + 10} style={mono(1, 34, { fontWeight: 600 })}>{(-END.y).toFixed(1)} m</text>
        <line x1={px(1000) + 8} y1={py(END.y)} x2={XA + 4} y2={py(END.y)} stroke={C.line} strokeWidth={1.2} strokeDasharray="3 6" opacity={0.6} />
      </g>
      <text x={560} y={150} textAnchor="middle" style={label(window01(v, T.less - 0.2, T.slows - 0.2, 0.3), { fontSize: 30 })}>WHY LESS?</text>

      {/* the y line again: the drag's up part */}
      <g opacity={ramp(v, T.remember - 0.2, 0.5)}>
        <Formula toks={FY} cx={760} y={990} size={46} reveal={ramp(v, T.remember - 0.1, 0.9)} active={v >= T.pushes - 0.2 && v < T.two ? "dy" : null} />
      </g>
      {(() => {
        const x = px(END.x), y = py(END.y);
        return (
          <g opacity={ramp(v, T.downward - 0.4, 0.5) * (1 - ramp(v, T.two + 2, 0.6))}>
            <ArrowA x={x} y={y} a={ang} len={90} w={2.5} head={12} opacity={0.6} />
            <text x={x + 40} y={y + 70} style={mono(0.7, 22)}>a little down</text>
            <g filter="url(#glow)"><Arrow x1={x - 22} y1={y - 8} x2={x - 22} y2={y - 8 - 70 * upHot} color={C.drag} w={4} head={14} /></g>
            <text x={x - 40} y={y + 48} textAnchor="end" style={label(upHot, { fontSize: 22, fill: C.drag })}>DRAG'S UP PART</text>
          </g>
        );
      })()}
      <text x={560} y={150} textAnchor="middle" style={label(window01(v, T.slows - 0.2, T.two, 0.3), { fontSize: 30, fill: C.drag })}>THE DRAG BRAKES THE FALL, TOO</text>

      {/* two things */}
      <g opacity={ramp(v, T.two - 0.2, 0.5)}>
        <text x={960} y={130} textAnchor="middle" style={label(ramp(v, T.longer - 0.4, 0.4), { fontSize: 28 })}>1 · A LONGER FLIGHT → MORE TIME TO FALL</text>
        <text x={960} y={180} textAnchor="middle" style={label(ramp(v, T.breaks - 0.4, 0.4), { fontSize: 28, fill: C.drag })}>2 · IT BRAKES THE FALL, A LITTLE</text>
      </g>

      <Dot x={px(s.x)} y={py(s.y)} />
    </Scene>
  );
};
