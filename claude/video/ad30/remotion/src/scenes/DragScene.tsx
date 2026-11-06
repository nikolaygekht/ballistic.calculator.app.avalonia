// Scene 5: measured drag. Lapua's Doppler-radar points appear, then the curve through them; the G7 shape at the
// box BC is drawn dashed for contrast - the bullet's own curve, not a G1/G7 label.
import React from "react";
import { C, FONT, MONO, easeInOut, glow, ramp } from "../theme";
import { useL } from "../layout";
import { Caption, Head, Kicker, Words, partial, scales, smoothPath, useT } from "../components";
import { D, line, scene } from "../data";

const S = scene("drag"), LN = line("drag");
const M0 = 0.5, M1 = 3.0, CD0 = 0.1, CD1 = 0.44;

export const DragScene: React.FC = () => {
  const t = useT();
  const L = useL();
  if (t < S.from - 0.4 || t > S.to + 0.4) return null;
  const r = L.plot;
  const { X, Y } = scales(r, M0, M1, CD0, CD1);
  const o = Math.min(ramp(t, S.from - 0.08, 0.18), 1 - ramp(t, S.to - 0.1, 0.18));

  const pts = D.radar.pts.filter((p) => p.mach >= M0 - 1e-6 && p.mach <= M1 + 1e-6);
  const xy = pts.map((p) => [X(p.mach), Y(p.cd)] as [number, number]);
  const g7 = D.radar.g7.filter((p) => p.mach >= M0 - 1e-6 && p.mach <= M1 + 1e-6).map((p) => [X(p.mach), Y(p.cd)] as [number, number]);
  const dotsAt = LN.start - 0.05, curveAt = LN.start + 0.85, g7At = LN.start + 1.55;
  const curveK = ramp(t, curveAt, 0.9, easeInOut);
  const g7K = ramp(t, g7At, 0.6, easeInOut);
  const gridK = ramp(t, S.from - 0.1, 0.4);
  const ticksX = [0.5, 1.0, 1.5, 2.0, 2.5, 3.0], ticksY = [0.1, 0.2, 0.3, 0.4];

  // label anchors
  const peak = pts.reduce((a, p) => (p.cd > a.cd ? p : a));
  const g7Lab = D.radar.g7.find((p) => p.mach >= 1.75)!;
  const radLab = pts.find((p) => p.mach >= 1.5)!;

  return (
    <div style={{ position: "absolute", inset: 0, opacity: o }}>
      <svg width={L.W} height={L.H} style={{ position: "absolute", inset: 0 }}>
        <g opacity={gridK}>
          {ticksX.map((m) => (
            <g key={m}>
              <line x1={X(m)} x2={X(m)} y1={r.y} y2={r.y + r.h} stroke={C.grid} strokeWidth={1.2} />
              <text x={X(m)} y={r.y + r.h + 34 * L.s} fill={C.muted} fontFamily={MONO} fontSize={20 * L.s} textAnchor="middle">{m.toFixed(1)}</text>
            </g>
          ))}
          {ticksY.map((v) => (
            <g key={v}>
              <line x1={r.x} x2={r.x + r.w} y1={Y(v)} y2={Y(v)} stroke={C.grid} strokeWidth={1.2} />
              <text x={r.x - 14} y={Y(v) + 7} fill={C.muted} fontFamily={MONO} fontSize={19 * L.s} textAnchor="end">{v.toFixed(1)}</text>
            </g>
          ))}
          <line x1={X(1)} x2={X(1)} y1={r.y} y2={r.y + r.h} stroke={C.axis} strokeWidth={1.5} strokeDasharray="6 8" />
          <text x={X(1) + 10} y={r.y + 26 * L.s} fill={C.muted} fontFamily={MONO} fontSize={19 * L.s}>speed of sound</text>
          <text x={r.x + r.w / 2} y={r.y + r.h + 74 * L.s} fill={C.ink2} fontFamily={FONT} fontSize={22 * L.s} fontWeight={600}
            letterSpacing="0.12em" textAnchor="middle">MACH</text>
          <text transform={`translate(${r.x - 78 * L.s},${r.y + r.h / 2}) rotate(-90)`} fill={C.ink2} fontFamily={FONT}
            fontSize={22 * L.s} fontWeight={600} letterSpacing="0.12em" textAnchor="middle">DRAG COEFFICIENT</text>
        </g>
        {/* G7 at one BC, for contrast */}
        {g7K > 0 && (
          <>
            <path d={smoothPath(partial(g7, g7K))} fill="none" stroke={C.ink2} strokeWidth={2.5} strokeDasharray="10 9" opacity={0.75} />
            {/* legend-style label in the empty lower right, clear of both curves */}
            <g opacity={g7K}>
              <line x1={X(1.75)} x2={X(1.75) + 44} y1={Y(0.19)} y2={Y(0.19)} stroke={C.ink2} strokeWidth={2.5} strokeDasharray="10 9" />
              <text x={X(1.75) + 56} y={Y(0.19) + 7} fill={C.ink2} fontFamily={MONO} fontSize={20 * L.s}>G7 at one BC (0.253)</text>
            </g>
          </>
        )}
        {/* the curve through the points */}
        {curveK > 0 && <path d={smoothPath(partial(xy, curveK))} fill="none" stroke={C.cyan} strokeWidth={4.5}
          strokeLinecap="round" style={{ filter: glow(C.cyan, 12) }} />}
        {/* measured points */}
        {xy.map(([x, y], i) => {
          const k = ramp(t, dotsAt + (i / xy.length) * 0.75, 0.25);
          return <circle key={i} cx={x} cy={y} r={7 * k} fill={C.amber} stroke={C.bg} strokeWidth={2.5} opacity={k} />;
        })}
        <text x={X(radLab.mach)} y={Y(radLab.cd) - 34 * L.s} fill={C.cyan} fontFamily={MONO} fontSize={21 * L.s}
          opacity={ramp(t, curveAt + 0.5, 0.4)}>radar-measured curve</text>
      </svg>
      <Head L={L}>
        <Kicker text={L.v ? "custom drag tables" : "custom drag tables · from radar, BCs or velocities"} t={t} at={S.from + 0.05} L={L} />
        <Words text="MEASURED DRAG DATA" accent={["DRAG"]} at={LN.start - 0.15} t={t} size={L.head.size} />
      </Head>
      <Caption L={L} t={t} a={S.from + 0.1} b={S.to + 0.2}
        text={`${D.radar.name} · ${pts.length} points, Mach ${M0}–${M1}`} />
    </div>
  );
};
