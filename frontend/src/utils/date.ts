/**
 * Pure date helpers working on ISO `yyyy-mm-dd` strings in UTC, so results are
 * stable regardless of the host timezone.
 */

export const MONTHS = [
  'stycznia',
  'lutego',
  'marca',
  'kwietnia',
  'maja',
  'czerwca',
  'lipca',
  'sierpnia',
  'września',
  'października',
  'listopada',
  'grudnia',
] as const;

/** Weekday labels, Monday-first. */
export const DOW = ['Pon', 'Wt', 'Śr', 'Czw', 'Pt', 'Sob', 'Nd'] as const;

/** Full weekday names indexed by `Date.getUTCDay()` (0 = Sunday). */
export const WEEKDAYS_LONG = [
  'Niedziela',
  'Poniedziałek',
  'Wtorek',
  'Środa',
  'Czwartek',
  'Piątek',
  'Sobota',
] as const;

/** Capitalised full weekday name for an ISO date. */
export const weekdayLong = (iso: string): string =>
  WEEKDAYS_LONG[parseUTC(iso).getUTCDay()];

export const parseUTC = (iso: string): Date => {
  const [y, m, d] = iso.split('-').map(Number);
  return new Date(Date.UTC(y, m - 1, d));
};

export const toISO = (date: Date): string => date.toISOString().slice(0, 10);

/** Returns a new ISO date `n` days after `iso` (negative `n` for the past). */
export const daysAdd = (iso: string, n: number): string => {
  const d = parseUTC(iso);
  d.setUTCDate(d.getUTCDate() + n);
  return toISO(d);
};

/** Whole-day difference `a - b`. */
export const diffDays = (a: string, b: string): number =>
  Math.round((parseUTC(a).getTime() - parseUTC(b).getTime()) / 86_400_000);

/** `21.07` style short label; `—` for a missing date. */
export const fmtShort = (iso: string | null): string => {
  if (!iso) return '—';
  const [, m, d] = iso.split('-');
  return `${d}.${m}`;
};

/** `21 lipca` style label (day + Polish month in genitive). */
export const fmtLong = (iso: string): string => {
  const d = parseUTC(iso);
  return `${d.getUTCDate()} ${MONTHS[d.getUTCMonth()]}`;
};

/** Monday-first weekday index (0 = Monday … 6 = Sunday). */
export const weekdayMondayFirst = (date: Date): number =>
  (date.getUTCDay() + 6) % 7;
