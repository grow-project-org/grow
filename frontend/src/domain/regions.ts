import type { Group, Plant } from '../types';

/**
 * A plant's location is derived entirely from its region-type group
 * membership (many-to-many) — there is no separate location field.
 */
export const regionsOf = (plant: Plant, groups: readonly Group[]): Group[] =>
  groups.filter((g) => g.type === 'region' && plant.groups.includes(g.name));

export const regionLabel = (plant: Plant, groups: readonly Group[]): string => {
  const names = regionsOf(plant, groups).map((g) => g.name);
  return names.length ? names.join(', ') : '—';
};
