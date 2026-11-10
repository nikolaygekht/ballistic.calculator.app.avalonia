// Episode 4 — "What a dot can't do: spin, wind and the turning Earth". Nine scenes; the physics of each effect first,
// then the program's formula for it, then the numbers for our shot.
import { makeEpisode } from "../lib/voiceTiming";
import { TM } from "./timing";
import { S01Hook } from "./S01Hook";
import { S02WhySpin } from "./S02WhySpin";
import { S03Stability } from "./S03Stability";
import { S04Drift } from "./S04Drift";
import { S05Jump } from "./S05Jump";
import { S06Coriolis } from "./S06Coriolis";
import { S07EastWest } from "./S07EastWest";
import { S08Honest } from "./S08Honest";
import { S09Outro } from "./S09Outro";

export const { SceneWithVoice: SceneWithVoice4, Episode: Episode4 } = makeEpisode(
  [S01Hook, S02WhySpin, S03Stability, S04Drift, S05Jump, S06Coriolis, S07EastWest, S08Honest, S09Outro], TM);
