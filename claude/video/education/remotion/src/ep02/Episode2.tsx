// Episode 2 — "How a dot flies: the point-mass model". Nine scenes back to back, each with its own voice clip.
import { makeEpisode } from "../lib/voiceTiming";
import { TM } from "./timing";
import { S01Hook } from "./S01Hook";
import { S02Six } from "./S02Six";
import { S03Forces } from "./S03Forces";
import { S04DragArrow } from "./S04DragArrow";
import { S05Add } from "./S05Add";
import { S06Step } from "./S06Step";
import { S07StepSize } from "./S07StepSize";
import { S08Flight } from "./S08Flight";
import { S09Outro } from "./S09Outro";

export const { SceneWithVoice: SceneWithVoice2, Episode: Episode2 } =
  makeEpisode([S01Hook, S02Six, S03Forces, S04DragArrow, S05Add, S06Step, S07StepSize, S08Flight, S09Outro], TM);
