import type { Group, LogEntry, Plant, Species } from '../types';
import { TODAY } from '../config';
import { daysAdd } from '../utils/date';
import { makeCode } from '../domain/ids';

/**
 * Deterministic seed data: a private species catalogue plus ~72 concrete
 * plant instances across it, with staggered last-care dates so the schedule
 * shows a realistic mix of due / overdue / upcoming plants against
 * {@link TODAY}. Two visually different varieties of the same kind of plant
 * (e.g. hot vs. sweet pepper) are two separate, flat species — not one
 * species with a variety name.
 */

interface GardenDef {
  species: string;
  emoji: string;
  prefix: string;
  /** Watering interval in days. */
  w: number;
  /** Fertilising interval in days, or null if not tracked. */
  f: number | null;
  region: string;
  /** Extra work/temporary groups beyond the region. */
  groups: string[];
  /** Number of instances to create. */
  n: number;
  potL: number;
  potCm: number;
}

const GARDEN_DEFS: readonly GardenDef[] = [
  { species: 'Papryka Ostra', emoji: '🌶️', prefix: 'PAO', w: 3, f: 14, region: 'Szklarnia', groups: ['Papryki'], n: 12, potL: 5, potCm: 20 },
  { species: 'Papryka Słodka', emoji: '🌶️', prefix: 'PAS', w: 3, f: 14, region: 'Szklarnia', groups: ['Papryki'], n: 8, potL: 5, potCm: 20 },
  { species: 'Pomidor Malinowy', emoji: '🍅', prefix: 'POM', w: 2, f: 14, region: 'Szklarnia', groups: ['Pomidory'], n: 6, potL: 10, potCm: 26 },
  { species: 'Pomidor Koktajlowy', emoji: '🍅', prefix: 'POK', w: 2, f: 14, region: 'Szklarnia', groups: ['Pomidory'], n: 4, potL: 7, potCm: 24 },
  { species: 'Ogórek Gruntowy', emoji: '🥒', prefix: 'OGR', w: 2, f: null, region: 'Szklarnia', groups: [], n: 5, potL: 8, potCm: 24 },
  { species: 'Sałata Dębolistna', emoji: '🥬', prefix: 'SAL', w: 2, f: null, region: 'Szklarnia', groups: [], n: 6, potL: 2, potCm: 14 },
  { species: 'Cukinia', emoji: '🌿', prefix: 'CUK', w: 3, f: 10, region: 'Szklarnia', groups: [], n: 3, potL: 12, potCm: 30 },
  { species: 'Truskawka', emoji: '🍓', prefix: 'TRU', w: 3, f: 14, region: 'Balkon', groups: ['Owoce'], n: 8, potL: 3, potCm: 16 },
  { species: 'Pelargonia', emoji: '🌸', prefix: 'PEL', w: 4, f: 10, region: 'Balkon', groups: [], n: 4, potL: 4, potCm: 18 },
  { species: 'Surfinia', emoji: '💐', prefix: 'SUR', w: 2, f: 7, region: 'Balkon', groups: [], n: 3, potL: 6, potCm: 22 },
  { species: 'Bazylia', emoji: '🌿', prefix: 'BAZ', w: 2, f: null, region: 'Parapet', groups: ['Zioła'], n: 4, potL: 1.5, potCm: 12 },
  { species: 'Mięta', emoji: '🌿', prefix: 'MIE', w: 3, f: null, region: 'Parapet', groups: ['Zioła'], n: 3, potL: 1, potCm: 10 },
  { species: 'Tymianek', emoji: '🌿', prefix: 'TYM', w: 5, f: null, region: 'Parapet', groups: ['Zioła'], n: 3, potL: 1, potCm: 10 },
  { species: 'Monstera', emoji: '🌴', prefix: 'MON', w: 7, f: null, region: 'Mieszkanie', groups: [], n: 1, potL: 15, potCm: 32 },
  { species: 'Fikus Lirata', emoji: '🪴', prefix: 'FIK', w: 6, f: null, region: 'Mieszkanie', groups: [], n: 1, potL: 12, potCm: 28 },
  { species: 'Zamiokulkas', emoji: '🪴', prefix: 'ZAM', w: 14, f: null, region: 'Mieszkanie', groups: [], n: 1, potL: 8, potCm: 24 },
];

export const SEED_GROUPS: readonly Group[] = [
  { name: 'Szklarnia', emoji: '🏡', type: 'region' },
  { name: 'Balkon', emoji: '🪴', type: 'region' },
  { name: 'Parapet', emoji: '🪟', type: 'region' },
  { name: 'Mieszkanie', emoji: '🛋️', type: 'region' },
  { name: 'Pomidory', emoji: '🍅', type: 'work' },
  { name: 'Papryki', emoji: '🌶️', type: 'work' },
  { name: 'Zioła', emoji: '🌿', type: 'work' },
  { name: 'Owoce', emoji: '🍓', type: 'work' },
];

export const buildSpecies = (): Species[] =>
  GARDEN_DEFS.map((d) => ({
    name: d.species,
    emoji: d.emoji,
    prefix: d.prefix,
    w: d.w,
    f: d.f,
  }));

export const buildGarden = (): Plant[] => {
  const garden: Plant[] = [];
  let id = 1;

  for (const def of GARDEN_DEFS) {
    for (let i = 0; i < def.n; i++) {
      const code = makeCode(def.prefix, i + 1);

      let lastWater = daysAdd(TODAY, -def.w + (i % (def.w + 1)));
      if (i % 5 === 0) lastWater = daysAdd(TODAY, -(def.w + 1));

      let lastFert: string | null = null;
      if (def.f) lastFert = daysAdd(TODAY, -def.f + Math.min(i % (def.f + 1), def.f));

      const noPot = i % 4 === 0;
      garden.push({
        id: id++,
        code,
        species: def.species,
        emoji: def.emoji,
        potL: noPot ? null : def.potL,
        potCm: noPot ? null : def.potCm,
        groups: [def.region, ...def.groups],
        lastWater,
        lastFert,
      });
    }
  }
  return garden;
};

export const buildLog = (garden: readonly Plant[], species: readonly Species[]): LogEntry[] => {
  const log: LogEntry[] = [];
  let uid = 1;

  for (const p of garden) {
    const info = species.find((s) => s.name === p.species);
    log.push({ uid: uid++, id: p.id, type: 'water', date: p.lastWater });
    if (info?.w) {
      log.push({ uid: uid++, id: p.id, type: 'water', date: daysAdd(p.lastWater, -info.w) });
    }
    if (p.lastFert) {
      log.push({ uid: uid++, id: p.id, type: 'fert', date: p.lastFert });
    }
    log.push({ uid: uid++, id: p.id, type: 'add', date: '2026-05-12' });
  }
  log.push({ uid: uid++, id: 1, type: 'repot', date: '2026-06-20', potL: 5, potCm: 20 });
  return log;
};

/** A fresh, fully-seeded initial dataset. */
export const buildSeed = (): {
  garden: Plant[];
  groups: Group[];
  log: LogEntry[];
  species: Species[];
} => {
  const species = buildSpecies();
  const garden = buildGarden();
  return {
    garden,
    groups: SEED_GROUPS.map((g) => ({ ...g })),
    log: buildLog(garden, species),
    species,
  };
};
