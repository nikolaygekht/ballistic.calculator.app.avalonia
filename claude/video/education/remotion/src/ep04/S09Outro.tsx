// Scene 9 — the series outro: the four motifs in a row (the drag, the dot and its steps, the wind, the spinning
// bullet), the link to the documentation. The last episode, so a farewell: thank you for watching, good days at the
// range, a steady wind. The author's own photo at the bench draws itself as lines, then turns into the photo. Apart
// from it, on the left, the last shot: the dot flies its arc in from the left and hits the centre of a line target.
import React, { useEffect, useState } from "react";
import { continueRender, delayRender, staticFile } from "remotion";
import { C, ease, lerp, ramp } from "../theme";
import { Bullet, P, Pen, label } from "../lib/draw";
import { Dot, Scene, SpinBullet, useVoiceTime } from "./kit";
import { cue } from "./timing";

const N = 9;
const T = {
  inside: cue(N, "inside"), drag: cue(N, "drag"), dot: cue(N, "dot"), wind: cue(N, "wind"), fixes: cue(N, "few small fixes"),
  formulas: cue(N, "formulas"), link: cue(N, "link"), end: cue(N, "end of the series"), thank: cue(N, "Thank"), now: cue(N, "Now"),
  wish: cue(N, "wish"), range: cue(N, "range"), steady: cue(N, "steady"), every: cue(N, "every shot"), aimed: cue(N, "aimed"),
};

// the photo panel (the photo is cropped to 16:9 at 1920 × 1080); the last shot is its own picture on the left
const PH = { x: 680, y: 300, w: 1180, h: 664 };
const TGT: P = [360, 780];
const START: P = [-20, TGT[1] - 40];
const arc = (k: number): P => [lerp(START[0], TGT[0], k), lerp(START[1], TGT[1], k) - 90 * 4 * k * (1 - k)];
const PHOTO = staticFile("ep04/range-photo.jpg"), LINES = staticFile("ep04/range-lines.png");
/** Hold the frame until both pictures are loaded (an SVG <image> is not tracked by Remotion). */
const usePictures = () => {
  const [h] = useState(() => delayRender("outro photo"));
  useEffect(() => {
    let left = 2;
    const done = () => { if (--left === 0) continueRender(h); };
    [PHOTO, LINES].forEach((src) => { const i = new Image(); i.onload = done; i.onerror = done; i.src = src; });
  }, [h]);
};

