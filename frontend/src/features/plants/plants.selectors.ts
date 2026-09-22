import type { Group, Plant, Species } from '../../types';
import { avatarBg, speciesName } from '../../domain/species';
import { isDue, nextDate, relLabel } from '../../domain/schedule';
import { regionLabel } from '../../domain/regions';
import { relColors } from '../../components/ui/relColors';

export type PlantsFilter = 'all' | 'water' | 'fert';

export interface InstanceRow {
  id: string;
  code: string;
  region: string;
  initial: string;
  avatarBg: string;
  next: { label: string; bg: string; ink: string };
}

export interface PlantVariety {
  specieId: string;
  name: string;
  initial: string;
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

const nextPill = (plant: Plant, today: string): InstanceRow['next'] => {
  const next = nextDate(plant, 'water');
  if (!next) return { label: '—', bg: 'var(--color-chip)', ink: 'var(--color-muted)' };

  const rel = relLabel(next, today);
  return { label: `💧 ${rel.text}`, ...relColors(rel) };
};

export const selectPlants = (
  species: readonly Species[],
  groups: readonly Group[],
  plants: readonly Plant[],
  today: string,
  query: string,
  filter: PlantsFilter,
): PlantsView => {
  const q = query.trim().toLowerCase();
  const dueWater = plants.filter((p) => isDue(p, 'water', today)).length;
  const dueFert = plants.filter((p) => isDue(p, 'fert', today)).length;

  const matches = (plant: Plant): boolean => {
    const textMatch =
      !q ||
      speciesName(species, plant.specieId).toLowerCase().includes(q) ||
      plant.code.toLowerCase().includes(q);
    const filterMatch =
      filter === 'all' ||
      (filter === 'water' && isDue(plant, 'water', today)) ||
      (filter === 'fert' && isDue(plant, 'fert', today));

    return textMatch && filterMatch;
  };

  const bySpecie = new Map<string, Plant[]>();
  for (const plant of plants.filter(matches)) {
    const list = bySpecie.get(plant.specieId) ?? [];
    list.push(plant);
    bySpecie.set(plant.specieId, list);
  }

  const varieties: PlantVariety[] = [...bySpecie.entries()].map(([specieId, list]) => {
    const name = speciesName(species, specieId);

    return {
      specieId,
      name,
      initial: name.slice(0, 1).toUpperCase(),
      count: list.length,
      sub: `${list.length} szt · ${regionLabel(list[0], groups)}`,
      dueW: list.filter((p) => isDue(p, 'water', today)).length,
      dueF: list.filter((p) => isDue(p, 'fert', today)).length,
      instances: list.map((p) => ({
        id: p.id,
        code: p.code,
        region: regionLabel(p, groups),
        initial: name.slice(0, 1).toUpperCase(),
        avatarBg: avatarBg(p.id),
        next: nextPill(p, today),
      })),
    };
  });

  return {
    total: plants.length,
    dueWater,
    dueFert,
    varieties,
    empty: varieties.length === 0,
    filterLabel: FILTER_LABEL[filter],
  };
};
