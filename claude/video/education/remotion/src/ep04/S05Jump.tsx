// Scene 5 — aerodynamic jump, on the 2.5D stage of scene 1, at the muzzle: the side wind tilts v_air (episode 3), the
// nose leaves along the barrel, so it is off the air flow, to the left; the air pushes it further left; by the rule
// the nose goes up — a right angle, seen in space. The nose circles the air flow in a shrinking spiral, the air's push
// follows the nose, and added up the pushes point a quarter turn on from the start: up. The path bends at the muzzle,
// then flies on straight, tilted up by a tiny angle. Litz's second formula; an angle, so a straight
// wedge; to scale at 1000 m: 11 cm up against 3.35 m sideways. A wind that starts at 300 m gives no jump.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, P, Pen, label, mono, seeded } from "../lib/draw";
import { Bullet3, Cam, V3, mixCam, pr, prLine } from "./stage3d";
import { ArrowA, BLEN, CG_F, CornerRule, Item, Legend, MathLine, Scene, Sub, Title, k_, keyPos, sub, t_, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 5;
const T = {
  sideWind: cue(N, "side wind"), muzzle: cue(N, "muzzle"), remember: cue(N, "Remember"), tilted: cue(N, "tilted"), third: cue(N, "third"),
  leaves: cue(N, "leaves the barrel"), first: cue(N, "very first"), airflow: cue(N, "airflow"), pushes: cue(N, "pushes the nose"), left: cue(N, "to the left"),
  rule: cue(N, "rule again"), pushLeft: cue(N, "Push left"), up: cue(N, "goes up"), swings: cue(N, "swings up"), dieOut: cue(N, "die out"),
  last: cue(N, "while they last"), tiltedUp: cue(N, "tilted up"), formula: cue(N, "another formula"), adds: cue(N, "adds"), length: cue(N, "length"),
  mult: cue(N, "multiplied"), notDist: cue(N, "not a distance"), angle: cue(N, "an angle"), every: cue(N, "every distance"), straight: cue(N, "straight line"),
  four: cue(N, "Four and a half"), n11: cue(N, "11"), itself: cue(N, "wind itself"), only: cue(N, "only the wind"), m300: cue(N, "300"), noJump: cue(N, "no jump"),
};

const FX = 960, FY = 200, FS = 56;
const AJ: Item[] = [{ toks: [t_("Δy' = (0.01 · "), k_("S", "sg"), sub("g", "sg"), t_(" − 0.0024 · "), k_("L", "L"), t_(" + 0.032) · "), k_("W", "w"), sub("⊥", "w")] }];
const STREAKS = (() => { const r = seeded(5); return Array.from({ length: 60 }, () => ({ x: r() * 2100, y: 90 + r() * 960, l: 26 + r() * 40, a: 0.12 + r() * 0.25 })); })();

// ---- the stage at the muzzle (drawn units: not to scale)
const CAM0: Cam = { yaw: 40, pitch: 16, dist: 640, focal: 1700, target: [330, 20, 0], sx: 900, sy: 560 };
const WIDE: Cam = { yaw: 62, pitch: 10, dist: 1900, focal: 1700, target: [700, 60, 0], sx: 900, sy: 560 };
const BCAL = 24;                                   // the bullet: drawn units per calibre
const GR = -190;                                   // the ground
const TILT = (0.33 * 20 * Math.PI) / 180;          // v_air off the barrel, drawn ×20, to the right (towards the wind)
const VAIR: V3 = [Math.cos(TILT), 0, Math.sin(TILT)];
const OM = 2.2, KD = 0.38, YAW0 = TILT;            // the swings: rad/s, damping 1/s, start: the nose along the barrel
const LIFT = 220;                                  // drawn units of path offset per (rad · s²)
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
const along = (k: number): V3 => [VAIR[0] * k, 0, VAIR[2] * k];
const add = (a: V3, b: V3): V3 => [a[0] + b[0], a[1] + b[1], a[2] + b[2]];
const STREAKS3 = (() => { const r = seeded(9); return Array.from({ length: 46 }, () => ({ x: -200 + r() * 2000, y: GR + 30 + r() * 330, z: r() * 900, l: 40 + r() * 60 })); })();

export const S05Jump: React.FC = () => {
  const v = useVoiceTime();
  // ---- the stage
  const stO = ramp(v, T.remember - 0.4, 0.6) * (1 - ramp(v, T.formula - 0.4, 0.4));
  const vairP = ramp(v, T.tilted - 0.2, 0.8, ease);
  const bulO = ramp(v, T.leaves - 0.2, 0.5);
  const gapHot = ramp(v, T.first - 0.2, 0.5) * (1 - ramp(v, T.up - 0.2, 0.4));
  const pushP = ramp(v, T.pushes - 0.1, 0.5) * (1 - ramp(v, T.swings - 0.2, 0.4));
  const s = v - (T.up - 0.3);                                     // the swings start when the nose goes up
  const sw = swingAt(s);
  const xB = X0 + U * Math.max(0, s);
  // before the swings the nose is along the barrel: 9 o'clock from behind, a bit further left once the air pushes it
  const yaw: P = s > 0 ? sw.yaw : [-YAW0 * (1 + 0.4 * ramp(v, T.left - 0.2, 0.8)), 0];
  const centre = (x: number, off: P): V3 => [x, LIFT * off[1], LIFT * off[0]];      // the bullet leaves along the barrel's line
  const C0 = centre(xB, s > 0 ? sw.off : [0, 0]);
  const axisD = (k: number): V3 => add(along(k), [0, yaw[1] * 3.2 * k, yaw[0] * 3.2 * k]);     // the yaw drawn ×3.2 more
  const trail: V3[] = s > 0 ? Array.from({ length: 41 }, (_, i) => { const q = (s * i) / 40; return centre(X0 + U * q, swingAt(q).off); }) : [];
  const liftO = ramp(v, T.last - 0.4, 0.5);
  const follow: Cam = { ...CAM0, target: [Math.max(330, xB + 180), 20 + 0.8 * C0[1], 0.5 * C0[2]] };
  const cam = mixCam(follow, WIDE, ramp(v, T.tiltedUp - 0.5, 1.8));
  const far = xB + 300;              // the lines and their labels run on ahead of the bullet
  const pathO = ramp(v, T.tiltedUp - 0.6, 0.6);
  // ---- formula, the wedge, the target, the late wind
  const fO = ramp(v, T.formula - 0.3, 0.5);
  const active = v >= T.notDist - 0.3 ? null : v >= T.mult - 0.1 ? "w" : v >= T.length - 0.1 ? "L" : v >= T.adds - 0.3 ? "sg" : null;
  const wedgeO = ramp(v, T.notDist - 0.3, 0.5) * (1 - ramp(v, T.four - 0.4, 0.5));
  const tgtO = ramp(v, T.four - 0.3, 0.5) * (1 - ramp(v, T.only - 0.4, 0.5));
  const lateO = ramp(v, T.only - 0.2, 0.5);

  return (
    <Scene n={N}>
      <Title text="AERODYNAMIC JUMP" o={ramp(v, -0.5, 0.5) * (1 - ramp(v, T.remember - 0.6, 0.4))} />
      <CornerRule spin={v * 0.8} push="left" pushP={ramp(v, T.pushLeft - 0.1, 0.5)} noseP={ramp(v, T.up - 0.2, 0.7)}
        o={ramp(v, 0, 0.6) * (1 - ramp(v, T.formula - 0.6, 0.5))} hot={window01(v, T.rule - 0.2, T.swings, 0.3)} />
      {/* ---------------- the stage at the muzzle, in space */}
      <g opacity={stO}>
        {/* the ground */}
        {Array.from({ length: 25 }, (_, i) => { const x = -200 + i * 100, a = pr(cam, [x, GR, -300]), b = pr(cam, [x, GR, 300]); return <line key={`g${i}`} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={1} opacity={0.16} />; })}
        {[-300, -200, -100, 0, 100, 200, 300].map((z) => { const a = pr(cam, [-200, GR, z]), b = pr(cam, [2200, GR, z]); return <line key={`z${z}`} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={1} opacity={0.16} />; })}
        {/* the wind, from the right */}
        <g opacity={ramp(v, T.sideWind - 0.2, 0.6) * (1 - ramp(v, T.swings - 0.3, 0.6))}>
          {STREAKS3.map((q, i) => {
            const z = 450 - ((((q.z + 180 * v) % 900) + 900) % 900);
            const a = pr(cam, [q.x, q.y, z]), b = pr(cam, [q.x, q.y, z - q.l]);
            return <line key={i} x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeOpacity={0.22} strokeWidth={2} />;
          })}
          <text x={1860} y={1000} textAnchor="end" style={label(0.8, { fontSize: 22 })}>SIDE WIND · FROM THE RIGHT</text>
          <text x={960} y={120} textAnchor="middle" style={label(window01(v, T.muzzle - 0.2, T.remember + 0.4, 0.3), { fontSize: 26 })}>RIGHT AT THE MUZZLE</text>
        </g>
        {/* the barrel and its line */}
        {(() => { const a = pr(cam, [-260, 0, 0]), b = pr(cam, [0, 0, 0]); return <line x1={a[0]} y1={a[1]} x2={b[0]} y2={b[1]} stroke={C.line} strokeWidth={16} opacity={0.9} />; })()}
        <Pen pts={prLine(cam, Array.from({ length: 23 }, (_, i) => [i * 100, 0, 0] as V3))} p={1} w={2} tip={false} dash="8 10" opacity={0.7} />
        <text x={pr(cam, [far, 0, 0])[0] + 12} y={pr(cam, [far, 0, 0])[1] - 10} style={label(0.6, { fontSize: 18 })}>THE BARREL'S LINE</text>
        {/* v_air: tilted a third of a degree towards the wind */}
        <g opacity={vairP}>
          <g filter="url(#glow)"><Pen pts={prLine(cam, [add(C0, along(-260 * vairP)), add(C0, along(400 * vairP))])} p={1} w={4} tip={false} /></g>
          <text x={pr(cam, add(C0, along(400)))[0] + 12} y={pr(cam, add(C0, along(400)))[1] + 30} style={mono(1, 32)}>v<tspan fontSize={20} dy={7}>air</tspan></text>
          <text x={pr(cam, add(C0, along(400)))[0] + 12} y={pr(cam, add(C0, along(400)))[1] + 62} style={label(ramp(v, T.third - 0.2, 0.4) * 0.8, { fontSize: 18 })}>AIR FLOW · 0.33° · DRAWN ×20</text>
        </g>
        {/* the bullet: its nose off the air flow, the push, the swings, the air's push following the nose */}
        <g opacity={bulO}>
          {s > 0 && <Pen pts={prLine(cam, trail)} p={1} w={3} tip={false} opacity={0.85} />}
          {(() => {
            const c = pr(cam, C0);
            const ax = axisD(1), tipW = add(C0, axisD((1 - CG_F) * BLEN * BCAL)), baseW = add(C0, axisD(-CG_F * BLEN * BCAL));
            const nose = pr(cam, add(C0, axisD(60)));
            const tg = pr(cam, [tipW[0], GR, tipW[2]]), bg = pr(cam, [baseW[0], GR, baseW[2]]), tp = pr(cam, tipW);
            const ym = Math.hypot(yaw[0], yaw[1]) || 1, ll = 70 + 110 * Math.min(1, ym / YAW0);
            const lift = pr(cam, add(C0, [0, (yaw[1] / ym) * ll, (yaw[0] / ym) * ll]));
            // directions round the bullet: a level ring (left, right) and a vertical (up)
            const RR = 190, ringO = window01(v, T.pushes - 0.5, T.swings + 1.2, 0.4);
            const ring = prLine(cam, Array.from({ length: 49 }, (_, i) => { const a = (i / 48) * Math.PI * 2; return add(C0, [RR * Math.cos(a), 0, RR * Math.sin(a)]); }));
            const lft = pr(cam, add(C0, [0, 0, -RR - 40])), rgt = pr(cam, add(C0, [0, 0, RR + 40])), upP = pr(cam, add(C0, [0, RR, 0]));
            const pa = pr(cam, add(C0, axisD(50))), pb = pr(cam, add(add(C0, axisD(50)), [0, 0, -RR * pushP]));
            const flow = pr(cam, add(C0, along(60)));
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
                <Bullet3 c={cam} cg={C0} axis={ax} cal={BCAL} spin={v * 0.9} />
                {/* the gap between the nose and the air flow */}
                <g opacity={gapHot}>
                  <line x1={nose[0]} y1={nose[1]} x2={flow[0]} y2={flow[1]} stroke={C.line} strokeWidth={4} filter="url(#glow)" />
                  <text x={960} y={120} textAnchor="middle" style={label(1, { fontSize: 26 })}>THE NOSE IS OFF THE AIR FLOW</text>
                </g>
                {pushP > 0.02 && <Arrow x1={pa[0]} y1={pa[1]} x2={pb[0]} y2={pb[1]} color={C.drag} w={6} head={18} />}
                <text x={960} y={170} textAnchor="middle" style={label(pushP, { fontSize: 24, fill: C.drag })}>THE AIR PUSHES THE NOSE LEFT, AWAY FROM THE WIND</text>
                <text x={960} y={220} textAnchor="middle" style={label(window01(v, T.up - 0.2, T.swings + 0.4, 0.3), { fontSize: 26 })}>PUSH LEFT → THE NOSE GOES UP</text>
                <g opacity={liftO * (s > 0 ? 1 : 0) * (1 - pathO)}>
                  <Arrow x1={c[0]} y1={c[1]} x2={lift[0]} y2={lift[1]} color={C.drag} w={5} head={14} />
                </g>
              </>
            );
          })()}
        </g>
        {/* the path: without the jump, along v_air; with it, tilted up */}
        <g opacity={pathO}>
          <text x={pr(cam, C0)[0] - 20} y={pr(cam, C0)[1] - 50} textAnchor="end" style={label(1, { fontSize: 22 })}>TILTED UP BY A TINY ANGLE</text>
          <text x={pr(cam, [far, 0, 0])[0] + 12} y={pr(cam, [far, 0, 0])[1] + 16} style={label(0.6, { fontSize: 18 })}>WITHOUT THE JUMP</text>
        </g>
        <text x={960} y={120} textAnchor="middle" style={label(window01(v, T.swings - 0.2, T.last, 0.3), { fontSize: 26 })}>THE NOSE SWINGS UP AND AROUND THE AIR FLOW · THE SWINGS DIE OUT</text>
        <text x={960} y={120} textAnchor="middle" style={label(window01(v, T.last - 0.2, T.tiltedUp - 0.2, 0.3), { fontSize: 26 })}>THE AIR PUSHES WHERE THE NOSE POINTS</text>
        <text x={960} y={120} textAnchor="middle" style={label(ramp(v, T.tiltedUp - 0.2, 0.4), { fontSize: 26 })}>ADDED UP, THE PUSHES POINT UP · SO THE PATH STAYS TILTED UP</text>
        {/* the nose, from behind: the spiral, and the pushes added up */}
        <g opacity={ramp(v, T.swings - 0.4, 0.5)}>
          {(() => {
            const cx = 1640, cy = 820, R = 110, k = (0.8 * R) / YAW0;
            const n = Math.max(2, Math.min(200, Math.floor(Math.max(0, s) / 0.05)));
            const pts: P[] = Array.from({ length: n + 1 }, (_, i) => { const y = swingAt((Math.max(0, s) * i) / n).yaw; return [cx + k * y[0], cy - k * y[1]]; });
            const e = pts[pts.length - 1];
            const sm = (0.8 * R) / (0.45 * YAW0);                       // the full sum (≈ 0.45 · start yaw · 1 s) drawn to 0.8 R
            const sumEnd: P = [cx + sw.sum[0] * sm, cy - sw.sum[1] * sm];
            return (
              <>
                <circle cx={cx} cy={cy} r={R} fill="none" stroke={C.line} strokeWidth={2.5} />
                <line x1={cx - 8} y1={cy} x2={cx + 8} y2={cy} stroke={C.line} strokeWidth={1.5} opacity={0.5} />
                <line x1={cx} y1={cy - 8} x2={cx} y2={cy + 8} stroke={C.line} strokeWidth={1.5} opacity={0.5} />
                <Pen pts={pts} p={1} w={2} tip={false} opacity={0.6} />
                <circle cx={e[0]} cy={e[1]} r={8} fill={C.line} filter="url(#glow)" />
                <g opacity={liftO}><Arrow x1={cx} y1={cy} x2={sumEnd[0]} y2={sumEnd[1]} color={C.drag} w={5} head={14} /></g>
                <text x={cx} y={cy + R + 32} textAnchor="middle" style={label(0.6, { fontSize: 16 })}>THE NOSE, FROM BEHIND</text>
                <text x={cx} y={cy + R + 56} textAnchor="middle" style={label(0.5, { fontSize: 14 })}>THE CROSS: THE AIR FLOW</text>
                <text x={cx} y={cy - R - 18} textAnchor="middle" style={label(liftO * 0.9, { fontSize: 16, fill: C.drag })}>THE PUSHES, ADDED UP</text>
              </>
            );
          })()}
        </g>
      </g>

      {/* ---------------- Litz's second formula */}
      <g opacity={fO}>
        <MathLine items={AJ} cx={FX} y={FY} size={FS} reveal={ramp(v, T.formula, 1.6)} active={active} />
        <text x={960} y={290} textAnchor="middle" style={label(window01(v, T.formula + 0.6, T.adds - 0.3, 0.4) * 0.8, { fontSize: 22 })}>BRYAN LITZ</text>
        <Legend x={60} y={400} v={v} o={1 - ramp(v, T.only - 0.6, 0.4)} rows={[
          { sym: "Δy'", text: "THE JUMP · AN ANGLE, MOA", at: T.adds - 0.3 },
          { sym: <>S<Sub s="g" /></>, text: "STABILITY · FROM MILLER", at: T.adds },
          { sym: "L", text: "LENGTH · CALIBRES", at: T.length },
          { sym: <>W<Sub s="⊥" /></>, text: "SIDE WIND AT THE MUZZLE · mph", at: T.mult },
        ]} />
        <text x={960} y={300} textAnchor="middle" style={mono(ramp(v, T.four - 0.3, 0.5) * (1 - ramp(v, T.only - 0.6, 0.4)), 30, { fontWeight: 600 })}>
          Δy' = (0.01 · 1.71 − 0.0024 · 4.53 + 0.032) · 10.1 = 0.39 MOA
        </text>
        {(() => {
          const L = MathLineKey(AJ, "sg"), Lp = MathLineKey(AJ, "L"), W = MathLineKey(AJ, "w");
          return (
            <>
              <text x={L[0]} y={FY - 70} textAnchor="middle" style={mono(window01(v, T.adds - 0.2, T.notDist, 0.3), 40, { fontWeight: 700 })}>+</text>
              <text x={Lp[0]} y={FY - 70} textAnchor="middle" style={mono(window01(v, T.length - 0.2, T.notDist, 0.3), 40, { fontWeight: 700, fill: C.drag })}>−</text>
              <g opacity={window01(v, T.mult - 0.2, T.notDist, 0.3)}><ArrowA x={W[0] + 70} y={FY - 80} a={Math.PI} len={140} w={4} head={16} /></g>
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
      {/* at 1000 m, to scale: 11 cm up, 3.35 m sideways */}
      <g opacity={tgtO}>
        {(() => {
          const c: P = [1400, 640], s = 250;          // px per metre
          return (
            <>
              <text x={960} y={900} textAnchor="middle" style={label(0.85, { fontSize: 26 })}>AT 1000 m · 4.5 m/s FROM THE RIGHT · TO SCALE</text>
              <circle cx={c[0]} cy={c[1]} r={s * 0.5} fill="none" stroke={C.line} strokeWidth={2} opacity={0.5} />
              <circle cx={c[0]} cy={c[1]} r={s * 0.25} fill="none" stroke={C.line} strokeWidth={1.5} opacity={0.4} />
              <circle cx={c[0]} cy={c[1]} r={6} fill={C.line} opacity={0.6} />
              <g opacity={ramp(v, T.n11 - 0.2, 0.4)}>
                <line x1={c[0]} y1={c[1]} x2={c[0]} y2={c[1] - 0.11 * s} stroke={C.line} strokeWidth={4} filter="url(#glow)" />
                <circle cx={c[0]} cy={c[1] - 0.11 * s} r={9} fill={C.line} filter="url(#glow)" />
                <text x={c[0] + 30} y={c[1] - 40} style={mono(1, 34, { fontWeight: 700 })}>11 cm up</text>
              </g>
              <g opacity={ramp(v, T.itself - 0.2, 0.4)}>
                <line x1={c[0]} y1={c[1] + 60} x2={c[0] - 3.35 * s * ramp(v, T.itself, 1.0, ease)} y2={c[1] + 60} stroke={C.drag} strokeWidth={5} />
                <text x={c[0] - 3.35 * s} y={c[1] + 110} style={mono(1, 32, { fill: C.drag, fontWeight: 600 })}>3.35 m: the wind itself</text>
              </g>
            </>
          );
        })()}
      </g>
      {/* only the wind at the muzzle */}
      <g opacity={lateO}>
        {(() => {
          const X0 = 260, X1 = 1660, Y = 640, xm = (m: number) => X0 + ((X1 - X0) * m) / 1000;
          return (
            <>
              <line x1={X0} y1={Y} x2={X1} y2={Y} stroke={C.line} strokeWidth={3} />
              <circle cx={X0} cy={Y} r={8} fill={C.line} />
              <text x={X0} y={Y + 44} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>MUZZLE</text>
              <text x={X1} y={Y + 44} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>1000 m</text>
              <line x1={xm(300)} y1={Y - 30} x2={xm(300)} y2={Y + 30} stroke={C.line} strokeWidth={2} />
              <text x={xm(300)} y={Y + 44} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>300 m</text>
              {Array.from({ length: 7 }, (_, i) => <ArrowA key={i} x={xm(380 + i * 95) + 40} y={Y - 90} a={Math.PI} len={70} w={3} head={12} />)}
              <text x={960} y={420} textAnchor="middle" style={label(ramp(v, T.only, 0.4), { fontSize: 28 })}>ONLY THE WIND AT THE MUZZLE COUNTS</text>
              <text x={960} y={800} textAnchor="middle" style={label(ramp(v, T.noJump - 0.3, 0.4), { fontSize: 30 })}>A WIND FROM 300 m: NO JUMP</text>
            </>
          );
        })()}
      </g>
    </Scene>
  );
};

const MathLineKey = (items: Item[], key: string) => keyPos(items, FX, FY, FS, key);
