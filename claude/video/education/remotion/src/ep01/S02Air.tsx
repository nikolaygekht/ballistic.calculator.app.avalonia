// Scene 2 — what the air does: particles pushed aside, pressure on the nose, friction on the sides, the hole
// behind the base; the three join into one drag arrow.
import React from "react";
import { C, clamp01, ease, lerp, ramp, window01 } from "../theme";
import { Arrow, Bullet, P, Pen, SMK, label, pathD, radius, seeded } from "../lib/draw";
import { Scene, useVoiceTime } from "./Scene";
import { cue } from "./timing";

const N = 2;
const T_PART = cue(N, "particles"), T_PUSH = cue(N, "push"), T_FRONT = cue(N, "In front"), T_BACK = cue(N, "pushes back");
const T_SIDES = cue(N, "Along"), T_RUBS = cue(N, "rubs"), T_BEHIND = cue(N, "behind"), T_HOLE = cue(N, "hole");
const T_PULLS = cue(N, "pulls"), T_ALL = cue(N, "All of this");

const AX = 540, CAL = 110, X0 = 690, LEN = SMK.len * CAL, NOSE = X0 + LEN;
const rAt = (x: number) => {
  const c = (x - X0) / CAL;
  if (c >= 0 && c <= SMK.len) return radius(SMK, c) * CAL;
  if (c > SMK.len) return 6 * Math.exp(-(x - NOSE) / 60);             // a little push ahead of the nose
  return SMK.rBase * CAL * Math.exp(-(X0 - x) / 260);                  // the hole closes slowly behind
};
const DOTS = (() => {
  const r = seeded(7);
  return Array.from({ length: 1100 }, () => {
    const u = r(), y = u < 0.5 ? AX + (r() - 0.5) * 380 : 180 + r() * 720;      // denser near the bullet's path
    return { x: r() * 2400, y, s: 1.4 + r() * 1.4, a: 0.35 + r() * 0.5 };
  });
})();
const inWake = (x: number, y: number) => {
  const d = X0 - x;
  if (d < 0 || d > 330) return 0;
  const half = SMK.rBase * CAL * (1 - d / 330);
  return Math.abs(y - AX) < half ? 1 - d / 330 : 0;
};

