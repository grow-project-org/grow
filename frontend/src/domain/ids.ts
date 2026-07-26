import type { LogEntry, Plant } from '../types';

/** Helpers for generating stable ids/codes for new plants and log entries. */

export const nextPlantId = (garden: readonly Plant[]): number =>
  garden.reduce((max, p) => Math.max(max, p.id), 0) + 1;

export const nextUid = (log: readonly LogEntry[]): number =>
  log.reduce((max, e) => Math.max(max, e.uid), 0) + 1;

export const makeCode = (prefix: string, n: number): string =>
  `${prefix}-${String(n).padStart(2, '0')}`;

/** Derive a code prefix from a freshly entered species name. */
export const prefixFromName = (name: string): string =>
  name.replace(/[^\p{L}]/gu, '').slice(0, 3).toUpperCase() || 'ROS';

/** How many existing plants already use this prefix (to continue numbering). */
export const countWithPrefix = (garden: readonly Plant[], prefix: string): number =>
  garden.filter((p) => (p.code || '').startsWith(`${prefix}-`)).length;

/** A plant's code is unique across the whole garden and immutable once set. */
export const codeTaken = (garden: readonly Plant[], code: string): boolean =>
  garden.some((p) => p.code.toLowerCase() === code.trim().toLowerCase());
