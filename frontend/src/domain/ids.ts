import type { LogEntry, Plant } from '../types';
import { PREFIX } from './species';

/** Helpers for generating stable ids/codes for new plants and log entries. */

export const nextPlantId = (garden: Plant[]): number =>
  garden.reduce((max, p) => Math.max(max, p.id), 0) + 1;

export const nextUid = (log: LogEntry[]): number =>
  log.reduce((max, e) => Math.max(max, e.uid), 0) + 1;

export const makeCode = (prefix: string, n: number): string =>
  `${prefix}-${String(n).padStart(2, '0')}`;

export const speciesPrefix = (species: string | null): string => {
  if (!species) return 'ROS';
  return PREFIX[species] ?? species.slice(0, 3).toUpperCase();
};

/** How many existing plants already use this prefix (to continue numbering). */
export const countWithPrefix = (garden: Plant[], prefix: string): number =>
  garden.filter((p) => (p.code || '').startsWith(`${prefix}-`)).length;
