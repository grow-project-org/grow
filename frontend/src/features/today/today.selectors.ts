import type { ActionType, Group, Plant, Species } from '../../types';
import { diffDays } from '../../utils/date';
import { ACTION_META, ACTION_TYPES } from '../../domain/actions';
import { avatarBg, speciesName } from '../../domain/species';
import { regionLabel } from '../../domain/regions';
import { isDoneToday, isDue, nextDate, relLabel } from '../../domain/schedule';

export interface TodayRow {
  id: string;
  name: string;
  initial: string;
  avatarBg: string;
  sub: string;
  done: boolean;
  overdue: boolean;
}

export interface TodaySection {
  type: ActionType;
  emoji: string;
  title: string;
  rows: TodayRow[];
}

export interface TodaySummary {
  water: number;
  fert: number;
  overdue: number;
}

export interface TodayView {
  summary: TodaySummary;
  sections: TodaySection[];
  left: number;
  allDone: boolean;
}

const buildRows = (
  species: readonly Species[],
  groups: readonly Group[],
  plants: readonly Plant[],
  type: ActionType,
  today: string,
): TodayRow[] =>
  plants
    .filter((p) => isDue(p, type, today) || isDoneToday(p, type, today))
    .map((p) => {
      const name = speciesName(species, p.specieId);
      const done = isDoneToday(p, type, today);
      const next = nextDate(p, type);
      const overdue = !done && next != null && diffDays(next, today) < 0;
      const parts = [p.code, regionLabel(p, groups)].filter((part) => part !== '—').join(' · ');

      return {
        id: p.id,
        name,
        initial: name.slice(0, 1).toUpperCase(),
        avatarBg: avatarBg(p.id),
        sub: overdue && next ? `${parts} · ${relLabel(next, today).text}` : parts,
        done,
        overdue,
      };
    });

export const selectToday = (
  species: readonly Species[],
  groups: readonly Group[],
  plants: readonly Plant[],
  today: string,
): TodayView => {
  const sections: TodaySection[] = [];
  const summary: TodaySummary = { water: 0, fert: 0, overdue: 0 };
  let left = 0;

  for (const type of ACTION_TYPES) {
    const rows = buildRows(species, groups, plants, type, today);
    if (!rows.length) continue;

    sections.push({ type, emoji: ACTION_META[type].emoji, title: ACTION_META[type].label, rows });

    const undone = rows.filter((r) => !r.done).length;
    left += undone;
    summary[type] = undone;
    summary.overdue += rows.filter((r) => r.overdue).length;
  }

  return { summary, sections, left, allDone: left === 0 };
};
