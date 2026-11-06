// Scenes 1-4 are one chart that keeps changing: the hook draws the trajectory, the variables gather around it,
// the chart settles into the application's table + chart, and a second load is drawn on it.
import React from "react";
import { C, FONT, MONO, ease, easeInOut, glow, lerp, ramp } from "../theme";
import { Layout, Rect, useL } from "../layout";
import { Caption, Head, Kicker, Words, fmt, partial, pathOf, scales, useT } from "../components";
import { D, at, line, scene, traj } from "../data";

const S1 = scene("hook"), S2 = scene("variables"), S3 = scene("product"), S4 = scene("compare");
const L2 = line("variables"), L3 = line("product"), L4 = line("compare");

const niceStep = (range: number, n: number) => {
  const raw = range / n, p = Math.pow(10, Math.floor(Math.log10(raw))), m = raw / p;
  return (m < 1.5 ? 1 : m < 3.5 ? 2 : m < 7.5 ? 5 : 10) * p;
};
const mixRect = (a: Rect, b: Rect, k: number): Rect =>
  ({ x: lerp(a.x, b.x, k), y: lerp(a.y, b.y, k), w: lerp(a.w, b.w, k), h: lerp(a.h, b.h, k) });

const Spark: React.FC<{ vals: number[]; w: number; h: number; color: string; k: number; axis?: number }> = ({ vals, w, h, color, k, axis }) => {
  const lo = Math.min(...vals), hi = Math.max(...vals);
  const pts = vals.map((v, i) => [(i / (vals.length - 1)) * w, h - ((v - lo) / (hi - lo || 1)) * h] as [number, number]);
  return (
    <svg width={w} height={h + 4} style={{ overflow: "visible" }}>
      <line x1={0} x2={w} y1={h} y2={h} stroke={C.grid2} strokeWidth={1} />
      <path d={pathOf(partial(pts, k))} fill="none" stroke={color} strokeWidth={axis ? 3.5 : 2.5} strokeLinecap="round" />
      {axis && <text x={0} y={h + axis + 6} fill={C.muted} fontFamily={MONO} fontSize={axis}>0</text>}
      {axis && <text x={w} y={h + axis + 6} fill={C.muted} fontFamily={MONO} fontSize={axis} textAnchor="end">1000 yd</text>}
    </svg>
  );
};

const CHIPS = [
  { name: "WIND", sub: "10 mph full-value crosswind", vals: traj.map((p) => p.wind), value: `${fmt(at(1000).wind)} in`, color: C.cyan },
  { name: "DRAG", sub: "velocity, G7 0.253", vals: traj.map((p) => p.fps), value: `${at(0).fps} → ${at(1000).fps} fps`, color: C.amber },
  { name: "SPIN", sub: "spin drift, 1:10 right twist", vals: traj.map((p) => -p.spin), value: `${fmt(-at(1000).spin)} in`, color: C.cyan },
  { name: "CORIOLIS", sub: "Earth's rotation, 45° N, firing east", vals: traj.map((p) => p.corV), value: `${fmt(at(1000).corV)} in up`, color: C.amber },
];

