import type { Group, Plant, Species } from '../../types';
import { TODAY } from '../../config';
import { diffDays, fmtLong, weekdayMondayFirst } from '../../utils/date';
import { dueDate, relLabel } from '../../domain/schedule';
import { regionLabel } from '../../domain/regions';

const YEAR = 2026;
const MONTH_INDEX = 6; // July (0-based)
const MONTH_PREFIX = '2026-07';

export interface CalCell {
  day: number;
  iso: string;
  hasEvents: boolean;
  isToday: boolean;
}

export interface CalEvent {
  id: number;
  name: string;
  emoji: string;
  loc: string;
  action: string;
  bg: string;
}

export interface CalendarView {
  title: string;
  /** Leading empty slots before day 1 (Monday-first). */
  leadingBlanks: number;
  cells: CalCell[];
  selectedTitle: string;
  events: CalEvent[];
  empty: boolean;
}

type DayEvents = Map<string, Array<{ plant: Plant; type: 'water' | 'fert' }>>;

const buildEventMap = (species: readonly Species[], garden: readonly Plant[]): DayEvents => {
  const map: DayEvents = new Map();
  for (const plant of garden) {
    for (const type of ['water', 'fert'] as const) {
      let due = dueDate(species, plant, type);
      if (!due) continue;
      if (diffDays(due, TODAY) < 0) due = TODAY; // roll overdue onto today
      if (due.slice(0, 7) !== MONTH_PREFIX) continue;
      const list = map.get(due) ?? [];
      list.push({ plant, type });
      map.set(due, list);
    }
  }
  return map;
};

export const selectCalendar = (
  species: readonly Species[],
  groups: readonly Group[],
  garden: readonly Plant[],
  selected: string,
): CalendarView => {
  const events = buildEventMap(species, garden);
  const firstOfMonth = new Date(Date.UTC(YEAR, MONTH_INDEX, 1));
  const daysInMonth = new Date(Date.UTC(YEAR, MONTH_INDEX + 1, 0)).getUTCDate();

  const cells: CalCell[] = [];
  for (let day = 1; day <= daysInMonth; day++) {
    const iso = `${MONTH_PREFIX}-${String(day).padStart(2, '0')}`;
    cells.push({ day, iso, hasEvents: events.has(iso), isToday: iso === TODAY });
  }

  const selectedEvents: CalEvent[] = (events.get(selected) ?? []).map(({ plant, type }) => ({
    id: plant.id,
    name: plant.species ?? 'Roślina',
    emoji: plant.emoji,
    loc: regionLabel(plant, groups),
    action: type === 'water' ? 'Podlewanie' : 'Nawożenie',
    bg: type === 'water' ? 'var(--color-water-bg)' : 'var(--color-fert-bg)',
  }));

  return {
    title: 'Lipiec 2026',
    leadingBlanks: weekdayMondayFirst(firstOfMonth),
    cells,
    selectedTitle: `${fmtLong(selected)} · ${relLabel(selected).text}`,
    events: selectedEvents,
    empty: selectedEvents.length === 0,
  };
};
