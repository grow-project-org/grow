import type { ActionType, Plant } from '../types';
import { diffDays } from '../utils/date';

export const nextDate = (plant: Plant, type: ActionType): string | null =>
  type === 'water' ? plant.nextWater : plant.nextFert;

export const lastDate = (plant: Plant, type: ActionType): string | null =>
  type === 'water' ? plant.lastWater : plant.lastFert;

export const isTracked = (plant: Plant, type: ActionType): boolean =>
  nextDate(plant, type) != null || lastDate(plant, type) != null;

export const isDoneToday = (plant: Plant, type: ActionType, today: string): boolean =>
  lastDate(plant, type) === today;

export const isDue = (plant: Plant, type: ActionType, today: string): boolean => {
  const next = nextDate(plant, type);
  return next != null && diffDays(next, today) <= 0 && !isDoneToday(plant, type, today);
};

export interface RelLabel {
  text: string;
  overdue?: boolean;
  today?: boolean;
}

export const relLabel = (date: string, today: string): RelLabel => {
  const n = diffDays(date, today);

  if (n < 0) {
    return { text: n === -1 ? 'wczoraj' : `${Math.abs(n)} dni temu`, overdue: true };
  }
  if (n === 0) return { text: 'dziś', today: true };
  if (n === 1) return { text: 'jutro' };
  return { text: `za ${n} dni` };
};
