import type { ActionType, Plant, Species } from '../types';
import { TODAY } from '../config';
import { daysAdd, diffDays } from '../utils/date';
import { interval } from './species';

/**
 * Scheduling logic — pure functions over the species catalogue + a plant.
 * No React, no state: safe to unit-test and reuse anywhere.
 */

export type DoneMap = Record<string, boolean>;

export const doneKey = (id: number, type: ActionType): string => `${id}:${type}`;

export const isDoneToday = (
  done: DoneMap,
  id: number,
  type: ActionType,
): boolean => !!done[doneKey(id, type)];

export const lastOf = (p: Plant, type: ActionType): string | null =>
  type === 'water' ? p.lastWater : p.lastFert;

/** Next scheduled date for an action, or `null` when the action isn't tracked. */
export const dueDate = (
  species: readonly Species[],
  p: Plant,
  type: ActionType,
): string | null => {
  const iv = interval(species, p.species, type);
  const last = lastOf(p, type);
  if (iv == null || !last) return null;
  return daysAdd(last, iv);
};

/** Is this action due (today or overdue) and not yet checked off today? */
export const isDue = (
  species: readonly Species[],
  p: Plant,
  type: ActionType,
  done: DoneMap,
  today: string = TODAY,
): boolean => {
  const due = dueDate(species, p, type);
  return due != null && diffDays(due, today) <= 0 && !isDoneToday(done, p.id, type);
};

export interface RelLabel {
  text: string;
  overdue?: boolean;
  today?: boolean;
}

/** Human-friendly relative label for a date against `today`. */
export const relLabel = (date: string, today: string = TODAY): RelLabel => {
  const n = diffDays(date, today);
  if (n < 0) {
    return { text: n === -1 ? 'wczoraj' : `${Math.abs(n)} dni temu`, overdue: true };
  }
  if (n === 0) return { text: 'dziś', today: true };
  if (n === 1) return { text: 'jutro' };
  return { text: `za ${n} dni` };
};
