// Scene 3 — stability. A balance: the spin against the air's turning push; S_g > 1, the spin wins. The full theory needs
// the whole shape; the program uses Don Miller's rule, fitted to it. Each term acted out on the bullet: weight, diameter,
// length (twice — a longer lever for the air), the twist (squared), two small fixes ≈ 1. Our bullet: 1.7 on a gauge.
// Without the length or the twist, both spin effects are skipped, with no warning.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Bracket, Bullet, P, label, mono, seeded } from "../lib/draw";
import { BLEN, CG_F, CP_F, Card, ClockBack, Gauge, Item, MathLine, Scene, SpinBullet, k_, keyPos, sub, sup, t_, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 3;
const T = {
  turn: cue(N, "turn the bullet"), keeps: cue(N, "keeps it straight"), strong: cue(N, "How strong"), stability: cue(N, "stability"), above: cue(N, "Above"),
  exactly: cue(N, "exactly"), miller: cue(N, "simple rule"), fitted: cue(N, "fitted"), weight: cue(N, "weight"), diameter: cue(N, "diameter"),
  length: cue(N, "length"), twice: cue(N, "twice"), long: cue(N, "A long bullet"), twist: cue(N, "And the twist"), oneTurn: cue(N, "one full turn"),
  squared: cue(N, "squared"), faster: cue(N, "A faster twist"), fixes: cue(N, "two small fixes"), standard: cue(N, "standard air"), our: cue(N, "Our bullet"),
  good: cue(N, "Above", 2), empty: cue(N, "if the length"), skips: cue(N, "skips"), warning: cue(N, "no warning"),
};

const FX = 960, FY = 210, FS = 50;
const MILLER: Item[] = [
  { toks: [t_("S"), sub("g"), t_(" = ")] },
  { num: [t_("30 · "), k_("w", "w")], den: [k_("t", "t"), sup("2", "t"), t_(" · "), k_("d", "d"), sup("3", "d"), t_(" · "), k_("L", "L"), t_(" · (1 + "), k_("L", "L"), sup("2", "L"), t_(")")] },
  { toks: [t_(" · ("), k_("V₀", "V"), t_("/2800)", "V"), sup("1/3", "V"), t_(" · ")] },
  { num: [k_("T", "T"), t_(" + 460", "T")], den: [t_("519", "T")] },
  { toks: [t_(" · ")] },
  { num: [t_("29.92", "P")], den: [k_("P", "P")] },
];
const POS = (key: string, nth = 1) => keyPos(MILLER, FX, FY, FS, key, nth);
const SCRIBBLE = (() => { const r = seeded(31); return Array.from({ length: 26 }, () => ({ x: 380 + r() * 1160, y: 760 + r() * 200, w: 60 + r() * 160 })); })();

export const S03Stability: React.FC = () => {
  const v = useVoiceTime();
  const formO = ramp(v, T.miller - 0.3, 0.5);
  const active =
    v >= T.empty - 0.2 ? null :
    v >= T.fixes - 0.2 ? null :
    v >= T.twist - 0.2 ? "t" : v >= T.length - 0.2 ? "L" : v >= T.diameter - 0.2 ? "d" : v >= T.weight - 0.2 ? "w" : null;
  const lit = v >= T.fixes - 0.2 && v < T.our - 0.2 ? new Set(["V", "T", "P"]) : undefined;
  const formDim = 1 - 0.65 * ramp(v, T.skips - 0.2, 0.6);

  // ---- the balance
  const balO = ramp(v, -0.4, 0.6) * (1 - ramp(v, T.exactly - 0.5, 0.5));
  const tilt = lerp(0, -9, ramp(v, T.keeps - 0.3, 1.0, ease));
  // ---- the beats
  const beat = (a: number, b: number) => window01(v, a - 0.3, b, 0.35);
  const theoryO = beat(T.exactly, T.miller + 0.6);
  const wO = beat(T.weight, T.diameter), dO = beat(T.diameter, T.length), lO = beat(T.length, T.twist), tO = beat(T.twist, T.fixes);
  const turnK = ramp(v, T.diameter - 0.2, 0.6) * (1 - ramp(v, T.length - 0.4, 0.4));
  const pitch = lerp(32, 20, ramp(v, T.faster - 0.1, 1.2, ease));          // calibres per turn (drawn)
  const gaugeO = ramp(v, T.our - 0.3, 0.5) * (1 - ramp(v, T.skips - 0.3, 0.5));
  const sgShown = lerp(0, 1.708, ramp(v, T.our - 0.1, 1.2, ease));

  return (
    <Scene n={N}>
      {/* ---------------- the balance: spin against the air's turning push */}
      <g opacity={balO}>
        <path d="M960,700 L920,770 L1000,770 Z" fill="none" stroke={C.line} strokeWidth={3} />
        <g transform={`rotate(${tilt},960,700)`}>
          <line x1={600} y1={700} x2={1320} y2={700} stroke={C.line} strokeWidth={4} />
          {/* the bullet from behind, spinning: the clock face of scene 2 */}
          <ClockBack cx={640} cy={644} r={54} spin={v * 0.8} nums={false} />
          <text x={640} y={730} textAnchor="middle" style={label(0.9, { fontSize: 22 })}>THE SPIN</text>
          <g transform="translate(1280,640)">
            <line x1={0} y1={40} x2={0} y2={-40} stroke={C.drag} strokeWidth={6} />
            <path d="M-14,-24 L0,-42 L14,-24" stroke={C.drag} strokeWidth={6} fill="none" />
            <text y={100} textAnchor="middle" style={label(0.9, { fontSize: 22, fill: C.drag })}>THE AIR, TURNING IT</text>
          </g>
        </g>
        <text x={960} y={300} textAnchor="middle" style={label(ramp(v, T.strong - 0.2, 0.4) * 0.85, { fontSize: 28 })}>HOW STRONG IS THE SPIN, COMPARED WITH THE AIR?</text>
        <text x={960} y={400} textAnchor="middle" style={mono(ramp(v, T.stability - 0.2, 0.5), 44, { fontWeight: 600 })}>
          S<tspan fontSize={26} dy={8}>g</tspan><tspan dy={-8}> = the spin / the air's turn</tspan>
        </text>
        <text x={960} y={470} textAnchor="middle" style={label(ramp(v, T.above - 0.2, 0.4), { fontSize: 30 })}>ABOVE 1: THE SPIN WINS</text>
      </g>

      {/* ---------------- the full theory: the whole shape and a lot of math */}
      <g opacity={theoryO}>
        <Bullet x0={960 - BLEN * 60} y={600} cal={120} w={3} />
        {Array.from({ length: 12 }, (_, i) => (
          <line key={i} x1={960 - BLEN * 60 + i * 45} y1={510} x2={960 - BLEN * 60 + i * 45} y2={530} stroke={C.line} strokeWidth={1.5} opacity={0.5} />
        ))}
        {SCRIBBLE.map((s, i) => <line key={i} x1={s.x} y1={s.y} x2={s.x + s.w} y2={s.y} stroke={C.line} strokeWidth={2} opacity={0.25} />)}
        <text x={960} y={440} textAnchor="middle" style={label(0.8, { fontSize: 24 })}>THE FULL THEORY: THE WHOLE SHAPE, AND A LOT OF MATH</text>
      </g>

      {/* ---------------- Miller's rule */}
      <g opacity={formO * formDim}>
        <MathLine items={MILLER} cx={FX} y={FY} size={FS} reveal={ramp(v, T.miller, 2.0)} active={active} lit={lit} />
        <text x={960} y={340} textAnchor="middle" style={label(window01(v, T.miller + 1.0, T.weight - 0.2, 0.4) * 0.8, { fontSize: 22 })}>
          DON MILLER · A SIMPLE RULE, FITTED TO THE FULL THEORY
        </text>
        <text x={960} y={340} textAnchor="middle" style={label(window01(v, T.weight - 0.2, T.fixes - 0.2, 0.4) * 0.6, { fontSize: 20 })}>
          w — GRAINS · d — INCHES · t AND L — IN CALIBRES
        </text>
      </g>
      {/* the two small fixes: ≈ 1 */}
      <g opacity={window01(v, T.fixes, T.our - 0.2, 0.4)}>
        {(["V", "T", "P"] as const).map((k) => {
          const p = POS(k);
          return <text key={k} x={p[0]} y={FY + 120} textAnchor="middle" style={mono(ramp(v, T.standard - 0.2, 0.4), 30, { fontWeight: 600 })}>≈ 1</text>;
        })}
        <text x={POS("V")[0]} y={FY + 160} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>VELOCITY</text>
        <text x={(POS("T")[0] + POS("P")[0]) / 2} y={FY + 160} textAnchor="middle" style={label(0.7, { fontSize: 18 })}>THE AIR</text>
      </g>

      {/* ---------------- w: the bullet on a scale */}
      <g opacity={wO}>
        <SpinBullet id="w" cx={960} cy={600} cal={80} />
        <line x1={760} y1={650} x2={1160} y2={650} stroke={C.line} strokeWidth={3} />
        <line x1={960} y1={650} x2={960} y2={760} stroke={C.line} strokeWidth={3} />
        <path d="M880,800 A80,80 0 0 1 1040,800" stroke={C.line} strokeWidth={2.5} fill="none" />
        <line x1={960} y1={800} x2={lerp(900, 1010, ramp(v, T.weight, 0.8, ease))} y2={745} stroke={C.line} strokeWidth={3} />
        <text x={1080} y={790} style={mono(ramp(v, T.weight + 0.4, 0.4), 36, { fontWeight: 600 })}>175 gr</text>
      </g>
      {/* d: nose-on */}
      <g opacity={dO}>
        <Bullet x0={960 - (BLEN * 80) / 2} y={620} cal={80} turn={turnK} w={3} />
        <g opacity={ramp(v, T.diameter + 0.3, 0.4)}>
          <line x1={920} y1={620} x2={1000} y2={620} stroke={C.line} strokeWidth={3} />
          <text x={960} y={720} textAnchor="middle" style={mono(1, 36, { fontWeight: 600 })}>0.308 in</text>
        </g>
      </g>
      {/* L: the length, in calibres, and the longer lever */}
      <g opacity={lO}>
        <SpinBullet id="l" cx={960} cy={600} cal={80} />
        {(() => {
          const x0 = 960 - CG_F * BLEN * 80, cg = 960, cp = 960 + (CP_F - CG_F) * BLEN * 80;
          return (
            <>
              {Array.from({ length: 5 }, (_, i) => (
                <g key={i} opacity={ramp(v, T.length + i * 0.12, 0.3)}>
                  <line x1={x0 + i * 80} y1={680} x2={x0 + i * 80} y2={700} stroke={C.line} strokeWidth={2} />
                </g>
              ))}
              <line x1={x0} y1={690} x2={x0 + BLEN * 80} y2={690} stroke={C.line} strokeWidth={2} />
              <text x={960} y={750} textAnchor="middle" style={mono(ramp(v, T.length + 0.4, 0.4), 34, { fontWeight: 600 })}>4.5 calibres</text>
              <g opacity={ramp(v, T.long - 0.2, 0.5)}>
                <circle cx={cg} cy={600} r={10} fill="none" stroke={C.line} strokeWidth={3} />
                <circle cx={cp} cy={600} r={9} fill={C.drag} />
                <line x1={cp} y1={600} x2={cp} y2={480} stroke={C.drag} strokeWidth={6} />
                <path d={`M${cp - 14},498 L${cp},480 L${cp + 14},498`} stroke={C.drag} strokeWidth={6} fill="none" />
                <Bracket x0={cg} x1={cp} y={520} p={ramp(v, T.long, 0.6)} down={-18} />
                <text x={(cg + cp) / 2} y={470} textAnchor="middle" style={label(0.9, { fontSize: 20 })}>THE AIR'S LEVER</text>
                <text x={960} y={830} textAnchor="middle" style={label(0.8, { fontSize: 22 })}>A LONGER BULLET: MORE FOR THE AIR TO TURN</text>
              </g>
            </>
          );
        })()}
      </g>
      {/* t: the twist, a barrel cut open */}
      <g opacity={tO}>
        {(() => {
          const X0 = 360, X1 = 1560, Y = 640, R = 70, cal = R * 2 / 1.0 / 2.2;
          const step = pitch * cal * 0.55;
          const lines: React.ReactNode[] = [];
          for (let x = X0 - step; x < X1; x += step / 3) lines.push(<line key={x} x1={x} y1={Y + R} x2={x + step / 2} y2={Y - R} stroke={C.line} strokeWidth={2} opacity={0.35} />);
          return (
            <>
              <clipPath id="bore"><rect x={X0} y={Y - R} width={X1 - X0} height={2 * R} /></clipPath>
              <g clipPath="url(#bore)">{lines}</g>
              <line x1={X0} y1={Y - R} x2={X1} y2={Y - R} stroke={C.line} strokeWidth={4} />
              <line x1={X0} y1={Y + R} x2={X1} y2={Y + R} stroke={C.line} strokeWidth={4} />
              <g opacity={window01(v, T.oneTurn - 0.2, T.faster, 0.3)}>
                <Bracket x0={X0 + 200} x1={X0 + 200 + step} y={Y + R + 16} p={ramp(v, T.oneTurn, 0.6)} />
                <text x={X0 + 200 + step / 2} y={Y + R + 80} textAnchor="middle" style={mono(1, 30, { fontWeight: 600 })}>one turn in 10 in = 32 calibres</text>
              </g>
              <text x={960} y={Y + R + 80} textAnchor="middle" style={label(ramp(v, T.faster, 0.4), { fontSize: 24 })}>A FASTER TWIST → A FASTER SPIN → S<tspan fontSize={16} dy={6}>g</tspan><tspan dy={-6}> GROWS A LOT</tspan></text>
              <text x={960} y={Y - R - 40} textAnchor="middle" style={label(window01(v, T.squared - 0.2, T.faster, 0.3), { fontSize: 26 })}>SQUARED</text>
            </>
          );
        })()}
      </g>

      {/* ---------------- our bullet: 1.7 */}
      <Gauge cx={960} cy={780} r={230} value={sgShown} o={gaugeO} p={ramp(v, T.our - 0.3, 0.8)} />
      <text x={1260} y={640} style={label(ramp(v, T.good - 0.2, 0.4) * gaugeO, { fontSize: 24 })}>ABOVE 1.4: GOOD</text>

      {/* ---------------- no length or no twist: skipped */}
      {(["L", "t"] as const).map((k, i) => {
        const p: P = POS(k);
        const o = ramp(v, T.empty + i * 0.5, 0.4) * (1 - ramp(v, T.skips - 0.2, 0.5));
        return <path key={k} d={`M${p[0] - 18},${p[1] - 18} L${p[0] + 18},${p[1] + 18} M${p[0] + 18},${p[1] - 18} L${p[0] - 18},${p[1] + 18}`} stroke={C.drag} strokeWidth={5} opacity={o} />;
      })}
      <text x={960} y={420} textAnchor="middle" style={label(window01(v, T.empty, T.skips + 0.3, 0.3), { fontSize: 26 })}>NO LENGTH OR NO TWIST → NO S<tspan fontSize={16} dy={6}>g</tspan></text>
      <g opacity={ramp(v, T.skips - 0.2, 0.5)}>
        {["SPIN DRIFT", "AERODYNAMIC JUMP"].map((s, i) => (
          <g key={s}>
            <Card x={700 + i * 520} y={620} w={420} text={s} o={0.5} />
            <line x1={500 + i * 520} y1={620} x2={900 + i * 520} y2={620} stroke={C.drag} strokeWidth={4} opacity={ramp(v, T.skips + 0.3 + i * 0.2, 0.3)} />
          </g>
        ))}
        <text x={960} y={760} textAnchor="middle" style={label(0.9, { fontSize: 28 })}>SKIPPED</text>
        <text x={960} y={820} textAnchor="middle" style={label(ramp(v, T.warning - 0.2, 0.4), { fontSize: 28, fill: C.drag })}>NO WARNING</text>
      </g>
    </Scene>
  );
};

