import type { ActionType, Group, GroupType, Plant } from '../../types';
import { fmtShort } from '../../utils/date';
import { interval } from '../../domain/species';
import {
  dueDate,
  isDoneToday,
  isDue,
  type DoneMap,
} from '../../domain/schedule';

export interface GroupActionRow {
  id: number;
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
  alignable: boolean;
  primaryBtn: string;
  allBtn: string;
  dueIds: number[];
  allIds: number[];
  rows: GroupActionRow[];
}

export interface GroupCard {
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

const TYPE_META: Record<GroupType, { label: string; bg: string; ink: string }> = {
  work: { label: 'Grupa robocza', bg: '#e7f0ff', ink: '#2f5fa8' },
  region: { label: 'Region', bg: '#f3ecff', ink: '#6b4bb0' },
  adhoc: { label: 'Doraźna', bg: '#fff0e0', ink: '#b5701a' },
};

const ACTION_META: Record<ActionType, { emoji: string; verb: string; label: string }> = {
  water: { emoji: '💧', verb: 'Podlej', label: 'Podlewanie' },
  fert: { emoji: '🌱', verb: 'Nawóź', label: 'Nawożenie' },
};

const buildAction = (
  group: Group,
  members: Plant[],
  type: ActionType,
  done: DoneMap,
): GroupAction | null => {
  const tracked = members.filter((p) => interval(p.species, type) != null);
  if (!tracked.length) return null;

  const dueList = tracked.filter((p) => isDue(p, type, done));
  const intervals = new Set(tracked.map((p) => interval(p.species, type)));
  const dueDates = new Set(tracked.map((p) => dueDate(p, type)));
  const mixedInterval = intervals.size > 1;
  const canAlign = !mixedInterval && dueDates.size > 1;
  const meta = ACTION_META[type];

  const rows: GroupActionRow[] = tracked.map((p) => {
    const done1 = isDoneToday(done, p.id, type);
    const nowDue = isDue(p, type, done);
    return {
      id: p.id,
      name: p.name,
      sub: `co ${interval(p.species, type)} dni · nast. ${fmtShort(dueDate(p, type))}`,
      done: done1,
      stateLabel: done1 ? 'zrobione' : nowDue ? 'dziś' : 'nie dziś',
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
    mixedInterval,
    alignable: canAlign && group.type !== 'region',
    primaryBtn: `${meta.verb} potrzebujące · ${dueList.length}`,
    allBtn: `${meta.verb} wszystkie · ${tracked.length}`,
    dueIds: dueList.map((p) => p.id),
    allIds: tracked.map((p) => p.id),
    rows,
  };
};

const buildCard = (
  group: Group,
  garden: Plant[],
  done: DoneMap,
  dismissed: Record<string, boolean>,
): GroupCard => {
  const members = garden.filter((p) => p.groups.includes(group.name));
  const water = buildAction(group, members, 'water', done);
  const fert = buildAction(group, members, 'fert', done);
  const actions = [water, fert].filter((a): a is GroupAction => a !== null);
  const totalDue = actions.reduce((sum, a) => sum + a.due, 0);
  const anyMixed = group.type !== 'region' && actions.some((a) => a.mixedInterval);
  const meta = TYPE_META[group.type];

  return {
    name: group.name,
    emoji: group.emoji,
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
    showWarning: anyMixed && !dismissed[group.name],
  };
};

export const selectGroups = (
  garden: Plant[],
  groups: Group[],
  done: DoneMap,
  dismissed: Record<string, boolean>,
): GroupCard[] => groups.map((g) => buildCard(g, garden, done, dismissed));
