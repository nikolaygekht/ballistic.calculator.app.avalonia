// Episode 1 — "Drag: where the velocity goes". Ten scenes back to back, each with its own voice clip.
import React from "react";
import { AbsoluteFill, Audio, Sequence, staticFile } from "remotion";
import { C } from "../theme";
import { LEAD, SCENES, sceneFrames, sceneStart } from "./timing";
import { FPS } from "../theme";
import { S01Hook } from "./S01Hook";
import { S02Air } from "./S02Air";
import { S03Shock } from "./S03Shock";
import { S04Formula } from "./S04Formula";
import { S05Watch } from "./S05Watch";
import { S06CdCurve } from "./S06CdCurve";
import { S07Standard } from "./S07Standard";
import { S08TwoBCs } from "./S08TwoBCs";
import { S09Breaks } from "./S09Breaks";
import { S10Outro } from "./S10Outro";

export const SCENE_COMPONENTS: React.FC[] = [S01Hook, S02Air, S03Shock, S04Formula, S05Watch, S06CdCurve, S07Standard, S08TwoBCs, S09Breaks, S10Outro];

/** One scene with its voice, starting LEAD seconds in. */
export const SceneWithVoice: React.FC<{ n: number }> = ({ n }) => {
  const Comp = SCENE_COMPONENTS[n - 1];
  return (
    <AbsoluteFill style={{ background: C.bg }}>
      <Comp />
      <Sequence from={Math.round(LEAD * FPS)} layout="none">
        <Audio src={staticFile(SCENES[n - 1].file)} />
      </Sequence>
    </AbsoluteFill>
  );
};

export const Episode1: React.FC = () => (
  <AbsoluteFill style={{ background: C.bg }}>
    {SCENES.map((s) => (
      <Sequence key={s.n} from={sceneStart(s.n)} durationInFrames={sceneFrames(s.n)} name={`S${String(s.n).padStart(2, "0")}`}>
        <SceneWithVoice n={s.n} />
      </Sequence>
    ))}
  </AbsoluteFill>
);
