// Episode 3, scene 3 — the side part: drag points against v_air, so it tilts back and a little left. The bookmarked z line
// comes back: a_z = −D · v_air,z / |v_air| = 48 g × 4.5 / 790 = 0.27 g. The bullet curves left, 3.35 m at 1000 m
// (the library, level bore). The balloon drifts 9 m in the same 2 s: big and light, it takes the wind's 4.5 m/s at
// once. The bullet starts with zero side velocity and the side drag builds it up slowly: 0.3 m/s at 100 m,
// 1.5 at 500 m, 2.7 at 1000 m (library) — on average 1.7 m/s, so 3.35 m, not 9. No drag, no drift.
import React from "react";
import { C, ease, ramp, window01 } from "../theme";
import { Arrow, Formula, P, Tok, label, layoutFormula, mono } from "../lib/draw";
import { ArrowA, Bookmark, Dot, FLIGHT, Magnifier, Scene, VecText, sub, t_, useVoiceTime, vec } from "../ep02/kit";
import { cue } from "./timing";

const N = 3;
const T = {
  tilted: cue(N, "tilted"), back: cue(N, "back"), left: cue(N, "left"), remember: cue(N, "Remember"), before: cue(N, "Before"),
  sidePart: cue(N, "side part"), sideDrag: cue(N, "side drag is"), dTimes: cue(N, "d times"), muzzle: cue(N, "At the muzzle"),
  quarter: cue(N, "quarter"), trick: cue(N, "whole trick"), hand: cue(N, "like a hand"), turns: cue(N, "turns"), km: cue(N, "At one kilometer"),
  interesting: cue(N, "something interesting"), balloon: cue(N, "balloon"), nine: cue(N, "nine meters"), why: cue(N, "Why does"),
  give: cue(N, "doesn't give"), push: cue(N, "only push"), slower: cue(N, "slower"), catches: cue(N, "catches up"), asFast: cue(N, "as fast as"),
  nothing: cue(N, "nothing left"), balloon2: cue(N, "balloon", 2), bigLight: cue(N, "balloon is big"), lots: cue(N, "Lots of drag"),
  atOnce: cue(N, "almost at once"), whole: cue(N, "whole time"), twoSec9: cue(N, "Two seconds nine"), small: cue(N, "bullet is small"),
  zero: cue(N, "zero side"), build: cue(N, "build it up"), at100: cue(N, "At 100"), at500: cue(N, "At 500"), at1k: cue(N, "At one kilometer", 2),
  never: cue(N, "never gets"), soBalloon: cue(N, "So the balloon"), starts: cue(N, "starts at zero"), average: cue(N, "On average"),
  forTwo: cue(N, "For two seconds", 2), noDrag: cue(N, "no drag"), lessDrag: cue(N, "less drag"),
};

const SIDE = 20, VL = 360, SL = VL * (4.5 / 790) * SIDE;
const D0: P = [560, 520];
const DL = 300, HV = Math.hypot(SL, VL);
const dEnd: P = [D0[0] - (SL / HV) * DL, D0[1] + (VL / HV) * DL];   // exactly against v_air

const FZ: Tok[] = [t_("a"), sub("z"), t_(" = − D · "), t_("v", "vz"), sub("air,z", "vz"), t_(" / |"), vec("v"), sub("air"), t_("|")];
const FN: Tok[] = [t_("a"), sub("z"), t_(" = 48 g × "), t_("4.5", "vz"), t_(" / 790")];
const FN_END = (() => { const L = layoutFormula(FN, 1240, 54); return L[L.length - 1].x + L[L.length - 1].w; })();

// the top-view track: downrange up, 0.78 px per metre; side 60 px per metre, to the left
const TX0 = 1300, TY0 = 980, KD = 0.78, KS = 60;
const tx = (z: number) => TX0 - z * KS, ty = (x: number) => TY0 - x * KD;
const END = FLIGHT[FLIGHT.length - 1];
const BL = 4.5 * END.t;                                  // the balloon's drift, 9.0 m

