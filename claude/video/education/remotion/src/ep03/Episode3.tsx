// Episode 3 — "The wind: how the air moves the dot". A new hook, episode 2's two wind scenes and its outro
// (voice reused), drawn with episode 2's kit and data.
import { makeEpisode } from "../lib/voiceTiming";
import { TM } from "./timing";
import { S01Hook } from "./S01Hook";
import { S02WindAir } from "./S02WindAir";
import { S03WindSide } from "./S03WindSide";
import { S04Outro } from "./S04Outro";

export const { SceneWithVoice: SceneWithVoice3, Episode: Episode3 } = makeEpisode([S01Hook, S02WindAir, S03WindSide, S04Outro], TM);
