// Everything plotted comes from the library (datagen/Program.cs -> public/data.json).
// Scene cuts come from ../timing.json; the voice lines' placed times from mix.py (public/placed.json).
import data from "../public/data.json";
import placed from "../public/placed.json";
import timing from "../../timing.json";

export const D = data;
export const LINES = placed.lines;
export const T = timing;

export type SceneId = "hook" | "variables" | "product" | "compare" | "drag" | "reticle" | "montecarlo" | "end";
export const scene = (id: SceneId) => {
  const s = T.scenes.find((x) => x.id === id)!;
  return { from: s.from, to: s.to };
};
export const line = (id: SceneId) => LINES.find((l) => l.scene === id)!;
export const CTA_LOCK = T.scenes.find((x) => x.id === "end")!.cta_locked_from as number;

export const traj = D.demo.traj;
export const at = (yd: number) => traj.find((p) => Math.abs(p.yd - yd) < 0.5)!;
