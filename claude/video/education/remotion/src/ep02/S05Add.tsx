// Scene 5 — add the arrows: a = a_drag + g, tip to tail. Then the same thing three times, once per axis: the drag
// box is the velocity box turned around and resized, so flying a little down means the drag pushes a little up;
// gravity is in the y line only; the z line is zero and gets a bookmark for the wind.
// The picture is the dot at 1000 m (level bore, the library's flight): 322 m/s, drag 5.7 g, path 2.4° down.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Formula, Tok, label, mono } from "../lib/draw";
import { ArrowA, Bookmark, Dot, FLIGHT, G, Scene, VecText, sub, t_, useVoiceTime, vec } from "./kit";
import { cue } from "./timing";

const N = 5;
const T = {
  add: cue(N, "add"), put: cue(N, "Put gravity"), end: cue(N, "end of the drag"), total: cue(N, "total"), doesnt: cue(N, "doesn't work"),
  numbers: cue(N, "numbers"), three: cue(N, "three times"), boxes: cue(N, "two boxes"), dbox: cue(N, "drag box"), turned: cue(N, "turned"),
  resized: cue(N, "resized"), each: cue(N, "each part"), share: cue(N, "share"), downward: cue(N, "downward"), pushes: cue(N, "pushes"),
  upward: cue(N, "upward"), gshows: cue(N, "Gravity shows"), updown: cue(N, "up and down"), side: cue(N, "side line"),
  noside: cue(N, "no side velocity"), remember: cue(N, "Remember"), wind: cue(N, "wind"),
};

const END = FLIGHT[FLIGHT.length - 1], PREV = FLIGHT[FLIGHT.length - 2];
const SLOPE = Math.atan2(PREV.y - END.y, END.x - PREV.x);       // ≈ 0.042 rad down
const EXAG = 5;
const TH = SLOPE * EXAG;                                         // drawn angle, screen (positive = down)
const DRAG_G = END.drag / G;                                     // ≈ 5.7 g
const PG = 82;                                                   // px per g

const FA: Tok[] = [vec("a"), t_(" = "), vec("a", "drag", C.drag), sub("drag", "drag", C.drag), t_(" + "), vec("g", "g")];
const FG: Tok[] = [vec("g"), t_(" = (0, −g, 0)")];
const SZ = 50;
const line = (ax: string, key: string, extra: boolean): Tok[] => [
  t_("a"), sub(ax), t_(" = "), t_("− D · v", key), sub(ax, key), t_(" / |", key), vec("v", key), t_("|", key), ...(extra ? [t_(" − g", "g")] : []),
];
const L1 = line("x", "dx", false), L2 = line("y", "dy", true), L3 = line("z", "dz", false);
const LX = 1000, LY = [760, 850, 940];
const shift = (extra: boolean) => (extra ? (4 * SZ * 0.6) / 2 : 0);   // left-align the three lines

