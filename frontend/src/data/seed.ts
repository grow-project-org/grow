import type { Group, LogEntry, Plant } from '../types';
import { TODAY } from '../config';
import { daysAdd } from '../utils/date';
import { PREFIX, SPECIES } from '../domain/species';
import { makeCode } from '../domain/ids';

/**
 * Deterministic seed data. Generates ~72 concrete plant instances across a
 * handful of varieties, with staggered last-care dates so the schedule shows a
 * realistic mix of due / overdue / upcoming plants against {@link TODAY}.
 */

interface GardenDef {
  sp: string;
  name: string;
  emoji: string;
  loc: string;
  groups: string[];
  /** Number of instances to create. */
  n: number;
  potL: number;
  potCm: number;
}

const GARDEN_DEFS: readonly GardenDef[] = [
  { sp: 'Papryka', name: 'Papryka Ostra', emoji: '🌶️', loc: 'Szklarnia', groups: ['Szklarnia', 'Papryki'], n: 12, potL: 5, potCm: 20 },
  { sp: 'Papryka', name: 'Papryka Słodka', emoji: '🌶️', loc: 'Szklarnia', groups: ['Szklarnia', 'Papryki'], n: 8, potL: 5, potCm: 20 },
  { sp: 'Pomidor', name: 'Pomidor Malinowy', emoji: '🍅', loc: 'Szklarnia', groups: ['Szklarnia', 'Pomidory'], n: 6, potL: 10, potCm: 26 },
  { sp: 'Pomidor', name: 'Pomidor Koktajl', emoji: '🍅', loc: 'Szklarnia', groups: ['Szklarnia', 'Pomidory'], n: 4, potL: 7, potCm: 24 },
  { sp: 'Ogórek', name: 'Ogórek Gruntowy', emoji: '🥒', loc: 'Szklarnia', groups: ['Szklarnia'], n: 5, potL: 8, potCm: 24 },
  { sp: 'Sałata', name: 'Sałata Dębolistna', emoji: '🥬', loc: 'Szklarnia', groups: ['Szklarnia'], n: 6, potL: 2, potCm: 14 },
  { sp: 'Cukinia', name: 'Cukinia', emoji: '🌿', loc: 'Szklarnia', groups: ['Szklarnia'], n: 3, potL: 12, potCm: 30 },
  { sp: 'Truskawka', name: 'Truskawka', emoji: '🍓', loc: 'Balkon', groups: ['Balkon', 'Owoce'], n: 8, potL: 3, potCm: 16 },
  { sp: 'Pelargonia', name: 'Pelargonia', emoji: '🌸', loc: 'Balkon', groups: ['Balkon'], n: 4, potL: 4, potCm: 18 },
  { sp: 'Surfinia', name: 'Surfinia', emoji: '💐', loc: 'Balkon', groups: ['Balkon'], n: 3, potL: 6, potCm: 22 },
  { sp: 'Bazylia', name: 'Bazylia', emoji: '🌿', loc: 'Parapet kuchenny', groups: ['Parapet', 'Zioła'], n: 4, potL: 1.5, potCm: 12 },
  { sp: 'Mięta', name: 'Mięta', emoji: '🌿', loc: 'Parapet kuchenny', groups: ['Parapet', 'Zioła'], n: 3, potL: 1, potCm: 10 },
  { sp: 'Tymianek', name: 'Tymianek', emoji: '🌿', loc: 'Parapet kuchenny', groups: ['Parapet', 'Zioła'], n: 3, potL: 1, potCm: 10 },
  { sp: 'Monstera', name: 'Monstera', emoji: '🌴', loc: 'Salon', groups: ['Mieszkanie'], n: 1, potL: 15, potCm: 32 },
  { sp: 'Fikus', name: 'Fikus lirata', emoji: '🪴', loc: 'Salon', groups: ['Mieszkanie'], n: 1, potL: 12, potCm: 28 },
  { sp: 'Zamiokulkas', name: 'Zamiokulkas', emoji: '🪴', loc: 'Sypialnia', groups: ['Mieszkanie'], n: 1, potL: 8, potCm: 24 },
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

export const buildGarden = (): Plant[] => {
  const garden: Plant[] = [];
  const perSpecies: Record<string, number> = {};
  let id = 1;

  for (const def of GARDEN_DEFS) {
    const info = SPECIES[def.sp] ?? { w: 3, f: null };
    const waterIv = info.w ?? 3;
    for (let i = 0; i < def.n; i++) {
      perSpecies[def.sp] = (perSpecies[def.sp] ?? 0) + 1;
      const prefix = PREFIX[def.sp] ?? def.sp.slice(0, 3).toUpperCase();
      const code = makeCode(prefix, perSpecies[def.sp]);

      let lastWater = daysAdd(TODAY, -waterIv + (i % (waterIv + 1)));
      if (i % 5 === 0) lastWater = daysAdd(TODAY, -(waterIv + 1));

      let lastFert: string | null = null;
      if (info.f) lastFert = daysAdd(TODAY, -info.f + Math.min(i % (info.f + 1), info.f));

      const noPot = i % 4 === 0;
      garden.push({
        id: id++,
        code,
        name: def.name,
        species: def.sp,
        emoji: def.emoji,
        loc: def.loc,
        potL: noPot ? null : def.potL,
        potCm: noPot ? null : def.potCm,
        groups: [...def.groups],
        lastWater,
        lastFert,
      });
    }
  }
  return garden;
};

export const buildLog = (garden: Plant[]): LogEntry[] => {
  const log: LogEntry[] = [];
  let uid = 1;

  for (const p of garden) {
    const info = SPECIES[p.species ?? ''] ?? { w: null, f: null };
    log.push({ uid: uid++, id: p.id, type: 'water', date: p.lastWater });
    if (info.w) {
      log.push({ uid: uid++, id: p.id, type: 'water', date: daysAdd(p.lastWater, -info.w) });
    }
    if (p.lastFert) {
      log.push({ uid: uid++, id: p.id, type: 'fert', date: p.lastFert });
    }
    log.push({ uid: uid++, id: p.id, type: 'add', date: '2026-05-12' });
  }
  log.push({ uid: uid++, id: 1, type: 'repot', date: '2026-06-20' });
  return log;
};

/** A fresh, fully-seeded initial dataset. */
export const buildSeed = (): { garden: Plant[]; groups: Group[]; log: LogEntry[] } => {
  const garden = buildGarden();
  return {
    garden,
    groups: SEED_GROUPS.map((g) => ({ ...g })),
    log: buildLog(garden),
  };
};
