// Everything plotted comes from the library (datagen/Program.cs -> public/data.json).
import data from "../public/data.json";

export const D = data;
export const ROWS = data.rows;
export const T_END = ROWS[ROWS.length - 1].t;
export const G = data.g;

export type State = { t: number; x: number; v: number; mach: number; cd: number; drag: number };

/** The flight state at flight time `tau` (s), linear between the library's 5 m rows. Past the end the
 *  bullet keeps its last speed, so the air keeps streaming while the readouts hold. */
export const stateAt = (tau: number): State => {
  if (tau <= 0) return { ...ROWS[0] };
  const last = ROWS[ROWS.length - 1];
  if (tau >= T_END) return { ...last, t: tau, x: last.x + last.v * (tau - T_END) };
  let lo = 0, hi = ROWS.length - 1;
  while (hi - lo > 1) {
    const mid = (lo + hi) >> 1;
    if (ROWS[mid].t <= tau) lo = mid; else hi = mid;
  }
  const a = ROWS[lo], b = ROWS[hi];
  const k = (tau - a.t) / (b.t - a.t);
  const l = (p: number, q: number) => p + (q - p) * k;
  return { t: tau, x: l(a.x, b.x), v: l(a.v, b.v), mach: l(a.mach, b.mach), cd: l(a.cd, b.cd), drag: l(a.drag, b.drag) };
};

/** Flight time at which the bullet drops below Mach 1. */
export const T_MACH1 = (() => {
  for (let i = 1; i < ROWS.length; i++)
    if (ROWS[i].mach < 1) {
      const a = ROWS[i - 1], b = ROWS[i];
      return a.t + (b.t - a.t) * (a.mach - 1) / (a.mach - b.mach);
    }
  return T_END;
})();