export const S05Add: React.FC = () => {
  const v = useVoiceTime();
  // ---------------- phase 1: tip to tail
  const o1 = 1 - ramp(v, T.doesnt - 0.2, 0.6);
  const P0x = 1250, P0y = 560;
  const dLen = DRAG_G * PG;
  const dTip: [number, number] = [P0x - dLen * Math.cos(TH), P0y - dLen * Math.sin(TH)];
  const gSlide = ramp(v, T.put - 0.1, 1.0, ease);
  const gx = lerp(P0x, dTip[0], gSlide), gy = lerp(P0y, dTip[1], gSlide);
  const tot = ramp(v, T.total - 1.6, 1.0, ease);
  // ---------------- phase 2: the boxes
  const o2 = ramp(v, T.doesnt, 0.6);
  const D0x = 1080, D0y = 400, VX = 330, VY = VX * Math.tan(TH);
  const K = 1.4;                                                // the drag box is the velocity box, ×K, turned around
  const turn = ramp(v, T.turned - 0.1, 0.9, ease), size = lerp(1, K, ramp(v, T.resized - 0.1, 0.7, ease));
  const vBox = ramp(v, T.boxes - 0.3, 0.8);
  const hot = (t0: number, t1: number) => window01(v, t0 - 0.15, t1, 0.25);
  const hx = hot(T.each, T.share), hy = hot(T.share, T.gshows);
  const hDown = hot(T.downward, T.side), hUp = hot(T.upward, T.side);
  const gP = ramp(v, T.gshows, 0.6, ease);
  const zDim = 1 - 0.5 * ramp(v, T.remember + 0.2, 0.6);

  const active = (l: number) => {
    if (l === 1 && v >= T.gshows - 0.1 && v < T.side) return "g";
    if (l === 0 && hx > 0.3) return "dx";
    if (l === 1 && hy > 0.3) return "dy";
    if (l === 2 && v >= T.side - 0.1 && v < T.remember + 0.4) return "dz";
    return null;
  };

  return (
    <Scene n={N}>
      <Formula toks={FA} cx={900} y={130} size={66} reveal={ramp(v, T.add - 0.2, 1.4)} active={v >= T.put && v < T.total ? "g" : null} />
      <Formula toks={FG} cx={1500} y={130} size={40} reveal={ramp(v, T.put, 0.8)} opacity={0.8} />

      {/* ---------------- phase 1 */}
      <g opacity={o1}>
        <text x={960} y={800} textAnchor="middle" style={label(ramp(v, -0.4, 0.6), { fontSize: 30 })}>LATER IN THE FLIGHT · AT 1000 m · {Math.round(END.v)} m/s</text>
        <ArrowA x={P0x} y={P0y} a={TH} len={300} w={2} head={14} opacity={0.5 * ramp(v, -0.2, 0.6)} />
        <VecText x={P0x + 300 * Math.cos(TH) + 14} y={P0y + 300 * Math.sin(TH) + 14} s="v" size={34} o={0.6 * ramp(v, -0.2, 0.6)} />
        <ArrowA x={P0x} y={P0y} a={Math.PI + TH} len={dLen} p={ramp(v, -0.3, 0.9, ease)} color={C.drag} w={5} head={22} glow />
        <text x={(P0x + dTip[0]) / 2} y={(P0y + dTip[1]) / 2 - 30} textAnchor="middle" style={mono(ramp(v, 0.2, 0.5), 32, { fill: C.drag })}>
          drag {DRAG_G.toFixed(1)} g
        </text>
        <Arrow x1={gx} y1={gy} x2={gx} y2={gy + PG} w={4} head={18} />
        <text x={gx - 18} y={gy + PG / 2 + 10} textAnchor="end" style={mono(1, 30)}>1 g</text>
        <g filter="url(#glow)">
          <Arrow x1={P0x} y1={P0y} x2={dTip[0]} y2={dTip[1] + PG} p={tot} w={6} head={24} />
        </g>
        <text x={(P0x + dTip[0]) / 2 + 30} y={(P0y + dTip[1] + PG) / 2 + 60} textAnchor="middle" style={label(ramp(v, T.total, 0.4), { fontSize: 26 })}>
          THE TOTAL ACCELERATION
        </text>
        <Dot x={P0x} y={P0y} />
      </g>

      {/* ---------------- phase 2: three lines */}
      <g opacity={o2}>
        <text x={960} y={250} textAnchor="middle" style={label(window01(v, T.doesnt, T.boxes, 0.3), { fontSize: 28 })}>THE PROGRAM WORKS WITH NUMBERS: ONE LINE PER DIRECTION</text>
        {[L1, L2, L3].map((L, i) => (
          <g key={i} opacity={i === 2 ? zDim : 1}>
            <Formula toks={L} cx={LX + shift(i === 1)} y={LY[i]} size={SZ} reveal={ramp(v, T.numbers + 0.5 + i * 0.7, 0.9)} active={active(i)} />
          </g>
        ))}
        {/* z: zero side velocity, zero side drag; a bookmark */}
        <text x={1440} y={LY[2]} style={mono(window01(v, T.noside - 0.1, T.remember + 0.4, 0.3), 34, { fill: C.line })}>
          v<tspan fontSize={20} dy={7}>z</tspan><tspan dy={-7}> = 0  →  a</tspan><tspan fontSize={20} dy={7}>z</tspan><tspan dy={-7}> = 0</tspan>
        </text>
        <Bookmark x={690} y={LY[2] - 4} o={ramp(v, T.remember + 0.3, 0.5)} s={1.3} />
        <text x={960} y={1030} textAnchor="middle" style={label(ramp(v, T.wind - 0.4, 0.5) * 0.8, { fontSize: 22 })}>NEXT EPISODE: THE WIND</text>

        {/* the velocity box */}
        <g opacity={vBox}>
          <rect x={D0x} y={D0y} width={VX} height={VY} fill="none" stroke={C.line} strokeWidth={1.5} strokeDasharray="6 7" opacity={0.6} />
          <line x1={D0x} y1={D0y} x2={D0x + VX} y2={D0y} stroke={C.line} strokeWidth={3 + 4 * hx} filter={hx > 0.3 ? "url(#glow)" : undefined} />
          <line x1={D0x + VX} y1={D0y} x2={D0x + VX} y2={D0y + VY} stroke={C.line} strokeWidth={3 + 4 * Math.max(hy, hDown)} filter={hy > 0.3 ? "url(#glow)" : undefined} />
          <ArrowA x={D0x} y={D0y} a={TH} len={Math.hypot(VX, VY)} w={4} head={18} />
          <text x={D0x + VX / 2} y={D0y - 18} textAnchor="middle" style={mono(0.9, 30)}>v<tspan fontSize={19} dy={7}>x</tspan></text>
          <text x={D0x + VX + 18} y={D0y + VY / 2 + 10} style={mono(0.9, 30)}>v<tspan fontSize={19} dy={7}>y</tspan></text>
          <VecText x={D0x + VX + 20} y={D0y + VY + 50} s="v" size={34} />
          <text x={D0x + VX + 60} y={D0y + VY + 40} style={label(hDown, { fontSize: 22 })}>A LITTLE DOWN</text>
        </g>
        {/* the drag box: the same box, turned around and resized */}
        <g opacity={ramp(v, T.dbox - 0.2, 0.5)}>
          <g transform={`translate(${D0x},${D0y}) rotate(${180 * turn}) scale(${size}) translate(${-D0x},${-D0y})`}>
            <rect x={D0x} y={D0y} width={VX} height={VY} fill="none" stroke={C.drag} strokeWidth={1.5 / size} strokeDasharray="6 7" opacity={0.7} />
            <line x1={D0x} y1={D0y} x2={D0x + VX} y2={D0y} stroke={C.drag} strokeWidth={(3 + 4 * hx) / size} />
            <line x1={D0x + VX} y1={D0y} x2={D0x + VX} y2={D0y + VY} stroke={C.drag} strokeWidth={(3 + 4 * Math.max(hy, hUp)) / size} />
            <Arrow x1={D0x} y1={D0y} x2={D0x + VX} y2={D0y + VY} color={C.drag} w={5 / size} head={20 / size} />
          </g>
          <g opacity={ramp(v, T.resized + 0.5, 0.4)}>
            <text x={D0x - (K * VX) / 2} y={D0y + 44} textAnchor="middle" style={mono(0.9, 28, { fill: C.drag })}>drag x</text>
            <text x={D0x - K * VX - 20} y={D0y - (K * VY) / 2 + 8} textAnchor="end" style={mono(0.9, 28, { fill: C.drag })}>drag y</text>
            <text x={D0x - K * VX - 20} y={D0y - K * VY - 26} textAnchor="end" style={label(hUp, { fontSize: 22, fill: C.drag })}>A LITTLE UP</text>
          </g>
        </g>
        <text x={960} y={640} textAnchor="middle" style={label(window01(v, T.dbox, T.each, 0.3), { fontSize: 26 })}>THE VELOCITY BOX, TURNED AROUND AND RESIZED</text>
        <text x={960} y={640} textAnchor="middle" style={label(window01(v, T.each, T.downward - 0.2, 0.3), { fontSize: 26 })}>EACH PART TAKES THE SAME SHARE</text>
        <text x={960} y={640} textAnchor="middle" style={label(window01(v, T.upward, T.side, 0.3), { fontSize: 26, fill: C.drag })}>FLYING A LITTLE DOWN · DRAG PUSHES A LITTLE UP</text>
        {/* gravity, in the y line only */}
        <ArrowA x={D0x} y={D0y + 14} a={Math.PI / 2} len={PG} p={gP} w={4} head={18} />
        <text x={D0x + 18} y={D0y + PG + 4} style={mono(gP, 28)}>g</text>
        <Dot x={D0x} y={D0y} o={o2} />
      </g>
    </Scene>
  );
};
