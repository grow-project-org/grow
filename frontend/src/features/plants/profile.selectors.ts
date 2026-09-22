import type { ActionType, Group, Plant, Species } from '../../types';
import { fmtShort } from '../../utils/date';
import { ACTION_META, ACTION_TYPES } from '../../domain/actions';
import { avatarBg, interval, speciesName } from '../../domain/species';
import { lastDate, nextDate, relLabel } from '../../domain/schedule';
import { groupsOf, regionLabel } from '../../domain/regions';
import { GROUP_TYPE_META } from '../../domain/groups';
import { relColors } from '../../components/ui/relColors';

export interface ScheduleItem {
  type: ActionType;
  emoji: string;
  bg: string;
  label: string;
  detail: string;
  rel: string;
  ink: string;
  pill: string;
  tracked: boolean;
}

export interface GroupTag {
  id: string;
  name: string;
  emoji: string;
}

export interface ProfileView {
  name: string;
  initial: string;
  avatarBg: string;
  region: string;
  groups: GroupTag[];
  schedule: ScheduleItem[];
}

const SCHEDULE_BG: Record<ActionType, string> = {
  water: 'var(--color-water-bg)',
  fert: 'var(--color-fert-bg)',
};

const NEUTRAL = { bg: 'var(--color-chip)', ink: 'var(--color-muted)' };

const buildSchedule = (
  species: readonly Species[],
  plant: Plant,
  today: string,
): ScheduleItem[] =>
  ACTION_TYPES.map((type) => {
    const iv = interval(species, plant.specieId, type);
    const last = lastDate(plant, type);
    const next = nextDate(plant, type);
    const rel = next ? relLabel(next, today) : null;
    const colors = rel ? relColors(rel) : NEUTRAL;

    const detail = iv == null
      ? 'Gatunek nie ma ustawionego interwału'
      : `co ${iv} dni · ostatnio ${fmtShort(last)}`;

    return {
      type,
      emoji: ACTION_META[type].emoji,
      bg: SCHEDULE_BG[type],
      label: ACTION_META[type].label,
      detail,
      rel: rel ? rel.text : 'brak terminu',
      ink: colors.ink,
      pill: colors.bg,
      tracked: iv != null,
    };
  });

export const selectProfile = (
  species: readonly Species[],
  plant: Plant,
  groups: readonly Group[],
  today: string,
): ProfileView => {
  const name = speciesName(species, plant.specieId);

  return {
    name,
    initial: name.slice(0, 1).toUpperCase(),
    avatarBg: avatarBg(plant.id),
    region: regionLabel(plant, groups),
    groups: groupsOf(plant, groups).map((g) => ({
      id: g.id,
      name: g.name,
      emoji: GROUP_TYPE_META[g.type].emoji,
    })),
    schedule: buildSchedule(species, plant, today),
  };
};
