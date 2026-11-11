// Scene 7 — east and west (Eötvös). The Earth's turn under a shooter has two parts: at the pole a carousel around
// the shooter (that's Coriolis, scene 6), at the equator a wheel standing on its edge, turning west to east, seen
// facing north. At 45° the turn arrow splits into the two (sin φ up, cos φ north). The wheel part always turns the
// ground in the east down — the sun comes up over it. Shot east: the ground ahead turns down, away; the bullet falls
// slower (g × a little less). West: the ground ahead turns up; it falls faster. North: along the wheel's axis,
// nothing. The program scales gravity's drop: the formula, cos φ as the wheel part on the cross-section, sin(Az) on a
// compass rose, V₀; for our shot g × 0.992 / g × 1.008, +12 cm / −12 cm at 1000 m.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, mono } from "../lib/draw";
import { ArrowA, Item, k_, Legend, MathLine, Scene, sub, Sub, t_, Title, TurnArrow, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 7;
const T = {
  remember: cue(N, "Remember"), pole: cue(N, "At the pole"), equator: cue(N, "At the equator"), west: cue(N, "west to"), eastW: cue(N, "west to east", 1, true),
  here: cue(N, "Here"), each: cue(N, "part of each"), wheelPart: cue(N, "wheel part"), eastDown: cue(N, "east down"), sun: cue(N, "sun"),
  shootE: cue(N, "Shoot to the east"), fliesE: cue(N, "bullet flies"), aheadE: cue(N, "ground ahead"), slower: cue(N, "slower"), weaker: cue(N, "weaker"),
  shootW: cue(N, "Shoot to the west"), aheadW: cue(N, "ground ahead", 2), faster: cue(N, "faster"), stronger: cue(N, "stronger"),
  north: cue(N, "To the north"), axis: cue(N, "axis"), nothing: cue(N, "nothing changes"), scales: cue(N, "scales"),
  omega: cue(N, "Omega again"), cosine: cue(N, "cosine"), equator2: cue(N, "At the equator", 2), poleNear: cue(N, "Near the pole"),
  shot: cue(N, "direction of the shot"), plus: cue(N, "plus one"), minus: cue(N, "minus one"), zero: cue(N, "zero"), muzzle: cue(N, "muzzle velocity"),
  ours: cue(N, "For our shot"), percent: cue(N, "percent"), n12: cue(N, "12"), shootingW: cue(N, "shooting west"),
};

const FX = 960, FY = 200, FS = 54;
const EOT: Item[] = [
  { num: [t_("g"), sub("eff")], den: [t_("g")] },
  { toks: [t_(" = 1 − ")] },
  { num: [t_("2 · "), k_("Ω", "om"), t_(" · "), k_("cos φ", "cos"), t_(" · "), k_("sin(Az)", "az"), t_(" · "), k_("V₀", "v")], den: [t_("g")] },
];
const rad = (d: number) => (d * Math.PI) / 180;

/** A little standing figure, feet at (x, y). */
const Figure: React.FC<{ x: number; y: number; s?: number }> = ({ x, y, s = 1 }) => (
  <g>
    <line x1={x} y1={y} x2={x} y2={y - 30 * s} stroke={C.line} strokeWidth={3} />
    <circle cx={x} cy={y - 40 * s} r={9 * s} fill="none" stroke={C.line} strokeWidth={3} />
  </g>
);

/** An arrow along a rim of centre c, radius r, from α0 to α1 (degrees from the top, clockwise). */
const RimArrow: React.FC<{ c: P; r: number; a0: number; a1: number; p: number; color?: string }> = ({ c, r, a0, a1, p, color = C.line }) => {
  if (p <= 0.01) return null;
  const at = (a: number): P => [c[0] + r * Math.sin(rad(a)), c[1] - r * Math.cos(rad(a))];
  const pts: P[] = Array.from({ length: 21 }, (_, i) => at(lerp(a0, lerp(a0, a1, p), i / 20)));
  const e = pts[20], d = pts[19], ang = Math.atan2(e[1] - d[1], e[0] - d[0]), h = 22;
  return (
    <g filter="url(#glow)">
      <Pen pts={pts} p={1} w={6} tip={false} color={color} />
      <path d={`M${e[0] - h * Math.cos(ang - 0.5)},${e[1] - h * Math.sin(ang - 0.5)} L${e[0]},${e[1]} L${e[0] - h * Math.cos(ang + 0.5)},${e[1] - h * Math.sin(ang + 0.5)}`}
        stroke={color} strokeWidth={6} fill="none" />
    </g>
  );
};

/** The Earth cut through its axis; our point at latitude `lat`; the turn arrow there split into the carousel part
 *  (up, sin φ) and the wheel part (north, cos φ). */
const XSection: React.FC<{ c: P; r: number; lat: number; o: number; parts: number; carO?: number; wheelHot?: number }> = ({ c, r, lat, o, parts, carO = 1, wheelHot = 0 }) => {
  const la = rad(lat), p: P = [c[0] + r * Math.cos(la), c[1] - r * Math.sin(la)], L = 190;
  const up: P = [Math.cos(la), -Math.sin(la)], no: P = [-Math.sin(la), -Math.cos(la)];
  const cs = Math.sin(la) * L, ws = Math.cos(la) * L;
  const ct: P = [p[0] + up[0] * cs, p[1] + up[1] * cs], wt: P = [p[0] + no[0] * ws, p[1] + no[1] * ws];
  return (
    <g opacity={o}>
      <circle cx={c[0]} cy={c[1]} r={r} fill="none" stroke={C.line} strokeWidth={3} />
      <line x1={c[0] - r - 30} y1={c[1]} x2={c[0] + r + 30} y2={c[1]} stroke={C.line} strokeWidth={1.2} strokeDasharray="4 8" opacity={0.4} />
      <text x={c[0] - r - 40} y={c[1] + 7} textAnchor="end" style={label(0.5, { fontSize: 16 })}>EQUATOR</text>
      <line x1={c[0]} y1={c[1] - r - 140} x2={c[0]} y2={c[1] + r + 40} stroke={C.line} strokeWidth={2} strokeDasharray="8 8" opacity={0.6} />
      <ArrowA x={c[0]} y={c[1] - r - 20} a={-Math.PI / 2} len={130} w={4} head={16} />
      <TurnArrow x={c[0]} y={c[1] - r - 90} rx={60} k={1} ph={0.1} w={2.5} />
      <text x={c[0] + 80} y={c[1] - r - 100} style={label(0.85, { fontSize: 20 })}>THE EARTH'S TURN</text>
      <circle cx={p[0]} cy={p[1]} r={10} fill={C.line} filter="url(#glow)" />
      <text x={p[0] + 26} y={p[1] + 34} style={mono(0.9, 26, { fontWeight: 600 })}>{Math.round(lat)}°</text>
      {/* the same turn arrow, moved to our point, then its two parts */}
      <g opacity={parts}>
        <line x1={p[0]} y1={p[1]} x2={p[0]} y2={p[1] - L} stroke={C.line} strokeWidth={2} strokeDasharray="6 6" opacity={0.5} />
        <line x1={ct[0]} y1={ct[1]} x2={p[0]} y2={p[1] - L} stroke={C.line} strokeWidth={1} strokeDasharray="3 6" opacity={0.4} />
        <line x1={wt[0]} y1={wt[1]} x2={p[0]} y2={p[1] - L} stroke={C.line} strokeWidth={1} strokeDasharray="3 6" opacity={0.4} />
        <g opacity={carO}>
          {cs > 4 && <ArrowA x={p[0]} y={p[1]} a={Math.atan2(up[1], up[0])} len={cs} w={4} head={16} />}
          <text x={ct[0] + 20} y={ct[1] - 8} style={label(0.95, { fontSize: 20 })}>UP · CAROUSEL · sin φ</text>
        </g>
        {ws > 4 && <ArrowA x={p[0]} y={p[1]} a={Math.atan2(no[1], no[0])} len={ws} w={lerp(4, 7, wheelHot)} head={lerp(16, 22, wheelHot)} color={C.drag} glow={wheelHot > 0.5} />}
        <text x={lat < 25 ? wt[0] + 20 : Math.min(wt[0], c[0]) - 20} y={wt[1] - 8} textAnchor={lat < 25 ? "start" : "end"} style={label(0.95, { fontSize: 20, fill: C.drag })}>NORTH · WHEEL · cos φ</text>
      </g>
    </g>
  );
};

export const S07EastWest: React.FC = () => {
  const v = useVoiceTime();

  // ---- A: carousel at the pole, wheel at the equator
  const aO = ramp(v, T.remember - 0.3, 0.5) * (1 - ramp(v, T.here - 0.3, 0.5));
  const wheelAO = ramp(v, T.equator - 0.3, 0.5);
  // ---- B: at 45° a part of each
  const bO = window01(v, T.here - 0.1, T.wheelPart + 0.6, 0.4);
  // ---- C + D: the big wheel; then zoomed in on the top for the shots
  const wO = ramp(v, T.wheelPart + 0.4, 0.5) * (1 - ramp(v, T.scales - 0.4, 0.5));
  const zoom = ramp(v, T.shootE - 0.5, 1.2, ease);
  const rotC = 24 * ramp(v, T.eastDown - 0.6, 3.2) * (1 - zoom);
  const cx = 960, cy = lerp(760, 1380, zoom), r = lerp(260, 800, zoom);
  const top: P = [cx, cy - r];
  const sunO = ramp(v, T.sun - 0.6, 0.5) * (1 - zoom);
  const caseE = window01(v, T.shootE - 0.2, T.shootW - 0.1, 0.3);
  const caseW = window01(v, T.shootW - 0.2, T.north - 0.1, 0.3);
  const caseN = ramp(v, T.north - 0.2, 0.3);
  const M: P = [top[0], top[1] - 22];                                   // the muzzle
  const path = (dir: number, drop: number, p: number): P[] =>
    Array.from({ length: 31 }, (_, i) => { const k = (i / 30) * p; return [M[0] + dir * 560 * k, M[1] + drop * k * k]; });
  // ---- E: the formula, cos φ as the wheel part, sin(Az), V₀; F: our shot
  const fO = ramp(v, T.scales - 0.3, 0.5);
  const active = v >= T.ours - 0.3 ? null : v >= T.muzzle - 0.1 ? "v" : v >= T.shot - 0.1 ? "az" : v >= T.cosine - 0.1 ? "cos" : v >= T.omega - 0.1 ? "om" : null;
  const xsO = window01(v, T.cosine - 0.3, T.shot - 0.1, 0.4);
  const lat = v < T.equator2 - 0.3 ? 45 : v < T.poleNear - 0.3 ? lerp(45, 0, ramp(v, T.equator2 - 0.3, 0.8, ease)) : lerp(0, 80, ramp(v, T.poleNear - 0.3, 1.0, ease));
  const roseO = window01(v, T.shot - 0.3, T.ours - 0.1, 0.4);
  const resO = ramp(v, T.ours - 0.2, 0.5);

  return (
    <Scene n={N}>
      <Title text="EAST AND WEST" o={ramp(v, -0.5, 0.5) * (1 - ramp(v, T.remember - 0.5, 0.4))} />

      {/* ---------------- A: two ways the ground can turn */}
      <g opacity={aO}>
        {(() => {
          const c: P = [560, 640], rx = 230, ry = 74, ph = v * 0.12;
          return (
            <g>
              <ellipse cx={c[0]} cy={c[1]} rx={rx} ry={ry} fill="none" stroke={C.line} strokeWidth={3} />
              {Array.from({ length: 8 }, (_, i) => {
                const a = ph * Math.PI * 2 + (i * Math.PI) / 4;
                return <line key={i} x1={c[0]} y1={c[1]} x2={c[0] + rx * Math.cos(a)} y2={c[1] + ry * Math.sin(a)} stroke={C.line} strokeWidth={1.2} opacity={0.3} />;
              })}
              <Figure x={c[0]} y={c[1]} />
              <TurnArrow x={c[0]} y={c[1] - 110} rx={150} k={1} ph={ph} />
              <text x={c[0]} y={c[1] + 180} textAnchor="middle" style={label(ramp(v, T.pole - 0.2, 0.4), { fontSize: 26 })}>AT THE POLE: A CAROUSEL</text>
              <text x={c[0]} y={c[1] + 216} textAnchor="middle" style={label(ramp(v, T.pole - 0.2, 0.4) * 0.6, { fontSize: 18 })}>SEEN FROM THE SIDE · IT TURNS AROUND YOU</text>
            </g>
          );
        })()}
        <g opacity={wheelAO}>
          {(() => {
            const c: P = [1380, 650], rr = 200, ph = v * 0.25;
            return (
              <g>
                <circle cx={c[0]} cy={c[1]} r={rr} fill="none" stroke={C.line} strokeWidth={3} />
                {Array.from({ length: 8 }, (_, i) => {
                  const a = ph + (i * Math.PI) / 4;
                  return <line key={i} x1={c[0]} y1={c[1]} x2={c[0] + rr * Math.cos(a)} y2={c[1] + rr * Math.sin(a)} stroke={C.line} strokeWidth={1.2} opacity={0.3} />;
                })}
                <Figure x={c[0]} y={c[1] - rr} />
                <RimArrow c={c} r={rr + 34} a0={25} a1={70} p={ramp(v, T.equator + 0.4, 0.8)} />
                <text x={c[0] - rr - 30} y={c[1] - rr + 10} textAnchor="end" style={label(ramp(v, T.west - 0.2, 0.4) * 0.85, { fontSize: 22 })}>WEST</text>
                <text x={c[0] + rr + 30} y={c[1] - rr + 10} style={label(ramp(v, T.eastW - 0.3, 0.4) * 0.85, { fontSize: 22 })}>EAST</text>
                <text x={c[0]} y={c[1] + rr + 70} textAnchor="middle" style={label(1, { fontSize: 26 })}>AT THE EQUATOR: A WHEEL ON ITS EDGE</text>
                <text x={c[0]} y={c[1] + rr + 106} textAnchor="middle" style={label(0.6, { fontSize: 18 })}>SEEN FACING NORTH · TURNING WEST TO EAST</text>
              </g>
            );
          })()}
        </g>
      </g>

      {/* ---------------- B: at 45°, a part of each */}
      <g opacity={bO}>
        <text x={960} y={110} textAnchor="middle" style={label(0.85, { fontSize: 28 })}>AT 45°: A PART OF EACH</text>
        <XSection c={[860, 680]} r={270} lat={45} o={1} parts={ramp(v, T.here + 0.5, 0.8)} wheelHot={ramp(v, T.wheelPart - 0.2, 0.4)} />
      </g>

      {/* ---------------- C + D: the wheel; the ground in the east goes down; then the shots */}
      <g opacity={wO}>
        <g transform={`rotate(${rotC},${cx},${cy})`}>
          <circle cx={cx} cy={cy} r={r} fill="none" stroke={C.line} strokeWidth={3} />
          <g opacity={1 - zoom}>
            {Array.from({ length: 8 }, (_, i) => {
              const a = (i * Math.PI) / 4 + 0.2;
              return <line key={i} x1={cx} y1={cy} x2={cx + r * Math.cos(a)} y2={cy + r * Math.sin(a)} stroke={C.line} strokeWidth={1.2} opacity={0.3} />;
            })}
            <line x1={top[0] - 380} y1={top[1]} x2={top[0] + 380} y2={top[1]} stroke={C.line} strokeWidth={2} strokeDasharray="8 8" opacity={0.6} />
            <text x={top[0] - 370} y={top[1] - 14} style={label(0.6, { fontSize: 18 })}>THE HORIZON</text>
          </g>
          {/* rim ticks, turning with the ground */}
          <g opacity={zoom}>
            {Array.from({ length: 24 }, (_, i) => {
              const a = rad(-60 + i * 5 + ((v * 1.2) % 5));
              return <line key={i} x1={cx + r * Math.sin(a)} y1={cy - r * Math.cos(a)} x2={cx + (r - 18) * Math.sin(a)} y2={cy - (r - 18) * Math.cos(a)} stroke={C.line} strokeWidth={2} opacity={0.5} />;
            })}
          </g>
          <Figure x={top[0]} y={top[1]} />
        </g>
        <g opacity={1 - zoom}>
          <RimArrow c={[cx, cy]} r={r + 34} a0={35 + rotC} a1={75 + rotC} p={ramp(v, T.eastDown - 0.8, 0.8)} />
          <text x={960} y={1000} textAnchor="middle" style={label(ramp(v, T.eastDown - 0.6, 0.4), { fontSize: 26 })}>THE WHEEL PART TURNS THE GROUND IN THE EAST DOWN</text>
          <text x={200} y={520} style={label(0.6, { fontSize: 22 })}>WEST</text>
          <text x={1720} y={520} textAnchor="end" style={label(0.6, { fontSize: 22 })}>EAST</text>
        </g>
        <g opacity={sunO}>
          <circle cx={1450} cy={565} r={28} fill="none" stroke={C.line} strokeWidth={3} filter="url(#glow)" />
          {Array.from({ length: 8 }, (_, i) => {
            const a = (i * Math.PI) / 4;
            return <line key={i} x1={1450 + 40 * Math.cos(a)} y1={565 + 40 * Math.sin(a)} x2={1450 + 54 * Math.cos(a)} y2={565 + 54 * Math.sin(a)} stroke={C.line} strokeWidth={2.5} />;
          })}
          <text x={1450} y={460} textAnchor="middle" style={label(ramp(v, T.sun - 0.2, 0.4) * 0.9, { fontSize: 22 })}>THE SUN COMES UP IN THE EAST</text>
        </g>

        {/* the shots, the top of the wheel close up */}
        <g opacity={zoom}>
          <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>FACING NORTH · EAST TO THE RIGHT · DRAWN MUCH BIGGER THAN REAL</text>
          <text x={180} y={top[1] - 30} style={label(0.6, { fontSize: 22 })}>WEST</text>
          <text x={1740} y={top[1] - 30} textAnchor="end" style={label(0.6, { fontSize: 22 })}>EAST</text>
        </g>
        {([[1, caseE, T.fliesE, T.aheadE, 52, "THE GROUND AHEAD TURNS DOWN", T.slower, "FALLS SLOWER", T.weaker, "g × A LITTLE LESS"],
           [-1, caseW, T.shootW + 0.3, T.aheadW, 132, "THE GROUND AHEAD TURNS UP", T.faster, "FALLS FASTER", T.stronger, "g × A LITTLE MORE"]] as const)
          .map(([dir, o, tFly, tAhead, drop, sAhead, tFall, sFall, tG, sG]) => {
            const p = ramp(v, tFly - 0.3, 1.4, ease);
            return (
              <g key={dir} opacity={o}>
                <Pen pts={path(dir, 92, p)} p={1} w={2} tip={false} dash="8 8" opacity={0.6} />
                <Pen pts={path(dir, drop, p)} p={1} w={5} color={C.drag} />
                <text x={M[0] + dir * 580} y={M[1] + 98} textAnchor={dir > 0 ? "start" : "end"} style={label(p * 0.55, { fontSize: 18 })}>IF THE GROUND DIDN'T TURN</text>
                <RimArrow c={[cx, cy]} r={r + 30} a0={dir > 0 ? 34 : -64} a1={dir > 0 ? 64 : -34} p={ramp(v, tAhead - 0.2, 0.8)} />
                <text x={cx + dir * 300} y={830} textAnchor="middle" style={label(ramp(v, tAhead - 0.2, 0.4), { fontSize: 22 })}>{sAhead}</text>
                <text x={960} y={250} textAnchor="middle" style={label(ramp(v, tFall - 0.3, 0.4), { fontSize: 44 })}>{sFall}</text>
                <text x={960} y={320} textAnchor="middle" style={mono(ramp(v, tG - 0.3, 0.4), 36, { fontWeight: 600 })}>{sG}</text>
              </g>
            );
          })}
        <g opacity={caseN}>
          <circle cx={M[0]} cy={M[1] - 60} r={26} fill="none" stroke={C.drag} strokeWidth={4} />
          <line x1={M[0] - 18} y1={M[1] - 78} x2={M[0] + 18} y2={M[1] - 42} stroke={C.drag} strokeWidth={4} />
          <line x1={M[0] + 18} y1={M[1] - 78} x2={M[0] - 18} y2={M[1] - 42} stroke={C.drag} strokeWidth={4} />
          <text x={M[0] + 50} y={M[1] - 52} style={label(1, { fontSize: 22, fill: C.drag })}>THE SHOT: NORTH, INTO THE PICTURE</text>
          <text x={960} y={250} textAnchor="middle" style={label(ramp(v, T.axis - 0.4, 0.4), { fontSize: 32 })}>ALONG THE WHEEL'S AXIS</text>
          <text x={960} y={320} textAnchor="middle" style={label(ramp(v, T.nothing - 0.3, 0.4), { fontSize: 44 })}>NOTHING CHANGES</text>
        </g>
      </g>

      {/* ---------------- E: the formula */}
      <g opacity={fO}>
        <MathLine items={EOT} cx={FX} y={FY} size={FS} reveal={ramp(v, T.scales, 1.6)} active={active} />
        <text x={960} y={330} textAnchor="middle" style={label(window01(v, T.scales + 0.4, T.omega, 0.4) * 0.85, { fontSize: 22 })}>THE PROGRAM SCALES THE DROP THAT GRAVITY MAKES, JUST A LITTLE</text>
        <Legend x={60} y={400} v={v} o={1 - ramp(v, T.ours - 0.6, 0.4)} rows={[
          { sym: <>g<Sub s="eff" /></>, text: "WHAT GRAVITY BECOMES", at: T.scales + 0.6 },
          { sym: "g", text: "GRAVITY · 9.81 m/s²", at: T.scales + 0.9 },
          { sym: "Ω", text: "THE EARTH'S TURN", at: T.omega },
          { sym: "φ", text: "YOUR LATITUDE", at: T.cosine },
          { sym: "Az", text: "THE SHOT'S DIRECTION", at: T.shot },
          { sym: <>V<Sub s="0" /></>, text: "MUZZLE VELOCITY · m/s", at: T.muzzle },
        ]} />
        <text x={960} y={340} textAnchor="middle" style={mono(ramp(v, T.ours - 0.2, 0.5), 30, { fontWeight: 600 })}>
          1 − 2 · 7.29·10<tspan fontSize={18} dy={-12}>−5</tspan><tspan dy={12}> · 0.71 · 1 · 790 / 9.81 = 0.992</tspan>
        </text>
      </g>
      {/* cos φ: the wheel part of the turn */}
      <g opacity={xsO}>
        <XSection c={[700, 700]} r={230} lat={lat} o={1} parts={1} carO={0.35} wheelHot={1} />
        <text x={1480} y={640} textAnchor="middle" style={mono(1, 36, { fontWeight: 600 })}>φ = {Math.round(lat)}° · cos φ = {Math.cos(rad(lat)).toFixed(2)}</text>
        <text x={1480} y={690} textAnchor="middle" style={label(0.7, { fontSize: 20 })}>THE WHEEL PART OF THE TURN</text>
      </g>
      {/* sin(Az): a compass rose */}
      <g opacity={roseO}>
        {(() => {
          const c: P = [1360, 640], rr = 180;
          const L = [["N", 0, -1, "0", T.zero], ["E", 1, 0, "+1", T.plus], ["S", 0, 1, "0", T.zero], ["W", -1, 0, "−1", T.minus]] as const;
          return (
            <>
              <circle cx={c[0]} cy={c[1]} r={rr} fill="none" stroke={C.line} strokeWidth={2} opacity={0.5} />
              {L.map(([n, dx, dy, s, t]) => (
                <g key={n}>
                  <line x1={c[0]} y1={c[1]} x2={c[0] + dx * rr} y2={c[1] + dy * rr} stroke={C.line} strokeWidth={2} opacity={0.5} />
                  <text x={c[0] + dx * (rr + 40)} y={c[1] + dy * (rr + 40) + 9} textAnchor="middle" style={label(0.8, { fontSize: 24 })}>{n}</text>
                  <text x={c[0] + dx * (rr - 60) + (dx === 0 ? 40 : 0)} y={c[1] + dy * (rr - 60) + 12} textAnchor="middle" style={mono(ramp(v, t - 0.2, 0.4), 32, { fontWeight: 700 })}>{s}</text>
                </g>
              ))}
              <text x={c[0]} y={c[1] + rr + 90} textAnchor="middle" style={label(0.7, { fontSize: 20 })}>sin(Az) · Az — THE DIRECTION OF THE SHOT</text>
            </>
          );
        })()}
      </g>
      {/* F: our shot */}
      <g opacity={resO}>
        <text x={960} y={520} textAnchor="middle" style={label(0.8, { fontSize: 24 })}>OUR SHOT · 45° NORTH · 790 m/s</text>
        {([["EAST", "0.992", "+12 cm", "FALLS SLOWER", T.n12, 600], ["WEST", "1.008", "−12 cm", "FALLS FASTER", T.shootingW, 1320]] as const).map(([d, g, cm, s, t, x]) => (
          <g key={d}>
            <text x={x} y={640} textAnchor="middle" style={label(0.8, { fontSize: 26 })}>{d}</text>
            <text x={x} y={720} textAnchor="middle" style={mono(1, 48, { fontWeight: 600 })}>g × {g}</text>
            <text x={x} y={760} textAnchor="middle" style={label(0.6, { fontSize: 18 })}>{s}</text>
            <text x={x} y={850} textAnchor="middle" style={mono(ramp(v, t - 0.2, 0.4), 48, { fontWeight: 700 })}>{cm}</text>
            <text x={x} y={890} textAnchor="middle" style={label(ramp(v, t - 0.2, 0.4) * 0.6, { fontSize: 18 })}>THE HIT AT 1000 m</text>
          </g>
        ))}
        <text x={960} y={980} textAnchor="middle" style={label(ramp(v, T.percent - 0.3, 0.4) * 0.7, { fontSize: 22 })}>GRAVITY: LESS THAN 1 % WEAKER TO THE EAST</text>
      </g>
    </Scene>
  );
};
