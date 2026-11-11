// Scene 6 — Coriolis. A carousel from above: the ball flies straight, the carousel turns under it; riding it, the
// path bends right. The North Pole is such a carousel, one turn a day; at the equator nothing turns around you; in
// between, the part sin φ. The formula, straight from physics; right in the north, left in the south; 10 cm at 45° N
// and 1 km, the same for every direction of the shot.
import React from "react";
import { C, ease, lerp, ramp, window01 } from "../theme";
import { P, Pen, label, mono } from "../lib/draw";
import { Globe, Item, Legend, MathLine, Scene, Title, TurnArrow, k_, t_, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 6;
const T = {
  earth: cue(N, "Earth"), turns: cue(N, "It turns"), carousel: cue(N, "carousel"), thrw: cue(N, "Throw"), straight: cue(N, "flies straight"),
  under: cue(N, "turns under"), someone: cue(N, "someone"), pole: cue(N, "North Pole"), day: cue(N, "once a day"), right: cue(N, "bends to the right"),
  equator: cue(N, "equator"), between: cue(N, "In between"), sine: cue(N, "sign of your"), need: cue(N, "doesn't need"), physics: cue(N, "physics"),
  omega: cue(N, "Omega"), oneTurn: cue(N, "one turn a day"), sin: cue(N, "sign of the"), dist: cue(N, "distance"), tof: cue(N, "time of flight"),
  north: cue(N, "northern"), south: cue(N, "southern"), n45: cue(N, "45"), n10: cue(N, "10"), noDir: cue(N, "no direction"), wherever: cue(N, "wherever"),
};

const FX = 960, FY = 190, FS = 64;
const COR: Item[] = [{ toks: [t_("Δz = "), k_("Ω", "om"), t_(" · "), k_("sin φ", "sin"), t_(" · "), k_("R", "R"), t_(" · "), k_("t", "t")] }];

// the carousel: centre, radius, the throw. An honest throw: the thrower aims straight at the friend, and the ball also
// takes the rim's own velocity. Seen from above it flies straight; on the carousel it starts at the friend and bends right.
const CC: P = [960, 600], CR = 320, OMEGA = 0.155;   // rad/s, drawn: about 27° while the ball flies
const A: P = [CC[0], CC[1] + CR], B0: P = [CC[0], CC[1] - CR];
const AIM = 210;                                      // px/s towards the friend, as the thrower sees it
const RIM = OMEGA * CR;                               // the rim's own velocity at the thrower: to the right on screen
const FLY = (2 * CR * AIM) / (AIM * AIM + RIM * RIM); // s from the rim to the rim
const rot = (p: P, a: number): P => {                 // rotate about the centre, y-down (positive a = clockwise on screen)
  const dx = p[0] - CC[0], dy = p[1] - CC[1];
  return [CC[0] + dx * Math.cos(a) - dy * Math.sin(a), CC[1] + dx * Math.sin(a) + dy * Math.cos(a)];
};
/** The ball from above (the ground's frame), q seconds after the throw. */
const ball = (q: number): P => [A[0] + RIM * q, A[1] - AIM * q];
/** The same ball on the carousel's floor (the carousel's frame, as at the throw). */
const onFloor = (q: number): P => rot(ball(q), OMEGA * q);

export const S06Coriolis: React.FC = () => {
  const v = useVoiceTime();
  // ---- the carousel
  const carO = ramp(v, T.carousel - 0.5, 0.5) * (1 - ramp(v, T.pole - 0.5, 0.5));
  const t0 = T.thrw + 0.4;
  const s = Math.max(0, Math.min(FLY, v - t0));
  // the carousel turns counterclockwise; once the ball is down, it stops, so nobody runs away from the picture
  const tStop = t0 + FLY;
  const turned = -OMEGA * (Math.min(v, tStop) - t0);                 // the carousel's angle since the throw
  const ride = ramp(v, T.someone - 0.3, 1.4, ease);                   // the camera turns with the carousel
  const view = -turned * ride;                                        // the camera's turn, radians (clockwise on screen)
  const trace: P[] = Array.from({ length: 41 }, (_, i) => rot(onFloor((s * i) / 40), turned));
  const missO = ramp(v, T.someone + 1.2, 0.5);
  // ---- the globe
  const gO = ramp(v, T.pole - 0.4, 0.5);
  const G: P = [lerp(960, 560, ramp(v, T.omega - 0.6, 0.8, ease)), lerp(600, 640, ramp(v, T.omega - 0.6, 0.8, ease))];
  const GR = lerp(300, 250, ramp(v, T.omega - 0.6, 0.8, ease));
  const lat = v < T.equator - 0.2 ? 90 : v < T.between ? lerp(90, 0, ramp(v, T.equator - 0.2, 1.4, ease)) : lerp(0, 45, ramp(v, T.between, 1.2, ease));
  const la = (lat * Math.PI) / 180, shooter: P = [G[0] + GR * Math.cos(la), G[1] - GR * Math.sin(la)];
  const shooterO = ramp(v, T.pole - 0.2, 0.4) * (1 - ramp(v, T.north - 0.4, 0.4));
  // ---- formula
  const fO = ramp(v, T.omega - 0.3, 0.5);
  const active = v >= T.north - 0.2 ? null : v >= T.tof - 0.1 ? "t" : v >= T.dist - 0.1 ? "R" : v >= T.sin - 0.1 ? "sin" : v >= T.omega - 0.1 ? "om" : null;
  const hemiO = ramp(v, T.north - 0.2, 0.5) * (1 - ramp(v, T.n45 - 0.4, 0.4));          // gone before our shot and the compass
  const dirO = ramp(v, T.noDir - 0.4, 0.5);

  return (
    <Scene n={N}>
      <Title text="THE TURNING EARTH" o={ramp(v, -0.5, 0.5) * (1 - ramp(v, T.carousel - 0.5, 0.4))} />
      <g opacity={ramp(v, -0.3, 0.6) * (1 - ramp(v, T.carousel - 0.5, 0.4))}>
        <Globe cx={960} cy={600} r={240} rot={v * 0.15} />
        <text x={960} y={960} textAnchor="middle" style={label(ramp(v, T.turns, 0.4) * 0.8, { fontSize: 26 })}>IT TURNS, AND WE TURN WITH IT</text>
      </g>

      {/* ---------------- the carousel */}
      <g opacity={carO}>
        <text x={60} y={70} style={label(0.6, { fontSize: 20 })}>{ride < 0.5 ? "A CAROUSEL, FROM ABOVE" : "RIDING THE CAROUSEL"}</text>
        {/* one picture: from above the carousel turns and the ball flies straight, leaving a chalk line on the floor;
            then the camera turns with the carousel, and the chalk line is the ball's path for the people on it */}
        <g transform={`rotate(${(view * 180) / Math.PI},${CC[0]},${CC[1]})`}>
          <g transform={`rotate(${(turned * 180) / Math.PI},${CC[0]},${CC[1]})`}>
            <circle cx={CC[0]} cy={CC[1]} r={CR} fill="none" stroke={C.line} strokeWidth={3} />
            {Array.from({ length: 8 }, (_, i) => {
              const a = (i * Math.PI) / 4 + 0.2;
              return <line key={i} x1={CC[0]} y1={CC[1]} x2={CC[0] + CR * Math.cos(a)} y2={CC[1] + CR * Math.sin(a)} stroke={C.line} strokeWidth={1.5} opacity={0.3} />;
            })}
            {/* the aim: straight at the friend */}
            <line x1={A[0]} y1={A[1]} x2={B0[0]} y2={B0[1]} stroke={C.dim} strokeWidth={2} strokeDasharray="8 10" opacity={ramp(v, T.thrw - 0.3, 0.4)} />
            <rect x={A[0] - 12} y={A[1] - 12} width={24} height={24} fill="none" stroke={C.line} strokeWidth={3} />
            <circle cx={B0[0]} cy={B0[1]} r={16} fill="none" stroke={C.line} strokeWidth={3} opacity={ramp(v, T.thrw - 0.3, 0.4)} />
          </g>
          {/* the ball's real, straight flight over the ground */}
          {s > 0 && <line x1={A[0]} y1={A[1]} x2={ball(s)[0]} y2={ball(s)[1]} stroke={C.line} strokeWidth={3} strokeDasharray="10 10" opacity={0.85 * (1 - ride)} />}
          {/* the chalk line it leaves on the turning floor */}
          {s > 0 && <Pen pts={trace} p={1} w={4} tip={false} />}
          {s > 0 && <circle cx={ball(s)[0]} cy={ball(s)[1]} r={12} fill={C.line} filter="url(#glow)" />}
        </g>
        <g opacity={missO}>
          <text x={B0[0] - 40} y={B0[1] - 30} textAnchor="end" style={label(0.9, { fontSize: 24 })}>THE FRIEND</text>
          <text x={onFloor(FLY)[0] + 30} y={onFloor(FLY)[1] + 8} style={label(1, { fontSize: 24 })}>MISSED: TO THE RIGHT</text>
        </g>
        <text x={1320} y={300} style={label(window01(v, t0 + 0.6, T.someone, 0.3) * 0.75, { fontSize: 20 })}>SOLID: THE CHALK LINE ON THE FLOOR</text>
        <text x={1320} y={330} style={label(window01(v, t0 + 0.6, T.someone, 0.3) * 0.75, { fontSize: 20 })}>DASHED: THE BALL'S STRAIGHT FLIGHT</text>
        <text x={960} y={1010} textAnchor="middle" style={label(window01(v, T.straight - 0.2, T.someone, 0.3), { fontSize: 26 })}>FROM ABOVE: THE BALL FLIES STRAIGHT · THE CAROUSEL TURNS UNDER IT</text>
        <text x={960} y={1010} textAnchor="middle" style={label(ramp(v, T.someone, 0.4), { fontSize: 26 })}>ON THE CAROUSEL: THE BALL BENDS TO THE RIGHT</text>
      </g>

      {/* ---------------- the globe: the pole is a carousel; sin φ of it turns around you */}
      <g opacity={gO}>
        <Globe cx={G[0]} cy={G[1]} r={GR} rot={v * 0.15} />
        <g opacity={window01(v, T.pole - 0.2, T.equator, 0.3)}>
          <TurnArrow x={G[0]} y={G[1] - GR} rx={90} k={1} ph={v * 0.3} />
          <text x={G[0]} y={G[1] - GR - 60} textAnchor="middle" style={label(ramp(v, T.day - 0.2, 0.4), { fontSize: 22 })}>ONE TURN A DAY · THE BULLET BENDS RIGHT</text>
        </g>
        <g opacity={shooterO}>
          <circle cx={shooter[0]} cy={shooter[1]} r={10} fill={C.line} filter="url(#glow)" />
          <g transform={`rotate(${90 - lat},${shooter[0]},${shooter[1]})`}>
            <TurnArrow x={shooter[0]} y={shooter[1] - 28} rx={70} k={Math.sin(la)} ph={v * 0.3} />
          </g>
          <text x={shooter[0] + 110} y={shooter[1] + 10} style={mono(ramp(v, T.equator - 0.3, 0.4), 30, { fontWeight: 600 })}>
            sin φ = {Math.sin(la).toFixed(2)}
          </text>
          <text x={shooter[0] + 110} y={shooter[1] + 44} style={label(ramp(v, T.equator - 0.3, 0.4) * 0.6, { fontSize: 18 })}>
            {lat < 5 ? "EQUATOR: NOTHING TURNS AROUND YOU" : lat > 85 ? "THE POLE" : `${Math.round(lat)}° NORTH: A PART OF THE TURN`}
          </text>
        </g>
        {/* north: right; south: left — each as a small top view of a shot, tied to its half of the globe */}
        <g opacity={hemiO}>
          {([[1, "NORTHERN HALF: RIGHT", T.north], [-1, "SOUTHERN HALF: LEFT", T.south]] as const).map(([dir, s, t]) => {
            const on: P = [G[0] + GR * 0.62, G[1] - dir * GR * 0.62];                  // a point on that half
            const c: P = [G[0] + GR + 200, G[1] - dir * 190], hw = 90;               // the badge
            const k = ramp(v, t - 0.2, 0.8, ease);
            const end: P = [c[0] + dir * 46, c[1] - 70];
            return (
              <g key={s} opacity={ramp(v, t - 0.3, 0.4)}>
                <circle cx={on[0]} cy={on[1]} r={7} fill={C.line} />
                <line x1={on[0]} y1={on[1]} x2={c[0] - hw} y2={c[1]} stroke={C.line} strokeWidth={1.5} opacity={0.4} />
                <rect x={c[0] - hw} y={c[1] - hw} width={2 * hw} height={2 * hw} rx={12} fill={C.bg} stroke={C.line} strokeWidth={2} opacity={0.9} />
                <line x1={c[0]} y1={c[1] + 72} x2={c[0]} y2={c[1] - 76} stroke={C.dim} strokeWidth={2} strokeDasharray="6 7" />
                <path d={`M${c[0]},${c[1] + 72} Q${c[0]},${c[1] - 40} ${lerp(c[0], end[0], k)},${lerp(c[1] + 72, end[1], k)}`} stroke={C.line} strokeWidth={4} fill="none" filter="url(#glow)" />
                <circle cx={c[0]} cy={c[1] + 72} r={6} fill={C.line} />
                <text x={c[0] + hw + 20} y={c[1] + 8} style={label(1, { fontSize: 22 })}>{s}</text>
                <text x={c[0]} y={c[1] - hw - 12} textAnchor="middle" style={label(0.55, { fontSize: 16 })}>A SHOT, FROM ABOVE</text>
              </g>
            );
          })}
        </g>
      </g>
      <text x={960} y={1010} textAnchor="middle" style={label(window01(v, T.need - 0.2, T.omega + 2, 0.4), { fontSize: 26 })}>NOT FROM MEASURED SHOTS · STRAIGHT FROM PHYSICS</text>

      {/* ---------------- the formula */}
      <g opacity={fO}>
        <MathLine items={COR} cx={FX} y={FY} size={FS} reveal={ramp(v, T.omega - 0.1, 1.2)} active={active} />
        <Legend x={1420} y={110} size={18} gap={34} v={v} o={1} rows={[
          { sym: "Δz", text: "TO THE SIDE · METRES", at: T.omega - 0.4 },
          { sym: "Ω", text: "THE EARTH'S TURN: 1 A DAY", at: T.omega },
          { sym: "φ", text: "YOUR LATITUDE", at: T.sin },
          { sym: "R", text: "THE DISTANCE · METRES", at: T.dist },
          { sym: "t", text: "TIME OF FLIGHT · SECONDS", at: T.tof },
        ]} />
        <text x={960} y={290} textAnchor="middle" style={mono(ramp(v, T.n45 - 0.3, 0.5), 30, { fontWeight: 600 })}>
          Δz = 7.29·10<tspan fontSize={18} dy={-12}>−5</tspan><tspan dy={12}> · 0.71 · 1000 · 2.01 = 0.10 m</tspan>
        </text>
      </g>
      <g opacity={ramp(v, T.n45 - 0.3, 0.5)}>
        <text x={1400} y={400} textAnchor="middle" style={label(0.8, { fontSize: 24 })}>45° NORTH · 1000 m</text>
        <text x={1400} y={460} textAnchor="middle" style={mono(ramp(v, T.n10 - 0.2, 0.4), 48, { fontWeight: 700 })}>10 cm right</text>
      </g>
      {/* every direction: the same 10 cm to the right */}
      <g opacity={dirO}>
        {[0, 90, 180, 270].map((az) => {
          const a = ((az - 90) * Math.PI) / 180, c: P = [1400, 720], L = 170;
          const e: P = [c[0] + L * Math.cos(a), c[1] + L * Math.sin(a)];
          const rgt: P = [-Math.sin(a), Math.cos(a)];                       // the shooter's right, on screen
          const bend: P = [e[0] + rgt[0] * 28, e[1] + rgt[1] * 28];
          return (
            <g key={az}>
              <line x1={c[0]} y1={c[1]} x2={e[0]} y2={e[1]} stroke={C.dim} strokeWidth={2} strokeDasharray="6 6" />
              <path d={`M${c[0]},${c[1]} Q${e[0]},${e[1]} ${bend[0]},${bend[1]}`} stroke={C.line} strokeWidth={3.5} fill="none" />
              <text x={c[0] + (L + 50) * Math.cos(a)} y={c[1] + (L + 50) * Math.sin(a) + 8} textAnchor="middle" style={label(0.8, { fontSize: 22 })}>{["N", "E", "S", "W"][az / 90]}</text>
            </g>
          );
        })}
        <circle cx={1400} cy={720} r={8} fill={C.line} />
        <text x={1400} y={980} textAnchor="middle" style={label(0.85, { fontSize: 22 })}>AT 45° NORTH, SHOOTING N, E, S OR W:</text>
        <text x={1400} y={1016} textAnchor="middle" style={label(ramp(v, T.wherever - 0.4, 0.4) * 0.85, { fontSize: 22 })}>10 cm TO THE RIGHT OF THE SHOT, EVERY TIME</text>
      </g>
    </Scene>
  );
};
