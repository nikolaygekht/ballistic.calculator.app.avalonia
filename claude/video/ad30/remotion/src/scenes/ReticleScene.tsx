// Scene 6: the chart grid closes into the library's Mil-Dot reticle (MilDotReticle: 6 mrad circle, thin lines to
// 5 mrad, thick posts 5-6, dots on whole mrad, BDC points at -1..-4 mrad). The BDC labels are the ranges where
// the bc2-demo hold equals that many mrad; the box is an 18 in target at 700 yd, drawn to scale at its hold.
import React from "react";
import { C, MONO, easeInOut, glow, lerp, ramp } from "../theme";
import { useL } from "../layout";
import { Caption, Head, Kicker, Words, useT } from "../components";
import { D, line, scene, traj } from "../data";

const S = scene("reticle"), LN = line("reticle");

/** Range (yd) where the hold reaches -m mrad. */
const rangeForHold = (m: number) => {
  for (let i = 1; i < traj.length; i++)
    if (traj[i].yd > 300 && traj[i].hold <= -m) {
      const a = traj[i - 1], b = traj[i];
      return a.yd + ((-m - a.hold) / (b.hold - a.hold)) * (b.yd - a.yd);
    }
  return NaN;
};

export const ReticleScene: React.FC = () => {
  const t = useT();
  const L = useL();
  if (t < S.from - 0.4 || t > S.to + 0.4) return null;
  // no side panel here: the reticle is centred in the frame
  const r = L.v ? { x: 72, y: 490, w: 936, h: 760 } : { x: 150, y: L.plot.y, w: L.W - 300, h: L.plot.h };
  const cx = r.x + r.w / 2, cy = r.y + r.h / 2;
  const u = (Math.min(r.w, r.h) / 2) / 6.6;                       // px per mrad
  const o = Math.min(ramp(t, S.from - 0.08, 0.18), 1 - ramp(t, S.to - 0.1, 0.18));

  const close = ramp(t, S.from - 0.1, 0.55, easeInOut);          // grid lines converge on the centre
  const circ = ramp(t, S.from + 0.2, 0.55, easeInOut);
  const dotsK = (i: number) => ramp(t, S.from + 0.45 + Math.abs(i) * 0.05, 0.25);
  const bdcAt = LN.start + 0.35;
  const tgt = D.reticle.target;
  const tgtHold = D.reticle.holds.find((h) => h.yd === tgt.yd)!.mrad;
  const tgtK = ramp(t, LN.start + 0.9, 0.4);

  const lines = [-3, -2, -1, 1, 2, 3];
  const line1 = (k: number) => 5 * u * k;

  return (
    <div style={{ position: "absolute", inset: 0, opacity: o }}>
      <svg width={L.W} height={L.H} style={{ position: "absolute", inset: 0 }}>
        {/* converging grid */}
        {lines.map((i) => (
          <g key={i} opacity={(1 - close) * 0.9}>
            <line x1={lerp(cx + i * r.w / 7, cx, close)} x2={lerp(cx + i * r.w / 7, cx, close)} y1={r.y} y2={r.y + r.h} stroke={C.grid2} strokeWidth={1.5} />
            <line y1={lerp(cy + i * r.h / 7, cy, close)} y2={lerp(cy + i * r.h / 7, cy, close)} x1={r.x} x2={r.x + r.w} stroke={C.grid2} strokeWidth={1.5} />
          </g>
        ))}
        {/* field circle (6 mrad) */}
        <circle cx={cx} cy={cy} r={6 * u} fill="none" stroke={C.white} strokeWidth={2.5}
          strokeDasharray={`${2 * Math.PI * 6 * u * circ} 99999`} transform={`rotate(-90 ${cx} ${cy})`} opacity={0.9} />
        {/* thin crosshair to 5 mrad */}
        <g stroke={C.white} strokeWidth={2} opacity={close}>
          <line x1={cx - line1(close)} x2={cx + line1(close)} y1={cy} y2={cy} />
          <line y1={cy - line1(close)} y2={cy + line1(close)} x1={cx} x2={cx} />
        </g>
        {/* posts 5..6 mrad (0.2 mrad thick) */}
        <g stroke={C.white} strokeWidth={0.2 * u} opacity={circ}>
          <line x1={cx - 6 * u} x2={cx - 5 * u} y1={cy} y2={cy} /><line x1={cx + 5 * u} x2={cx + 6 * u} y1={cy} y2={cy} />
          <line y1={cy - 6 * u} y2={cy - 5 * u} x1={cx} x2={cx} /><line y1={cy + 5 * u} y2={cy + 6 * u} x1={cx} x2={cx} />
        </g>
        {/* dots on whole mrad */}
        {[-4, -3, -2, -1, 1, 2, 3, 4].map((i) => (
          <g key={i} fill={C.white} opacity={dotsK(i)}>
            <circle cx={cx + i * u} cy={cy} r={0.1 * u * 1.4 * dotsK(i)} />
            <circle cx={cx} cy={cy + i * u} r={0.1 * u * 1.4 * dotsK(i)} />
          </g>
        ))}
        {/* BDC: ranges at -1..-4 mrad */}
        {[1, 2, 3, 4].map((m, i) => {
          const k = ramp(t, bdcAt + i * 0.1, 0.3);
          const y = cy + m * u;
          return (
            <g key={m} opacity={k}>
              <circle cx={cx} cy={y} r={0.16 * u + 3} fill="none" stroke={C.amber} strokeWidth={2.5} style={{ filter: glow(C.amber, 6) }} />
              <text x={cx + 1 * u - 8 + (1 - k) * 20} y={y + 8} fill={C.amber} fontFamily={MONO} fontSize={22 * L.s}
                stroke={C.bg} strokeWidth={6} paintOrder="stroke">
                {Math.round(rangeForHold(m))} yd
              </text>
            </g>
          );
        })}
        {/* 18 in target at 700 yd, at its hold */}
        <g opacity={tgtK}>
          <rect x={cx - (tgt.mrad / 2) * u} y={cy - tgtHold * u - (tgt.mrad / 2) * u} width={tgt.mrad * u} height={tgt.mrad * u}
            fill={`${C.cyan}22`} stroke={C.cyan} strokeWidth={2.5} style={{ filter: glow(C.cyan, 8) }} />
          <text x={cx - (tgt.mrad / 2) * u - 14} y={cy - tgtHold * u + 8} fill={C.cyan} fontFamily={MONO} fontSize={21 * L.s} textAnchor="end"
            stroke={C.bg} strokeWidth={7} paintOrder="stroke">
            {tgt.sizeIn} in · {tgt.yd} yd
          </text>
        </g>
      </svg>
      <Head L={L}>
        <Kicker text="reticle view · BDC from your trajectory" t={t} at={S.from + 0.05} L={L} />
        <Words text="RETICLE VIEW" accent={["VIEW"]} at={LN.start - 0.15} t={t} size={L.head.size} />
      </Head>
      <Caption L={L} t={t} a={S.from + 0.15} b={S.to + 0.2}
        text="Mil-Dot reticle, milliradians · BDC ranges and the target box from the .308 175 gr trajectory" />
    </div>
  );
};
