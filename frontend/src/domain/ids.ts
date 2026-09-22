import type { Plant } from '../types';

export const prefixFromName = (name: string): string =>
  name.replace(/[^\p{L}]/gu, '').slice(0, 3).toUpperCase() || 'ROS';

export const makeCode = (prefix: string, n: number): string =>
  `${prefix}-${String(n).padStart(2, '0')}`;

export const codeTaken = (plants: readonly Plant[], code: string): boolean =>
  plants.some((p) => p.code.toLowerCase() === code.trim().toLowerCase());

export const suggestCode = (plants: readonly Plant[], speciesName: string): string => {
  const prefix = prefixFromName(speciesName);
  let n = plants.filter((p) => p.code.startsWith(`${prefix}-`)).length + 1;

  while (codeTaken(plants, makeCode(prefix, n))) n++;

  return makeCode(prefix, n);
};
