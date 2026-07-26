import type { ActionType, Group, LogEntry, LogType, Plant, Species } from '../../types';
import { fmtLong, fmtShort } from '../../utils/date';
import { avatarBg, interval } from '../../domain/species';
import { dueDate, lastOf, relLabel } from '../../domain/schedule';
import { regionLabel } from '../../domain/regions';
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
}

export interface HistoryItem {
  key: number;
  emoji: string;
  label: string;
  bg: string;
  date: string;
  showLine: boolean;
}

export interface GroupTag {
  name: string;
  emoji: string;
}

export interface ProfileView {
  avatarBg: string;
  region: string;
  groups: GroupTag[];
  schedule: ScheduleItem[];
  history: HistoryItem[];
  histCount: string;
  potText: string;
  potNote: string;
}

const SCHEDULE_META: Record<ActionType, { emoji: string; bg: string; label: string }> = {
  water: { emoji: '💧', bg: 'var(--color-water-bg)', label: 'Podlewanie' },
  fert: { emoji: '🌱', bg: 'var(--color-fert-bg)', label: 'Nawożenie' },
};

const LOG_META: Record<LogType, { emoji: string; label: string; bg: string }> = {
  water: { emoji: '💧', label: 'Podlano', bg: 'var(--color-water-bg)' },
  fert: { emoji: '🌱', label: 'Nawożono', bg: 'var(--color-fert-bg)' },
  repot: { emoji: '🪴', label: 'Przesadzono', bg: '#ffd6e6' },
  add: { emoji: '🌱', label: 'Dodano do ogrodu', bg: '#e2f5d8' },
  prune: { emoji: '✂️', label: 'Podcięto', bg: '#e8e2ff' },
  harvest: { emoji: '🧺', label: 'Zbiór', bg: '#ffe9c7' },
  custom: { emoji: '📝', label: 'Zdarzenie', bg: '#eee7dd' },
};

const describe = (e: LogEntry): string => {
  const meta = LOG_META[e.type];
  if (e.type === 'repot' && (e.potL != null || e.potCm != null)) {
    const size = e.potL != null ? `${e.potL} l` : `Ø ${e.potCm} cm`;
    return `${meta.label} → ${size}`;
  }
  if (e.type === 'harvest' && (e.qty != null || e.weight != null)) {
    const parts: string[] = [];
    if (e.qty != null) parts.push(`${e.qty} szt.`);
    if (e.weight != null) parts.push(`${e.weight} g`);
    return `${meta.label}: ${parts.join(', ')}`;
  }
  if (e.type === 'custom' && e.note) return e.note;
  return meta.label;
};

const buildSchedule = (species: readonly Species[], p: Plant): ScheduleItem[] =>
  (['water', 'fert'] as const)
    .map((type): ScheduleItem | null => {
      const iv = interval(species, p.species, type);
      if (iv == null) return null;
      const due = dueDate(species, p, type) as string;
      const rel = relLabel(due);
      const colors = relColors(rel);
      return {
        type,
        ...SCHEDULE_META[type],
        detail: `co ${iv} dni · ostatnio ${fmtShort(lastOf(p, type))}`,
        rel: rel.text,
        ink: colors.ink,
        pill: colors.bg,
      };
    })
    .filter((x): x is ScheduleItem => x !== null);

const buildHistory = (p: Plant, log: readonly LogEntry[]): HistoryItem[] => {
  const entries = log
    .filter((e) => e.id === p.id)
    .sort((a, b) => b.date.localeCompare(a.date) || b.uid - a.uid);
  return entries.map((e, i) => {
    const meta = LOG_META[e.type];
    return {
      key: e.uid,
      emoji: meta.emoji,
      label: describe(e),
      bg: meta.bg,
      date: `${fmtLong(e.date)} ${e.date.slice(0, 4)}`,
      showLine: i < entries.length - 1,
    };
  });
};

const potText = (p: Plant): string => {
  if (p.potL != null) return `${p.potL} l${p.potCm ? ` · Ø ${p.potCm} cm` : ''}`;
  if (p.potCm != null) return `Ø ${p.potCm} cm`;
  return 'Nie podano';
};

export const selectProfile = (
  species: readonly Species[],
  p: Plant,
  log: readonly LogEntry[],
  groups: readonly Group[],
): ProfileView => {
  const history = buildHistory(p, log);
  return {
    avatarBg: avatarBg(p.id),
    region: regionLabel(p, groups),
    groups: p.groups.map((name) => ({
      name,
      emoji: groups.find((g) => g.name === name)?.emoji ?? '📁',
    })),
    schedule: buildSchedule(species, p),
    history,
    histCount: `${history.length} wpisów`,
    potText: potText(p),
    potNote:
      p.potL != null
        ? 'Wpływa na częstotliwość podlewania'
        : 'Dodaj, by lepiej prognozować podlewanie',
  };
};
