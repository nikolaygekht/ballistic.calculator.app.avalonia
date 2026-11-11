// Scene timing for episode 4, driven by the voice (public/voice/ep04/timing.json, voice/build_timing.py --ep 4).
import timing from "../../public/voice/ep04/timing.json";
import { makeTiming } from "../lib/voiceTiming";

export { LEAD } from "../lib/voiceTiming";
export const TM = makeTiming(timing, { 1: 3.0, 2: 1.4, 3: 1.6, 4: 1.6, 5: 1.6, 6: 1.4, 7: 1.6, 8: 1.6, 9: 3.4 });
export const { SCENES, tail, sceneSeconds, sceneFrames, sceneStart, TOTAL_FRAMES, cue, voiceEnd } = TM;
