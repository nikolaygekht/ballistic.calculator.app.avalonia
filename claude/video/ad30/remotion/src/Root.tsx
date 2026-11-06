import React from "react";
import { Composition, continueRender, delayRender } from "remotion";
import "@fontsource-variable/inter";
import "@fontsource-variable/jetbrains-mono";
import { Ad } from "./Ad";
import { T } from "./data";

// Hold every frame until both variable fonts are loaded, so no frame renders in a fallback face.
const fonts = delayRender("fonts");
Promise.all([
  document.fonts.load("700 40px 'Inter Variable'"),
  document.fonts.load("400 40px 'Inter Variable'"),
  document.fonts.load("400 40px 'JetBrains Mono Variable'"),
]).then(() => continueRender(fonts));

export const Root: React.FC = () => (
  <>
    <Composition id="Ad16x9" component={Ad} durationInFrames={T.frames} fps={T.fps} width={1920} height={1080} />
    <Composition id="Ad9x16" component={Ad} durationInFrames={T.frames} fps={T.fps} width={1080} height={1920} />
  </>
);
