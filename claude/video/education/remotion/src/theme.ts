import { Easing, interpolate } from "remotion";

// Line-art look: deep midnight ground, one champagne line, one coral accent for the drag force.
export const C = {
  bg: "#0B1320",
  line: "#E9D7A5",
  dim: "rgba(233,215,165,0.35)",
  faint: "rgba(233,215,165,0.16)",
  drag: "#FF8A65",
  dragDim: "rgba(255,138,101,0.35)",
};

export const FONT = "'Inter Variable', 'Segoe UI', sans-serif";
export const MONO = "'JetBrains Mono Variable', Consolas, monospace";
export const FPS = 30;
export const W = 1920;
export const H = 1080;

export const ease = Easing.bezier(0.22, 1, 0.36, 1);
export const easeInOut = Easing.bezier(0.65, 0, 0.35, 1);

/** 0..1 progress of a ramp that starts at `start` s and lasts `dur` s. */
export const ramp = (t: number, start: number, dur: number, e = easeInOut) =>
  interpolate(t, [start, start + dur], [0, 1], { extrapolateLeft: "clamp", extrapolateRight: "clamp", easing: e });

/** Visible 0..1 inside [a, b] with fades of `f` s at both ends. */
export const window01 = (t: number, a: number, b: number, f = 0.3) =>
  Math.min(ramp(t, a, f), 1 - ramp(t, b - f, f));

export const lerp = (a: number, b: number, k: number) => a + (b - a) * k;
export const clamp01 = (v: number) => Math.max(0, Math.min(1, v));
