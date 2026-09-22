import type { ActionType, Group, Plant, Species } from '../../types';
import {
  MONTHS_NOMINATIVE,
  diffDays,
  fmtLong,
  parseUTC,
  toISO,
  weekdayMondayFirst,
} from '../../utils/date';
import { ACTION_META, ACTION_TYPES } from '../../domain/actions';
import { nextDate, relLabel } from '../../domain/schedule';
import { speciesName } from '../../domain/species';
import { regionLabel } from '../../domain/regions';

export interface CalCell {
  day: number;
  iso: string;
  hasEvents: boolean;
  isToday: boolean;
}

export interface CalEvent {
  id: string;
  name: string;
  initial: string;
  loc: string;
  action: string;
  bg: string;
}

export interface CalendarView {
  title: string;
  prevMonth: string;
  nextMonth: string;
  leadingBlanks: number;
  cells: CalCell[];
  selectedTitle: string;
  events: CalEvent[];
  empty: boolean;
}

const EVENT_BG: Record<ActionType, string> = {
  water: 'var(--color-water-bg)',
  fert: 'var(--color-fert-bg)',
};

type DayEvents = Map<string, Array<{ plant: Plant; type: ActionType }>>;

const buildEventMap = (plants: readonly Plant[], monthPrefix: string, today: string): DayEvents => {
  const map: DayEvents = new Map();

  for (const plant of plants) {
    for (const type of ACTION_TYPES) {
      let due = nextDate(plant, type);
      if (!due) continue;
      if (diffDays(due, today) < 0) due = today;
      if (due.slice(0, 7) !== monthPrefix) continue;

      const list = map.get(due) ?? [];
      list.push({ plant, type });
      map.set(due, list);
    }
  }

  return map;
};

const shiftMonth = (iso: string, delta: number): string => {
  const date = parseUTC(iso);
  return toISO(new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth() + delta, 1)));
};

export const selectCalendar = (
  species: readonly Species[],
  groups: readonly Group[],
  plants: readonly Plant[],
  selected: string,
  today: string,
): CalendarView => {
  const monthPrefix = selected.slice(0, 7);
  const events = buildEventMap(plants, monthPrefix, today);

  const anchor = parseUTC(selected);
  const year = anchor.getUTCFullYear();
  const month = anchor.getUTCMonth();
  const daysInMonth = new Date(Date.UTC(year, month + 1, 0)).getUTCDate();

  const cells: CalCell[] = [];
  for (let day = 1; day <= daysInMonth; day++) {
    const iso = `${monthPrefix}-${String(day).padStart(2, '0')}`;
    cells.push({ day, iso, hasEvents: events.has(iso), isToday: iso === today });
  }

  const selectedEvents: CalEvent[] = (events.get(selected) ?? []).map(({ plant, type }) => {
    const name = speciesName(species, plant.specieId);
    return {
      id: plant.id,
      name,
      initial: name.slice(0, 1).toUpperCase(),
      loc: regionLabel(plant, groups),
      action: ACTION_META[type].label,
      bg: EVENT_BG[type],
    };
  });

  return {
    title: `${MONTHS_NOMINATIVE[month]} ${year}`,
    prevMonth: shiftMonth(selected, -1),
    nextMonth: shiftMonth(selected, 1),
    leadingBlanks: weekdayMondayFirst(new Date(Date.UTC(year, month, 1))),
    cells,
    selectedTitle: `${fmtLong(selected)} · ${relLabel(selected, today).text}`,
    events: selectedEvents,
    empty: selectedEvents.length === 0,
  };
};
