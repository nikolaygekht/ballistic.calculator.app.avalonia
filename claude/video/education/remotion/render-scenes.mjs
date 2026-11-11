// Render an episode scene by scene, then join the scenes: the video without re-encoding, the sound as one track.
// Usage: node render-scenes.mjs <ep> [scene ...] [end] [--all]
//   node render-scenes.mjs 2 4 10   re-render scenes 4 and 10 of episode 2 (plus any scene not rendered yet), then join
//   node render-scenes.mjs 2 --all  re-render every scene
// Scenes go to out/epNN/sNN.mp4; the joined episode to the same file as `npm run render:epNN`.
import { bundle } from "@remotion/bundler";
import { renderMedia, selectComposition } from "@remotion/renderer";
import { execFileSync } from "node:child_process";
import path from "node:path";
import fs from "node:fs";

const OUT = { 1: "out/ep01-drag.mp4", 2: "out/ep02-point-mass.mp4", 3: "out/ep03-wind.mp4", 4: "out/ep04-spin-earth.mp4" };
const CONCURRENCY = 16;   // see README: measured on the Ryzen 9 9950X
const FFMPEG = path.resolve("node_modules/@remotion/compositor-win32-x64-msvc/ffmpeg.exe");

const args = process.argv.slice(2);
const ep = Number(args[0]);
if (!OUT[ep]) { console.error("usage: node render-scenes.mjs <ep> [scene ...] [--all]"); process.exit(1); }
const all = args.includes("--all");
const wanted = new Set(args.slice(1).filter((a) => /^\d+$/.test(a)).map(Number));
const pad = (n) => String(n).padStart(2, "0");
const dir = path.resolve(`out/ep${pad(ep)}`);
fs.mkdirSync(dir, { recursive: true });

const t0 = Date.now();
const serveUrl = await bundle({ entryPoint: path.resolve("src/index.ts") });
const files = [], seconds = [];
for (let n = 1; ; n++) {
  const id = `Ep${pad(ep)}-S${pad(n)}`;
  let composition;
  try { composition = await selectComposition({ serveUrl, id }); } catch { break; }
  const file = path.join(dir, `s${pad(n)}.mp4`);
  files.push(file);
  seconds.push(composition.durationInFrames / composition.fps);
  if (!all && !wanted.has(n) && fs.existsSync(file)) continue;
  const t = Date.now();
  await renderMedia({ serveUrl, composition, codec: "h264", outputLocation: file, concurrency: CONCURRENCY,
    imageFormat: "jpeg", jpegQuality: 92, chromiumOptions: { gl: "angle" } });
  console.log(`${id}: ${composition.durationInFrames} frames in ${((Date.now() - t) / 1000).toFixed(0)} s`);
}

// The silent end card for YouTube's end screen goes last (re-rendered with --all, or with "end" in the list).
{
  const composition = await selectComposition({ serveUrl, id: `Ep${pad(ep)}-End` });
  const file = path.join(dir, "end.mp4");
  files.push(file);
  seconds.push(composition.durationInFrames / composition.fps);
  if (all || args.includes("end") || !fs.existsSync(file)) {
    await renderMedia({ serveUrl, composition, codec: "h264", outputLocation: file, concurrency: CONCURRENCY, enforceAudioTrack: true,
      imageFormat: "jpeg", jpegQuality: 92, chromiumOptions: { gl: "angle" } });
    console.log(`Ep${pad(ep)}-End: ${composition.durationInFrames} frames`);
  }
}

// Video: the scenes joined as they are. Audio: never joined as AAC — every AAC piece starts and ends on its own
// frame padding, and those seams click. Each scene's sound is decoded to PCM, cut or padded to exactly the scene's
// length, the PCM is joined sample-exact, and the whole track is encoded once.
const list = path.join(dir, "concat.txt");
fs.writeFileSync(list, files.map((f) => `file '${f.replace(/\\/g, "/")}'`).join("\n"));
const wavs = files.map((f, i) => {
  const wav = f.replace(/\.mp4$/, ".wav");
  execFileSync(FFMPEG, ["-y", "-v", "error", "-i", f, "-vn", "-af", `apad,atrim=0:${seconds[i].toFixed(6)}`,
    "-ar", "48000", "-ac", "2", "-c:a", "pcm_s16le", wav]);
  return wav;
});
const alist = path.join(dir, "concat-audio.txt");
fs.writeFileSync(alist, wavs.map((f) => `file '${f.replace(/\\/g, "/")}'`).join("\n"));
execFileSync(FFMPEG, ["-y", "-v", "error", "-f", "concat", "-safe", "0", "-i", list, "-f", "concat", "-safe", "0", "-i", alist,
  "-map", "0:v", "-map", "1:a", "-c:v", "copy", "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart", path.resolve(OUT[ep])]);
console.log(`${OUT[ep]}: ${files.length} scenes joined, ${((Date.now() - t0) / 1000).toFixed(0)} s total`);
