import type { Group, Plant } from '../types';

export const groupsOf = (plant: Plant, groups: readonly Group[]): Group[] =>
  groups.filter((g) => plant.groupIds.includes(g.id));

export const regionsOf = (plant: Plant, groups: readonly Group[]): Group[] =>
  groupsOf(plant, groups).filter((g) => g.type === 'region');

export const regionLabel = (plant: Plant, groups: readonly Group[]): string => {
  const names = regionsOf(plant, groups).map((g) => g.name);
  return names.length ? names.join(', ') : '—';
};