export const TrajectoryAct: React.FC = () => {
  const t = useT();
  const L = useL();
  if (t > S4.to + 0.4) return null;

  // ---------- geometry: full-width hook, then the plot shrinks to make room for the side panel
  const full: Rect = L.v ? { x: 130, y: 530, w: 850, h: 700 } : { x: 150, y: 270, w: 1640, h: 600 };
  const kShrink = ramp(t, S2.from - 0.05, 0.55, easeInOut);
  const r = mixRect(full, L.plot, kShrink);

  // ---------- the "camera": starts close on the first 250 yd and pulls back as the bullet flies
  const head = lerp(0, 1000, ramp(t, 0.12, 2.6, easeInOut));
  const seen = traj.filter((p) => p.yd <= head + 0.01);
  const lowest = Math.min(...seen.map((p) => p.path));
  const xMax = Math.max(260, Math.min(1000, head * 1.06 + 40));
  const yMin = Math.min(-14, lowest * 1.1 - 6);
  const yMax = Math.max(12, 8 - yMin * 0.04);
  const { X, Y } = scales(r, 0, xMax, yMin, yMax);
  const P = (pts: number[][]) => pts.map(([x, y]) => [X(x), Y(y)] as [number, number]);

  const main = P(traj.map((p) => [p.yd, p.path]));
  const drawn = partial(main, head / 1000);
  const hd = drawn[drawn.length - 1];
  const hp = traj.reduce((a, p) => (Math.abs(p.yd - head) < Math.abs(a.yd - head) ? p : a));

  const appK = ramp(t, S3.from + 0.1, 0.6);                     // application look: labelled axes
  const xs = niceStep(xMax, 8), ys = niceStep(yMax - yMin, 6);
  const gx: number[] = []; for (let v = 0; v <= xMax + 1e-6; v += xs) gx.push(v);
  const gy: number[] = []; for (let v = Math.ceil(yMin / ys) * ys; v <= yMax; v += ys) gy.push(v);

  // second load (scene 4)
  const b = P(D.compare.b.pts);
  const bK = ramp(t, L4.words[1] - 0.05, 1.05, easeInOut);
  const bDrawn = partial(b, bK);

  const actOut = 1 - ramp(t, S4.to - 0.1, 0.18);
  const pulse = (w: number) => { const k = (t - w) / 0.6; return k > 0 && k < 1 ? k : 0; };
  const back = t > S4.from - 0.3;
  const chartO = !L.v ? 1 : back ? ramp(t, S4.from - 0.1, 0.35) : 1 - ramp(t, S2.from - 0.05, 0.3);
  const chartDy = !L.v ? 0 : back ? 40 * (1 - ramp(t, S4.from - 0.1, 0.35)) : -40 * ramp(t, S2.from - 0.05, 0.3);
  const panel = L.v ? { x: L.head.x, y: L.plot.y, w: L.head.maxW } : L.side;   // where chips and table go

  return (
    <div style={{ position: "absolute", inset: 0, opacity: actOut }}>
      <div style={{ position: "absolute", inset: 0, opacity: chartO, transform: `translateY(${chartDy}px)` }}>
      <svg width={L.W} height={L.H} style={{ position: "absolute", inset: 0 }}>
        {/* grid */}
        {gx.map((v) => (
          <g key={`x${v}`} opacity={Math.min(1, (head - v + 60) / 60) * 0.9 + 0.1}>
            <line x1={X(v)} x2={X(v)} y1={r.y} y2={r.y + r.h} stroke={C.grid} strokeWidth={1.2} />
            <text x={X(v)} y={r.y + r.h + 34 * L.s} fill={v <= head ? C.ink2 : C.muted} fontFamily={MONO}
              fontSize={20 * L.s} textAnchor="middle">{v}</text>
          </g>
        ))}
        {gy.map((v) => (
          <g key={`y${v}`} opacity={0.35 + 0.65 * appK}>
            <line x1={r.x} x2={r.x + r.w} y1={Y(v)} y2={Y(v)} stroke={C.grid} strokeWidth={1.2} />
            <text x={r.x - 14} y={Y(v) + 7} fill={C.muted} fontFamily={MONO} fontSize={19 * L.s} textAnchor="end">{fmt(v, 0)}</text>
          </g>
        ))}
        {/* line of sight */}
        <line x1={r.x} x2={r.x + r.w} y1={Y(0)} y2={Y(0)} stroke={C.axis} strokeWidth={1.5} strokeDasharray="8 8" />
        <text x={r.x + r.w - 6} y={Y(0) - 12} fill={C.muted} fontFamily={MONO} fontSize={19 * L.s} textAnchor="end"
          opacity={appK}>line of sight</text>
        {/* axis titles (application look) */}
        <text x={r.x + r.w / 2} y={r.y + r.h + 74 * L.s} fill={C.ink2} fontFamily={FONT} fontSize={22 * L.s}
          fontWeight={600} letterSpacing="0.12em" textAnchor="middle" opacity={appK}>RANGE, YD</text>
        <text transform={`translate(${r.x - 78 * L.s},${r.y + r.h / 2}) rotate(-90)`} fill={C.ink2} fontFamily={FONT}
          fontSize={22 * L.s} fontWeight={600} letterSpacing="0.12em" textAnchor="middle" opacity={appK}>DROP, IN</text>

        {/* the .338 (scene 4) */}
        {bK > 0 && <path d={pathOf(bDrawn)} fill="none" stroke={C.amber} strokeWidth={4} strokeLinecap="round"
          style={{ filter: glow(C.amber, 8) }} />}
        {/* the trajectory */}
        <path d={pathOf(drawn)} fill="none" stroke={C.cyan} strokeWidth={4.5} strokeLinecap="round" style={{ filter: glow(C.cyan, 12) }} />
        {/* head + pulses on the spoken variables */}
        <circle cx={hd[0]} cy={hd[1]} r={7} fill={C.white} style={{ filter: glow(C.cyan, 14) }} />
        {L2.words.map((w, i) => pulse(w) > 0 && (
          <circle key={i} cx={hd[0]} cy={hd[1]} r={10 + 60 * pulse(w)} fill="none" stroke={CHIPS[i].color}
            strokeWidth={3} opacity={1 - pulse(w)} />
        ))}
      </svg>

      {/* live readout at the head (hook + variables) */}
      <div style={{
        position: "absolute", left: Math.min(hd[0] + 20, L.W - 330), top: hd[1] + (hp.path > -40 ? -64 : 18),
        fontFamily: MONO, fontSize: 24 * L.s, color: C.white, opacity: 1 - appK, whiteSpace: "nowrap",
        textShadow: `0 0 14px ${C.bg}`,
      }}>
        <span style={{ color: C.cyan }}>{Math.round(hp.yd)} yd</span>  {fmt(hp.path)} in
      </div>
      </div>

      {/* ---------- scene 1 words */}
      <Head L={L}>
        {t < S2.from + 0.3 && <Words text="MORE THAN A NUMBER" at={0.3} t={t} size={L.head.size} out={S2.from - 0.15} />}
      </Head>

      {/* ---------- scene 2: the variables, word by word, and their curves */}
      {t > S2.from - 0.2 && t < S3.from + 0.5 && (
        <>
          <Head L={L}>
            <div style={{ opacity: 1 - ramp(t, S3.from - 0.1, 0.25), display: "flex", flexWrap: "wrap", columnGap: 18 }}>
              {["WIND", "DRAG", "SPIN", "CORIOLIS"].map((w, i) => (
                <React.Fragment key={w}>
                  {i > 0 && <Words text="·" at={L2.words[i] - 0.08} t={t} size={L.head.size} color={C.muted} />}
                  <Words text={w} at={L2.words[i] - 0.08} t={t} size={L.head.size} />
                </React.Fragment>
              ))}
            </div>
          </Head>
          <div style={{
            position: "absolute", left: panel.x, top: panel.y, width: panel.w,
            display: "grid", gridTemplateColumns: "1fr", gap: L.v ? 28 : 14,
            opacity: 1 - ramp(t, S3.from - 0.05, 0.3), alignContent: "start",
          }}>
            <Kicker text="the same shot, at 1000 yd" t={t} at={S2.from + 0.05} L={L} />
            {CHIPS.map((c, i) => {
              const k = ramp(t, L2.words[i] - 0.05, 0.45);
              return (
                <div key={c.name} style={{
                  opacity: k, transform: `translateX(${(1 - k) * 40}px)`, background: C.bg2, borderLeft: `3px solid ${c.color}`,
                  padding: L.v ? "28px 28px" : "14px 18px", display: "flex", alignItems: "center", gap: L.v ? 28 : 16, borderRadius: 4,
                }}>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontFamily: FONT, fontWeight: 750, fontSize: (L.v ? 36 : 24) * L.s, color: C.white, letterSpacing: "0.06em" }}>{c.name}</div>
                    <div style={{ fontFamily: FONT, fontSize: (L.v ? 20 : 18) * L.s, color: C.muted, marginTop: 2, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{c.sub}</div>
                    <div style={{ fontFamily: MONO, fontSize: (L.v ? 34 : 22) * L.s, color: c.color, marginTop: 8, whiteSpace: "nowrap" }}>{c.value}</div>
                  </div>
                  {L.v ? <Spark vals={c.vals} w={360} h={118} color={c.color} k={ramp(t, L2.words[i], 0.7, ease)} axis={18} />
                    : <Spark vals={c.vals} w={150} h={54} color={c.color} k={ramp(t, L2.words[i], 0.7, ease)} />}
                </div>
              );
            })}
          </div>
        </>
      )}

      {/* ---------- scene 3: the product name, the table */}
      {t > S3.from - 0.1 && t < S4.from + 0.5 && (
        <>
          <Head L={L}>
            <Kicker text="free · open source · desktop" t={t} at={L3.start - 0.15} L={L} out={S4.from - 0.1} />
            <Words text="BALLISTIC CALCULATOR 2" accent={["2"]} at={L3.start - 0.3} t={t} size={L.head.size * 1.05}
              out={S4.from - 0.12} stagger={0.08} />
          </Head>
          <Table L={L} t={t} panel={panel} />
        </>
      )}

      {/* ---------- scene 4: compare two loads */}
      {t > S4.from - 0.1 && (
        <>
          <Head L={L}>
            {t < L4.words[1] + 0.1 && <Words text="TRAJECTORIES" at={S4.from + 0.05} t={t} size={L.head.size} out={L4.words[1] - 0.2} />}
            {t > L4.words[1] - 0.2 && <Words text="COMPARE LOADS" at={L4.words[1] - 0.12} t={t} size={L.head.size} accent={["LOADS"]} />}
          </Head>
          <Legend L={L} t={t} />
        </>
      )}

      <Caption L={L} t={t} a={S3.from + 0.2} b={S4.from}
        text={`${D.demo.name} · standard air · computed by the Ballistic Calculator 2 engine`} />
      <Caption L={L} t={t} a={S4.from + 0.1} b={S4.to}
        text="Both loads: 3 in sight, 300 yd zero, standard air. .338: G7 0.484, 3079 fps." />
    </div>
  );
};

