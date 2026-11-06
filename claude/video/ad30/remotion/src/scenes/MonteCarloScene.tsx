// Scene 7: the library's Monte Carlo (HitProbability.Estimate, fixed seed) - impacts gather around the circular
// vital zone; the result is a simulated estimate, stated as such.
import React from "react";
import { C, FONT, MONO, easeInOut, glow, ramp } from "../theme";
import { Layout, useL } from "../layout";
import { Caption, Head, Kicker, Words, useT } from "../components";
import { D, line, scene } from "../data";

const S = scene("montecarlo"), LN = line("montecarlo");
const H = D.hit;
/** The scatter area; vertical gets a taller one (the stats sit below it). Shared with the end card's collapse. */
export const mcRect = (L: Layout) => (L.v ? { x: 72, y: 490, w: 936, h: 700 } : L.plot);

export const MonteCarloScene: React.FC = () => {
  const t = useT();
  const L = useL();
  if (t < S.from - 0.4 || t > S.to + 0.6) return null;
  const r = mcRect(L);
  const cx = r.x + (L.v ? r.w / 2 : r.w * 0.36), cy = r.y + r.h / 2;
  const u = (r.h / 2) / 24;                                      // px per inch, +-24 in
  const o = Math.min(ramp(t, S.from - 0.08, 0.18), 1 - ramp(t, S.to - 0.05, 0.25));

  const ringK = ramp(t, S.from, 0.5, easeInOut);
  const t0 = LN.start - 0.1, t1 = LN.start + 2.3;
  const shown = Math.floor(H.impacts.length * Math.pow(ramp(t, t0, t1 - t0, (x) => x), 1.6));
  const hits = H.impacts.slice(0, shown).filter(([x, y]) => x * x + y * y <= (H.zoneIn / 2) ** 2).length;
  const resK = ramp(t, t1 - 0.2, 0.45);
  const second = t > LN.start + 1.2;

  return (
    <div style={{ position: "absolute", inset: 0, opacity: o }}>
      <svg width={L.W} height={L.H} style={{ position: "absolute", inset: 0 }}>
        {[8, 16, 24].map((rr) => (
          <circle key={rr} cx={cx} cy={cy} r={rr * u} fill="none" stroke={C.grid2} strokeWidth={1.2} opacity={ringK} />
        ))}
        <line x1={cx - 24 * u} x2={cx + 24 * u} y1={cy} y2={cy} stroke={C.grid2} strokeWidth={1.2} opacity={ringK} />
        <line y1={cy - 24 * u} y2={cy + 24 * u} x1={cx} x2={cx} stroke={C.grid2} strokeWidth={1.2} opacity={ringK} />
        {H.impacts.slice(0, shown).map(([x, y], i) => {
          const hit = x * x + y * y <= (H.zoneIn / 2) ** 2;
          return <circle key={i} cx={cx + x * u} cy={cy - y * u} r={hit ? 3.6 : 3} fill={hit ? C.cyan : C.muted} opacity={hit ? 0.95 : 0.55} />;
        })}
        <circle cx={cx} cy={cy} r={(H.zoneIn / 2) * u} fill="none" stroke={C.amber} strokeWidth={3.5}
          strokeDasharray={`${2 * Math.PI * (H.zoneIn / 2) * u * ringK} 9999`} transform={`rotate(-90 ${cx} ${cy})`}
          style={{ filter: glow(C.amber, 8) }} />
        <line x1={cx + (H.zoneIn / 2) * u * 0.71} y1={cy - (H.zoneIn / 2) * u * 0.71} x2={cx + 17 * u} y2={cy - 22 * u}
          stroke={C.amber} strokeWidth={1.5} opacity={ringK * 0.8} />
        <text x={cx + 17 * u + 8} y={cy - 22 * u - 8} fill={C.amber} fontFamily={MONO} fontSize={21 * L.s}
          opacity={ringK}>{H.zoneIn} in vital zone</text>
      </svg>

      <div style={{
        position: "absolute", fontFamily: MONO, color: C.white,
        ...(L.v ? { left: L.side.x, top: L.side.y, width: L.side.w } : { left: r.x + r.w * 0.78, top: r.y + 60, width: 520 }),
      }}>
        <div style={{ display: "flex", flexDirection: L.v ? "row" : "column", gap: L.v ? 44 : 22, alignItems: L.v ? "flex-end" : "flex-start" }}>
          <div>
            <div style={{ fontFamily: FONT, fontSize: 18 * L.s, letterSpacing: "0.14em", color: C.muted }}>SIMULATED SHOTS</div>
            <div style={{ fontSize: 46 * L.s, marginTop: 4 }}>{shown.toLocaleString("en-US")}</div>
          </div>
          <div>
            <div style={{ fontFamily: FONT, fontSize: 18 * L.s, letterSpacing: "0.14em", color: C.muted }}>INSIDE THE ZONE</div>
            <div style={{ fontSize: 46 * L.s, marginTop: 4, color: C.cyan }}>{hits.toLocaleString("en-US")}</div>
          </div>
          <div style={{ opacity: resK, transform: `translateY(${(1 - resK) * 12}px)`, marginTop: L.v ? 0 : 4 }}>
            <div style={{ fontFamily: FONT, fontSize: 18 * L.s, letterSpacing: "0.14em", color: C.muted }}>SINGLE-SHOT ESTIMATE</div>
            <div style={{ fontSize: 64 * L.s, marginTop: 4, color: C.white, fontWeight: 600, lineHeight: 1 }}>{(H.probability * 100).toFixed(1)} %</div>
          </div>
        </div>
        <div style={{ fontFamily: FONT, fontSize: 18 * L.s, color: C.ink2, marginTop: 12, opacity: resK }}>
          from {H.simulated.toLocaleString("en-US")} simulated shots · an estimate, not a promise
        </div>
      </div>

      <Head L={L}>
        <Kicker text="hit probability · Monte Carlo" t={t} at={S.from + 0.05} L={L} />
        {!second && <Words text="MONTE CARLO" at={LN.start - 0.2} t={t} size={L.head.size} out={LN.start + 1.05} />}
        {second && <Words text="HIT PROBABILITY" accent={["PROBABILITY"]} at={LN.start + 1.15} t={t} size={L.head.size} />}
      </Head>
      <Caption L={L} t={t} a={S.from + 0.15} b={S.to + 0.2}
        text={`${H.rangeYd} yd · ${H.groupMoa} MOA group · ${H.windMph} mph wind read ±25 % · range ±5 % · MV ±1 % · .308 175 gr`} />
    </div>
  );
};
