// YouTube thumbnails (src/thumbs/Thumbs.tsx), 1280x720 JPEG.  Usage: node thumbs.mjs [1 2 3 4]  ->  ../thumbs/epNN.jpg
import { bundle } from "@remotion/bundler";
import { renderStill, selectComposition } from "@remotion/renderer";
import path from "node:path";
import fs from "node:fs";

const eps = process.argv.slice(2).map(Number);
const out = path.resolve("../thumbs");
fs.mkdirSync(out, { recursive: true });
const serveUrl = await bundle({ entryPoint: path.resolve("src/index.ts") });
for (const ep of eps.length ? eps : [1, 2, 3, 4]) {
  const composition = await selectComposition({ serveUrl, id: `Thumb0${ep}` });
  const output = path.join(out, `ep0${ep}.jpg`);
  await renderStill({ serveUrl, composition, frame: 0, output, imageFormat: "jpeg", jpegQuality: 92, scale: 2 / 3 });
  console.log(output, `${(fs.statSync(output).size / 1e6).toFixed(2)} MB`);
}
