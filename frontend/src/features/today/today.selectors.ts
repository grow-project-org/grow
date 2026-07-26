import type { ActionType, Group, Plant, Species } from '../../types';
import { TODAY } from '../../config';
import { diffDays } from '../../utils/date';
import { avatarBg } from '../../domain/species';
import { regionLabel } from '../../domain/regions';
import {
  dueDate,
  isDoneToday,
  relLabel,
  type DoneMap,
} from '../../domain/schedule';

export interface TodayRow {
  id: number;
  name: string;
  emoji: string;
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

export interface TodayView {
  sections: TodaySection[];
  left: number;
  allDone: boolean;
}

const SECTION_META: Record<ActionType, { emoji: string; title: string }> = {
  water: { emoji: '💧', title: 'Podlewanie' },
  fert: { emoji: '🌱', title: 'Nawożenie' },
};

const buildRows = (
  species: readonly Species[],
  groups: readonly Group[],
  garden: readonly Plant[],
  done: DoneMap,
  type: ActionType,
): TodayRow[] =>
  garden
    .filter((p) => {
      const due = dueDate(species, p, type);
      return due != null && diffDays(due, TODAY) <= 0;
    })
    .map((p) => {
      const due = dueDate(species, p, type) as string;
      const isDone = isDoneToday(done, p.id, type);
      const overdue = diffDays(due, TODAY) < 0;
      const parts = [p.code, regionLabel(p, groups)].filter(Boolean).join(' · ');
      const sub = overdue ? `${parts} · ${relLabel(due).text}` : parts;
      return {
        id: p.id,
        name: p.species ?? 'Roślina',
        emoji: p.emoji,
        avatarBg: avatarBg(p.id),
        sub,
        done: isDone,
        overdue: overdue && !isDone,
      };
    });

/** Build the "Dziś" screen model: due sections and the outstanding count. */
export const selectToday = (
  species: readonly Species[],
  groups: readonly Group[],
  garden: readonly Plant[],
  done: DoneMap,
): TodayView => {
  const sections: TodaySection[] = [];
  let left = 0;

  (['water', 'fert'] as const).forEach((type) => {
    const rows = buildRows(species, groups, garden, done, type);
    if (!rows.length) return;
    sections.push({ type, ...SECTION_META[type], rows });
    left += rows.filter((r) => !r.done).length;
  });

  return { sections, left, allDone: left === 0 };
};
