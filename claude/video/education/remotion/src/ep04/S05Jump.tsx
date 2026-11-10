// Scene 5 — aerodynamic jump. Top view at the muzzle: the side wind tilts v_air (episode 3), the nose leaves along the
// barrel, so it is off the air flow; the air pushes it away from the wind (left); by the rule the nose goes up. Side
// view: the nose swings and settles, the path keeps a tiny upward angle. Litz's second formula; an angle, so a straight
// wedge; to scale at 1000 m: 11 cm up against 3.35 m sideways. A wind that starts at 300 m gives no jump.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, mono, seeded } from "../lib/draw";
import { ArrowA, CornerRule, Item, MathLine, Scene, SpinBullet, Title, k_, keyPos, sub, t_, useVoiceTime } from "./kit";
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

const M: P = [960, 900];                       // the muzzle, top view
const VL = 560, TILT = (0.33 * 20 * Math.PI) / 180;
const S0: P = [220, 700];                      // the muzzle, side view

export const S05Jump: React.FC = () => {
  const v = useVoiceTime();
  // ---- top view
  const topO = ramp(v, T.remember - 0.4, 0.5) * (1 - ramp(v, T.swings - 0.5, 0.5));
  const vairP = ramp(v, T.tilted - 0.2, 0.8, ease);
  const bulO = ramp(v, T.leaves - 0.2, 0.5);
  const gapHot = ramp(v, T.first - 0.2, 0.5);
  const pushP = ramp(v, T.pushes - 0.1, 0.5);
  const noseDeg = 90 + 7 * ramp(v, T.left - 0.2, 0.8);
  // ---- side view: the swings and the tilt
  const sideO = ramp(v, T.swings - 0.4, 0.5) * (1 - ramp(v, T.formula - 0.4, 0.4));
  const ts = Math.max(0, v - T.swings);
  const jumpK = ramp(v, T.last - 0.2, 2.6, ease);
  const jumpDeg = 4 * jumpK;                                 // drawn
  const bx = lerp(S0[0] + 120, 1500, ramp(v, T.swings - 0.2, 9, (x) => x));
  const wob = 14 * Math.exp(-0.55 * ts) * Math.sin(5.5 * ts);
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
      {/* the wind, from the right */}
      <g opacity={ramp(v, T.sideWind - 0.2, 0.6) * (1 - ramp(v, T.swings - 0.5, 0.5))}>
        {STREAKS.map((s, i) => {
          const x = ((((s.x - 150 * v) % 2100) + 2100) % 2100) - 100;
          return <line key={i} x1={x} y1={s.y} x2={x + s.l} y2={s.y} stroke={C.line} strokeOpacity={s.a} strokeWidth={2} />;
        })}
        <ArrowA x={1820} y={420} a={Math.PI} len={260} w={5} head={22} />
        <text x={1820} y={390} textAnchor="end" style={label(0.8, { fontSize: 22 })}>SIDE WIND · FROM THE RIGHT</text>
        <g opacity={window01(v, T.muzzle - 0.2, T.remember, 0.3)}>
          <rect x={930} y={880} width={60} height={140} fill="none" stroke={C.line} strokeWidth={3} />
          <text x={1010} y={900} style={label(1, { fontSize: 24 })}>RIGHT AT THE MUZZLE</text>
        </g>
      </g>

      {/* ---------------- top view at the muzzle */}
      <g opacity={topO}>
        <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>TOP VIEW · DOWNRANGE IS UP</text>
        <path d={`M940,1060 L940,${M[1]} L980,${M[1]} L980,1060`} stroke={C.line} strokeWidth={3} fill="none" />
        <line x1={M[0]} y1={M[1]} x2={M[0]} y2={M[1] - VL - 80} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
        <text x={M[0] - 16} y={M[1] - VL - 90} textAnchor="end" style={label(0.6, { fontSize: 18 })}>THE BARREL'S LINE</text>
        <g opacity={vairP} filter="url(#glow)">
          <line x1={M[0]} y1={M[1]} x2={M[0] + VL * Math.sin(TILT) * vairP} y2={M[1] - VL * Math.cos(TILT)} stroke={C.line} strokeWidth={5} />
        </g>
        <g opacity={vairP}>
          <text x={M[0] + 90} y={M[1] - VL + 40} style={mono(1, 36)}>v<tspan fontSize={22} dy={8}>air</tspan></text>
          <text x={M[0] + 90} y={M[1] - VL + 80} style={label(ramp(v, T.third - 0.2, 0.4) * 0.85, { fontSize: 20 })}>0.33° · DRAWN ×20</text>
        </g>
        <g opacity={bulO}>
          <SpinBullet id="j" cx={M[0]} cy={M[1] - 220} cal={40} deg={noseDeg} spin={v} marks={0.8} />
          <g opacity={gapHot}>
            {(() => {
              const r = 330, a0 = -Math.PI / 2 - ((noseDeg - 90) * Math.PI) / 180, a1 = -Math.PI / 2 + TILT, c: P = [M[0], M[1] - 220];
              const pts: P[] = Array.from({ length: 13 }, (_, i) => { const a = lerp(a0, a1, i / 12); return [c[0] + r * Math.cos(a), c[1] + r * Math.sin(a)]; });
              return <Pen pts={pts} p={1} w={4} tip={false} />;
            })()}
            <text x={M[0] + 110} y={M[1] - 250} style={label(1, { fontSize: 24 })}>THE NOSE IS OFF THE AIR FLOW</text>
          </g>
          {pushP > 0 && (
            <g opacity={pushP} filter="url(#glow)">
              <line x1={M[0] - 20} y1={M[1] - 330} x2={M[0] - 20 - 150 * pushP} y2={M[1] - 330} stroke={C.drag} strokeWidth={6} strokeLinecap="round" />
              <path d={`M${M[0] - 152 - 20 * pushP},${M[1] - 344} L${M[0] - 170 * pushP - 20},${M[1] - 330} L${M[0] - 152 - 20 * pushP},${M[1] - 316}`} stroke={C.drag} strokeWidth={6} fill="none" />
            </g>
          )}
          <text x={M[0] - 200} y={M[1] - 390} textAnchor="end" style={label(pushP, { fontSize: 22, fill: C.drag })}>AWAY FROM THE WIND</text>
          <text x={M[0] - 200} y={M[1] - 360} textAnchor="end" style={label(ramp(v, T.up - 0.2, 0.4), { fontSize: 22 })}>PUSH LEFT → THE NOSE GOES UP</text>
        </g>
      </g>

      {/* ---------------- side view: the swings die out, the tilt stays */}
      <g opacity={sideO}>
        <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>SIDE VIEW</text>
        <path d={`M60,${S0[1] - 22} L${S0[0]},${S0[1] - 22} L${S0[0]},${S0[1] + 22} L60,${S0[1] + 22}`} stroke={C.line} strokeWidth={3} fill="none" />
        <line x1={S0[0]} y1={S0[1]} x2={1800} y2={S0[1]} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" />
        <text x={1800} y={S0[1] + 40} textAnchor="end" style={label(0.55, { fontSize: 18 })}>WITHOUT THE JUMP</text>
        {(() => {
          const a = (jumpDeg * Math.PI) / 180, yAt = (x: number) => S0[1] - (x - S0[0]) * Math.tan(a);
          return (
            <>
              <line x1={S0[0]} y1={S0[1]} x2={1800} y2={yAt(1800)} stroke={C.line} strokeWidth={3} opacity={0.4 + 0.6 * jumpK} />
              <SpinBullet id="s" cx={bx} cy={yAt(bx)} cal={34} deg={jumpDeg + wob} spin={v} marks={0.7} />
              <text x={1800} y={yAt(1800) - 24} textAnchor="end" style={label(ramp(v, T.tiltedUp - 0.2, 0.5), { fontSize: 24 })}>THE PATH: TILTED UP BY A TINY ANGLE · DRAWN ×600</text>
            </>
          );
        })()}
        <text x={960} y={250} textAnchor="middle" style={label(window01(v, T.swings - 0.2, T.last, 0.3), { fontSize: 26 })}>THE NOSE SWINGS UP AND AROUND · THE SWINGS DIE OUT</text>
        <text x={960} y={250} textAnchor="middle" style={label(ramp(v, T.last - 0.2, 0.4), { fontSize: 26 })}>WHILE THEY LAST, THE AIR PUSHES THE BULLET UP</text>
        {/* the nose, seen from behind: a shrinking circle */}
        <g opacity={ramp(v, T.swings - 0.2, 0.5)}>
          <circle cx={1560} cy={860} r={110} fill="none" stroke={C.line} strokeWidth={2.5} />
          {(() => {
            const pts: P[] = [];
            for (let i = 0; i <= 140; i++) {
              const s = (i / 140) * Math.min(ts, 6), r = 80 * Math.exp(-0.55 * s);
              pts.push([1560 + r * Math.sin(5.5 * s), 860 - r * Math.cos(5.5 * s)]);
            }
            const e = pts[pts.length - 1];
            return (<><Pen pts={pts} p={1} w={2} tip={false} opacity={0.6} /><circle cx={e[0]} cy={e[1]} r={8} fill={C.line} filter="url(#glow)" /></>);
          })()}
          <text x={1560} y={1010} textAnchor="middle" style={label(0.6, { fontSize: 16 })}>THE NOSE, FROM BEHIND</text>
        </g>
      </g>

      {/* ---------------- Litz's second formula */}
      <g opacity={fO}>
        <MathLine items={AJ} cx={FX} y={FY} size={FS} reveal={ramp(v, T.formula, 1.6)} active={active} />
        <text x={960} y={290} textAnchor="middle" style={label(window01(v, T.formula + 0.6, T.adds - 0.3, 0.4) * 0.8, { fontSize: 22 })}>BRYAN LITZ</text>
        <text x={960} y={290} textAnchor="middle" style={label(ramp(v, T.adds - 0.3, 0.4) * 0.6, { fontSize: 20 })}>W⊥ — SIDE WIND AT THE MUZZLE, mph · Δy' — AN ANGLE, MOA</text>
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
              <text x={960} y={420} textAnchor="middle" style={label(ramp(v, T.angle - 0.2, 0.4), { fontSize: 28 })}>NOT A DISTANCE: AN ANGLE, THE SAME AT EVERY DISTANCE</text>
              <text x={960} y={470} textAnchor="middle" style={label(ramp(v, T.straight - 0.2, 0.4) * 0.8, { fontSize: 24 })}>SO IT GROWS IN A STRAIGHT LINE</text>
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
              <text x={960} y={400} textAnchor="middle" style={label(0.85, { fontSize: 26 })}>AT 1000 m · 4.5 m/s FROM THE RIGHT · TO SCALE</text>
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
