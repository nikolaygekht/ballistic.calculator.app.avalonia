// Scene timing for episode 2, driven by the voice (public/voice/ep02/timing.json, voice/build_timing.py --ep 2).
import timing from "../../public/voice/ep02/timing.json";
import { makeTiming } from "../lib/voiceTiming";

export { LEAD } from "../lib/voiceTiming";
export const TM = makeTiming(timing, { 1: 1.6, 2: 1.4, 4: 1.4, 6: 1.6, 7: 1.4, 8: 1.6, 9: 2.6 });
export const { SCENES, tail, sceneSeconds, sceneFrames, sceneStart, TOTAL_FRAMES, cue, voiceEnd } = TM;
