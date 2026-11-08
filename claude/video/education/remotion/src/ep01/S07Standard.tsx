// Scene 7 — standard bullets and the BC: G1 and G7 with their curves; Cd / SD folds into Cd_G7(M) / BC, the
// one number you type; our bullet's own curve lies 4 % above G7; twice the BC, half the drag.
import React from "react";
import { D } from "../data";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Bracket, Bullet, Formula, G1S, G7S, P, Pen, SMK, Tok, label, layoutFormula, mono, pathD } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 7;
const T = {
  std: cue(N, "standard"), g1: cue(N, "G1"), g7: cue(N, "G7"), modern: cue(N, "modern"), never: cue(N, "But you never"),
  one: cue(N, "one number"), bc: cue(N, "ballistic"), packs: cue(N, "packs"), shape: cue(N, "shape"), takes: cue(N, "takes"),
  divides: cue(N, "divides"), bigger: cue(N, "bigger"), twice: cue(N, "Twice"), half: cue(N, "half"), simple: cue(N, "simple"),
};
const SD = 175 / 7000 / (0.308 * 0.308);
const K = { x0: 230, x1: 1720, y0: 950, y1: 560, mMin: 0.5, mMax: 2.7, cdMax: 0.7 };
const kx = (m: number) => lerp(K.x0, K.x1, (m - K.mMin) / (K.mMax - K.mMin));
const ky = (cd: number) => lerp(K.y0, K.y1, cd / K.cdMax);
const curve = (ys: number[], scale = 1): P[] => D.curves.mach.map((m, i) => [kx(m), ky(ys[i] * scale)] as P);

const FY = 200, FS = 56;
const FA: Tok[] = [{ s: "drag", color: C.drag }, { s: " = ½ ρ · v² · " }, { s: "Cd", key: "cd" }, { s: " / " }, { s: "SD", key: "sd" }];
const FB: Tok[] = [{ s: "drag", color: C.drag }, { s: " = ½ ρ · v² · " }, { s: "Cd", key: "g" }, { s: "G7", key: "g", sub: true }, { s: "(M)", key: "g" }, { s: " / " }, { s: "BC", key: "bc", color: C.drag }];
const LA = layoutFormula(FA, 960, FS);

