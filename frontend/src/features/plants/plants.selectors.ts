import type { Plant } from '../../types';
import { TODAY } from '../../config';
import { diffDays } from '../../utils/date';
import { avatarBg } from '../../domain/species';
import { dueDate, relLabel, type DoneMap } from '../../domain/schedule';
import { relColors } from '../../components/ui/relColors';

export type PlantsFilter = 'all' | 'water' | 'fert';

export interface InstanceRow {
  id: number;
  code: string;
  loc: string;
  emoji: string;
  avatarBg: string;
  next: { label: string; bg: string; ink: string };
}

export interface PlantVariety {
  name: string;
  emoji: string;
  count: number;
  sub: string;
  dueW: number;
  dueF: number;
  instances: InstanceRow[];
}

export interface PlantsView {
  total: number;
  dueWater: number;
  dueFert: number;
  varieties: PlantVariety[];
  empty: boolean;
  filterLabel: string;
}

const FILTER_LABEL: Record<PlantsFilter, string> = {
  all: 'Wszystkie odmiany',
  water: 'Filtr: do podlania dziś',
  fert: 'Filtr: do nawożenia dziś',
};

const isDue = (p: Plant, type: 'water' | 'fert', done: DoneMap): boolean => {
  const due = dueDate(p, type);
  return due != null && diffDays(due, TODAY) <= 0 && !done[`${p.id}:${type}`];
};

const nextPill = (p: Plant): InstanceRow['next'] => {
  const due = dueDate(p, 'water');
  if (!due) return { label: '—', bg: 'var(--color-chip)', ink: 'var(--color-muted)' };
  const rel = relLabel(due);
  return { label: `💧 ${rel.text}`, ...relColors(rel) };
};

/** Build the collapsed "Rośliny" dashboard grouped by variety. */
export const selectPlants = (
  garden: Plant[],
  done: DoneMap,
  query: string,
  filter: PlantsFilter,
): PlantsView => {
  const q = query.trim().toLowerCase();
  const total = garden.length;
  const dueWater = garden.filter((p) => isDue(p, 'water', done)).length;
  const dueFert = garden.filter((p) => isDue(p, 'fert', done)).length;

  const matches = (p: Plant): boolean => {
    const textMatch =
      !q ||
      p.name.toLowerCase().includes(q) ||
      (p.code || '').toLowerCase().includes(q) ||
      (p.species || '').toLowerCase().includes(q);
    const filterMatch =
      filter === 'all' ||
      (filter === 'water' && isDue(p, 'water', done)) ||
      (filter === 'fert' && isDue(p, 'fert', done));
    return textMatch && filterMatch;
  };

  const byName = new Map<string, Plant[]>();
  for (const p of garden.filter(matches)) {
    const list = byName.get(p.name) ?? [];
    list.push(p);
    byName.set(p.name, list);
  }

  const varieties: PlantVariety[] = [...byName.entries()].map(([name, list]) => {
    const first = list[0];
    return {
      name,
      emoji: first.emoji,
      count: list.length,
      sub: `${list.length} szt · ${first.loc ?? '—'}`,
      dueW: list.filter((p) => isDue(p, 'water', done)).length,
      dueF: list.filter((p) => isDue(p, 'fert', done)).length,
      instances: list.map((p) => ({
        id: p.id,
        code: p.code,
        loc: p.loc ?? '—',
        emoji: p.emoji,
        avatarBg: avatarBg(p.id),
        next: nextPill(p),
      })),
    };
  });

  return {
    total,
    dueWater,
    dueFert,
    varieties,
    empty: varieties.length === 0,
    filterLabel: FILTER_LABEL[filter],
  };
};