export const S02Air: React.FC = () => {
  const v = useVoiceTime();
  const tFlow = T_PUSH - 0.4;
  const dt = Math.max(0, v - tFlow);
  const shift = 230 * (dt < 1 ? (dt * dt) / 2 : dt - 0.5);
  const bulletP = ramp(v, T_PART + 0.4, 1.4);
  const flowOn = ramp(v, tFlow - 0.6, 0.8);

  const front = window01(v, T_FRONT - 0.2, T_ALL + 0.2, 0.4);
  const sides = window01(v, T_SIDES - 0.2, T_ALL + 0.2, 0.4);
  const wake = window01(v, T_BEHIND - 0.2, T_ALL + 0.8, 0.4);
  const merge = ramp(v, T_ALL, 1.0, ease);

  // the three forces, then they slide together behind the base
  const pPress = ramp(v, T_BACK - 0.3, 0.6, ease), pFric = ramp(v, T_RUBS, 0.6, ease), pBase = ramp(v, T_PULLS - 0.2, 0.6, ease);
  const bigX = X0 - 30;

  return (
    <Scene n={N}>
      {/* the air */}
      {DOTS.map((d, i) => {
        const span = 2400;
        const x = ((((d.x - shift) % span) + span) % span) - 240;
        const dy0 = d.y - AX, sgn = dy0 < 0 ? -1 : 1, dd = Math.abs(dy0);
        const yAt = (xx: number) => AX + sgn * (dd + rAt(xx) * Math.exp(-dd / 150) * flowOn * bulletP);
        let y = yAt(x);
        const slope = (yAt(x + 6) - y) / 6;
        // crowding ahead of the nose: squeeze dots towards the tip
        const ahead = x - NOSE;
        let o = d.a * ramp(v, (d.x / 2400) * 1.2, 0.6);
        if (ahead > -10 && ahead < 140 && dd < 140) {
          const k = front * Math.exp(-ahead / 70) * (1 - dd / 140);
          y = lerp(y, AX + sgn * Math.min(dd, 60), k * 0.5);
          o = Math.min(1, o + k * 0.6);
        }
        o *= 1 - 0.85 * wake * inWake(x, y) * 1.4;
        if (x > X0 - 4 && x < NOSE + 2 && Math.abs(y - AX) < rAt(x) - 1) return null;
        const L = 4 + 14 * flowOn, n = 1 / Math.sqrt(1 + slope * slope);
        return <line key={i} x1={x} y1={y} x2={x + L * n} y2={y + L * slope * n} stroke={C.line} strokeWidth={d.s} strokeLinecap="round" opacity={clamp01(o)} />;
      })}
      {/* the crowd at the nose */}
      {Array.from({ length: 70 }, (_, i) => {
        const r1 = ((i * 7919) % 100) / 100, r2 = ((i * 104729) % 100) / 100;
        const a = (r1 - 0.5) * 2.2, rr = 14 + r2 * 70 + 6 * Math.sin(v * 9 + i);
        return <circle key={`c${i}`} cx={NOSE + 6 + rr * Math.cos(a) * 0.8} cy={AX + rr * Math.sin(a)} r={2.2} fill={C.line}
          opacity={front * ramp(v, T_FRONT + r1 * 0.6, 0.4) * (1 - r2 * 0.5)} />;
      })}

      {/* pressure: the nose meets a crowd of air */}
      <g opacity={front * (1 - merge)}>
        <path d={pathD(Array.from({ length: 21 }, (_, i) => {
          const a = (-60 + i * 6) * Math.PI / 180;
          return [NOSE + 30 + 70 * (1 - Math.cos(a)) * 0.3, AX + 90 * Math.sin(a)] as P;
        }))} stroke={C.drag} strokeWidth={2} fill="none" strokeDasharray="5 7" opacity={0.7} />
        {[-36, 0, 36].map((dy, i) => (
          <Arrow key={i} x1={NOSE + 90} y1={AX + dy} x2={NOSE + 14} y2={AX + dy * 0.4} p={pPress} color={C.drag} w={3} head={14} />
        ))}
        <text x={NOSE + 60} y={AX - 120} textAnchor="middle" style={label(pPress, { fontSize: 22, fill: C.drag })}>PRESSURE</text>
      </g>

      {/* friction: the air rubs along the sides */}
      <g opacity={sides * (1 - merge)}>
        {[-1, 1].map((sg) => (
          <g key={sg}>
            {Array.from({ length: 7 }, (_, i) => {
              const x = X0 + 30 + i * 52 + ((-(v * 40)) % 52 + 52) % 52;
              const y = AX + sg * (radius(SMK, (x - X0) / CAL) * CAL + 9);
              return <line key={i} x1={x} y1={y} x2={x - 22} y2={y} stroke={C.drag} strokeWidth={2} opacity={0.75 * pFric} strokeLinecap="round" />;
            })}
            <Arrow x1={X0 + 260} y1={AX + sg * 84} x2={X0 + 110} y2={AX + sg * 84} p={pFric} color={C.drag} w={3} head={13} />
          </g>
        ))}
        <text x={X0 + 190} y={AX - 112} textAnchor="middle" style={label(pFric, { fontSize: 22, fill: C.drag })}>FRICTION</text>
      </g>

      {/* the hole behind the base */}
      <g opacity={wake * (1 - merge)}>
        <Pen pts={[[X0, AX - SMK.rBase * CAL], [X0 - 160, AX - 26], [X0 - 330, AX], [X0 - 160, AX + 26], [X0, AX + SMK.rBase * CAL]]}
          p={ramp(v, T_HOLE - 0.3, 0.8)} color={C.drag} w={2} tip={false} dash="6 8" />
        {[0, 1, 2].map((i) => {
          const cx = X0 - 70 - i * 80, r = 18 - i * 4, a0 = v * 3 + i;
          const pts: P[] = Array.from({ length: 24 }, (_, j) => {
            const a = a0 + (j / 24) * Math.PI * 1.6, rr = r * (0.4 + (0.6 * j) / 24);
            return [cx + rr * Math.cos(a), AX + (i % 2 ? 1 : -1) * 6 + rr * Math.sin(a)];
          });
          return <path key={i} d={pathD(pts)} stroke={C.drag} strokeWidth={1.6} fill="none" opacity={0.6 * ramp(v, T_HOLE, 0.6)} />;
        })}
        <text x={X0 - 170} y={AX + 100} textAnchor="middle" style={label(ramp(v, T_HOLE, 0.5), { fontSize: 22, fill: C.drag })}>A HOLE IN THE AIR</text>
        <Arrow x1={X0 - 20} y1={AX} x2={X0 - 140} y2={AX} p={pBase} color={C.drag} w={3} head={14} />
      </g>

      {/* all together */}
      {merge > 0 && (
        <>
          {[[NOSE + 90, AX - 36], [X0 + 260, AX - 84], [X0 - 20, AX], [X0 + 260, AX + 84], [NOSE + 90, AX + 36]].map(([x, y], i) => (
            <Arrow key={i} x1={lerp(x, bigX, merge)} y1={lerp(y, AX, merge)} x2={lerp(x - 80, bigX - 80, merge)} y2={lerp(y, AX, merge)}
              color={C.drag} w={3} head={13} opacity={1 - merge} />
          ))}
          <g filter="url(#glow)">
            <Arrow x1={bigX} y1={AX} x2={bigX - 360} y2={AX} p={ramp(v, T_ALL + 0.4, 0.8, ease)} color={C.drag} w={6} head={26} />
          </g>
          <text x={bigX - 180} y={AX - 40} textAnchor="middle" style={label(ramp(v, cue(N, "drag"), 0.4), { fontSize: 40, fill: C.drag, letterSpacing: "0.3em" })}>DRAG</text>
        </>
      )}

      <Bullet x0={X0} y={AX} cal={CAL} p={bulletP} />
      <text x={960} y={150} textAnchor="middle" style={label(window01(v, 0, T_PUSH + 0.3, 0.4), { fontSize: 30 })}>AIR IS NOT EMPTY</text>
    </Scene>
  );
};
