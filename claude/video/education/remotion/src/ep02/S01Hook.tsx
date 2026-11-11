// Scene 1 — hook: the program forgets the bullet's shape; what is left is a dot with a mass. The shape survives
// only in episode 1's drag curve.
import React from "react";
import { D } from "../data";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Bullet, P, Pen, SMK, label, mono, outline, pathD } from "../lib/draw";
import { Dot, Scene, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 1;
const T = {
  forgets: cue(N, "forgets"), nose: cue(N, "nose"), length: cue(N, "length"), spin: cue(N, "spin"), left: cue(N, "What's left"),
  mass: cue(N, "mass"), shape: cue(N, "The shape"), curve: cue(N, "drag curve"), today: cue(N, "Today"),
};

const CAL = 125, AX = 520;
const X0 = 960 - (SMK.len * CAL) / 2, X1 = X0 + SMK.len * CAL;
const CG: P = [X0 + SMK.len * CAL * 0.42, AX];            // the dot sits at the centre of mass, a bit behind the middle
const OUT = outline(SMK, X0, AX, CAL, 120);

// the little Cd chart the shape retreats into
const CX0 = 1240, CX1 = 1720, CY0 = 900, CY1 = 640;
const cx = (m: number) => lerp(CX0, CX1, (m - 0.5) / 2.2), cy = (cd: number) => lerp(CY0, CY1, cd / 0.45);
const CURVE: P[] = D.cdCurve.map((p) => [cx(p.mach), cy(p.cd)] as P);

export const S01Hook: React.FC = () => {
  const v = useVoiceTime();
  const draw = ramp(v, -0.6, 1.6);
  const pull = ramp(v, T.left - 0.3, 1.3, ease);                   // the outline pulls together into the dot
  const feat = (t0: number) => window01(v, t0 - 0.15, T.left - 0.1, 0.3);
  const spinA = (v - T.spin) * 5;
  // the outline, contracting toward the centre of mass
  const pts = OUT.map(([x, y]) => [lerp(x, CG[0], pull), lerp(y, CG[1], pull)] as P);
  const ghost = ramp(v, T.shape - 0.2, 1.4, ease);                 // a ghost of the outline flies into the chart
  const gS = lerp(1, 0.22, ghost), gx = lerp(CG[0], (CX0 + CX1) / 2, ghost), gy = lerp(CG[1], CY1 - 60, ghost);
  const titleK = ramp(v, T.today, 1.2);
  const sceneO = 1 - 0.75 * ramp(v, T.today - 0.2, 0.6);

  return (
    <Scene n={N}>
      <g opacity={sceneO}>
        {/* the bullet, drawn by the line, then pulled into a point */}
        {pull < 1 && <Pen pts={pts} p={draw} w={3} opacity={1 - ramp(v, T.left + 0.9, 0.3)} />}
        {pull < 0.02 && draw >= 1 && <line x1={X0 + 1.55 * CAL} y1={AX - 0.5 * CAL} x2={X0 + 1.55 * CAL} y2={AX + 0.5 * CAL} stroke={C.line} strokeWidth={1.5} opacity={0.6} />}

        {/* the nose, the length, the spin: lit one by one, then forgotten */}
        <g opacity={feat(T.nose)}>
          <path d={pathD(OUT.filter(([x]) => x >= X1 - SMK.noseLen * CAL))} stroke={C.line} strokeWidth={7} fill="none" filter="url(#glow)" />
          <text x={X1 - 40} y={AX - 0.5 * CAL - 40} textAnchor="middle" style={label(1, { fontSize: 26 })}>THE NOSE</text>
        </g>
        <g opacity={feat(T.length)}>
          <Arrow x1={(X0 + X1) / 2} y1={AX + 0.5 * CAL + 50} x2={X0} y2={AX + 0.5 * CAL + 50} head={14} w={2.5} />
          <Arrow x1={(X0 + X1) / 2} y1={AX + 0.5 * CAL + 50} x2={X1} y2={AX + 0.5 * CAL + 50} head={14} w={2.5} />
          <text x={(X0 + X1) / 2} y={AX + 0.5 * CAL + 100} textAnchor="middle" style={label(1, { fontSize: 26 })}>THE LENGTH</text>
        </g>
        <g opacity={feat(T.spin)}>
          {/* a ring around the body, turning: the spin */}
          <ellipse cx={X0 + 2.2 * CAL} cy={AX} rx={22} ry={0.5 * CAL + 26} fill="none" stroke={C.line} strokeWidth={2.5} strokeDasharray="10 9" strokeDashoffset={-spinA * 30} />
          <Arrow x1={X0 + 2.2 * CAL + 20} y1={AX - 0.5 * CAL - 20} x2={X0 + 2.2 * CAL + 40} y2={AX - 0.5 * CAL - 4} head={12} w={2.5} />
          <text x={X0 + 2.2 * CAL} y={AX - 0.5 * CAL - 50} textAnchor="middle" style={label(1, { fontSize: 26 })}>THE SPIN</text>
        </g>

        {/* what is left: a dot with a mass */}
        <Dot x={CG[0]} y={CG[1]} r={lerp(4, 14, ramp(v, T.left + 0.6, 0.6))} o={ramp(v, T.left + 0.5, 0.5)} />
        <text x={CG[0]} y={CG[1] - 50} textAnchor="middle" style={mono(ramp(v, T.mass - 0.1, 0.5), 40)}>m = 11.3 g</text>
        <text x={CG[0]} y={CG[1] + 80} textAnchor="middle" style={label(window01(v, T.left + 0.5, T.shape, 0.4) * 0.75, { fontSize: 24 })}>A DOT WITH A MASS</text>

        {/* the shape lives on only in the drag curve */}
        {ghost > 0 && ghost < 1 && (
          <g transform={`translate(${gx},${gy}) scale(${gS}) translate(${-CG[0]},${-CG[1]})`} opacity={0.6 * (1 - ramp(v, T.shape + 1.1, 0.3))}>
            <path d={pathD(OUT)} stroke={C.line} strokeWidth={3 / gS} fill="none" />
          </g>
        )}
        <g opacity={ramp(v, T.curve - 0.6, 0.5)}>
          <Pen pts={[[CX0, CY1 - 20], [CX0, CY0], [CX1 + 10, CY0]]} p={1} color={C.dim} w={1.6} tip={false} />
          <text x={CX1 + 10} y={CY0 + 30} textAnchor="end" style={label(0.6, { fontSize: 16 })}>MACH</text>
          <text x={CX0 - 12} y={CY1 - 4} textAnchor="end" style={label(0.6, { fontSize: 16 })}>Cd</text>
        </g>
        <g filter="url(#glow)"><Pen pts={CURVE} p={ramp(v, T.curve - 0.4, 1.4)} color={C.drag} w={3} /></g>
        <text x={(CX0 + CX1) / 2} y={CY0 + 66} textAnchor="middle" style={label(ramp(v, T.curve, 0.5) * 0.8, { fontSize: 20, fill: C.drag })}>
          THE SHAPE LIVES HERE · EPISODE 1
        </text>
      </g>

      {/* the title */}
      <g opacity={ramp(v, T.today, 0.4)}>
        <clipPath id="t2Clip"><rect x={0} y={150} width={lerp(300, 1700, titleK)} height={200} /></clipPath>
        <text x={960} y={290} textAnchor="middle" clipPath="url(#t2Clip)" style={label(1, { fontSize: 100, letterSpacing: "0.32em", fontWeight: 200 })}>HOW A DOT FLIES</text>
        <text x={960} y={350} textAnchor="middle" style={label(ramp(v, T.today + 0.8, 0.6) * 0.6, { fontSize: 22 })}>HOW BALLISTIC CALCULATOR 2 WORKS · 2</text>
      </g>
    </Scene>
  );
};
