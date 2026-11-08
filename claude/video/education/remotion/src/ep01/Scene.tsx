import React from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import { C, FPS, clamp01 } from "../theme";
import { Defs, Vignette, seeded } from "../lib/draw";
import { LEAD, sceneSeconds } from "./timing";

/** Voice time in seconds for the current scene (negative during the lead-in). */
export const useVoiceTime = () => useCurrentFrame() / FPS - LEAD;

/** One scene: the midnight ground, shared defs, a short fade at both ends, and the vignette on top. */
export const Scene: React.FC<{ n: number; children: React.ReactNode; fadeIn?: number; fadeOut?: number }> =
  ({ n, children, fadeIn = 0.35, fadeOut = 0.35 }) => {
    const t = useCurrentFrame() / FPS, total = sceneSeconds(n);
    const o = Math.min(clamp01(t / fadeIn), clamp01((total - t) / fadeOut));
    return (
      <AbsoluteFill style={{ background: C.bg }}>
        <svg width={1920} height={1080} viewBox="0 0 1920 1080" style={{ opacity: o }}>
          <Defs />
          {children}
          <Vignette />
        </svg>
      </AbsoluteFill>
    );
  };

/** Still air streaming past a bullet: horizontal streaks, world-fixed, drifting left by `shift` px. */
const STREAKS = (() => {
  const r = seeded(20261107);
  return Array.from({ length: 70 }, () => ({ u: r(), x: r() * 2600, w: 0.5 + r() * 0.9, a: 0.12 + r() * 0.3, d: r() * 0.8 }));
})();
export const Streaks: React.FC<{ shift: number; y0: number; y1: number; gapY?: number; gap?: number; len?: number; o?: number; count?: number }> =
  ({ shift, y0, y1, gapY, gap = 0, len = 120, o = 1, count = 70 }) => (
    <g strokeLinecap="round" opacity={o}>
      {STREAKS.slice(0, count).map((k, i) => {
        let y = y0 + k.u * (y1 - y0);
        if (gapY !== undefined && Math.abs(y - gapY) < gap) y += y < gapY ? -gap : gap;
        const span = 2600, x = ((((k.x - shift) % span) + span) % span) - 300;
        return <line key={i} x1={x} y1={y} x2={x + len * (0.6 + k.w * 0.5)} y2={y} stroke={C.line} strokeOpacity={k.a} strokeWidth={k.w * 1.6} />;
      })}
    </g>
  );
