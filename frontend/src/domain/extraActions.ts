import type { LogType } from '../types';

export interface ExtraAction {
  emoji: string;
  label: string;
  /** History entry kind recorded when performed. */
  kind: LogType;
}

/**
 * Fixed one-off event types, on top of watering/fertilising (scheduled) and
 * adding a plant (automatic on creation). Anything outside this list is a
 * custom event with a free-text description.
 */
export const EXTRA_ACTIONS: readonly ExtraAction[] = [
  { emoji: '✂️', label: 'Podcinanie', kind: 'prune' },
  { emoji: '🧺', label: 'Zbiór plonów', kind: 'harvest' },
  { emoji: '📝', label: 'Własne zdarzenie', kind: 'custom' },
];
