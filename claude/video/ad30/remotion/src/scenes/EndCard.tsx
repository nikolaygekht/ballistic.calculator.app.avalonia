// Scene 8: the scatter collapses into a point that draws the trajectory once more, under the name; then the
// call to action. Everything is in place by CTA_LOCK and holds to the last frame.
import React from "react";
import { C, FONT, MONO, easeInOut, glow, lerp, ramp } from "../theme";
import { useL } from "../layout";
import { Words, partial, pathOf, useT } from "../components";
import { CTA_LOCK, D, line, scene, traj } from "../data";
import { mcRect } from "./MonteCarloScene";

const S = scene("end"), LN = line("end");
const URL = "github.com/nikolaygekht/ballistic.calculator.app.avalonia";

export const EndCard: React.FC = () => {
  const t = useT();
  const L = useL();
  if (t < S.from - 0.3) return null;
  const v = L.v;

  // collapsing scatter
  const col = ramp(t, S.from - 0.05, 0.5, easeInOut);
  const r = mcRect(L);
  const mcx = r.x + (v ? r.w / 2 : r.w * 0.36), mcy = r.y + r.h / 2, mu = (r.h / 2) / 24;
  const scatter = col < 1 && (
    <g opacity={1 - ramp(t, S.from + 0.35, 0.2)}>
      {D.hit.impacts.slice(0, 500).map(([x, y], i) => (
        <circle key={i} cx={lerp(mcx + x * mu, mcx, col)} cy={lerp(mcy - y * mu, mcy, col)} r={3} fill={C.cyan} opacity={0.7} />
      ))}
    </g>
  );

  // the arc under the title
  const arc = v ? { x: 72, y: 860, w: 936, h: 160 } : { x: 120, y: 560, w: 1680, h: 150 };
  const ymin = -360.51, ymax = 6;
  const pts = traj.map((p) => [arc.x + (p.yd / 1000) * arc.w, arc.y + ((ymax - p.path) / (ymax - ymin)) * arc.h] as [number, number]);
  const arcK = ramp(t, S.from + 0.25, 1.3, easeInOut);
  const drawn = partial(pts, arcK);
  const hd = drawn[drawn.length - 1];

  const titleSize = v ? 104 : 120;
  const lock = (a: number) => Math.min(a, CTA_LOCK - 0.45);      // every element finishes by the lock
  const tag = (txt: string, a: number, color: string, size: number, mono = false) => {
    const k = ramp(t, lock(a), 0.4);
    return (
      <div style={{ opacity: k, transform: `translateY(${(1 - k) * 14}px)`, fontFamily: mono ? MONO : FONT, fontSize: size,
        color, fontWeight: mono ? 500 : 700, letterSpacing: mono ? "0.01em" : "0.14em" }}>{txt}</div>
    );
  };
  const free = LN.words[1];

  return (
    <div style={{ position: "absolute", inset: 0 }}>
      <svg width={L.W} height={L.H} style={{ position: "absolute", inset: 0 }}>
        {scatter}
        {arcK > 0 && <path d={pathOf(drawn)} fill="none" stroke={C.cyan} strokeWidth={4} strokeLinecap="round" style={{ filter: glow(C.cyan, 12) }} />}
        {arcK > 0 && arcK < 1 && <circle cx={hd[0]} cy={hd[1]} r={6} fill={C.white} style={{ filter: glow(C.cyan, 14) }} />}
      </svg>
      <div style={{ position: "absolute", left: v ? 72 : 120, top: v ? 330 : 210, width: v ? 936 : 1680 }}>
        {tag("ADVANCED EXTERNAL BALLISTICS", LN.start, C.amber, v ? 30 : 30)}
        <div style={{ height: 18 }} />
        <Words text="BALLISTIC CALCULATOR 2" accent={["2"]} at={LN.start + 0.05} t={t} size={titleSize} weight={800} stagger={0.07} />
      </div>
      <div style={{ position: "absolute", left: v ? 72 : 120, top: v ? 1080 : 780, width: v ? 900 : 1680,
        display: "flex", flexDirection: v ? "column" : "row", gap: v ? 26 : 70, alignItems: v ? "flex-start" : "flex-end" }}>
        <div style={{ display: "flex", flexDirection: "column", gap: 14 }}>
          {tag("FREE & OPEN SOURCE", free - 0.05, C.white, v ? 46 : 44)}
          {tag("WINDOWS · LINUX · macOS", free + 0.25, C.ink2, v ? 32 : 30)}
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
          {tag("GET IT ON GITHUB", free + 0.5, C.cyan, v ? 40 : 38)}
          {v ? (
            <>
              {tag("github.com/nikolaygekht/", free + 0.6, C.white, 32, true)}
              {tag("ballistic.calculator.app.avalonia", free + 0.65, C.white, 32, true)}
            </>
          ) : tag(URL, free + 0.6, C.white, 28, true)}
        </div>
      </div>
    </div>
  );
};
