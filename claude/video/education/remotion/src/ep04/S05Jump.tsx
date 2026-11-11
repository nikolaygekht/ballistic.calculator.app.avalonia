// Scene 5 — aerodynamic jump, on the 2.5D stage of scene 1, at the muzzle: the side wind tilts v_air (episode 3), the
// nose leaves exactly along the barrel, so it is off the air flow, to the left; the air pushes it further left (a pale
// "no spin" ghost turns left, the real nose does not); by the rule the nose goes up and holds; only then the swings:
// the nose circles the air flow and the swings die out. One coral arrow on the bullet: the pushes added up, growing up.
// The nose turns into the air flow and stays there — nothing pushes up any more. Litz's second formula with the sign of
// the wind; an angle, so a straight wedge; to scale at 1000 m: 11 cm up against 3.35 m sideways; a wind from the left
// mirrors it (the hit and the path go down). A wind that changes downrange adds a little; the program ignores it.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, P, Pen, label, mono, seeded } from "../lib/draw";
import { Bullet3, Cam, V3, mixCam, pr, prLine } from "./stage3d";
import { ArrowA, BLEN, CG_F, CornerRule, Item, Legend, MathLine, Scene, Sub, Title, k_, keyPos, sub, t_, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 5;
const T = {
  sideWind: cue(N, "side wind"), muzzle: cue(N, "muzzle"), remember: cue(N, "Remember"), tilted: cue(N, "tilted"), third: cue(N, "third"),
  leaves: cue(N, "leaves the barrel"), first: cue(N, "very first"), pushes: cue(N, "pushes the nose"), left: cue(N, "to the left"),
  rule: cue(N, "rule again"), pushLeft: cue(N, "Push left"), up: cue(N, "goes up"), swings: cue(N, "swings up"),
  last: cue(N, "while they last"), tiltedUp: cue(N, "tilted up"), into: cue(N, "turns into"), flows: cue(N, "flows straight"),
  onlyStart: cue(N, "only at the start"), dragStill: cue(N, "drag still"), formula: cue(N, "another formula"), adds: cue(N, "adds"),
  length: cue(N, "length"), mult: cue(N, "multiplied"), sign: cue(N, "has a sign"), notDist: cue(N, "not a distance"),
  angle: cue(N, "an angle"), straight: cue(N, "straight line"), four: cue(N, "Four and a half"), n11: cue(N, "11"),
  itself: cue(N, "wind itself"), fromLeft: cue(N, "A wind from the left"), goesDown: cue(N, "goes down"), lower: cue(N, "lower"),
  lh: cue(N, "left hand"), changes: cue(N, "wind changes"), small: cue(N, "small new"), counts: cue(N, "counts only"),
};

const FX = 960, FY = 200, FS = 56;
const AJ: Item[] = [{ toks: [t_("Δy' = (0.01 · "), k_("S", "sg"), sub("g", "sg"), t_(" − 0.0024 · "), k_("L", "L"), t_(" + 0.032) · "), k_("W", "w"), sub("⊥", "w")] }];

// ---- the stage at the muzzle (drawn units: not to scale)
const CAM0: Cam = { yaw: 40, pitch: 16, dist: 640, focal: 1700, target: [330, 20, 0], sx: 900, sy: 560 };
const WIDE: Cam = { yaw: 62, pitch: 10, dist: 1900, focal: 1700, target: [700, 60, 0], sx: 900, sy: 560 };
const BCAL = 24;                                   // the bullet: drawn units per calibre
const GR = -190;                                   // the ground
const TILT = (0.33 * 50 * Math.PI) / 180;          // v_air off the barrel, drawn ×50, to the right (towards the wind)
const VAIR: V3 = [Math.cos(TILT), 0, Math.sin(TILT)];
const OM = 2.2, KD = 0.38, YAW0 = TILT;            // the swings: rad/s, damping 1/s, start: the nose along the barrel
const Q1 = Math.PI / (2 * OM);                     // a quarter swing: the nose from 9 o'clock to 12
const LIFT = 88;                                   // drawn units of path offset per (rad · s²)
const X0 = 150, U = 80;                            // the bullet waits just past the muzzle, then flies on at U units/s
/** The swings, s seconds after they start: the nose's offset from v_air as (right, up) in radians, the push added up
 *  (rad·s), and the path's offset (rad·s²). From behind, the nose starts at 9 o'clock and circles clockwise. */
const swingAt = (s: number) => {
  let sz = 0, sy = 0, pz = 0, py = 0;
  const dt = 0.02, n = Math.floor(Math.max(0, s) / dt);
  for (let i = 0; i < n; i++) {
    const t = i * dt, a = Math.PI - OM * t, A = YAW0 * Math.exp(-KD * t);
    sz += A * Math.cos(a) * dt; sy += A * Math.sin(a) * dt; pz += sz * dt; py += sy * dt;
  }
  const a = Math.PI - OM * Math.max(0, s), A = YAW0 * Math.exp(-KD * Math.max(0, s));
  return { yaw: [A * Math.cos(a), A * Math.sin(a)] as P, sum: [sz, sy] as P, off: [pz, py] as P };
};
const SUMF = Math.hypot(...swingAt(14).sum);       // the pushes, all added up
const along = (k: number): V3 => [VAIR[0] * k, 0, VAIR[2] * k];
const add = (a: V3, b: V3): V3 => [a[0] + b[0], a[1] + b[1], a[2] + b[2]];
/** The nose's direction for a yaw (right, up) off v_air: an exact turn, so a yaw of −TILT lies on the barrel. */
const noseDir = (yaw: P): V3 => { const h = TILT + yaw[0], u = yaw[1]; return [Math.cos(h) * Math.cos(u), Math.sin(u), Math.sin(h) * Math.cos(u)]; };
const scale = (a: V3, k: number): V3 => [a[0] * k, a[1] * k, a[2] * k];
const STREAKS3 = (() => { const r = seeded(9); return Array.from({ length: 46 }, () => ({ x: r() * 2000, y: GR + 30 + r() * 330, z: r() * 900, l: 40 + r() * 60 })); })();
const STREAKS2 = (() => { const r = seeded(13); return Array.from({ length: 34 }, () => ({ x: r() * 2200, y: 430 + r() * 340, l: 40 + r() * 50 })); })();

export const S05Jump: React.FC = () => {
  const v = useVoiceTime();
  // ---- the stage
  const stO = ramp(v, T.remember - 0.4, 0.6) * (1 - ramp(v, T.formula - 0.4, 0.4));
  const vairP = ramp(v, T.tilted - 0.2, 0.8, ease);
  const bulO = ramp(v, T.leaves - 0.2, 0.5);
  const gapHot = ramp(v, T.first - 0.2, 0.5) * (1 - ramp(v, T.up - 0.2, 0.4));
  const pushP = ramp(v, T.pushes - 0.1, 0.5) * (1 - ramp(v, T.swings - 0.2, 0.4));
  // the swing clock: a quarter swing on "goes up" (the nose 9 → 12 o'clock), a hold, then the swings on "swings up"
  const s = Q1 * ramp(v, T.up - 0.35, 1.0, ease) + Math.max(0, v - (T.swings - 0.2));
  const sw = swingAt(s);
  const xB = X0 + U * s;
  const settle = 1 - ramp(v, T.into - 0.2, 1.4, ease);              // "the nose turns into the air flow, and stays there"
  const yaw: P = s > 0 ? [sw.yaw[0] * settle, sw.yaw[1] * settle] : [-YAW0, 0];
  const centre = (x: number, off: P): V3 => [x, LIFT * off[1], LIFT * off[0]];      // the bullet leaves along the barrel's line
  const C0 = centre(xB, sw.off);
  const axisD = (k: number): V3 => scale(noseDir(yaw), k);
  const trail: V3[] = s > 0 ? Array.from({ length: 61 }, (_, i) => { const q = (s * i) / 60; return centre(X0 + U * q, swingAt(q).off); }) : [];
  const sumO = ramp(v, T.swings + 0.2, 0.5);
  const follow: Cam = { ...CAM0, target: [Math.max(330, xB + 180), 20 + 0.8 * C0[1], 0.5 * C0[2]] };
  const wide: Cam = { ...WIDE, target: [Math.max(700, xB - 300), 60 + 0.7 * C0[1], 0.4 * C0[2]] };
  const close: Cam = { ...CAM0, dist: 820, target: [xB + 120, 20 + C0[1], 0.6 * C0[2]] };
  const cam = mixCam(mixCam(follow, wide, ramp(v, T.tiltedUp - 0.5, 1.8)), close, ramp(v, T.into - 0.6, 1.6));
  const far = xB + 300;              // the lines and their labels run on ahead of the bullet
  const pathO = ramp(v, T.tiltedUp - 0.6, 0.6);
  const gx0 = Math.floor((cam.target[0] - 1400) / 100) * 100;      // the ground grid around the camera
  // ---- formula, the wedge, the target, the late wind
  const fO = ramp(v, T.formula - 0.3, 0.5);
  const active = v >= T.notDist - 0.3 ? null : v >= T.mult - 0.1 ? "w" : v >= T.length - 0.1 ? "L" : v >= T.adds - 0.3 ? "sg" : null;
  const wedgeO = ramp(v, T.notDist - 0.3, 0.5) * (1 - ramp(v, T.four - 0.4, 0.5));
  const tgtO = ramp(v, T.four - 0.3, 0.5) * (1 - ramp(v, T.changes - 0.4, 0.5));
  const flip = ramp(v, T.fromLeft - 0.1, 1.4, ease);                 // the wind from the left: everything mirrored
  const lateO = ramp(v, T.changes - 0.2, 0.5);
  const cap = (text: string, y: number, a: number, b: number, size = 26, fill?: string) =>
    <text x={960} y={y} textAnchor="middle" style={label(window01(v, a, b, 0.3), { fontSize: size, ...(fill ? { fill } : {}) })}>{text}</text>;

  return (
    <Scene n={N}>
      <Title text="AERODYNAMIC JUMP" o={ramp(v, -0.5, 0.5) * (1 - ramp(v, T.remember - 0.6, 0.4))} />
      <CornerRule spin={v * 0.8} push="left" pushP={ramp(v, T.pushLeft - 0.1, 0.5)} noseP={ramp(v, T.up - 0.2, 0.7)}
        o={ramp(v, 0, 0.6) * (1 - ramp(v, T.formula - 0.6, 0.5))} hot={window01(v, T.rule - 0.2, T.swings, 0.3)} />
      {/* ---------------- the stage at the muzzle, in space */}
      <g opacity={stO}>
        {/* the ground */}
        {Array.from({ length: 37 }, (_, i) => { const x = gx0 + i * 100, a = pr(cam, [x, GR, -300]), b = pr(cam, [x, GR, 300]); return <line key={`g${i}`} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={1} opacity={0.16} />; })}
        {[-300, -200, -100, 0, 100, 200, 300].map((z) => { const a = pr(cam, [Math.max(-200, gx0), GR, z]), b = pr(cam, [gx0 + 3600, GR, z]); return <line key={`z${z}`} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={1} opacity={0.16} />; })}
        {/* the wind, from the right */}
        <g opacity={ramp(v, T.sideWind - 0.2, 0.6) * (1 - ramp(v, T.swings - 0.3, 0.6))}>
          {STREAKS3.map((q, i) => {
            const z = 450 - ((((q.z + 180 * v) % 900) + 900) % 900);
            const a = pr(cam, [q.x - 200, q.y, z]), b = pr(cam, [q.x - 200, q.y, z - q.l]);
            return <line key={i} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeOpacity={0.22} strokeWidth={2} />;
          })}
          <text x={1860} y={1000} textAnchor="end" style={label(0.8, { fontSize: 22 })}>SIDE WIND · FROM THE RIGHT</text>
        </g>
        {cap("RIGHT AT THE MUZZLE", 120, T.muzzle - 0.2, T.remember + 0.4)}
        {/* the barrel and its line */}
        {(() => { const a = pr(cam, [-260, 0, 0]), b = pr(cam, [0, 0, 0]); return <line x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={16} opacity={0.9} />; })()}
        <Pen pts={prLine(cam, Array.from({ length: Math.ceil((xB + 900) / 100) + 1 }, (_, i) => [i * 100, 0, 0] as V3))} p={1} w={2} tip={false} dash="8 10" opacity={0.7} />
        <text x={pr(cam, [far, 0, 0])[0] + 12} y={pr(cam, [far, 0, 0])[1] - 10} style={label(0.6, { fontSize: 18 })}>THE BARREL'S LINE</text>
        {/* v_air: tilted towards the wind, through the bullet */}
        <g opacity={vairP}>
          <g filter="url(#glow)"><Pen pts={prLine(cam, [add(C0, along(-260 * vairP)), add(C0, along(400 * vairP))])} p={1} w={4} tip={false} /></g>
          <text x={pr(cam, add(C0, along(400)))[0] + 12} y={pr(cam, add(C0, along(400)))[1] + 30} style={mono(1, 32)}>v<tspan fontSize={20} dy={7}>air</tspan></text>
          <text x={pr(cam, add(C0, along(400)))[0] + 12} y={pr(cam, add(C0, along(400)))[1] + 62} style={label(ramp(v, T.third - 0.2, 0.4) * 0.8, { fontSize: 18 })}>AIR FLOW · 0.33° · DRAWN ×50</text>
          {/* the nose on the air flow */}
          <g opacity={window01(v, T.into + 0.4, T.formula - 0.5, 0.5)} filter="url(#glow)">
            <Pen pts={prLine(cam, [add(C0, along(-140)), add(C0, along(180))])} p={1} w={8} tip={false} opacity={0.5} />
          </g>
        </g>
        {/* the bullet: its nose off the air flow, the push, the swings */}
        <g opacity={bulO}>
          {s > 0 && <Pen pts={prLine(cam, trail)} p={1} w={3} tip={false} opacity={0.85} />}
          {(() => {
            const c = pr(cam, C0);
            const tipW = add(C0, axisD((1 - CG_F) * BLEN * BCAL)), baseW = add(C0, axisD(-CG_F * BLEN * BCAL));
            const nose = pr(cam, add(C0, axisD(60)));
            const tg = pr(cam, [tipW[0], GR, tipW[2]]), bg = pr(cam, [baseW[0], GR, baseW[2]]), tp = pr(cam, tipW);
            // directions round the bullet: a level ring (left, right) and a vertical (up)
            const RR = 190, ringO = window01(v, T.pushes - 0.5, T.swings + 1.2, 0.4);
            const ring = prLine(cam, Array.from({ length: 49 }, (_, i) => { const a = (i / 48) * Math.PI * 2; return add(C0, [RR * Math.cos(a), 0, RR * Math.sin(a)]); }));
            const lft = pr(cam, add(C0, [0, 0, -RR - 40])), rgt = pr(cam, add(C0, [0, 0, RR + 40])), upP = pr(cam, add(C0, [0, RR, 0]));
            const pa = pr(cam, add(C0, axisD(50))), pb = pr(cam, add(add(C0, axisD(50)), [0, 0, -RR * pushP]));
            const flow = pr(cam, add(C0, along(60)));
            // the ghost: what the nose would do without the spin — turn further left
            const gO = window01(v, T.left - 0.2, T.up - 0.1, 0.3);
            const gAx = noseDir([-YAW0 - 0.8 * YAW0 * ramp(v, T.left - 0.1, 0.9, ease), 0]);
            const gN = pr(cam, add(C0, scale(gAx, 120)));
            // the pushes added up: one coral arrow, growing up
            const sumK = 150 / SUMF, sumE = pr(cam, add(C0, [0, sw.sum[1] * sumK, sw.sum[0] * sumK]));
            return (
              <>
                {/* the nose's stake down to the ground, and the bullet's shadow: up shows in the stake, sideways in the shadow */}
                <line x1={tp[0]} y1={tp[1]} x2={tg[0]} y2={tg[1]} stroke={C.line} strokeWidth={1.4} strokeDasharray="5 6" opacity={0.55} />
                <line x1={bg[0]} y1={bg[1]} x2={tg[0]} y2={tg[1]} stroke={C.line} strokeWidth={5} strokeLinecap="round" opacity={0.3} />
                <g opacity={ringO}>
                  <Pen pts={ring} p={1} w={1.5} tip={false} dash="6 8" opacity={0.55} />
                  <line x1={lft[0]} y1={lft[1]} x2={rgt[0]} y2={rgt[1]} stroke={C.line} strokeWidth={1.2} opacity={0.35} />
                  <text x={lft[0] - 10} y={lft[1] + 8} textAnchor="end" style={label(0.8, { fontSize: 20 })}>LEFT</text>
                  <text x={rgt[0] + 10} y={rgt[1] + 8} style={label(0.8, { fontSize: 20 })}>RIGHT</text>
                  <g opacity={ramp(v, T.up - 0.3, 0.4)}>
                    <line x1={c[0]} y1={c[1]} x2={upP[0]} y2={upP[1]} stroke={C.line} strokeWidth={1.2} strokeDasharray="6 8" opacity={0.6} />
                    <text x={upP[0]} y={upP[1] - 12} textAnchor="middle" style={label(0.8, { fontSize: 20 })}>UP</text>
                  </g>
                </g>
                {gO > 0.01 && (
                  <g opacity={gO}>
                    <Bullet3 c={cam} cg={C0} axis={gAx} cal={BCAL} o={0.45} />
                    <text x={gN[0] - 14} y={gN[1] - 16} textAnchor="end" style={label(0.6, { fontSize: 18 })}>WITHOUT SPIN</text>
                  </g>
                )}
                <Bullet3 c={cam} cg={C0} axis={axisD(1)} cal={BCAL} spin={v * 0.9} />
                {/* the gap between the nose and the air flow */}
                <g opacity={gapHot}>
                  <line x1={nose[0]} y1={nose[1]} x2={flow[0]} y2={flow[1]} stroke={C.line} strokeWidth={4} filter="url(#glow)" />
                </g>
                {pushP > 0.02 && <Arrow x1={pa[0]} y1={pa[1]} x2={pb[0]} y2={pb[1]} color={C.drag} w={6} head={18} />}
                {sumO > 0.01 && Math.hypot(sumE[0] - c[0], sumE[1] - c[1]) > 12 && (
                  <g opacity={sumO}>
                    <Arrow x1={c[0]} y1={c[1]} x2={sumE[0]} y2={sumE[1]} color={C.drag} w={7} head={20} />
                    <text x={sumE[0] + 22} y={sumE[1] + 10} style={label(0.95, { fontSize: 22, fill: C.drag })}>THE PUSHES, ADDED UP</text>
                  </g>
                )}
              </>
            );
          })()}
        </g>
        {/* the path: without the jump, along the barrel; with it, tilted up */}
        <g opacity={pathO}>
          <text x={pr(cam, [far, 0, 0])[0] + 12} y={pr(cam, [far, 0, 0])[1] + 16} style={label(0.6, { fontSize: 18 })}>WITHOUT THE JUMP</text>
        </g>
        <g opacity={window01(v, T.tiltedUp - 0.3, T.into - 0.3, 0.4)}>
          <text x={pr(cam, C0)[0] - 30} y={pr(cam, C0)[1] + 70} textAnchor="end" style={label(1, { fontSize: 22 })}>TILTED UP BY A TINY ANGLE</text>
        </g>
        {cap("THE NOSE IS OFF THE AIR FLOW", 120, T.first - 0.2, T.up - 0.2)}
        {cap("THE AIR PUSHES THE NOSE LEFT, AWAY FROM THE WIND", 170, T.pushes - 0.1, T.swings - 0.2, 24, C.drag)}
        {cap("WITHOUT SPIN, THE NOSE WOULD TURN LEFT · BUT IT SPINS", 220, T.left - 0.1, T.up - 0.3, 24)}
        {cap("PUSH LEFT → THE NOSE GOES UP", 220, T.up - 0.2, T.swings + 0.4)}
        {cap("THE NOSE SWINGS UP AND AROUND THE AIR FLOW · THE SWINGS DIE OUT", 120, T.swings - 0.2, T.last)}
        {cap("WHILE THEY LAST, THE AIR PUSHES THE BULLET UP", 120, T.last - 0.2, T.tiltedUp - 0.2)}
        {cap("ADDED UP, THE PUSHES POINT UP · SO THE PATH STAYS TILTED UP", 120, T.tiltedUp - 0.2, T.into - 0.3)}
        {cap("THE NOSE TURNS INTO THE AIR FLOW · AND STAYS THERE", 120, T.into - 0.2, T.onlyStart - 0.3)}
        {cap("THE AIR FLOWS ALONG THE BULLET: NOTHING PUSHES THE NOSE", 170, T.flows - 0.2, T.onlyStart - 0.3, 24)}
        {cap("SO THE JUMP HAPPENS ONLY AT THE START", 120, T.onlyStart - 0.2, T.formula - 0.4)}
        {cap("THE DRAG STILL PUSHES SIDEWAYS · NOTHING PUSHES UP ANY MORE", 170, T.dragStill - 0.2, T.formula - 0.4, 24)}
      </g>

      {/* ---------------- Litz's second formula */}
      <g opacity={fO}>
        <MathLine items={AJ} cx={FX} y={FY} size={FS} reveal={ramp(v, T.formula, 1.6)} active={active} />
        <text x={960} y={290} textAnchor="middle" style={label(window01(v, T.formula + 0.6, T.adds - 0.3, 0.4) * 0.8, { fontSize: 22 })}>BRYAN LITZ</text>
        <Legend x={60} y={400} v={v} o={1 - ramp(v, T.changes - 0.6, 0.4)} rows={[
          { sym: "Δy'", text: "THE JUMP · AN ANGLE, MOA", at: T.adds - 0.3 },
          { sym: <>S<Sub s="g" /></>, text: "STABILITY · FROM MILLER", at: T.adds },
          { sym: "L", text: "LENGTH · CALIBRES", at: T.length },
          { sym: <>W<Sub s="⊥" /></>, text: "SIDE WIND AT THE MUZZLE · mph", at: T.mult },
          { sym: "±", text: "+ FROM THE RIGHT · − FROM THE LEFT", at: T.sign },
        ]} />
        <text x={960} y={300} textAnchor="middle" style={mono(ramp(v, T.four - 0.3, 0.5) * (1 - ramp(v, T.fromLeft - 0.4, 0.4)), 30, { fontWeight: 600 })}>
          Δy' = (0.01 · 1.71 − 0.0024 · 4.53 + 0.032) · 10.1 = 0.39 MOA
        </text>
        <text x={960} y={300} textAnchor="middle" style={mono(ramp(v, T.fromLeft - 0.2, 0.5) * (1 - ramp(v, T.changes - 0.6, 0.4)), 30, { fontWeight: 600 })}>
          Δy' = (0.01 · 1.71 − 0.0024 · 4.53 + 0.032) · (−10.1) = −0.39 MOA
        </text>
        {(() => {
          const L = MathLineKey(AJ, "sg"), Lp = MathLineKey(AJ, "L"), W = MathLineKey(AJ, "w");
          return (
            <>
              <text x={L[0]} y={FY - 70} textAnchor="middle" style={mono(window01(v, T.adds - 0.2, T.notDist, 0.3), 40, { fontWeight: 700 })}>+</text>
              <text x={Lp[0]} y={FY - 70} textAnchor="middle" style={mono(window01(v, T.length - 0.2, T.notDist, 0.3), 40, { fontWeight: 700, fill: C.drag })}>−</text>
              <g opacity={window01(v, T.mult - 0.2, T.sign + 1.6, 0.3)}><ArrowA x={W[0] + 70} y={FY - 80} a={Math.PI} len={140} w={4} head={16} /></g>
              {/* the sign: + from the right, − from the left */}
              <g opacity={window01(v, T.sign + 0.6, T.notDist, 0.3)}>
                <text x={W[0] - 40} y={FY + 80} textAnchor="end" style={mono(ramp(v, T.sign + 0.7, 0.3), 30, { fontWeight: 700 })}>+</text>
                <ArrowA x={W[0] + 90} y={FY + 70} a={Math.PI} len={110} w={3} head={14} />
                <text x={W[0] - 40} y={FY + 130} textAnchor="end" style={mono(ramp(v, T.sign + 1.6, 0.3), 30, { fontWeight: 700, fill: C.drag })}>−</text>
                <g opacity={ramp(v, T.sign + 1.6, 0.3)}><ArrowA x={W[0] - 20} y={FY + 120} a={0} len={110} w={3} head={14} /></g>
              </g>
            </>
          );
        })()}
      </g>
      {/* an angle: a straight wedge */}
      <g opacity={wedgeO}>
        {(() => {
          const X0 = 260, X1 = 1660, Y = 760, a = 0.09;
          const y = (x: number) => Y - (x - X0) * a;
          return (
            <>
              <line x1={X0} y1={Y} x2={X1} y2={Y} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
              <line x1={X0} y1={Y} x2={lerp(X0, X1, ramp(v, T.notDist, 1.0))} y2={y(lerp(X0, X1, ramp(v, T.notDist, 1.0)))} stroke={C.line} strokeWidth={4} />
              {[[0.5, "5.6 cm"], [1, "11 cm"]].map(([k, s]) => {
                const x = lerp(X0, X1, k as number);
                return (
                  <g key={s as string} opacity={ramp(v, T.straight - 0.2 + (k as number) * 0.6, 0.4)}>
                    <line x1={x} y1={Y} x2={x} y2={y(x)} stroke={C.line} strokeWidth={3} />
                    <text x={x + 14} y={(Y + y(x)) / 2 + 10} style={mono(1, 32, { fontWeight: 600 })}>{s}</text>
                    <text x={x} y={Y + 40} textAnchor="middle" style={label(0.6, { fontSize: 18 })}>{k === 1 ? "1000 m" : "500 m"}</text>
                  </g>
                );
              })}
              <text x={960} y={900} textAnchor="middle" style={label(ramp(v, T.angle - 0.2, 0.4), { fontSize: 28 })}>NOT A DISTANCE: AN ANGLE, THE SAME AT EVERY DISTANCE</text>
              <text x={960} y={950} textAnchor="middle" style={label(ramp(v, T.straight - 0.2, 0.4) * 0.8, { fontSize: 24 })}>SO IT GROWS IN A STRAIGHT LINE</text>
            </>
          );
        })()}
      </g>
      {/* at 1000 m, to scale: 11 cm up, 3.35 m sideways; then the wind from the left mirrors it */}
      <g opacity={tgtO}>
        {(() => {
          const c: P = [1000, 600], k = 250;          // px per metre
          const sg = 1 - 2 * flip;                     // +1: wind from the right, −1: from the left
          const hitY = c[1] - 0.11 * k * sg;
          const tf = T.fromLeft;                       // the streaks reverse there, without a jump
          const ph = v < tf ? v : tf - (v - tf);
          const sO = 1 - 4 * flip * (1 - flip);        // the streaks dip while they turn round
          return (
            <>
              <g opacity={sO * 0.9}>
                {STREAKS2.map((q, i) => {
                  const x = ((((q.x - 260 * ph) % 2200) + 2200) % 2200) - 140;
                  return <line key={i} x1={x} y1={q.y} x2={x + q.l} y2={q.y} stroke={C.line} strokeOpacity={0.16} strokeWidth={2} />;
                })}
              </g>
              <text x={960} y={1010} textAnchor="middle" style={label(0.85 * (1 - flip), { fontSize: 26 })}>AT 1000 m · 4.5 m/s FROM THE RIGHT · TO SCALE</text>
              <text x={960} y={1010} textAnchor="middle" style={label(0.85 * flip * (1 - ramp(v, T.lh - 0.3, 0.4)), { fontSize: 26 })}>AT 1000 m · 4.5 m/s FROM THE LEFT · TO SCALE</text>
              <text x={960} y={1010} textAnchor="middle" style={label(ramp(v, T.lh - 0.2, 0.4), { fontSize: 26 })}>A LEFT-HAND TWIST: ALL THE OTHER WAY ROUND</text>
              <circle cx={c[0]} cy={c[1]} r={k * 0.5} fill="none" stroke={C.line} strokeWidth={2} opacity={0.5} />
              <circle cx={c[0]} cy={c[1]} r={k * 0.25} fill="none" stroke={C.line} strokeWidth={1.5} opacity={0.4} />
              <circle cx={c[0]} cy={c[1]} r={6} fill={C.line} opacity={0.6} />
              <g opacity={ramp(v, T.n11 - 0.2, 0.4)}>
                <line x1={c[0]} y1={c[1]} x2={c[0]} y2={hitY} stroke={C.line} strokeWidth={4} filter="url(#glow)" />
                <circle cx={c[0]} cy={hitY} r={9} fill={C.line} filter="url(#glow)" />
                <text x={c[0] + 30} y={c[1] - 40} style={mono(1 - flip, 34, { fontWeight: 700 })}>11 cm up</text>
                <text x={c[0] + 30} y={c[1] + 64} style={mono(ramp(v, T.lower - 0.3, 0.4), 34, { fontWeight: 700 })}>11 cm down</text>
              </g>
              <g opacity={ramp(v, T.itself - 0.2, 0.4)}>
                {(() => {
                  const end = c[0] - 3.35 * k * ramp(v, T.itself, 1.0, ease) * sg;
                  return (
                    <>
                      <line x1={c[0]} y1={c[1] + 150} x2={end} y2={c[1] + 150} stroke={C.drag} strokeWidth={5} />
                      <text x={end} y={c[1] + 196} textAnchor={sg > 0 ? "start" : "end"} style={mono(Math.abs(sg), 32, { fill: C.drag, fontWeight: 600 })}>3.35 m: the wind itself</text>
                    </>
                  );
                })()}
              </g>
              <text x={c[0]} y={420} textAnchor="middle" style={label(ramp(v, T.goesDown - 0.3, 0.4), { fontSize: 28 })}>A WIND FROM THE LEFT: THE JUMP GOES DOWN</text>
              {/* the path, from the side: tilted up, then down */}
              <g opacity={ramp(v, T.fromLeft + 0.2, 0.5)}>
                {(() => {
                  const A0 = 120, A1 = 610, Y = 900, d = 70 * sg;
                  return (
                    <>
                      <line x1={A0} y1={Y} x2={A1} y2={Y} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
                      <line x1={A0} y1={Y} x2={A1} y2={Y - d} stroke={C.line} strokeWidth={4} />
                      <circle cx={A0} cy={Y} r={6} fill={C.line} />
                      <text x={A0} y={Y + 40} textAnchor="middle" style={label(0.6, { fontSize: 16 })}>MUZZLE</text>
                      <text x={A1} y={Y + 40} textAnchor="end" style={label(0.6, { fontSize: 16 })}>1000 m</text>
                      <text x={(A0 + A1) / 2} y={Y - 100} textAnchor="middle" style={label(0.6, { fontSize: 16 })}>THE PATH, FROM THE SIDE</text>
                    </>
                  );
                })()}
              </g>
            </>
          );
        })()}
      </g>
      {/* a wind that changes downrange: a small new push, not counted */}
      <g opacity={lateO}>
        {(() => {
          const X0 = 260, X1 = 1660, Y = 680, xm = (m: number) => X0 + ((X1 - X0) * m) / 1000;
          const j0 = 0.05, kink = 0.035 * ramp(v, T.small - 0.2, 1.0, ease);
          const yAt = (m: number) => Y - (xm(m) - X0) * j0 - Math.max(0, xm(m) - xm(300)) * kink;
          const pts: P[] = Array.from({ length: 41 }, (_, i) => { const m = i * 25; return [xm(m), yAt(m)]; });
          return (
            <>
              <line x1={X0} y1={Y} x2={X1} y2={Y} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
              <Pen pts={pts} p={ramp(v, T.changes, 1.0)} w={4} tip={false} />
              <circle cx={X0} cy={Y} r={8} fill={C.line} />
              <text x={X0} y={Y + 44} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>MUZZLE</text>
              <text x={X1} y={Y + 44} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>1000 m</text>
              <line x1={xm(300)} y1={Y - 30} x2={xm(300)} y2={Y + 30} stroke={C.line} strokeWidth={2} />
              <text x={xm(300)} y={Y + 44} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>300 m</text>
              <g opacity={ramp(v, T.changes + 0.4, 0.5)}>
                {Array.from({ length: 6 }, (_, i) => <ArrowA key={i} x={xm(380 + i * 110) + 60} y={Y + 120} a={Math.PI} len={90} w={3} head={12} />)}
                <text x={xm(650)} y={Y + 190} textAnchor="middle" style={label(0.7, { fontSize: 20 })}>THE WIND CHANGES AT 300 m</text>
              </g>
              <text x={xm(560)} y={yAt(560) - 50} textAnchor="middle" style={label(ramp(v, T.small - 0.2, 0.4), { fontSize: 24, fill: C.drag })}>A SMALL NEW PUSH</text>
              <text x={960} y={420} textAnchor="middle" style={label(ramp(v, T.changes - 0.2, 0.4), { fontSize: 28 })}>IF THE WIND CHANGES DOWNRANGE</text>
              <text x={960} y={960} textAnchor="middle" style={label(ramp(v, T.counts - 0.3, 0.4), { fontSize: 28 })}>THE PROGRAM COUNTS ONLY THE WIND AT THE MUZZLE</text>
            </>
          );
        })()}
      </g>
    </Scene>
  );
};

const MathLineKey = (items: Item[], key: string) => keyPos(items, FX, FY, FS, key);
