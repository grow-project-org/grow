import type { GardenState } from './gardenReducer';

/** Snapshot persistence to localStorage so the app works fully offline. */
const STORAGE_KEY = 'hodowla:garden:v1';

/**
 * A snapshot from localStorage may predate the current domain shape (e.g. an
 * older schema without a species catalogue) — trusting it blindly crashes the
 * app deep inside a selector instead of at this boundary.
 */
const isCurrentShape = (value: unknown): value is GardenState => {
  if (!value || typeof value !== 'object') return false;
  const v = value as Record<string, unknown>;
  return (
    Array.isArray(v.garden) &&
    Array.isArray(v.groups) &&
    Array.isArray(v.log) &&
    Array.isArray(v.species) &&
    typeof v.done === 'object' &&
    typeof v.dismissed === 'object'
  );
};

export const loadSnapshot = (): GardenState | null => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed: unknown = JSON.parse(raw);
    return isCurrentShape(parsed) ? parsed : null;
  } catch {
    return null;
  }
};

export const saveSnapshot = (state: GardenState): void => {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  } catch {
    // Ignore quota / private-mode errors — persistence is best-effort.
  }
};