// the windy flight in its own time (tw): drift z and side velocity dz/dt
const zAt = (tau: number) => {
  const i = FLIGHT.findIndex((r) => r.tw >= tau);
  if (i < 0) return { z: END.z, vz: (END.z - FLIGHT[FLIGHT.length - 2].z) / (END.tw - FLIGHT[FLIGHT.length - 2].tw), x: 1000 };
  if (i === 0) return { z: 0, vz: 0, x: 0 };
  const a = FLIGHT[i - 1], b = FLIGHT[i], k = (tau - a.tw) / (b.tw - a.tw);
  return { z: a.z + (b.z - a.z) * k, vz: (b.z - a.z) / (b.tw - a.tw), x: a.x + (b.x - a.x) * k };
};
const twAt = (x: number) => FLIGHT.find((r) => r.x >= x)!.tw;
const AVG = END.z / END.tw;                              // 1.67 m/s
// side-velocity bars: right end at AX, growing to the left, KB px per m/s
const AX = 880, KB = 110, RB = 560, RU = 760;
const SideBar: React.FC<{ y: number; vel: number; name: string; note?: string; noteO?: number }> = ({ y, vel, name, note, noteO = 1 }) => {
  const x0 = AX - 4.5 * KB, xv = AX - vel * KB, gap = 4.5 - vel;
  return (
    <g>
      <text x={x0} y={y - 34} style={label(0.9, { fontSize: 24 })}>{name}</text>
      {note ? <text x={x0 + 180} y={y - 34} style={label(noteO * 0.7, { fontSize: 20 })}>{note}</text> : null}
      <rect x={x0} y={y - 18} width={4.5 * KB} height={36} fill="none" stroke={C.line} strokeWidth={1.5} strokeDasharray="6 6" opacity={0.7} />
      {gap > 0.05 && <rect x={x0} y={y - 18} width={xv - x0} height={36} fill={C.drag} opacity={0.28} />}
      <text x={(x0 + xv) / 2} y={y + 8} textAnchor="middle" style={label(Math.min(1, gap / 0.8), { fontSize: 20, fill: C.drag })}>← PUSH</text>
      <rect x={xv} y={y - 18} width={vel * KB} height={36} fill={C.line} opacity={0.85} />
      <text x={AX + 20} y={y + 10} style={mono(1, 30, { fontWeight: 600 })}>{vel.toFixed(1)} m/s</text>
    </g>
  );
};

