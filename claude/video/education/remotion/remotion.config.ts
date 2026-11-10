import { Config } from "@remotion/cli/config";
Config.setVideoImageFormat("jpeg");
Config.setJpegQuality(92);
Config.setConcurrency(16);
Config.setChromiumOpenGlRenderer("angle");   // GPU for the browser: ~2x faster with render-scenes.mjs
