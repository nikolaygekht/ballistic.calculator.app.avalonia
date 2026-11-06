import { Easing, interpolate } from "remotion";

export const C = {
  bg: "#0a0d12",
  bg2: "#10151d",
  grid: "#18202a",
  grid2: "#232d3a",
  axis: "#3a4656",
  cyan: "#22d3ee",
  cyanDim: "#0e7f90",
  amber: "#ffb020",
  white: "#f3f6f9",
  ink2: "#a9b4c2",
  muted: "#6b7889",
};

export const FONT = "'Inter Variable', 'Segoe UI', sans-serif";
export const MONO = "'JetBrains Mono Variable', Consolas, monospace";

export const FPS = 30;
export const ease = Easing.bezier(0.22, 1, 0.36, 1);
export const easeInOut = Easing.bezier(0.65, 0, 0.35, 1);

/** 0..1 progress of a ramp that starts at `start` seconds and lasts `dur` seconds (global time). */
export const ramp = (t: number, start: number, dur: number, e = ease) =>
  interpolate(t, [start, start + dur], [0, 1], { extrapolateLeft: "clamp", extrapolateRight: "clamp", easing: e });

/** Visible 0..1 inside [a, b] with fades of `f` seconds at both ends. */
export const window01 = (t: number, a: number, b: number, f = 0.2) =>
  Math.min(ramp(t, a, f, easeInOut), 1 - ramp(t, b - f, f, easeInOut));

export const lerp = (a: number, b: number, k: number) => a + (b - a) * k;

export const glow = (color: string, px = 10) => `drop-shadow(0 0 ${px}px ${color}99)`;
