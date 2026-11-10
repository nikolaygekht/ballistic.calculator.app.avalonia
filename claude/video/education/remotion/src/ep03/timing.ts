// Scene timing for episode 3, driven by the voice (public/voice/ep03/timing.json, voice/build_timing.py --ep 3).
import timing from "../../public/voice/ep03/timing.json";
import { makeTiming } from "../lib/voiceTiming";

export { LEAD } from "../lib/voiceTiming";
export const TM = makeTiming(timing, { 1: 1.6, 3: 1.8, 4: 3.4 });
export const { SCENES, tail, sceneSeconds, sceneFrames, sceneStart, TOTAL_FRAMES, cue, voiceEnd } = TM;
