import type { LogType } from '../types';

export interface ExtraAction {
  emoji: string;
  label: string;
  /** History entry kind recorded when performed on a single plant. */
  kind: LogType;
}

/** Non-scheduled care actions offered in the plant / group action sheet. */
export const EXTRA_ACTIONS: readonly ExtraAction[] = [
  { emoji: '✂️', label: 'Podcinanie', kind: 'prune' },
  { emoji: '🧺', label: 'Zbiór plonów', kind: 'harvest' },
  { emoji: '🪴', label: 'Przesadzanie', kind: 'repot' },
  { emoji: '📍', label: 'Zmiana lokalizacji', kind: 'move' },
  { emoji: '💨', label: 'Oprysk', kind: 'prune' },
  { emoji: '🐛', label: 'Szkodniki', kind: 'prune' },
];
