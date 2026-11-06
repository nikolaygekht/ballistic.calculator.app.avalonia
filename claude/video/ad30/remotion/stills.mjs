// Render review stills for both compositions in one bundle.
// Usage: node stills.mjs [frame,frame,...]   -> out/stills/<comp>-<frame>.jpg
import { bundle } from "@remotion/bundler";
import { renderStill, selectComposition } from "@remotion/renderer";
import path from "node:path";
import fs from "node:fs";

const frames = (process.argv[2] ?? "30,120,225,345,465,590,690,825").split(",").map(Number);
const out = path.resolve("out/stills");
fs.mkdirSync(out, { recursive: true });
const serveUrl = await bundle({ entryPoint: path.resolve("src/index.ts") });
for (const id of ["Ad16x9", "Ad9x16"]) {
  const composition = await selectComposition({ serveUrl, id });
  for (const frame of frames) {
    const output = path.join(out, `${id}-${String(frame).padStart(3, "0")}.jpg`);
    await renderStill({ serveUrl, composition, frame, output, imageFormat: "jpeg", jpegQuality: 88 });
    console.log(output);
  }
}
