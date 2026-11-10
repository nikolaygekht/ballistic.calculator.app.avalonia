// Voice-driven scene timing shared by episodes 2+: public/voice/epNN/timing.json holds each clip's duration and word
// times (voice/build_timing.py --ep N). Animation beats are keyed to words with cue(). makeEpisode() strings the
// scenes together, each with its clip, and tells the shared kit how long the current scene is (TimingContext).
import React, { createContext } from "react";
import { AbsoluteFill, Audio, Sequence, staticFile } from "remotion";
import { C, FPS } from "../theme";

export type Word = { w: string; s: number; e: number };
export type SceneTiming = { n: number; take: string; file: string; dur: number; words: Word[] };

/** Silence before the voice starts in each scene. */
export const LEAD = 0.7;

const norm = (s: string) => s.toLowerCase().replace(/[^a-z0-9']/g, "");

/** Timing for one episode. `tails`: silence after the voice, per scene, where the picture needs more than 1 s. */
export const makeTiming = (timing: { scenes: SceneTiming[] }, tails: Record<number, number>) => {
  const SCENES: SceneTiming[] = timing.scenes;
  const tail = (n: number) => tails[n] ?? 1.0;
  const sceneSeconds = (n: number) => LEAD + SCENES[n - 1].dur + tail(n);
  const sceneFrames = (n: number) => Math.round(sceneSeconds(n) * FPS);
  const sceneStart = (n: number) => {
    let f = 0;
    for (let i = 1; i < n; i++) f += sceneFrames(i);
    return f;
  };
  /** Voice time (s, from the scene's voice start) of the first word of `phrase`; `nth` picks a later occurrence.
   *  Whisper splits "2.3" into "2" ".3", so a number cue is written the same way ("2 3"). */
  const cue = (n: number, phrase: string, nth = 1, end = false): number => {
    const ws = SCENES[n - 1].words.filter((w) => norm(w.w) !== ""), want = phrase.split(/\s+/).map(norm);
    let seen = 0;
    for (let i = 0; i + want.length <= ws.length; i++) {
      if (want.every((w, j) => norm(ws[i + j].w) === w) && ++seen === nth)
        return end ? ws[i + want.length - 1].e : ws[i].s;
    }
    throw new Error(`cue not found: scene ${n} "${phrase}" #${nth}`);
  };
  /** Voice time at which scene `n`'s voice ends. */
  const voiceEnd = (n: number) => SCENES[n - 1].dur;
  return { SCENES, tail, sceneSeconds, sceneFrames, sceneStart, TOTAL_FRAMES: sceneStart(SCENES.length + 1), cue, voiceEnd };
};
export type Timing = ReturnType<typeof makeTiming>;

/** The timing of the episode being rendered, for the shared scene frame. */
export const TimingContext = createContext<Timing | null>(null);

/** The episode composition and its per-scene compositions (one scene with its voice, starting LEAD seconds in). */
export const makeEpisode = (scenes: React.FC[], tm: Timing) => {
  const SceneWithVoice: React.FC<{ n: number }> = ({ n }) => {
    const Comp = scenes[n - 1];
    return (
      <TimingContext.Provider value={tm}>
        <AbsoluteFill style={{ background: C.bg }}>
          <Comp />
          <Sequence from={Math.round(LEAD * FPS)} layout="none">
            <Audio src={staticFile(tm.SCENES[n - 1].file)} />
          </Sequence>
        </AbsoluteFill>
      </TimingContext.Provider>
    );
  };
  const Episode: React.FC = () => (
    <AbsoluteFill style={{ background: C.bg }}>
      {tm.SCENES.map((s) => (
        <Sequence key={s.n} from={tm.sceneStart(s.n)} durationInFrames={tm.sceneFrames(s.n)} name={`S${String(s.n).padStart(2, "0")}`}>
          <SceneWithVoice n={s.n} />
        </Sequence>
      ))}
    </AbsoluteFill>
  );
  return { SceneWithVoice, Episode };
};
