// Scene 2 — why a bullet spins. The air's push sits in front of the centre of mass, so a nose that turns off the path
// is turned further, and the bullet would tumble. An arrow has feathers; a bullet spins instead. The spinning top's
// strange rule, then the same rule for the bullet seen from behind: a push moves the nose a quarter turn further, the
// way it spins. The clock face shrinks into the corner for scenes 4 and 5.
import React from "react";
import { C, clamp01, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, seeded } from "../lib/draw";
import { BLEN, CG_F, CP_F, ClockBack, Scene, SpinBullet, Title, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 2;
const T = {
  why: cue(N, "why"), air: cue(N, "The air pushes"), notMid: cue(N, "not in the middle"), closer: cue(N, "closer"), center: cue(N, "center of mass"),
  off: cue(N, "turns a little off"), more: cue(N, "even more"), tumble: cue(N, "tumble"), arrow: cue(N, "arrow"), feathers: cue(N, "feathers"),
  none: cue(N, "has none"), spins: cue(N, "so it spins"), strange: cue(N, "strange rule"), top: cue(N, "spinning top"), push: cue(N, "Push it"),
  fall: cue(N, "doesn't fall"), side: cue(N, "moves to the side"), bullet: cue(N, "For the bullet"), behind: cue(N, "from behind"),
  clockwise: cue(N, "clockwise"), left: cue(N, "Push the nose"), up: cue(N, "goes up"), pushUp: cue(N, "Push it up"), right: cue(N, "to the right"),
  always: cue(N, "Always"), remember: cue(N, "Remember"),
};

const CX = 960, CY = 560, CAL = 80;
const STREAKS = (() => { const r = seeded(42); return Array.from({ length: 40 }, () => ({ x: r() * 2000, y: 300 + r() * 520, l: 30 + r() * 50 })); })();

/** A curved arrow along pts, with a head at the end: "it turns this way". */
const ArcHead: React.FC<{ pts: P[] }> = ({ pts }) => {
  const e = pts[pts.length - 1], d = pts[pts.length - 3], ang = Math.atan2(e[1] - d[1], e[0] - d[0]), h = 16;
  return (
    <g opacity={0.9}>
      <Pen pts={pts} p={1} w={3.5} tip={false} />
      <path d={`M${e[0] - h * Math.cos(ang - 0.5)},${e[1] - h * Math.sin(ang - 0.5)} L${e[0]},${e[1]} L${e[0] - h * Math.cos(ang + 0.5)},${e[1] - h * Math.sin(ang + 0.5)}`} stroke={C.line} strokeWidth={3.5} fill="none" strokeLinejoin="round" />
    </g>
  );
};

export const S02WhySpin: React.FC = () => {
  const v = useVoiceTime();

  // ---- A: the air's push in front of the centre of mass; a tip grows into a tumble
  const aO = ramp(v, T.air - 0.6, 0.6) * (1 - ramp(v, T.arrow - 0.6, 0.5));
  const tip = lerp(0, 6, ramp(v, T.off - 0.1, 0.8)) + lerp(0, 16, ramp(v, T.more - 0.2, 1.0));
  const tumbleK = ramp(v, T.tumble - 0.6, 1.4);
  const deg = tip + tumbleK * 200;
  const cpOff = (CP_F - CG_F) * BLEN * CAL;
  const cpHot = window01(v, T.notMid - 0.2, T.center + 0.6, 0.3);
  const lift = clamp01(tip / 22);

  // ---- B: an arrow rights itself; the bullet spins instead
  const bO = ramp(v, T.arrow - 0.3, 0.5) * (1 - ramp(v, T.spins - 0.6, 0.4));
  const ta = Math.max(0, v - (T.feathers + 0.2));
  const arrowDeg = v < T.feathers + 0.2 ? lerp(0, 14, ramp(v, T.arrow + 0.2, 0.6)) : 14 * Math.exp(-1.6 * ta) * Math.cos(4.2 * ta);
  const b2O = ramp(v, T.spins - 0.4, 0.5) * (1 - ramp(v, T.strange - 0.4, 0.5));
  const marks = ramp(v, T.spins - 0.2, 0.5);
  const spin = Math.max(0, v - T.spins) * 1.5;

  // ---- C: the spinning top, from above
  const cO = ramp(v, T.top - 0.6, 0.6) * (1 - ramp(v, T.bullet - 0.5, 0.5));
  const topSpin = (v - T.top) * 1.2;
  const pushP = ramp(v, T.push - 0.1, 0.5);
  const ghost = window01(v, T.fall - 0.2, T.side + 0.3, 0.3);
  const moveP = ramp(v, T.side - 0.1, 1.0);
  const TC: P = [960, lerp(560, 680, moveP)];

  // ---- D: the bullet from behind, then into the corner
  const dO = ramp(v, T.bullet - 0.3, 0.6);
  const second = v >= T.pushUp - 0.3;
  const push = second ? "up" : "left";
  const pP = second ? ramp(v, T.pushUp, 0.5) : ramp(v, T.left + 0.4, 0.6);
  const nP = second ? ramp(v, T.right - 0.3, 0.8) : ramp(v, T.up - 0.3, 0.8);
  const toCorner = ramp(v, T.remember + 0.2, 1.2);
  const ccx = lerp(960, 1760, toCorner), ccy = lerp(580, 170, toCorner), cr = lerp(230, 62, toCorner);

  return (
    <Scene n={N}>
      <Title text="WHY DOES A BULLET SPIN?" o={ramp(v, T.why - 0.6, 0.6) * (1 - ramp(v, T.top - 0.6, 0.5))} />

      {/* ---------------- A */}
      <g opacity={aO}>
        {STREAKS.map((s, i) => {
          const x = ((((s.x - 520 * v) % 2000) + 2000) % 2000) - 40;
          return <line key={i} x1={x} y1={s.y} x2={x + s.l} y2={s.y} stroke={C.line} strokeOpacity={0.14} strokeWidth={2} />;
        })}
        <line x1={300} y1={CY} x2={1640} y2={CY} stroke={C.dim} strokeWidth={2} strokeDasharray="10 10" />
        <text x={1640} y={CY + 40} textAnchor="end" style={label(0.6, { fontSize: 20 })}>THE PATH</text>
        <g opacity={1 - 0.75 * tumbleK}>
          <SpinBullet id="a" cx={CX} cy={CY} cal={CAL} deg={deg} />
        </g>
        <g transform={`rotate(${-deg},${CX},${CY})`} opacity={1 - tumbleK}>
          {/* centre of mass */}
          <g opacity={ramp(v, T.center - 0.3, 0.5)}>
            <circle cx={CX} cy={CY} r={12} fill="none" stroke={C.line} strokeWidth={3} />
            <path d={`M${CX - 12},${CY} L${CX + 12},${CY} M${CX},${CY - 12} L${CX},${CY + 12}`} stroke={C.line} strokeWidth={2} />
          </g>
          {/* the air's push */}
          <g opacity={ramp(v, T.notMid - 0.2, 0.5)}>
            <circle cx={CX + cpOff} cy={CY} r={9} fill={C.drag} filter={cpHot > 0.3 ? "url(#glow)" : undefined} />
            {lift > 0.02 && (
              <g filter="url(#glow)">
                <line x1={CX + cpOff} y1={CY} x2={CX + cpOff} y2={CY - 150 * lift} stroke={C.drag} strokeWidth={6} strokeLinecap="round" />
                <path d={`M${CX + cpOff - 14},${CY - 150 * lift + 18} L${CX + cpOff},${CY - 150 * lift} L${CX + cpOff + 14},${CY - 150 * lift + 18}`} stroke={C.drag} strokeWidth={6} fill="none" />
              </g>
            )}
          </g>
        </g>
        <g opacity={(1 - tumbleK) * (1 - ramp(v, T.off - 0.3, 0.4))}>
          <text x={CX + cpOff} y={CY - 90} textAnchor="middle" style={label(ramp(v, T.notMid, 0.4), { fontSize: 22, fill: C.drag })}>THE AIR PUSHES HERE</text>
          <text x={CX + cpOff} y={CY - 62} textAnchor="middle" style={label(ramp(v, T.closer, 0.4) * 0.75, { fontSize: 18 })}>CLOSER TO THE NOSE</text>
          <text x={CX - 40} y={CY + 110} textAnchor="middle" style={label(ramp(v, T.center, 0.4), { fontSize: 22 })}>CENTRE OF MASS</text>
          <line x1={CX - 20} y1={CY + 84} x2={CX - 4} y2={CY + 20} stroke={C.line} strokeWidth={1.5} opacity={ramp(v, T.center, 0.4) * 0.6} />
        </g>
        <text x={960} y={250} textAnchor="middle" style={label(window01(v, T.off - 0.2, T.tumble - 0.2, 0.3), { fontSize: 28 })}>A LITTLE OFF THE PATH → THE AIR TURNS IT MORE</text>
        <text x={960} y={250} textAnchor="middle" style={label(window01(v, T.tumble - 0.2, T.arrow, 0.3), { fontSize: 30, fill: C.drag })}>IT WOULD TUMBLE</text>
      </g>

      {/* ---------------- B: the arrow, then the bullet spins */}
      <g opacity={bO}>
        {/* the bullet, as we left it: the push in front of the centre of mass turns it further */}
        {(() => {
          const cy = 380, d = 10, a = (-d * Math.PI) / 180, off = (CP_F - CG_F) * BLEN * 60;
          const cp: P = [CX + off * Math.cos(a), cy + off * Math.sin(a)];
          const arc: P[] = Array.from({ length: 13 }, (_, i) => { const q = a - 0.06 - (i / 12) * 0.36; return [CX + 200 * Math.cos(q), cy + 200 * Math.sin(q)]; });
          return (
            <g>
              <line x1={420} y1={cy} x2={1500} y2={cy} stroke={C.dim} strokeWidth={2} strokeDasharray="10 10" />
              <SpinBullet id="cmpb" cx={CX} cy={cy} cal={60} deg={d} />
              <circle cx={CX} cy={cy} r={10} fill="none" stroke={C.line} strokeWidth={3} />
              <circle cx={cp[0]} cy={cp[1]} r={8} fill={C.drag} />
              <g filter="url(#glow)">
                <line x1={cp[0]} y1={cp[1]} x2={cp[0]} y2={cp[1] - 100} stroke={C.drag} strokeWidth={6} strokeLinecap="round" />
                <path d={`M${cp[0] - 13},${cp[1] - 84} L${cp[0]},${cp[1] - 100} L${cp[0] + 13},${cp[1] - 84}`} stroke={C.drag} strokeWidth={6} fill="none" />
              </g>
              <ArcHead pts={arc} />
              <text x={CX} y={cy + 110} textAnchor="middle" style={label(0.95, { fontSize: 24 })}>A BULLET: THE PUSH IN FRONT OF THE CENTRE → IT TURNS FURTHER</text>
            </g>
          );
        })()}
        {/* the arrow: the feathers put the push behind the centre of mass, so it turns back */}
        <g opacity={ramp(v, T.arrow - 0.3, 0.5)}>
          {(() => {
            const cy = 760, cg = 1010, cpX = 680;
            const a = (-arrowDeg * Math.PI) / 180;
            const rot = (x: number): P => [cg + (x - cg) * Math.cos(a), cy + (x - cg) * Math.sin(a)];
            const cp = rot(cpX), pushK = ramp(v, T.feathers - 0.1, 0.4) * clamp01(arrowDeg / 6);
            const arc: P[] = Array.from({ length: 13 }, (_, i) => { const q = Math.PI + a - 0.1 + (i / 12) * 0.3; return [cg + 420 * Math.cos(q), cy + 420 * Math.sin(q)]; });
            return (
              <>
                <line x1={420} y1={cy} x2={1500} y2={cy} stroke={C.dim} strokeWidth={2} strokeDasharray="10 10" />
                <g transform={`rotate(${-arrowDeg},${cg},${cy})`}>
                  <line x1={640} y1={cy} x2={1290} y2={cy} stroke={C.line} strokeWidth={5} />
                  <path d={`M1260,${cy - 22} L1300,${cy} L1260,${cy + 22}`} stroke={C.line} strokeWidth={5} fill="none" strokeLinejoin="round" />
                  {[0, 1, 2].map((i) => (
                    <g key={i} filter={ramp(v, T.feathers, 0.4) > 0.5 ? "url(#glow)" : undefined}>
                      <path d={`M${650 + i * 24},${cy} L${620 + i * 24},${cy - 40}`} stroke={C.line} strokeWidth={3} />
                      <path d={`M${650 + i * 24},${cy} L${620 + i * 24},${cy + 40}`} stroke={C.line} strokeWidth={3} />
                    </g>
                  ))}
                  <circle cx={cg} cy={cy} r={10} fill={C.bg} stroke={C.line} strokeWidth={3} />
                </g>
                <g opacity={ramp(v, T.feathers - 0.2, 0.4)}>
                  <circle cx={cp[0]} cy={cp[1]} r={8} fill={C.drag} />
                  {pushK > 0.05 && (
                    <g filter="url(#glow)">
                      <line x1={cp[0]} y1={cp[1]} x2={cp[0]} y2={cp[1] - 100 * pushK} stroke={C.drag} strokeWidth={6} strokeLinecap="round" />
                      <path d={`M${cp[0] - 13},${cp[1] - 100 * pushK + 16} L${cp[0]},${cp[1] - 100 * pushK} L${cp[0] + 13},${cp[1] - 100 * pushK + 16}`} stroke={C.drag} strokeWidth={6} fill="none" />
                    </g>
                  )}
                  {pushK > 0.3 && <ArcHead pts={arc} />}
                </g>
                <text x={CX} y={cy + 120} textAnchor="middle" style={label(ramp(v, T.feathers, 0.4) * 0.95, { fontSize: 24 })}>AN ARROW: FEATHERS PUT THE PUSH BEHIND THE CENTRE → IT TURNS BACK</text>
              </>
            );
          })()}
        </g>
        <text x={960} y={200} textAnchor="middle" style={label(0.6, { fontSize: 20 })}>THE CROSS-CIRCLE: THE CENTRE OF MASS · CORAL: WHERE THE AIR PUSHES</text>
      </g>
      <g opacity={b2O}>
        <SpinBullet id="b" cx={CX} cy={CY} cal={CAL} spin={spin} marks={marks} />
        {marks > 0 && (() => {
          const xb = CX - CG_F * BLEN * CAL + 30, pts: P[] = [];
          for (let i = 0; i <= 26; i++) { const a = -Math.PI * 0.85 + (i / 26) * Math.PI * 1.6; pts.push([xb + 26 * Math.cos(a), CY + 70 * Math.sin(a)]); }
          return <Pen pts={pts} p={marks} w={3} opacity={0.8} />;
        })()}
        <text x={960} y={250} textAnchor="middle" style={label(ramp(v, T.none - 0.1, 0.4), { fontSize: 28 })}>
          A BULLET HAS NO FEATHERS<tspan opacity={marks}> · SO IT SPINS</tspan>
        </text>
      </g>

      {/* ---------------- C: the top, from above */}
      <g opacity={cO}>
        <text x={960} y={250} textAnchor="middle" style={label(0.85, { fontSize: 28 })}>A SPINNING TOP, FROM ABOVE</text>
        <circle cx={TC[0]} cy={TC[1]} r={170} fill="none" stroke={C.line} strokeWidth={3} />
        {Array.from({ length: 8 }, (_, i) => {
          const a = topSpin * Math.PI * 2 + (i * Math.PI) / 4;
          return <line key={i} x1={TC[0] + 46 * Math.cos(a)} y1={TC[1] + 46 * Math.sin(a)} x2={TC[0] + 160 * Math.cos(a)} y2={TC[1] + 160 * Math.sin(a)} stroke={C.line} strokeWidth={1.5} opacity={0.35} />;
        })}
        <circle cx={TC[0]} cy={TC[1]} r={40} fill={C.bg} stroke={C.line} strokeWidth={3} />
        <circle cx={TC[0]} cy={TC[1]} r={10} fill={C.line} filter="url(#glow)" />
        {/* spin direction */}
        {(() => {
          const pts: P[] = [];
          for (let i = 0; i <= 24; i++) { const a = -2.4 + (i / 24) * 1.6; pts.push([960 + 215 * Math.cos(a), 560 + 215 * Math.sin(a)]); }
          return <Pen pts={pts} p={1} w={3} opacity={0.7} />;
        })()}
        {/* the push, from the left */}
        <g opacity={pushP} filter="url(#glow)">
          <line x1={640} y1={560} x2={lerp(640, 900, pushP)} y2={560} stroke={C.drag} strokeWidth={7} strokeLinecap="round" />
          <path d={`M${lerp(640, 900, pushP) - 22},538 L${lerp(640, 900, pushP)},560 L${lerp(640, 900, pushP) - 22},582`} stroke={C.drag} strokeWidth={7} fill="none" />
          <text x={640} y={530} style={label(1, { fontSize: 24, fill: C.drag })}>PUSH</text>
        </g>
        <g opacity={ghost}>
          <circle cx={1260} cy={560} r={40} fill="none" stroke={C.line} strokeWidth={2} strokeDasharray="6 6" />
          <path d="M1240,540 L1280,580 M1280,540 L1240,580" stroke={C.drag} strokeWidth={4} />
          <line x1={1000} y1={560} x2={1214} y2={560} stroke={C.line} strokeWidth={2} strokeDasharray="6 6" opacity={0.5} />
          <text x={1260} y={490} textAnchor="middle" style={label(0.8, { fontSize: 20 })}>NOT THE WAY YOU PUSHED</text>
        </g>
        <g opacity={ramp(v, T.side, 0.5)}>
          <line x1={960} y1={600} x2={960} y2={TC[1] - 50} stroke={C.line} strokeWidth={2} strokeDasharray="6 6" opacity={0.6} />
          <text x={1150} y={TC[1] + 8} style={label(0.95, { fontSize: 24 })}>IT MOVES TO THE SIDE</text>
        </g>
      </g>

      {/* ---------------- D: the bullet from behind */}
      <g opacity={dO}>
        <text x={960} y={250} textAnchor="middle" style={label(ramp(v, T.behind - 0.2, 0.4) * (1 - toCorner), { fontSize: 28 })}>THE BULLET, FROM BEHIND · IT SPINS CLOCKWISE</text>
        <ClockBack cx={ccx} cy={ccy} r={cr} spin={(v - T.bullet) * 0.8} push={push} pushP={pP} noseP={nP} quarter={ramp(v, T.always - 0.2, 0.8)} nums={toCorner < 0.5} />
        <g opacity={1 - toCorner}>
          <text x={680} y={588} textAnchor="end" style={label(pP * (second ? 0 : 1), { fontSize: 22, fill: C.drag })}>PUSH TO 9</text>
          <text x={960} y={292} textAnchor="middle" style={label(nP * (second ? 0 : 1), { fontSize: 22 })}>THE NOSE GOES TO 12</text>
          <text x={960} y={300} textAnchor="middle" style={label(pP * (second ? 1 : 0), { fontSize: 22, fill: C.drag })}>PUSH TO 12</text>
          <text x={1250} y={588} style={label(nP * (second ? 1 : 0), { fontSize: 22 })}>THE NOSE GOES TO 3</text>
          <text x={960} y={920} textAnchor="middle" style={label(ramp(v, T.always - 0.2, 0.5), { fontSize: 30 })}>A PUSH → A QUARTER TURN FURTHER, THE WAY IT SPINS</text>
        </g>
        <text x={1760} y={290} textAnchor="middle" style={label(toCorner * 0.6, { fontSize: 16 })}>FROM BEHIND</text>
      </g>
    </Scene>
  );
};
