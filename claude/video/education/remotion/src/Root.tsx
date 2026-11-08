import React from "react";
import { Composition, continueRender, delayRender } from "remotion";
import "@fontsource-variable/inter";
import "@fontsource-variable/jetbrains-mono";
import { DecelPilot, PILOT_SECONDS } from "./DecelPilot";
import { Episode1, SceneWithVoice } from "./ep01/Episode1";
import { SCENES, TOTAL_FRAMES, sceneFrames } from "./ep01/timing";
import { FPS, H, W } from "./theme";

// Hold every frame until both variable fonts are loaded, so no frame renders in a fallback face.
const fonts = delayRender("fonts");
Promise.all([
  document.fonts.load("300 40px 'Inter Variable'"),
  document.fonts.load("500 40px 'Inter Variable'"),
  document.fonts.load("400 40px 'JetBrains Mono Variable'"),
  document.fonts.load("700 40px 'JetBrains Mono Variable'"),
]).then(() => continueRender(fonts));

export const Root: React.FC = () => (
  <>
    <Composition id="Ep01" component={Episode1} durationInFrames={TOTAL_FRAMES} fps={FPS} width={W} height={H} />
    {SCENES.map((s) => (
      <Composition key={s.n} id={`Ep01-S${String(s.n).padStart(2, "0")}`} component={SceneWithVoice} defaultProps={{ n: s.n }}
        durationInFrames={sceneFrames(s.n)} fps={FPS} width={W} height={H} />
    ))}
    <Composition id="DecelPilot" component={DecelPilot} durationInFrames={PILOT_SECONDS * FPS} fps={FPS} width={W} height={H} />
  </>
);