export const S07Standard: React.FC = () => {
  const v = useVoiceTime();
  const phase2 = ramp(v, T.never - 0.3, 0.6);
  const fold = ramp(v, T.packs + 0.6, 0.8, ease);
  const bcK = ramp(v, T.twice, 0.9, ease);                       // BC 0.253 -> 0.506
  const bc = lerp(0.253, 0.506, bcK);
  const own = ramp(v, T.shape - 0.2, 0.9);
  const g1o = (1 - ramp(v, T.packs, 0.6)) * ramp(v, T.g1, 0.8);
  const cdX0 = LA.find((t) => t.key === "cd")!.x, sdT = LA.find((t) => t.key === "sd")!;
  const divideHot = window01(v, T.divides - 0.3, T.twice, 0.3);

  return (
    <Scene n={N}>
      {/* ---- part 1: the standard bullets, side by side */}
      <g opacity={1 - phase2}>
        <text x={960} y={110} textAnchor="middle" style={label(ramp(v, 0, 0.6), { fontSize: 30 })}>STANDARD BULLETS, MEASURED LONG AGO</text>
        <Bullet s={G1S} x0={330} y={310} cal={70} p={ramp(v, T.g1 - 0.6, 1.2)} cannelure={false} />
        <text x={330 + G1S.len * 35} y={410} textAnchor="middle" style={mono(ramp(v, T.g1, 0.4), 40)}>G1</text>
        <text x={330 + G1S.len * 35} y={448} textAnchor="middle" style={label(ramp(v, T.g1 + 0.6, 0.4) * 0.7, { fontSize: 20 })}>OLD · FLAT BASE</text>
        <Bullet s={G7S} x0={1060} y={310} cal={70} p={ramp(v, T.g7 - 0.6, 1.2)} cannelure={false} />
        <text x={1060 + G7S.len * 35} y={410} textAnchor="middle" style={mono(ramp(v, T.g7, 0.4), 40)}>G7</text>
        <text x={1060 + G7S.len * 35} y={448} textAnchor="middle" style={label(ramp(v, T.g7 + 0.6, 0.4) * 0.7, { fontSize: 20 })}>LONG · BOAT TAIL</text>
        <g opacity={ramp(v, T.modern - 0.2, 0.6)}>
          <Bullet x0={1060} y={200} cal={70} color={C.dim} w={2} cannelure={false} />
          <text x={1060 + SMK.len * 70 + 30} y={210} style={label(0.8, { fontSize: 20 })}>A MATCH BULLET</text>
        </g>
      </g>

      {/* ---- part 2: the formula folds into the BC */}
      <g opacity={phase2}>
        <Formula toks={FA} cx={960} y={FY} size={FS} reveal={1} active={v > T.packs - 0.2 && fold < 0.5 ? "sd" : v > T.never ? "cd" : null} opacity={1 - fold} />
        <g opacity={window01(v, T.packs - 0.3, T.packs + 1.0, 0.3)}>
          <Bracket x0={cdX0 - 6} x1={sdT.x + sdT.w + 6} y={FY + 22} p={ramp(v, T.packs - 0.3, 0.5)} />
        </g>
        <Formula toks={FB} cx={960} y={FY} size={FS} reveal={1} active={divideHot > 0.5 || bcK > 0 ? "bc" : null} opacity={fold} />
        <text x={960} y={FY + 70} textAnchor="middle" style={label(ramp(v, T.never + 0.4, 0.5) * (1 - fold), { fontSize: 22 })}>
          YOU NEVER TYPE IN Cd · AREA · MASS
        </text>
        <text x={960} y={FY + 70} textAnchor="middle" style={label(fold * 0.75, { fontSize: 22 })}>
          THE BC PACKS THE SECTIONAL DENSITY AND THE SHAPE
        </text>
        {/* the one field */}
        <g opacity={ramp(v, T.one - 0.2, 0.5)} transform="translate(1440, 330)">
          <rect x={0} y={0} width={300} height={74} rx={10} fill="none" stroke={C.line} strokeWidth={2} />
          <text x={20} y={48} style={label(0.7, { fontSize: 20 })}>BC</text>
          <text x={150} y={50} textAnchor="middle" style={mono(1, 34, { fill: C.drag, fontWeight: 600 })}>{bc.toFixed(3)}</text>
          <text x={280} y={48} textAnchor="end" style={mono(0.7, 22)}>G7</text>
        </g>
        {/* a bullet whose drag arrow halves */}
        <g opacity={ramp(v, T.bigger - 0.3, 0.5)}>
          <Bullet x0={300} y={400} cal={30} />
          <g filter="url(#glow)"><Arrow x1={286} y1={400} x2={286 - 220 * (0.253 / bc)} y2={400} color={C.drag} w={4} head={16} /></g>
          <text x={286} y={446} textAnchor="end" style={label(ramp(v, T.half - 0.2, 0.4), { fontSize: 22, fill: C.drag })}>HALF THE DRAG</text>
        </g>
      </g>

      {/* ---- the chart, under both parts */}
      <g opacity={ramp(v, T.g1 - 0.4, 0.8)}>
        <Pen pts={[[K.x0, K.y1 - 20], [K.x0, K.y0], [K.x1 + 20, K.y0]]} p={1} color={C.dim} w={1.8} tip={false} />
        {[0.5, 1, 1.5, 2, 2.5].map((m) => <text key={m} x={kx(m)} y={K.y0 + 32} textAnchor="middle" style={mono(0.6, 18)}>{m.toFixed(1)}</text>)}
        {[0.2, 0.4, 0.6].map((c) => <text key={c} x={K.x0 - 14} y={ky(c) + 6} textAnchor="end" style={mono(0.6, 18)}>{c.toFixed(1)}</text>)}
        <text x={K.x0} y={K.y1 - 34} style={label(0.7, { fontSize: 18 })}>Cd · MACH</text>
      </g>
      <g opacity={g1o}>
        <Pen pts={curve(D.curves.g1)} p={ramp(v, T.g1 + 0.2, 1.4)} color={C.dim} w={2.5} tip={false} />
        <text x={kx(1.45)} y={ky(0.67) - 12} style={mono(0.8, 24)}>G1</text>
      </g>
      <g opacity={ramp(v, T.g7, 0.8)}>
        <Pen pts={curve(D.curves.g7)} p={ramp(v, T.g7 + 0.2, 1.4)} color={C.line} w={2.5} tip={false} />
        <text x={kx(1.12)} y={ky(0.41) - 14} style={mono(0.9, 24)}>G7</text>
      </g>
      <g opacity={own}>
        <path d={pathD(curve(D.curves.g7, (SD / bc)))} stroke={C.drag} strokeWidth={3.5} fill="none" filter={divideHot > 0.5 ? "url(#glow)" : undefined} />
        <text x={kx(1.6)} y={ky(0.36 * (0.253 / bc) * 1.04) - 16} style={label(1, { fontSize: 20, fill: C.drag })}>
          {bcK > 0.5 ? "TWICE THE BC → HALF" : "OUR BULLET ≈ G7 × 1.04"}
        </text>
      </g>
      <text x={960} y={1030} textAnchor="middle" style={label(divideHot * 0.85, { fontSize: 22 })}>
        THE PROGRAM TAKES THE STANDARD CURVE AND DIVIDES IT BY THE BC
      </text>
    </Scene>
  );
};
