import type { Group, Plant, Species } from '../../types';
import { TODAY } from '../../config';
import { diffDays } from '../../utils/date';
import { avatarBg } from '../../domain/species';
import { dueDate, relLabel, type DoneMap } from '../../domain/schedule';
import { regionLabel } from '../../domain/regions';
import { relColors } from '../../components/ui/relColors';

export type PlantsFilter = 'all' | 'water' | 'fert';

export interface InstanceRow {
  id: number;
  code: string;
  region: string;
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
  all: 'Wszystkie gatunki',
  water: 'Filtr: do podlania dziś',
  fert: 'Filtr: do nawożenia dziś',
};

const isDue = (species: readonly Species[], p: Plant, type: 'water' | 'fert', done: DoneMap): boolean => {
  const due = dueDate(species, p, type);
  return due != null && diffDays(due, TODAY) <= 0 && !done[`${p.id}:${type}`];
};

const nextPill = (species: readonly Species[], p: Plant): InstanceRow['next'] => {
  const due = dueDate(species, p, 'water');
  if (!due) return { label: '—', bg: 'var(--color-chip)', ink: 'var(--color-muted)' };
  const rel = relLabel(due);
  return { label: `💧 ${rel.text}`, ...relColors(rel) };
};

/** Build the collapsed "Rośliny" dashboard grouped by species. */
export const selectPlants = (
  species: readonly Species[],
  groups: readonly Group[],
  garden: readonly Plant[],
  done: DoneMap,
  query: string,
  filter: PlantsFilter,
): PlantsView => {
  const q = query.trim().toLowerCase();
  const total = garden.length;
  const dueWater = garden.filter((p) => isDue(species, p, 'water', done)).length;
  const dueFert = garden.filter((p) => isDue(species, p, 'fert', done)).length;

  const matches = (p: Plant): boolean => {
    const textMatch =
      !q ||
      (p.species || '').toLowerCase().includes(q) ||
      (p.code || '').toLowerCase().includes(q);
    const filterMatch =
      filter === 'all' ||
      (filter === 'water' && isDue(species, p, 'water', done)) ||
      (filter === 'fert' && isDue(species, p, 'fert', done));
    return textMatch && filterMatch;
  };

  const byName = new Map<string, Plant[]>();
  for (const p of garden.filter(matches)) {
    const key = p.species ?? 'Bez gatunku';
    const list = byName.get(key) ?? [];
    list.push(p);
    byName.set(key, list);
  }

  const varieties: PlantVariety[] = [...byName.entries()].map(([name, list]) => {
    const first = list[0];
    return {
      name,
      emoji: first.emoji,
      count: list.length,
      sub: `${list.length} szt · ${regionLabel(first, groups)}`,
      dueW: list.filter((p) => isDue(species, p, 'water', done)).length,
      dueF: list.filter((p) => isDue(species, p, 'fert', done)).length,
      instances: list.map((p) => ({
        id: p.id,
        code: p.code,
        region: regionLabel(p, groups),
        emoji: p.emoji,
        avatarBg: avatarBg(p.id),
        next: nextPill(species, p),
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
