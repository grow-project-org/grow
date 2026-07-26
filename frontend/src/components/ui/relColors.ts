import type { RelLabel } from '../../domain/schedule';

export interface RelColors {
  bg: string;
  ink: string;
}

/** Map a relative-time label to the palette used across cards and pills. */
export const relColors = (rel: RelLabel): RelColors => {
  if (rel.overdue) return { bg: 'var(--color-overdue-bg)', ink: 'var(--color-overdue-ink)' };
  if (rel.today) return { bg: 'var(--color-water-bg)', ink: 'var(--color-water-ink)' };
  return { bg: 'var(--color-ok-tint)', ink: 'var(--color-ok-ink)' };
};
