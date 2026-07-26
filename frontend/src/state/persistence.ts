import type { GardenState } from './gardenReducer';

/** Snapshot persistence to localStorage so the app works fully offline. */
const STORAGE_KEY = 'hodowla:garden:v1';

export const loadSnapshot = (): GardenState | null => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as GardenState) : null;
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
