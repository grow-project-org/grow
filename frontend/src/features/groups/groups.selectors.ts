import type { ActionType, Group, GroupType, Plant, Species } from '../../types';
import { fmtShort } from '../../utils/date';
import { ACTION_META } from '../../domain/actions';
import { GROUP_TYPE_META } from '../../domain/groups';
import { interval } from '../../domain/species';
import { isDoneToday, isDue, nextDate } from '../../domain/schedule';

export interface GroupActionRow {
  id: string;
  name: string;
  sub: string;
  done: boolean;
  stateLabel: string;
}

export interface GroupAction {
  type: ActionType;
  emoji: string;
  verb: string;
  label: string;
  due: number;
  trackedCount: number;
  headStat: string;
  partial: boolean;
  none: boolean;
  mixedInterval: boolean;
  primaryBtn: string;
  allBtn: string;
  dueIds: string[];
  allIds: string[];
  rows: GroupActionRow[];
}

export interface GroupCard {
  id: string;
  name: string;
  emoji: string;
  type: GroupType;
  isRegion: boolean;
  typeLabel: string;
  tagBg: string;
  tagInk: string;
  members: number;
  memberSub: string;
  actions: GroupAction[];
  allClear: boolean;
  clearLabel: string;
  regionWaterDue: number;
  regionFertDue: number;
  showWarning: boolean;
}

const buildAction = (
  species: readonly Species[],
  members: Plant[],
  type: ActionType,
  today: string,
): GroupAction | null => {
  const tracked = members.filter((p) => interval(species, p.specieId, type) != null);
  if (!tracked.length) return null;

  const dueList = tracked.filter((p) => isDue(p, type, today));
  const intervals = new Set(tracked.map((p) => interval(species, p.specieId, type)));
  const meta = ACTION_META[type];

  const rows: GroupActionRow[] = tracked.map((p) => {
    const done = isDoneToday(p, type, today);
    const due = isDue(p, type, today);

    return {
      id: p.id,
      name: p.code,
      sub: `co ${interval(species, p.specieId, type)} dni · nast. ${fmtShort(nextDate(p, type))}`,
      done,
      stateLabel: done ? 'zrobione' : due ? 'dziś' : 'nie dziś',
    };
  });

  return {
    type,
    emoji: meta.emoji,
    verb: meta.verb,
    label: meta.label,
    due: dueList.length,
    trackedCount: tracked.length,
    headStat: `${dueList.length} z ${tracked.length}`,
    partial: dueList.length > 0 && dueList.length < tracked.length,
    none: dueList.length === 0,
    mixedInterval: intervals.size > 1,
    primaryBtn: `${meta.verb} potrzebujące · ${dueList.length}`,
    allBtn: `${meta.verb} wszystkie · ${tracked.length}`,
    dueIds: dueList.map((p) => p.id),
    allIds: tracked.map((p) => p.id),
    rows,
  };
};

const buildCard = (
  species: readonly Species[],
  group: Group,
  plants: readonly Plant[],
  today: string,
): GroupCard => {
  const members = plants.filter((p) => group.plantIds.includes(p.id));
  const water = buildAction(species, members, 'water', today);
  const fert = buildAction(species, members, 'fert', today);
  const actions = [water, fert].filter((a): a is GroupAction => a !== null);
  const totalDue = actions.reduce((sum, a) => sum + a.due, 0);
  const meta = GROUP_TYPE_META[group.type];

  return {
    id: group.id,
    name: group.name,
    emoji: meta.emoji,
    type: group.type,
    isRegion: group.type === 'region',
    typeLabel: meta.label,
    tagBg: meta.bg,
    tagInk: meta.ink,
    members: members.length,
    memberSub: `${members.length} roślin`,
    actions,
    allClear: totalDue === 0,
    clearLabel: members.length ? 'Wszystko na dziś ogarnięte' : 'Pusta grupa',
    regionWaterDue: water?.due ?? 0,
    regionFertDue: fert?.due ?? 0,
    showWarning: group.type !== 'region' && actions.some((a) => a.mixedInterval),
  };
};

export const selectGroups = (
  species: readonly Species[],
  plants: readonly Plant[],
  groups: readonly Group[],
  today: string,
): GroupCard[] => groups.map((g) => buildCard(species, g, plants, today));
