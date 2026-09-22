import type { ActionType, Species } from '../types';

export const AVATARS = [
  '#cdeccd',
  '#ffe0a8',
  '#bfe3ff',
  '#ffcfe0',
  '#e2d5ff',
  '#d7f0c2',
] as const;

const hash = (value: string): number => {
  let result = 0;
  for (let i = 0; i < value.length; i++) {
    result = (result * 31 + value.charCodeAt(i)) >>> 0;
  }
  return result;
};

export const avatarBg = (id: string): string => AVATARS[hash(id) % AVATARS.length];

export const findSpecies = (list: readonly Species[], id: string): Species | undefined =>
  list.find((s) => s.id === id);

export const speciesName = (list: readonly Species[], id: string): string =>
  findSpecies(list, id)?.name ?? 'Nieznany gatunek';

export const interval = (
  list: readonly Species[],
  id: string,
  type: ActionType,
): number | null => {
  const species = findSpecies(list, id);
  if (!species) return null;
  return type === 'water' ? species.w : species.f;
};