export const S09Outro: React.FC = () => {
  const v = useVoiceTime();
  usePictures();
  const m = (t: number) => ramp(v, t - 0.3, 0.5);
  const reveal = ramp(v, T.now - 0.2, 2.8, ease);                   // the lines draw in from the shooter to the muzzle
  const photoO = ramp(v, T.wish - 0.4, 1.6, ease);
  const linesO = ramp(v, T.now - 0.2, 0.4) * (1 - 0.8 * photoO);
  const zoom = 1 + 0.05 * ramp(v, T.wish - 0.4, 14);
  const away = 1 - ramp(v, T.end - 0.2, 0.9);           // the motifs make room for the farewell
  const Y = 560;
  const fly = ramp(v, T.every - 0.2, T.aimed + 0.2 - (T.every - 0.2), (x) => x);
  const hit = Math.max(0, v - (T.aimed + 0.2));
  const head = arc(fly);
  return (
    <Scene n={N}>
      <g opacity={away}>
        <text x={960} y={200} textAnchor="middle" style={label(ramp(v, -0.3, 0.8), { fontSize: 54, letterSpacing: "0.22em", fontWeight: 200 })}>INSIDE THE CALCULATOR</text>
        <text x={960} y={256} textAnchor="middle" style={label(ramp(v, 0.2, 0.8) * 0.6, { fontSize: 22 })}>HOW BALLISTIC CALCULATOR 2 WORKS · 1 – 4</text>

        {/* the drag */}
        <g opacity={m(T.drag)}>
          <Bullet x0={260} y={Y} cal={26} w={2.5} />
          <line x1={250} y1={Y} x2={150} y2={Y} stroke={C.drag} strokeWidth={6} />
          <path d={`M168,${Y - 14} L150,${Y} L168,${Y + 14}`} stroke={C.drag} strokeWidth={6} fill="none" />
          <text x={300} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>THE DRAG</text>
        </g>
        {/* the dot, step by step */}
        <g opacity={m(T.dot)}>
          {Array.from({ length: 7 }, (_, i) => <Dot key={i} x={620 + i * 40} y={Y - 30 + i * i * 2.4} r={i === 6 ? 10 : 5} />)}
          <text x={740} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>THE DOT</text>
        </g>
        {/* the wind */}
        <g opacity={m(T.wind)}>
          {Array.from({ length: 6 }, (_, i) => {
            const x = 1080 + ((((i * 53 - 90 * v) % 240) + 240) % 240);
            return <line key={i} x1={x} y1={Y - 50 + i * 20} x2={x + 46} y2={Y - 50 + i * 20} stroke={C.line} strokeOpacity={0.6} strokeWidth={2.5} strokeLinecap="round" />;
          })}
          <text x={1180} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>THE WIND</text>
        </g>
        {/* the spin, and the turning Earth */}
        <g opacity={m(T.fixes)}>
          <SpinBullet id="o" cx={1620} cy={Y} cal={30} spin={v * 1.4} marks={0.9} />
          <text x={1620} y={Y + 110} textAnchor="middle" style={label(0.9, { fontSize: 24 })}>A FEW SMALL FIXES</text>
        </g>

        <text x={960} y={830} textAnchor="middle" style={label(m(T.formulas), { fontSize: 30 })}>ALL THE FORMULAS: IN THE DOCUMENTATION</text>
        <text x={960} y={890} textAnchor="middle" style={label(m(T.link) * 0.7, { fontSize: 24 })}>THE LINK IS IN THE DESCRIPTION</text>
      </g>

      {/* ---------------- the farewell: the author at the bench, first as lines, then as the photo */}
      <defs>
        <filter id="phFeather" x="-10%" y="-10%" width="120%" height="120%"><feGaussianBlur stdDeviation="22" /></filter>
        <mask id="phMask" maskUnits="userSpaceOnUse" x={0} y={0} width={1920} height={1080}>
          <rect x={PH.x + 26} y={PH.y + 26} width={PH.w - 52} height={PH.h - 52} fill="white" filter="url(#phFeather)" />
        </mask>
        <clipPath id="phWipe"><rect x={PH.x + PH.w * (1 - reveal) - 40} y={PH.y - 20} width={PH.w * reveal + 80} height={PH.h + 40} /></clipPath>
      </defs>
      <g mask="url(#phMask)">
        <g transform={`translate(${PH.x + PH.w / 2},${PH.y + PH.h / 2}) scale(${zoom}) translate(${-(PH.x + PH.w / 2)},${-(PH.y + PH.h / 2)})`}>
          <image href={PHOTO} x={PH.x} y={PH.y} width={PH.w} height={PH.h} opacity={photoO * 0.92} preserveAspectRatio="xMidYMid slice" />
          <g clipPath="url(#phWipe)">
            <image href={LINES} x={PH.x} y={PH.y} width={PH.w} height={PH.h} opacity={linesO} preserveAspectRatio="xMidYMid slice" />
          </g>
        </g>
      </g>
      <text x={PH.x + PH.w - 20} y={PH.y + PH.h + 36} textAnchor="end" style={label(photoO * 0.55, { fontSize: 16 })}>THE AUTHOR</text>

      <text x={960} y={150} textAnchor="middle" style={label(ramp(v, T.thank - 0.2, 0.9), { fontSize: 64, letterSpacing: "0.2em", fontWeight: 200 })}>THANK YOU FOR WATCHING</text>
      <text x={960} y={208} textAnchor="middle" style={label(ramp(v, T.now - 0.1, 0.8) * 0.7, { fontSize: 24 })}>NOW YOU KNOW WHERE THE NUMBERS COME FROM</text>
      <text x={960} y={266} textAnchor="middle" style={label(1, { fontSize: 28, letterSpacing: "0.18em" })}>
        <tspan opacity={ramp(v, T.range - 0.5, 0.6)}>GOOD DAYS AT THE RANGE</tspan>
        <tspan opacity={ramp(v, T.steady - 0.2, 0.6)}> · A STEADY WIND</tspan>
        <tspan opacity={ramp(v, T.every - 0.2, 0.6)}> · ACCURATE SHOTS</tspan>
      </text>

      {/* the last shot: the dot flies in and hits the centre */}
      <g opacity={ramp(v, T.wish - 0.2, 0.8)}>
        {[110, 72, 36].map((r, i) => <circle key={r} cx={TGT[0]} cy={TGT[1]} r={r} fill="none" stroke={C.line} strokeWidth={i === 2 ? 2.5 : 1.5} opacity={0.35 + 0.2 * i} />)}
        <line x1={TGT[0] - 130} y1={TGT[1]} x2={TGT[0] + 130} y2={TGT[1]} stroke={C.line} strokeWidth={1} opacity={0.25} />
        <line x1={TGT[0]} y1={TGT[1] - 130} x2={TGT[0]} y2={TGT[1] + 130} stroke={C.line} strokeWidth={1} opacity={0.25} />
      </g>
      {fly > 0 && (
        <>
          <Pen pts={Array.from({ length: 41 }, (_, i) => arc((fly * i) / 40))} p={1} w={2} tip={false} dash="6 10" opacity={0.5} />
          <Dot x={head[0]} y={head[1]} r={11} />
        </>
      )}
      {hit > 0 && (
        <>
          <circle cx={TGT[0]} cy={TGT[1]} r={14 + 120 * ease(Math.min(1, hit / 1.2))} fill="none" stroke={C.line} strokeWidth={2} opacity={Math.max(0, 1 - hit / 1.2)} />
          <circle cx={TGT[0]} cy={TGT[1]} r={14} fill={C.line} filter="url(#glowBig)" />
        </>
      )}
    </Scene>
  );
};
