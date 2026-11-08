// Scene 5 — watch it slow down: the pilot shot, timed so the flight starts at "The bullet slows down", crosses
// Mach 1 on "930 metres" and reaches 1600 m on "one thousand six hundred".
import React from "react";
import { AbsoluteFill, useCurrentFrame } from "remotion";
import { Decel } from "../DecelPilot";
import { FPS, clamp01 } from "../theme";
import { LEAD, cue, sceneSeconds } from "./timing";

const N = 5;
export const S05Watch: React.FC = () => {
  const t = useCurrentFrame() / FPS, total = sceneSeconds(N);
  const o = Math.min(clamp01(t / 0.35), clamp01((total - t) / 0.35));
  return (
    <AbsoluteFill style={{ opacity: o, background: "#0B1320" }}>
      <Decel
        f0={LEAD + cue(N, "The bullet slows")}
        fm={LEAD + cue(N, "930") + 0.2}
        f1={LEAD + cue(N, "1600")}
        capMuzzle={[LEAD + cue(N, "Every tenth") - 0.2, LEAD + cue(N, "The bullet slows") - 0.1]}
      />
    </AbsoluteFill>
  );
};