export const S03WindSide: React.FC = () => {
  const v = useVoiceTime();
  // ---------------- part 1: the tilted drag, the z line
  const o1 = 1 - ramp(v, T.trick - 0.2, 0.6);
  const dragP = ramp(v, -0.3, 0.9, ease);
  const split = ramp(v, T.back - 0.2, 0.7);
  const lens = ramp(v, T.muzzle - 0.2, 0.7);
  // ---------------- part 2: the bullet curves left
  const o2 = ramp(v, T.trick - 0.2, 0.6);
  // the flight runs once on "like a hand", then replays from the barrel on "zero side velocity", slowly at first
  const replay = v >= T.zero - 0.2;
  const keys: [number, number][] = [[T.zero, 0], [T.build, 0], [T.at100 + 0.4, twAt(100)], [T.at500 + 0.4, twAt(500)], [T.at1k + 0.4, END.tw]];
  let tau2 = END.tw;
  for (let k = 1; k < keys.length; k++) if (v < keys[k][0]) { const [a, ta] = keys[k - 1], [b, tb] = keys[k]; tau2 = ta + (tb - ta) * Math.max(0, (v - a) / (b - a)); break; }
  const tau = replay ? tau2 : ramp(v, T.hand - 0.6, 3.6, (k) => k) * END.t;
  const i = Math.max(1, FLIGHT.findIndex((r) => r.tw >= tau));
  const trail = [...FLIGHT.slice(0, i).map((r) => [tx(r.z), ty(r.x)] as P), [tx(zAt(tau).z), ty(zAt(tau).x)] as P];
  const dot = trail[trail.length - 1];
  const ghost = FLIGHT.map((r) => [tx(r.z), ty(r.x)] as P);
  const trackO = 1;
  const bulletV = replay ? zAt(tau2).vz : 0;
  // the top bar: first "something" (demo), then the balloon
  const isBalloon = v >= T.balloon2 - 0.3;
  const rowA = isBalloon
    ? { name: "BALLOON", note: v >= T.lots - 0.2 && v < T.whole ? "BIG · LIGHT · LOTS OF DRAG" : "", vel: 4.5 * ramp(v, T.atOnce - 0.2, 0.4, ease) }
    : { name: "SOMETHING", note: "", vel: 1.2 + 3.3 * ramp(v, T.catches, 2.8, ease) };
  // ---------------- part 3: the balloon, and the bullet building up its side velocity
  const bK = ramp(v, T.balloon, 2.0, ease);
  const MARKS = [
    { x: 100, at: T.at100 + 0.4, text: "0.3 m/s · 2 cm" },
    { x: 500, at: T.at500 + 0.4, text: "1.5 m/s · 65 cm" },
    { x: 1000, at: T.at1k + 0.4, text: "2.7 m/s · 3.35 m" },
  ].map((m) => ({ ...m, z: zAt(twAt(m.x)).z }));

  return (
    <Scene n={N}>
      {/* ---------------- part 1 */}
      <g opacity={o1}>
        <Arrow x1={D0[0]} y1={D0[1]} x2={D0[0] + SL} y2={D0[1] - VL} w={4} head={20} opacity={0.75} />
        <VecText x={D0[0] + SL + 18} y={D0[1] - VL + 30} s="v" subS="air" size={36} />
        <g filter="url(#glow)">
          <Arrow x1={D0[0]} y1={D0[1]} x2={dEnd[0]} y2={dEnd[1]} p={dragP} color={C.drag} w={6} head={24} />
        </g>
        <text x={D0[0] + 30} y={(D0[1] + dEnd[1]) / 2} style={mono(dragP, 32, { fill: C.drag })}>drag</text>
        {/* its two parts: back, and a little to the left */}
        <g opacity={split}>
          <line x1={D0[0]} y1={D0[1]} x2={D0[0]} y2={dEnd[1]} stroke={C.drag} strokeWidth={2} strokeDasharray="6 6" opacity={0.7} />
          <g filter="url(#glow)"><Arrow x1={D0[0]} y1={dEnd[1]} x2={dEnd[0]} y2={dEnd[1]} p={ramp(v, T.left - 0.4, 0.6)} color={C.drag} w={4} head={12} /></g>
          <text x={D0[0] + 20} y={dEnd[1] + 50} style={label(ramp(v, T.back - 0.1, 0.4), { fontSize: 22, fill: C.drag })}>BACK</text>
          <text x={dEnd[0] - 20} y={dEnd[1] + 10} textAnchor="end" style={label(ramp(v, T.left - 0.2, 0.4), { fontSize: 22, fill: C.drag })}>A LITTLE LEFT</text>
        </g>
        <text x={560} y={130} textAnchor="middle" style={label(window01(v, -0.2, T.remember, 0.3), { fontSize: 26 })}>AGAINST THE VELOCITY THROUGH THE AIR</text>
        {/* the z line, back from scene 5 */}
        <g opacity={ramp(v, T.remember - 0.3, 0.5)}>
          <Bookmark x={930} y={300} o={1} s={1.3} />
          <Formula toks={FZ} cx={1340} y={310} size={50} reveal={ramp(v, T.remember, 0.9)} active={v >= T.sidePart - 0.1 && v < T.muzzle ? "vz" : null} />
          <text x={1000} y={400} style={mono(window01(v, T.before - 0.2, T.sidePart, 0.3) * 0.8, 32)} xmlSpace="preserve">before:  side velocity 0  →  side drag 0</text>
          <text x={1000} y={400} style={mono(ramp(v, T.sidePart - 0.1, 0.4), 32)} xmlSpace="preserve">now:  side part through the air = 4.5 m/s</text>
          <Formula toks={FN} cx={1240} y={520} size={54} reveal={ramp(v, T.dTimes - 0.3, 1.6)} active={v >= T.dTimes + 0.6 && v < T.muzzle ? "vz" : null} />
          <text x={FN_END + 16} y={520} style={mono(ramp(v, T.muzzle - 0.2, 0.4), 54, { fill: C.drag, fontWeight: 700 })} filter="url(#glow)">= 0.27 g</text>
          <text x={1340} y={600} textAnchor="middle" style={label(ramp(v, T.quarter - 0.2, 0.4), { fontSize: 26, fill: C.drag })}>ABOUT A QUARTER OF A g · TO THE LEFT</text>
        </g>
        <Magnifier cx={300} cy={860} r={120} p={lens} id="side">
          <g transform={`translate(300,860) scale(3) translate(${-(D0[0] + dEnd[0]) / 2},${-dEnd[1]})`}>
            <line x1={D0[0]} y1={dEnd[1]} x2={dEnd[0]} y2={dEnd[1]} stroke={C.drag} strokeWidth={4 / 3} />
            <line x1={D0[0]} y1={D0[1]} x2={dEnd[0]} y2={dEnd[1]} stroke={C.drag} strokeWidth={2} opacity={0.5} />
          </g>
          <text x={300} y={900} textAnchor="middle" style={mono(1, 30, { fill: C.drag, fontWeight: 600 })}>0.27 g</text>
        </Magnifier>
        <Dot x={D0[0]} y={D0[1]} />
      </g>

      {/* ---------------- part 2: the bullet curves left */}
      <g opacity={o2}>
        <g opacity={trackO}>
          <line x1={TX0} y1={TY0} x2={TX0} y2={ty(1000) - 30} stroke={C.faint} strokeWidth={1.5} strokeDasharray="8 8" />
          <line x1={tx(10)} y1={ty(1000)} x2={TX0 + 60} y2={ty(1000)} stroke={C.faint} strokeWidth={1.5} />
          <text x={TX0 + 70} y={ty(1000) + 8} style={mono(0.6, 22)}>1000 m</text>
          <path d={`M${TX0 - 14},${TY0 + 60} L${TX0 - 14},${TY0 + 4} L${TX0 + 14},${TY0 + 4} L${TX0 + 14},${TY0 + 60}`} stroke={C.line} strokeWidth={3} fill="none" />
          {replay && <path d={"M" + ghost.map((q) => q.join(",")).join(" L")} stroke={C.line} strokeWidth={2} fill="none" opacity={0.25} />}
          <path d={"M" + trail.map((q) => q.join(",")).join(" L")} stroke={C.line} strokeWidth={3} fill="none" />
          <Dot x={dot[0]} y={dot[1]} />
          <g opacity={ramp(v, T.km - 0.2, 0.5)}>
            <line x1={tx(END.z)} y1={ty(1000) + 30} x2={TX0} y2={ty(1000) + 30} stroke={C.line} strokeWidth={2} />
            <text x={tx(END.z) - 20} y={ty(1000) + 42} textAnchor="end" style={mono(1, 34, { fontWeight: 600 })}>3.35 m left</text>
          </g>
          <text x={1560} y={720} style={label(ramp(v, T.km, 0.4) * 0.6, { fontSize: 20 })}>WIND 4.5 m/s ←</text>
        </g>
        <text x={560} y={300} textAnchor="middle" style={label(window01(v, T.hand - 0.4, T.interesting, 0.3), { fontSize: 30 })}>THE WIND DOESN'T PUSH LIKE A HAND</text>
        <text x={560} y={350} textAnchor="middle" style={label(window01(v, T.turns - 0.2, T.interesting, 0.3), { fontSize: 30, fill: C.drag })}>IT TURNS THE DRAG A LITTLE</text>
        {/* the balloon drifts with the air: 9 m in the same 2 s */}
        <g opacity={ramp(v, T.balloon - 0.3, 0.5)}>
          <line x1={TX0} y1={ty(1000) - 40} x2={tx(BL * bK)} y2={ty(1000) - 40} stroke={C.line} strokeWidth={2} strokeDasharray="6 6" />
          <circle cx={tx(BL * bK)} cy={ty(1000) - 40} r={22} fill={C.bg} stroke={C.line} strokeWidth={3} />
          <text x={tx(BL * bK)} y={ty(1000) - 80} textAnchor="middle" style={mono(ramp(v, T.nine - 0.3, 0.4), 34, { fontWeight: 600 })}>9 m</text>
          <text x={tx(BL) - 40} y={ty(1000) - 30} textAnchor="end" style={label(ramp(v, T.balloon, 0.4) * 0.7, { fontSize: 20 })}>THE AIR</text>
        </g>
        {/* marks left by the replayed flight: side velocity and drift at 100 / 500 / 1000 m */}
        {MARKS.map((m, k) => (
          <g key={k} opacity={ramp(v, m.at, 0.4)}>
            <circle cx={tx(m.z)} cy={ty(m.x)} r={5} fill={C.line} />
            <text x={TX0 + 30} y={ty(m.x) + (m.x === 1000 ? 50 : 8)} style={mono(1, 26, { fontWeight: m.x === 1000 ? 600 : 400 })}>{m.text}</text>
          </g>
        ))}
        <text x={560} y={300} textAnchor="middle" style={label(window01(v, T.why - 0.2, T.give - 0.2, 0.3), { fontSize: 30 })}>WHY ONLY ABOUT A THIRD?</text>
        <text x={560} y={300} textAnchor="middle" style={label(window01(v, T.give - 0.2, T.bigLight - 0.3, 0.3), { fontSize: 28 })}>THE WIND CAN ONLY PUSH · THROUGH THE DRAG</text>

        {/* side velocity bars, growing to the left: the dashed box is the air's 4.5 m/s, the coral gap is the push */}
        <g opacity={ramp(v, T.push - 0.2, 0.5) * (1 - ramp(v, T.noDrag - 0.4, 0.5) * 0.5)}>
          <text x={AX - 4.5 * KB} y={440} style={label(0.7, { fontSize: 22 })}>SIDE VELOCITY · TO THE LEFT</text>
          <text x={AX - 4.5 * KB} y={472} style={mono(0.5, 20)}>dashed: the air, 4.5 m/s</text>
          <SideBar y={RB} vel={rowA.vel} name={rowA.name} note={rowA.note} />
          <text x={AX - 4.5 * KB} y={RB + 70} style={label(ramp(v, T.nothing - 0.2, 0.4) * (1 - ramp(v, T.balloon2 - 0.4, 0.3)), { fontSize: 22 })}>NOTHING LEFT TO PUSH</text>
          <text x={AX - 4.5 * KB} y={RB + 70} style={mono(ramp(v, T.twoSec9 - 0.2, 0.4), 28)} xmlSpace="preserve">4.5 m/s × 2 s = <tspan fontWeight={700}>9 m</tspan></text>
          <g opacity={ramp(v, T.small - 0.3, 0.5)}>
            <SideBar y={RU} vel={bulletV} name="BULLET" note={window01(v, T.small - 0.2, T.zero + 0.6, 0.3) > 0 ? "SMALL · HEAVY FOR ITS SIZE" : ""} noteO={window01(v, T.small - 0.2, T.zero + 0.6, 0.3)} />
            <text x={AX - 4.5 * KB} y={RU + 70} style={label(window01(v, T.never - 0.2, T.soBalloon, 0.3), { fontSize: 22 })}>IT NEVER GETS TO 4.5</text>
            <text x={AX - 4.5 * KB} y={RU + 70} style={mono(ramp(v, T.starts - 0.2, 0.4), 28)} xmlSpace="preserve">0 → 2.7 m/s</text>
            <text x={AX - 4.5 * KB} y={RU + 110} style={mono(ramp(v, T.forTwo - 0.2, 0.4), 28)} xmlSpace="preserve">≈ 1.7 m/s × 2 s = <tspan fontWeight={700}>3.35 m</tspan></text>
            <g opacity={ramp(v, T.average - 0.2, 0.4)}>
              <line x1={AX - AVG * KB} y1={RU - 34} x2={AX - AVG * KB} y2={RU + 34} stroke={C.line} strokeWidth={2.5} strokeDasharray="4 5" />
              <text x={AX - AVG * KB} y={RU - 44} textAnchor="middle" style={mono(0.9, 22)}>average 1.7</text>
            </g>
          </g>
        </g>
        <text x={560} y={300} textAnchor="middle" style={label(ramp(v, T.noDrag - 0.2, 0.4), { fontSize: 34 })}>NO DRAG, NO WIND DRIFT</text>
        <text x={560} y={350} textAnchor="middle" style={label(ramp(v, T.lessDrag - 0.2, 0.4) * 0.85, { fontSize: 26 })}>LESS DRAG → LESS SIDE VELOCITY → LESS DRIFT</text>
      </g>
    </Scene>
  );
};
