// Scene 3 — two forces and Newton's law: a = F / m. Same force on twice the mass, half the acceleration; gravity
// gives every mass the same 1 g; episode 1's drag already divides by m, 48 g at the muzzle — gravity needs a
// magnifier next to it, and stays while the drag shrinks.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Formula, Tok, label, layoutFormula, mono } from "../lib/draw";
import { ArrowA, Dot, G, Magnifier, Scene, T_FLIGHT, flightAtT, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 3;
const T = {
  gravity: cue(N, "gravity"), drag: cue(N, "drag"), alone: cue(N, "alone"), accel: cue(N, "acceleration"), newton: cue(N, "Newton's"),
  force: cue(N, "force divided"), mass: cue(N, "mass"), same: cue(N, "same force"), twice: cue(N, "twice"), half: cue(N, "half"),
  special: cue(N, "Gravity is special"), pulls: cue(N, "pulls"), harder: cue(N, "harder to move"), falls: cue(N, "falls"), oneg: cue(N, "1g"),
  every: cue(N, "Every second"), formula: cue(N, "drag formula"), divides: cue(N, "divides"), muzzle: cue(N, "muzzle"), g48: cue(N, "48g"),
  next: cue(N, "Next to it"), magnifier: cue(N, "magnifier"), never: cue(N, "never"), smaller: cue(N, "smaller"),
};

const FA: Tok[] = [vec("a", "a"), t_(" = "), vec("F", "F"), t_(" / "), t_("m", "m")];
const FD: Tok[] = [t_("drag", undefined, C.drag), t_(" = ½ ρ · v² · Cd · A "), t_("/ m", "m")];
const FD_END = (() => { const L = layoutFormula(FD, 900, 46); return L[L.length - 1].x + L[L.length - 1].w; })();

export const S03Forces: React.FC = () => {
  const v = useVoiceTime();
  const oA = window01(v, -0.5, T.newton, 0.4);
  const oB = window01(v, T.newton - 0.2, T.special, 0.4);
  const oC = window01(v, T.special - 0.2, T.formula, 0.4);
  const oD = ramp(v, T.formula - 0.2, 0.5);

  const active =
    v >= T.half - 0.1 && v < T.special ? "a" : v >= T.twice - 0.1 && v < T.half ? "m" : v >= T.same - 0.1 && v < T.twice ? "F" :
    v >= T.mass - 0.1 && v < T.same ? "m" : v >= T.force - 0.1 && v < T.mass ? "F" : v >= T.newton + 1.2 && v < T.force ? "a" : null;

  // ---- A: two forces on the dot, and what acceleration means
  const vLen = lerp(420, 250, ramp(v, T.accel + 0.3, 3.2));
  // ---- C: falling side by side
  const tau = Math.max(0, Math.min(v - T.falls, 4.0));
  const fall = 0.5 * G * tau * tau * 6;
  // ---- D/E: drag vs gravity, to scale (1 g = 20 px); time runs after "never changes"
  const run = Math.max(0, v - (T.never + 0.4)) * 0.42;
  const fl = flightAtT(Math.min(run, T_FLIGHT));
  const dragG = fl.drag / G;
  const DX = 1580, DY = 640, PXG = 20;
  const mag = ramp(v, T.magnifier - 0.3, 0.8);
  const Z = 5;
  const SHARE_W = 760;

  return (
    <Scene n={N}>
      <Formula toks={FA} cx={960} y={150} size={72} reveal={ramp(v, T.newton, 1.2)} active={active} opacity={1 - ramp(v, T.formula - 0.3, 0.5) * 0.65} />

      {/* ---------------- A */}
      <g opacity={oA}>
        <ArrowA x={1080} y={560} a={Math.PI} len={380} p={ramp(v, T.drag - 0.2, 0.6, ease)} color={C.drag} w={5} head={20} glow />
        <text x={890} y={530} textAnchor="middle" style={label(ramp(v, T.drag - 0.1, 0.4), { fontSize: 26, fill: C.drag })}>DRAG</text>
        <ArrowA x={1100} y={580} a={Math.PI / 2} len={130} p={ramp(v, T.gravity - 0.2, 0.6, ease)} w={4} head={18} />
        <text x={1124} y={680} style={label(ramp(v, T.gravity - 0.1, 0.4), { fontSize: 26 })}>GRAVITY</text>
        <g opacity={ramp(v, T.accel - 0.3, 0.5)}>
          <ArrowA x={1120} y={560} a={0} len={vLen} w={3} head={16} opacity={0.9} />
          <ArrowA x={1120} y={560} a={0} len={420} w={1.5} head={10} opacity={0.3} />
          <text x={1120 + vLen / 2} y={530} textAnchor="middle" style={mono(0.9, 30)}>velocity</text>
          <text x={960} y={820} textAnchor="middle" style={label(1, { fontSize: 28 })}>ACCELERATION = HOW FAST THE VELOCITY CHANGES</text>
        </g>
        <Dot x={1100} y={560} o={ramp(v, -0.4, 0.5)} />
      </g>

      {/* ---------------- B: same force, twice the mass, half the acceleration */}
      <g opacity={oB}>
        {[
          { y: 470, r: 16, fill: "rgba(233,215,165,0.18)", m: "m", a: 380, aS: "a", t0: T.newton + 1.0, ta: T.newton + 1.4 },
          { y: 770, r: 26, fill: "rgba(233,215,165,0.7)", m: "2m", a: 190, aS: "a / 2", t0: T.same - 0.2, ta: T.half - 0.2 },
        ].map((d) => {
          const o = ramp(v, d.t0, 0.5), fP = ramp(v, d.t0 + 0.3, 0.6, ease), aP = ramp(v, d.ta, 0.7, ease);
          return (
            <g key={d.m} opacity={o}>
              <circle cx={640} cy={d.y} r={d.r} fill={d.fill} stroke={C.line} strokeWidth={3} />
              <text x={600 - d.r} y={d.y + 12} textAnchor="end" style={mono(1, 40)}>{d.m}</text>
              <ArrowA x={640 + d.r + 10} y={d.y} a={0} len={300} p={fP} w={3} head={16} />
              <text x={640 + d.r + 160} y={d.y - 22} textAnchor="middle" style={mono(fP, 32)}>F</text>
              <ArrowA x={640 + d.r + 10} y={d.y + 62} a={0} len={d.a} p={aP} w={6} head={22} glow />
              <text x={640 + d.r + 30 + d.a} y={d.y + 74} style={mono(aP, 36, { fontWeight: 600 })}>{d.aS}</text>
            </g>
          );
        })}
        <text x={1420} y={630} textAnchor="middle" style={label(ramp(v, T.twice, 0.4), { fontSize: 26 })}>TWICE THE MASS</text>
        <text x={1420} y={672} textAnchor="middle" style={label(ramp(v, T.half, 0.4), { fontSize: 26 })}>HALF THE ACCELERATION</text>
      </g>

      {/* ---------------- C: gravity gives everything the same acceleration */}
      <g opacity={oC}>
        {[
          { x: 760, r: 16, fill: "rgba(233,215,165,0.18)", f: 90, fS: "F" },
          { x: 1160, r: 26, fill: "rgba(233,215,165,0.7)", f: 180, fS: "2F" },
        ].map((d) => {
          const y = 330 + fall;
          const fP = ramp(v, T.pulls - 0.1, 0.6, ease) * (1 - ramp(v, T.falls - 0.3, 0.4));
          const aP = ramp(v, T.harder - 0.1, 0.6, ease);
          return (
            <g key={d.x}>
              {/* a ghost every second of the fall */}
              {[1, 2, 3].filter((s) => tau >= s).map((s) => (
                <circle key={s} cx={d.x} cy={330 + 0.5 * G * s * s * 6} r={d.r} fill="none" stroke={C.line} strokeWidth={1.5} opacity={0.35} />
              ))}
              <circle cx={d.x} cy={y} r={d.r} fill={d.fill} stroke={C.line} strokeWidth={3} />
              <ArrowA x={d.x - 40} y={y + d.r + 6} a={Math.PI / 2} len={d.f} p={fP} w={3} head={14} />
              <text x={d.x - 60} y={y + d.r + 6 + d.f / 2} textAnchor="end" style={mono(fP, 30)}>{d.fS}</text>
              <ArrowA x={d.x + 40} y={y + d.r + 6} a={Math.PI / 2} len={120} p={aP} w={6} head={20} glow />
              <text x={d.x + 60} y={y + d.r + 70} style={mono(aP, 30, { fontWeight: 600 })}>a</text>
            </g>
          );
        })}
        <text x={960} y={1000} textAnchor="middle" style={label(window01(v, T.harder, T.oneg, 0.3), { fontSize: 28 })}>HEAVIER: PULLED HARDER, BUT HARDER TO MOVE</text>
        <text x={960} y={1000} textAnchor="middle" style={label(ramp(v, T.oneg - 0.2, 0.4), { fontSize: 30 })}>1 g = 9.8 m/s MORE, EVERY SECOND</text>
        <text x={1420} y={330 + fall + 10} style={mono(ramp(v, T.falls, 0.4), 34)}>{(G * tau).toFixed(1)} m/s</text>
        <text x={1420} y={330 + fall + 50} style={mono(ramp(v, T.falls, 0.4) * 0.6, 26)}>t = {tau.toFixed(1)} s</text>
      </g>

      {/* ---------------- D/E: the drag acceleration next to gravity, to scale */}
      <g opacity={oD}>
        <Formula toks={FD} cx={900} y={330} size={46} reveal={ramp(v, T.formula - 0.1, 1.2)} active={v >= T.divides - 0.1 && v < T.g48 ? "m" : null} />
        <text x={FD_END + 20} y={330} style={mono(ramp(v, T.g48 - 0.2, 0.4), 46, { fill: C.drag, fontWeight: 700 })} filter="url(#glow)">= 48 g</text>
        <text x={960} y={392} textAnchor="middle" style={label(window01(v, T.divides - 0.1, T.muzzle, 0.3) * 0.8, { fontSize: 22 })}>ALREADY DIVIDED BY THE MASS: AN ACCELERATION</text>

        <ArrowA x={DX - 16} y={DY} a={Math.PI} len={dragG * PXG} p={ramp(v, T.muzzle, 0.9, ease)} color={C.drag} w={6} head={24} glow />
        <text x={DX - 16 - dragG * PXG / 2} y={DY - 30} textAnchor="middle" style={mono(ramp(v, T.g48 - 0.2, 0.4), 40, { fill: C.drag, fontWeight: 600 })}>
          drag {Math.round(dragG)} g
        </text>
        <ArrowA x={DX} y={DY + 6} a={Math.PI / 2} len={PXG} p={ramp(v, T.next + 0.4, 0.4)} w={3} head={8} />
        <Dot x={DX} y={DY} o={1} />
        <Magnifier cx={DX} cy={DY + 230} r={150} p={mag} id="g">
          <g transform={`translate(${DX},${DY + 230}) scale(${Z}) translate(${-DX},${-DY - 10})`}>
            <line x1={DX - 3} y1={DY} x2={DX - 400} y2={DY} stroke={C.drag} strokeWidth={6 / Z} />
            <ArrowA x={DX} y={DY + 6 / Z} a={Math.PI / 2} len={PXG} w={3 / Z} head={8 / Z} />
            <circle cx={DX} cy={DY} r={10 / Z} fill={C.line} />
          </g>
          <text x={DX + 34} y={DY + 270} style={mono(1, 40, { fontWeight: 600 })}>1 g</text>
        </Magnifier>
        <line x1={DX} y1={DY + 30} x2={DX} y2={DY + 80} stroke={C.line} strokeWidth={1.5} strokeDasharray="4 5" opacity={mag * 0.6} />
        <text x={DX - 200} y={DY + 240} textAnchor="end" style={label(ramp(v, T.next, 0.5), { fontSize: 24 })}>GRAVITY, TO SCALE</text>
        {/* time runs: the drag shrinks, gravity stays */}
        <g opacity={ramp(v, T.never + 0.2, 0.5)}>
          <text x={420} y={880} style={mono(0.9, 34)}>t = {Math.min(run, T_FLIGHT).toFixed(2)} s</text>
          <text x={420} y={930} style={mono(1, 34, { fill: C.drag })} xmlSpace="preserve">drag    {dragG.toFixed(1)} g</text>
          <text x={420} y={980} style={mono(1, 34)}>gravity 1.0 g</text>
          {/* gravity's share: the drag bar keeps its length, so the gravity bar grows as the drag shrinks */}
          <text x={420} y={700} style={label(0.85, { fontSize: 22 })}>GRAVITY, COMPARED WITH THE DRAG</text>
          <rect x={420} y={722} width={SHARE_W} height={14} rx={3} fill={C.drag} />
          <rect x={420} y={752} width={SHARE_W / dragG} height={14} rx={3} fill={C.line} filter="url(#glow)" />
          <text x={420 + SHARE_W + 24} y={770} style={mono(1, 36, { fontWeight: 600 })}>1 : {Math.round(dragG)}</text>
        </g>
        <text x={960} y={470} textAnchor="middle" style={label(window01(v, T.never - 0.1, T.smaller, 0.3), { fontSize: 28 })}>GRAVITY NEVER CHANGES</text>
        <text x={960} y={470} textAnchor="middle" style={label(ramp(v, T.smaller - 0.1, 0.4), { fontSize: 28, fill: C.drag })}>THE DRAG SHRINKS AS THE BULLET SLOWS</text>
      </g>
    </Scene>
  );
};
