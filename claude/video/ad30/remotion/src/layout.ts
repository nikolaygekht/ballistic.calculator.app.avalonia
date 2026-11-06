import { useVideoConfig } from "remotion";

export type Rect = { x: number; y: number; w: number; h: number };

/** One layout per orientation. Vertical keeps text out of the Shorts UI: ~220 px on top, ~380 px at the bottom,
 *  and nothing important in the right 130 px of the lower half (the action buttons). */
export type Layout = {
  W: number; H: number; v: boolean;
  head: { x: number; y: number; size: number; maxW: number };   // the big on-screen words
  plot: Rect;                                                   // the main chart / reticle / scatter area
  side: Rect;                                                   // chips, table, legend
  caption: { x: number; y: number; size: number; maxW: number };
  s: number;                                                    // general type scale
};

export const useL = (): Layout => {
  const { width: W, height: H } = useVideoConfig();
  if (W >= H)
    return {
      W, H, v: false,
      head: { x: 120, y: 92, size: 76, maxW: 1680 },
      plot: { x: 150, y: 250, w: 1060, h: 620 },
      side: { x: 1310, y: 250, w: 480, h: 620 },
      caption: { x: 120, y: 990, size: 22, maxW: 1680 },
      s: 1,
    };
  return {
    W, H, v: true,
    // caption sits under the head (room for a kicker + two head lines); everything ends above ~1490
    head: { x: 72, y: 170, size: 84, maxW: 936 },
    plot: { x: 130, y: 545, w: 850, h: 580 },
    side: { x: 72, y: 1245, w: 860, h: 240 },
    caption: { x: 72, y: 432, size: 23, maxW: 900 },
    s: 1.15,
  };
};
