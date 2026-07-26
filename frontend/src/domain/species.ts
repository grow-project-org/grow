import type { ActionType, Species } from '../types';

/**
 * Pure helpers over the user's own species catalogue (state, not a static
 * import) — a species is user-defined, private, and flat.
 */

export const DEFAULT_EMOJI = '🌱';

/** Rotating avatar backgrounds, indexed deterministically by plant id. */
export const AVATARS = [
  '#cdeccd',
  '#ffe0a8',
  '#bfe3ff',
  '#ffcfe0',
  '#e2d5ff',
  '#d7f0c2',
] as const;

export const avatarBg = (id: number): string =>
  AVATARS[(id - 1) % AVATARS.length];

export const findSpecies = (
  list: readonly Species[],
  name: string | null,
): Species | undefined => (name ? list.find((s) => s.name === name) : undefined);

/** Care interval (days) for a plant's species, or `null` if not tracked. */
export const interval = (
  list: readonly Species[],
  name: string | null,
  type: ActionType,
): number | null => {
  const sp = findSpecies(list, name);
  if (!sp) return null;
  return type === 'water' ? sp.w : sp.f;
};

export const emojiForSpecies = (
  list: readonly Species[],
  name: string | null,
): string => findSpecies(list, name)?.emoji ?? DEFAULT_EMOJI;
