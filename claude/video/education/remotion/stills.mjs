// Render review stills.  Usage: node stills.mjs [frame,frame,...] [composition]  -> out/stills/<comp>-<frame>.jpg
import { bundle } from "@remotion/bundler";
import { renderStill, selectComposition } from "@remotion/renderer";
import path from "node:path";
import fs from "node:fs";

// Frames, or seconds with an "s" suffix (e.g. 3.5s). Several compositions: "Ep01-S01:2s,5s Ep01-S02:4s".
const spec = (v) => v.split(",").map((x) => (x.endsWith("s") ? Math.round(parseFloat(x) * 30) : Number(x)));
const frames = spec(process.argv[2] ?? "30,75,120,165,200,260,330,360,420,560,640");
const id = process.argv[3] ?? "DecelPilot";
const out = path.resolve("out/stills");
fs.mkdirSync(out, { recursive: true });
const serveUrl = await bundle({ entryPoint: path.resolve("src/index.ts") });
const jobs = process.argv[2]?.includes(":") ? process.argv.slice(2).map((a) => { const [c, f] = a.split(":"); return [c, spec(f)]; }) : [[id, frames]];
for (const [cid, fr] of jobs) {
  const composition = await selectComposition({ serveUrl, id: cid });
  for (const frame of fr) {
    const output = path.join(out, `${cid}-${String(frame).padStart(4, "0")}.jpg`);
    await renderStill({ serveUrl, composition, frame: Math.min(frame, composition.durationInFrames - 1), output, imageFormat: "jpeg", jpegQuality: 85, scale: 0.5 });
    console.log(output);
  }
}
