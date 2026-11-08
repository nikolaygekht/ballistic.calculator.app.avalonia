// Scene timing for episode 1, driven by the voice: public/voice/ep01/timing.json holds each clip's duration and
// word times (voice/build_timing.py). Animation beats are keyed to words with cue().
import timing from "../../public/voice/ep01/timing.json";
import { FPS } from "../theme";

export type Word = { w: string; s: number; e: number };
export type SceneTiming = { n: number; take: string; file: string; dur: number; words: Word[] };
export const SCENES: SceneTiming[] = timing.scenes;

/** Silence before the voice starts in each scene, and after it ends (scene-specific where the picture needs it). */
export const LEAD = 0.7;
const TAIL: Record<number, number> = { 1: 1.4, 2: 1.6, 3: 1.2, 4: 1.6, 5: 2.4, 9: 1.8, 10: 3.0 };
export const tail = (n: number) => TAIL[n] ?? 1.0;

export const sceneSeconds = (n: number) => LEAD + SCENES[n - 1].dur + tail(n);
export const sceneFrames = (n: number) => Math.round(sceneSeconds(n) * FPS);
export const sceneStart = (n: number) => {
  let f = 0;
  for (let i = 1; i < n; i++) f += sceneFrames(i);
  return f;
};
export const TOTAL_FRAMES = sceneStart(SCENES.length + 1);

const norm = (s: string) => s.toLowerCase().replace(/[^a-z0-9']/g, "");

/** Voice time (s, from the scene's voice start) of the first word of `phrase`; `nth` picks a later occurrence. */
export const cue = (n: number, phrase: string, nth = 1, end = false): number => {
  const ws = SCENES[n - 1].words, want = phrase.split(/\s+/).map(norm);
  let seen = 0;
  for (let i = 0; i + want.length <= ws.length; i++) {
    if (want.every((w, j) => norm(ws[i + j].w) === w) && ++seen === nth)
      return end ? ws[i + want.length - 1].e : ws[i].s;
  }
  throw new Error(`cue not found: scene ${n} "${phrase}" #${nth}`);
};