const Table: React.FC<{ L: Layout; t: number; panel: { x: number; y: number; w: number } }> = ({ L, t, panel }) => {
  const rows = [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000].map(at);
  const o = 1 - ramp(t, S4.from - 0.05, 0.3);
  const cell: React.CSSProperties = { textAlign: "right", padding: L.v ? "12px 14px" : "9px 12px", whiteSpace: "nowrap" };
  return (
    <div style={{ position: "absolute", left: panel.x, top: panel.y, width: panel.w, opacity: o }}>
      <table style={{ borderCollapse: "collapse", width: "100%", fontFamily: MONO, fontSize: (L.v ? 25 : 21) * L.s, color: C.white }}>
        <thead>
          <tr style={{ color: C.muted, fontFamily: FONT, fontSize: 16 * L.s, letterSpacing: "0.1em", opacity: ramp(t, S3.from + 0.35, 0.3) }}>
            {["RANGE", "VELOCITY", "DROP", "HOLD"].map((h) => <th key={h} style={{ ...cell, fontWeight: 600 }}>{h}</th>)}
          </tr>
        </thead>
        <tbody>
          {rows.map((p, i) => {
            const k = ramp(t, S3.from + 0.45 + i * 0.07, 0.35);
            return (
              <tr key={p.yd} style={{ opacity: k, transform: `translateY(${(1 - k) * 10}px)`, borderTop: `1px solid ${C.grid2}` }}>
                <td style={{ ...cell, color: C.ink2 }}>{p.yd} yd</td>
                <td style={cell}>{p.fps}</td>
                <td style={{ ...cell, color: p.yd === 1000 ? C.cyan : C.white }}>{fmt(p.path)}</td>
                <td style={{ ...cell, color: C.amber }}>{fmt(p.hold, 2)}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
      <div style={{ fontFamily: MONO, fontSize: (L.v ? 20 : 16) * L.s, color: C.muted, marginTop: 10, opacity: ramp(t, S3.from + 1.2, 0.4) }}>
        velocity fps · drop in · hold mrad
      </div>
    </div>
  );
};

const Legend: React.FC<{ L: Layout; t: number }> = ({ L, t }) => {
  const items = [
    { c: C.cyan, name: D.compare.a.name, v: D.compare.a.pts[D.compare.a.pts.length - 1][1], at: S4.from + 0.15 },
    { c: C.amber, name: D.compare.b.name, v: D.compare.b.pts[D.compare.b.pts.length - 1][1], at: L4.words[1] },
  ];
  return (
    <div style={{ position: "absolute", left: L.side.x, top: L.side.y + (L.v ? 0 : 20), width: L.side.w }}>
      <Kicker text="drop at 1000 yd" t={t} at={S4.from + 0.15} L={L} />
      <div style={{ display: "flex", flexDirection: L.v ? "row" : "column", gap: L.v ? 40 : 0 }}>
      {items.map((it) => {
        const k = ramp(t, it.at, 0.4);
        return (
          <div key={it.name} style={{ opacity: k, transform: `translateX(${(1 - k) * 30}px)`, marginBottom: L.v ? 0 : 26 * L.s, display: "flex", gap: 18, alignItems: "center" }}>
            <div style={{ width: 44, height: 5, background: it.c, borderRadius: 3, boxShadow: `0 0 12px ${it.c}` }} />
            <div>
              <div style={{ fontFamily: FONT, fontSize: 26 * L.s, fontWeight: 650, color: C.white }}>{it.name}</div>
              <div style={{ fontFamily: MONO, fontSize: 30 * L.s, color: it.c, marginTop: 4 }}>{fmt(it.v)} in</div>
            </div>
          </div>
        );
      })}
      </div>
    </div>
  );
};
