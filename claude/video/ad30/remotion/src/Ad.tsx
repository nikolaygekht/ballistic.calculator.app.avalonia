import React from "react";
import { AbsoluteFill, Audio, getStaticFiles, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
import { C, FONT, MONO } from "./theme";
import { TrajectoryAct } from "./scenes/TrajectoryAct";
import { DragScene } from "./scenes/DragScene";
import { ReticleScene } from "./scenes/ReticleScene";
import { MonteCarloScene } from "./scenes/MonteCarloScene";
import { EndCard } from "./scenes/EndCard";

/** Fine technical grid with a slow drift, and a vignette. */
const Backdrop: React.FC = () => {
  const f = useCurrentFrame();
  const { width: W, height: H } = useVideoConfig();
  const step = 48, off = (f * 0.25) % step;
  return (
    <AbsoluteFill style={{ background: C.bg }}>
      <svg width={W} height={H} style={{ position: "absolute", inset: 0, opacity: 0.55 }}>
        <defs>
          <pattern id="g" width={step} height={step} patternUnits="userSpaceOnUse" x={-off} y={0}>
            <path d={`M ${step} 0 L 0 0 0 ${step}`} fill="none" stroke="#121821" strokeWidth={1} />
          </pattern>
          <radialGradient id="v" cx="50%" cy="45%" r="75%">
            <stop offset="55%" stopColor={C.bg} stopOpacity={0} />
            <stop offset="100%" stopColor="#000" stopOpacity={0.75} />
          </radialGradient>
        </defs>
        <rect width={W} height={H} fill="url(#g)" />
      </svg>
      <svg width={W} height={H} style={{ position: "absolute", inset: 0 }}><rect width={W} height={H} fill="url(#v)" /></svg>
    </AbsoluteFill>
  );
};

const hasAudio = () => getStaticFiles().some((f) => f.name === "audio/mix.wav");

export const Ad: React.FC = () => (
  <AbsoluteFill style={{ fontFamily: FONT }}>
    <Backdrop />
    <TrajectoryAct />
    <DragScene />
    <ReticleScene />
    <MonteCarloScene />
    <EndCard />
    {hasAudio() ? <Audio src={staticFile("audio/mix.wav")} /> : (
      <div style={{ position: "absolute", right: 24, top: 20, fontFamily: MONO, fontSize: 20, color: C.amber }}>
        NO AUDIO: public/audio/mix.wav is missing (silent animatic)
      </div>
    )}
  </AbsoluteFill>
);
