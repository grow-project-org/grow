import type { ActionType, SpeciesInfo } from '../types';

/**
 * Species catalogue: care intervals (days) keyed by species name.
 * `null` means the action is not tracked for that species.
 */
export const SPECIES: Record<string, SpeciesInfo> = {
  Pomidor: { w: 2, f: 14 },
  Papryka: { w: 3, f: 14 },
  Ogórek: { w: 2, f: null },
  Sałata: { w: 2, f: null },
  Cukinia: { w: 3, f: 10 },
  Bazylia: { w: 2, f: null },
  Mięta: { w: 3, f: null },
  Tymianek: { w: 5, f: null },
  Szałwia: { w: 5, f: null },
  Truskawka: { w: 3, f: 14 },
  Pelargonia: { w: 4, f: 10 },
  Surfinia: { w: 2, f: 7 },
  Monstera: { w: 7, f: null },
  Fikus: { w: 6, f: null },
  Zamiokulkas: { w: 14, f: null },
};

/** ID prefix per species, e.g. `Papryka` → `PAP-05`. */
export const PREFIX: Record<string, string> = {
  Pomidor: 'POM',
  Papryka: 'PAP',
  Ogórek: 'OGR',
  Sałata: 'SAL',
  Cukinia: 'CUK',
  Bazylia: 'BAZ',
  Mięta: 'MIE',
  Tymianek: 'TYM',
  Szałwia: 'SZA',
  Truskawka: 'TRU',
  Pelargonia: 'PEL',
  Surfinia: 'SUR',
  Monstera: 'MON',
  Fikus: 'FIK',
  Zamiokulkas: 'ZAM',
};

/** Emoji used when a plant is created for a known species. */
export const SPECIES_EMOJI: Record<string, string> = {
  Pomidor: '🍅',
  Papryka: '🌶️',
  Ogórek: '🥒',
  Sałata: '🥬',
  Truskawka: '🍓',
  Bazylia: '🌿',
  Mięta: '🌿',
  Tymianek: '🌿',
  Szałwia: '🌿',
  Cukinia: '🌿',
  Pelargonia: '🌸',
  Surfinia: '💐',
  Monstera: '🌴',
  Fikus: '🪴',
  Zamiokulkas: '🪴',
};

/** Rotating avatar backgrounds, indexed deterministically by plant id. */
export const AVATARS = [
  '#cdeccd',
  '#ffe0a8',
  '#bfe3ff',
  '#ffcfe0',
  '#e2d5ff',
  '#d7f0c2',
] as const;

export const DEFAULT_EMOJI = '🌱';

export const avatarBg = (id: number): string =>
  AVATARS[(id - 1) % AVATARS.length];

/** Care interval (days) for a plant's species, or `null` if not tracked. */
export const interval = (
  species: string | null,
  type: ActionType,
): number | null => {
  if (!species) return null;
  const info = SPECIES[species];
  if (!info) return null;
  return type === 'water' ? info.w : info.f;
};

export const emojiForSpecies = (species: string | null): string =>
  (species && SPECIES_EMOJI[species]) || DEFAULT_EMOJI;
