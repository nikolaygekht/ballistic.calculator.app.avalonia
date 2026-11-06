import React from "react";
import { useCurrentFrame } from "remotion";
import { C, FONT, FPS, MONO, ease, ramp } from "./theme";
import { Layout, Rect } from "./layout";

export const useT = () => useCurrentFrame() / FPS;   // global time, seconds (scenes are not wrapped in Sequences)

/** Big words. Each word slides up out of a mask, starting at `at` with a small stagger. */
export const Words: React.FC<{
  text: string; at: number; t: number; size: number; color?: string; accent?: string[]; weight?: number;
  stagger?: number; style?: React.CSSProperties; out?: number;
}> = ({ text, at, t, size, color = C.white, accent = [], weight = 760, stagger = 0.06, style, out }) => {
  const words = text.split(" ");
  const o = out === undefined ? 1 : 1 - ramp(t, out, 0.22);
  return (
    <div style={{ display: "flex", flexWrap: "wrap", columnGap: size * 0.28, rowGap: size * 0.05, opacity: o, ...style }}>
      {words.map((w, i) => {
        const k = ramp(t, at + i * stagger, 0.42);
        return (
          <span key={i} style={{ overflow: "hidden", display: "inline-block", lineHeight: 1.08, paddingBottom: size * 0.04 }}>
            <span style={{
              display: "inline-block", transform: `translateY(${(1 - k) * 105}%)`, fontFamily: FONT, fontWeight: weight,
              fontSize: size, letterSpacing: "-0.01em", color: accent.includes(w) ? C.cyan : color,
            }}>{w}</span>
          </span>
        );
      })}
    </div>
  );
};

export const Head: React.FC<{ L: Layout; children: React.ReactNode }> = ({ L, children }) => (
  <div style={{ position: "absolute", left: L.head.x, top: L.head.y, width: L.head.maxW }}>{children}</div>
);

export const Kicker: React.FC<{ text: string; t: number; at: number; L: Layout; color?: string; out?: number }> =
  ({ text, t, at, L, color = C.amber, out }) => {
    const k = ramp(t, at, 0.35);
    const o = out === undefined ? 1 : 1 - ramp(t, out, 0.2);
    return (
      <div style={{
        fontFamily: MONO, fontSize: 22 * L.s, letterSpacing: "0.18em", color, textTransform: "uppercase",
        opacity: k * o, transform: `translateX(${(1 - k) * -16}px)`, marginBottom: 14 * L.s, fontWeight: 600,
      }}>{text}</div>
    );
  };

/** Small source line at the bottom: what the plotted numbers are. */
export const Caption: React.FC<{ text: string; t: number; a: number; b: number; L: Layout }> = ({ text, t, a, b, L }) => {
  const o = Math.min(ramp(t, a, 0.3), 1 - ramp(t, b - 0.25, 0.25));
  return (
    <div style={{
      position: "absolute", left: L.caption.x, top: L.caption.y, width: L.caption.maxW, fontFamily: MONO,
      fontSize: L.caption.size, color: C.muted, opacity: o, letterSpacing: "0.02em",
    }}>{text}</div>
  );
};

/** Linear scales for a rect. */
export const scales = (r: Rect, x0: number, x1: number, y0: number, y1: number) => ({
  X: (x: number) => r.x + ((x - x0) / (x1 - x0)) * r.w,
  Y: (y: number) => r.y + r.h - ((y - y0) / (y1 - y0)) * r.h,
});

export const pathOf = (pts: [number, number][]) =>
  pts.map(([x, y], i) => `${i ? "L" : "M"}${x.toFixed(1)},${y.toFixed(1)}`).join("");

/** Catmull-Rom through the points, as cubic Beziers (smooth, passes through every point). */
export const smoothPath = (pts: [number, number][]) => {
  if (pts.length < 3) return pathOf(pts);
  let d = `M${pts[0][0].toFixed(1)},${pts[0][1].toFixed(1)}`;
  for (let i = 0; i < pts.length - 1; i++) {
    const p0 = pts[Math.max(0, i - 1)], p1 = pts[i], p2 = pts[i + 1], p3 = pts[Math.min(pts.length - 1, i + 2)];
    const c1 = [p1[0] + (p2[0] - p0[0]) / 6, p1[1] + (p2[1] - p0[1]) / 6];
    const c2 = [p2[0] - (p3[0] - p1[0]) / 6, p2[1] - (p3[1] - p1[1]) / 6];
    d += `C${c1[0].toFixed(1)},${c1[1].toFixed(1)} ${c2[0].toFixed(1)},${c2[1].toFixed(1)} ${p2[0].toFixed(1)},${p2[1].toFixed(1)}`;
  }
  return d;
};

/** Points of a polyline up to fraction k of its x extent, with the interpolated head. */
export const partial = (pts: [number, number][], k: number): [number, number][] => {
  if (k <= 0) return [pts[0]];
  if (k >= 1) return pts;
  const xEnd = pts[0][0] + (pts[pts.length - 1][0] - pts[0][0]) * k;
  const out: [number, number][] = [];
  for (let i = 0; i < pts.length; i++) {
    if (pts[i][0] <= xEnd) { out.push(pts[i]); continue; }
    const a = pts[i - 1], b = pts[i], f = (xEnd - a[0]) / (b[0] - a[0]);
    out.push([xEnd, a[1] + (b[1] - a[1]) * f]);
    break;
  }
  return out;
};

export const fmt = (v: number, d = 1) => {
  const s = Math.abs(v).toLocaleString("en-US", { minimumFractionDigits: d, maximumFractionDigits: d });
  return (v < -0.00001 ? "−" : "") + s;
};
