// Scene 4 — the drag arrow: a_drag = − D · v / |v|. D (how much) from episode 1 via the Mach number and the Cd
// curve, drawn as a bar with no direction; v / |v| is a pointer of length 1; the minus turns it back; D stretches it.
import React from "react";
import { D as DATA } from "../data";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Formula, P, Pen, Tok, label, layoutFormula, mono } from "../lib/draw";
import { ArrowA, Dot, G, SOUND, Scene, VecText, sub, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 4;
const T = {
  formula: cue(N, "formula"), remember: cue(N, "Remember"), withIt: cue(N, "letter with it"), without: cue(N, "without it"), onlyV: cue(N, "only V"), d: cue(N, "D is"), mach: cue(N, "Mach"),
  sound: cue(N, "sound"), v790: cue(N, "790"), c340: cue(N, "340"), m23: cue(N, "2 3"), withT: cue(N, "With that"), reads: cue(N, "reads"),
  gives: cue(N, "formula gives"), g48: cue(N, "48G"), just: cue(N, "just a number"), how: cue(N, "how much"), dir: cue(N, "direction comes"),
  varrow: cue(N, "velocity arrow"), divide: cue(N, "divide"), stays: cue(N, "stays"), becomes: cue(N, "becomes"), pointer: cue(N, "pointer"),
  minus: cue(N, "minus"), back: cue(N, "points back"), stretches: cue(N, "stretches"), darrow: cue(N, "drag arrow"), exactly: cue(N, "exactly"),
};

const F1: Tok[] = [vec("a", "a"), sub("drag", "a"), t_(" = "), t_("−", "minus"), t_(" "), t_("D", "D"), t_(" · "), vec("v", "v"), t_(" / "), t_("|", "len"), vec("v", "len"), t_("|", "len")];
const F2: Tok[] = [t_("D", "D"), t_(" = ½ ρ · v² · "), t_("Cd(M)", "cd"), t_(" · A / m")];
const F3: Tok[] = [t_("M = "), t_("|"), vec("v"), t_("| / c")];

// where the v⃗ and D of F1 sit, for the "size + direction" / "one number" notes under them
const L1 = layoutFormula(F1, 960, 70);
const XV = L1[7].cx, XD = L1[5].cx;

// the little Cd chart
const KX0 = 560, KX1 = 1360, KY0 = 860, KY1 = 470;
const kx = (m: number) => lerp(KX0, KX1, (m - 0.5) / 2.2), ky = (cd: number) => lerp(KY0, KY1, cd / 0.45);
const CURVE: P[] = DATA.cdCurve.map((p) => [kx(p.mach), ky(p.cd)] as P);
const M0 = 790 / SOUND;
const cdAt = (m: number) => {
  const c = DATA.cdCurve;
  for (let i = 1; i < c.length; i++) if (c[i].mach >= m) return lerp(c[i - 1].cd, c[i].cd, (m - c[i - 1].mach) / (c[i].mach - c[i - 1].mach));
  return c[c.length - 1].cd;
};

export const S04DragArrow: React.FC = () => {
  const v = useVoiceTime();
  // formula emphasis: the letters with arrows, then the plain one, then the only arrow on the right
  const lit = v >= T.withIt - 0.1 && v < T.without - 0.1 ? new Set(["a", "v", "len"]) : v >= T.without - 0.1 && v < T.onlyV - 0.1 ? new Set(["D"])
    : v >= T.onlyV - 0.1 && v < T.d - 0.3 ? new Set(["v", "len"]) : undefined;
  const noteV = window01(v, T.withIt + 0.4, T.d - 0.3, 0.3), noteD = window01(v, T.without + 0.2, T.d - 0.3, 0.3);
  const active =
    v >= T.stretches - 0.1 && v < T.exactly + 1.5 ? "D" : v >= T.minus - 0.1 && v < T.stretches ? "minus" : v >= T.divide - 0.1 && v < T.minus ? "len" :
    v >= T.dir - 0.1 && v < T.divide ? "v" : v >= T.d - 0.1 && v < T.dir ? "D" : null;

  // ---- D: the Mach ruler, the Cd curve, the bar
  const oF2 = window01(v, T.d - 0.1, T.gives + 0.9, 0.4);
  const oRuler = window01(v, T.mach - 0.1, T.withT + 0.2, 0.4);
  const SC = 1.4, RX = 380;
  const copies = [ramp(v, T.c340 - 0.1, 0.4, ease), ramp(v, T.c340 + 0.35, 0.4, ease), ramp(v, T.c340 + 0.8, 0.4, ease)];
  const oChart = window01(v, T.withT - 0.2, T.g48 + 0.6, 0.4);
  const mk = lerp(0.6, M0, ramp(v, T.withT + 0.2, 1.2, ease));
  const readP = ramp(v, T.reads, 0.6);
  const bar = ramp(v, T.g48 - 0.3, 0.7, ease);
  const barO = 1 - ramp(v, T.stretches + 0.6, 0.6);
  const BAR = 480, BX = 300, BY = 930;

  // ---- the pointer
  const DX = 1150, DY = 600, TH = 0.1;
  const vP = ramp(v, T.dir, 0.8, ease);
  const shrink = ramp(v, T.divide + 0.2, 1.4, ease);
  const turn = ramp(v, T.minus + 0.2, 1.1, ease);
  const grow = ramp(v, T.stretches + 0.1, 1.3, ease);
  const len = grow > 0 ? lerp(110, BAR, grow) : lerp(560, 110, shrink);
  const ang = TH + Math.PI * turn;
  const isDrag = grow > 0.05;
  const tipX = DX + len * Math.cos(ang), tipY = DY + len * Math.sin(ang);
  const oPointer = ramp(v, T.dir - 0.3, 0.5);

  return (
    <Scene n={N}>
      <Formula toks={F1} cx={960} y={130} size={70} reveal={ramp(v, T.formula - 0.2, 1.4)} active={active} lit={lit} />
      <text x={XV - 20} y={200} textAnchor="start" style={label(noteV, { fontSize: 22 })}>SIZE + DIRECTION</text>
      <text x={XD + 20} y={200} textAnchor="end" style={label(noteD, { fontSize: 22 })}>ONE NUMBER</text>
      <text x={960} y={290} textAnchor="middle" style={label(window01(v, T.onlyV - 0.1, T.d - 0.2, 0.25), { fontSize: 24 })}>THE ONLY ARROW ON THE RIGHT · THE DIRECTION COMES FROM THE VELOCITY</text>

      {/* ---------------- D from episode 1 */}
      <g opacity={oF2}>
        <Formula toks={F2} cx={960} y={260} size={48} reveal={ramp(v, T.d, 1.0)} active={v >= T.mach - 0.2 ? "cd" : null} />
        <text x={960} y={318} textAnchor="middle" style={label(ramp(v, T.d + 0.4, 0.4) * 0.7, { fontSize: 20 })}>EPISODE 1 · HOW MUCH THE AIR SLOWS THE BULLET</text>
      </g>
      <g opacity={oRuler}>
        <Formula toks={F3} cx={960} y={420} size={44} reveal={ramp(v, T.mach, 0.8)} />
        <Arrow x1={RX} y1={560} x2={RX + 790 * SC} y2={560} p={ramp(v, T.sound - 0.4, 0.8, ease)} w={4} head={20} />
        <text x={RX + 395 * SC} y={536} textAnchor="middle" style={mono(ramp(v, T.v790 - 0.2, 0.4), 34)}>790 m/s</text>
        {copies.map((k, i) => {
          const x0 = RX + i * 340 * SC, l = (i < 2 ? 340 : 790 - 680) * SC;
          return (
            <g key={i} opacity={k}>
              <Arrow x1={x0} y1={640} x2={x0 + l * k} y2={640} w={3} head={14} color={C.line} opacity={i < 2 ? 1 : 0.7} />
              <line x1={x0} y1={624} x2={x0} y2={656} stroke={C.line} strokeWidth={2} />
              <text x={x0 + l / 2} y={690} textAnchor="middle" style={mono(0.8, 28)}>{i < 2 ? "340" : "0.3"}</text>
            </g>
          );
        })}
        <text x={RX} y={740} style={mono(ramp(v, T.c340 - 0.2, 0.4) * 0.8, 28)}>c = 340 m/s · THE SPEED OF SOUND</text>
        <text x={RX + 790 * SC} y={760} textAnchor="end" style={mono(ramp(v, T.m23 - 0.2, 0.4), 52, { fontWeight: 600 })} filter="url(#glow)">M = 2.3</text>
      </g>
      <g opacity={oChart}>
        <Pen pts={[[KX0, KY1 - 20], [KX0, KY0], [KX1 + 20, KY0]]} p={1} color={C.dim} w={1.8} tip={false} />
        {[1, 1.5, 2, 2.5].map((m) => <text key={m} x={kx(m)} y={KY0 + 32} textAnchor="middle" style={mono(0.6, 20)}>{m.toFixed(1)}</text>)}
        <text x={KX1 + 20} y={KY0 - 14} textAnchor="end" style={label(0.6, { fontSize: 18 })}>MACH</text>
        <text x={KX0 - 14} y={KY1 - 24} textAnchor="end" style={label(0.6, { fontSize: 18 })}>Cd</text>
        <path d={"M" + CURVE.map((q) => q.join(",")).join(" L")} stroke={C.drag} strokeWidth={3} fill="none" opacity={0.9} />
        <line x1={kx(M0)} y1={KY0} x2={kx(M0)} y2={ky(cdAt(M0))} stroke={C.line} strokeWidth={1.5} strokeDasharray="5 6" opacity={readP} />
        <line x1={kx(M0)} y1={ky(cdAt(M0))} x2={KX0} y2={ky(cdAt(M0))} stroke={C.line} strokeWidth={1.5} strokeDasharray="5 6" opacity={readP} />
        <circle cx={kx(mk)} cy={ky(cdAt(mk))} r={11} fill={C.drag} filter="url(#glow)" />
        <text x={kx(M0)} y={KY0 + 66} textAnchor="middle" style={mono(readP, 26)}>M 2.3</text>
        <text x={KX0 + 20} y={ky(cdAt(M0)) - 16} style={mono(readP, 30, { fontWeight: 600 })}>Cd = {cdAt(M0).toFixed(2)}</text>
      </g>
      {/* D: a bar — a length, no direction */}
      <g opacity={bar * barO}>
        <line x1={BX} y1={BY} x2={BX + BAR * bar} y2={BY} stroke={C.drag} strokeWidth={14} />
        <line x1={BX} y1={BY - 18} x2={BX} y2={BY + 18} stroke={C.drag} strokeWidth={3} />
        <line x1={BX + BAR * bar} y1={BY - 18} x2={BX + BAR * bar} y2={BY + 18} stroke={C.drag} strokeWidth={3} />
        <text x={BX + BAR / 2} y={BY - 34} textAnchor="middle" style={mono(1, 44, { fill: C.drag, fontWeight: 600 })}>D = {Math.round(DATA.rows[0].drag / G)} g</text>
        <text x={BX + BAR / 2} y={BY + 58} textAnchor="middle" style={label(window01(v, T.how - 0.2, T.stretches, 0.3), { fontSize: 22 })}>HOW MUCH · NOT WHICH WAY</text>
        {[BX - 50, BX + BAR + 50].map((x, i) => (
          <text key={i} x={x} y={BY + 14} textAnchor="middle" style={mono(window01(v, T.how + 0.4, T.dir + 0.6, 0.3) * 0.8, 40)}>?</text>
        ))}
      </g>

      {/* ---------------- the pointer */}
      <g opacity={oPointer}>
        {/* the original velocity, as a ghost once it has shrunk */}
        <ArrowA x={DX} y={DY} a={TH} len={560} w={2} head={14} opacity={0.3 * shrink} />
        <VecText x={DX + 560 * Math.cos(TH) + 16} y={DY + 560 * Math.sin(TH) + 12} s="v" size={40} o={0.4 + 0.6 * (1 - shrink)} />
        <text x={DX + 280} y={DY - 24} textAnchor="middle" style={mono(vP * (1 - shrink), 32)}>790 m/s</text>
        <g filter={isDrag ? "url(#glow)" : undefined}>
          <Arrow x1={DX} y1={DY} x2={tipX} y2={tipY} p={vP} color={isDrag ? C.drag : C.line} w={isDrag ? 6 : 4} head={isDrag ? 24 : 18} />
        </g>
        {/* length-1 marks */}
        <g opacity={window01(v, T.becomes - 0.2, T.minus + 0.2, 0.3)}>
          <text x={DX + 55} y={DY - 30} textAnchor="middle" style={mono(1, 32, { fontWeight: 600 })}>1</text>
        </g>
        <text x={960} y={800} textAnchor="middle" style={label(window01(v, T.stays - 0.2, T.minus, 0.3), { fontSize: 28 })}>THE SAME DIRECTION · LENGTH 1</text>
        <text x={960} y={800} textAnchor="middle" style={label(window01(v, T.minus + 0.6, T.stretches, 0.3), { fontSize: 28 })}>TURNED AROUND · IT POINTS BACK</text>
        <text x={960} y={800} textAnchor="middle" style={label(ramp(v, T.darrow - 0.2, 0.4), { fontSize: 28, fill: C.drag })}>
          THE DRAG ARROW · 48 g · EXACTLY AGAINST THE VELOCITY
        </text>
        <g opacity={window01(v, T.pointer - 0.3, T.minus + 0.2, 0.3)}>
          <VecText x={DX + 130} y={DY + 70} s="v" size={36} />
          <text x={DX + 156} y={DY + 70} style={mono(1, 36)} xmlSpace="preserve"> / |</text>
          <VecText x={DX + 242} y={DY + 70} s="v" size={36} />
          <text x={DX + 264} y={DY + 70} style={mono(1, 36)}>|</text>
          <text x={DX + 310} y={DY + 70} style={label(0.8, { fontSize: 22 })}>A POINTER</text>
        </g>
        <Dot x={DX} y={DY} />
      </g>
    </Scene>
  );
};
