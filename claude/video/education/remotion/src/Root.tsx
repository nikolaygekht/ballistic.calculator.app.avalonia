import React from "react";
import { Composition, continueRender, delayRender } from "remotion";
import "@fontsource-variable/inter";
import "@fontsource-variable/jetbrains-mono";
import { DecelPilot, PILOT_SECONDS } from "./DecelPilot";
import { Episode1, SceneWithVoice } from "./ep01/Episode1";
import { SCENES, TOTAL_FRAMES, sceneFrames } from "./ep01/timing";
import { Episode2, SceneWithVoice2 } from "./ep02/Episode2";
import * as T2 from "./ep02/timing";
import { Episode3, SceneWithVoice3 } from "./ep03/Episode3";
import * as T3 from "./ep03/timing";
import { Episode4, SceneWithVoice4 } from "./ep04/Episode4";
import * as T4 from "./ep04/timing";
import { Thumb } from "./thumbs/Thumbs";
import { EndCard, END_SECONDS } from "./EndCard";
import { FPS, H, W } from "./theme";

// Hold every frame until both variable fonts are loaded, so no frame renders in a fallback face.
const fonts = delayRender("fonts");
Promise.all([
  document.fonts.load("300 40px 'Inter Variable'"),
  document.fonts.load("500 40px 'Inter Variable'"),
  document.fonts.load("700 40px 'Inter Variable'"),
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
    <Composition id="Ep02" component={Episode2} durationInFrames={T2.TOTAL_FRAMES} fps={FPS} width={W} height={H} />
    {T2.SCENES.map((s) => (
      <Composition key={`e2-${s.n}`} id={`Ep02-S${String(s.n).padStart(2, "0")}`} component={SceneWithVoice2} defaultProps={{ n: s.n }}
        durationInFrames={T2.sceneFrames(s.n)} fps={FPS} width={W} height={H} />
    ))}
    <Composition id="Ep03" component={Episode3} durationInFrames={T3.TOTAL_FRAMES} fps={FPS} width={W} height={H} />
    {T3.SCENES.map((s) => (
      <Composition key={`e3-${s.n}`} id={`Ep03-S${String(s.n).padStart(2, "0")}`} component={SceneWithVoice3} defaultProps={{ n: s.n }}
        durationInFrames={T3.sceneFrames(s.n)} fps={FPS} width={W} height={H} />
    ))}
    <Composition id="Ep04" component={Episode4} durationInFrames={T4.TOTAL_FRAMES} fps={FPS} width={W} height={H} />
    {T4.SCENES.map((s) => (
      <Composition key={`e4-${s.n}`} id={`Ep04-S${String(s.n).padStart(2, "0")}`} component={SceneWithVoice4} defaultProps={{ n: s.n }}
        durationInFrames={T4.sceneFrames(s.n)} fps={FPS} width={W} height={H} />
    ))}
    {[1, 2, 3, 4].map((ep) => (
      <Composition key={`end-${ep}`} id={`Ep0${ep}-End`} component={EndCard} defaultProps={{ ep }} durationInFrames={END_SECONDS * FPS} fps={FPS} width={W} height={H} />
    ))}
    {[1, 2, 3, 4].map((ep) => (
      <Composition key={`th-${ep}`} id={`Thumb0${ep}`} component={Thumb} defaultProps={{ ep }} durationInFrames={1} fps={FPS} width={W} height={H} />
    ))}
    <Composition id="DecelPilot" component={DecelPilot} durationInFrames={PILOT_SECONDS * FPS} fps={FPS} width={W} height={H} />
  </>
);
