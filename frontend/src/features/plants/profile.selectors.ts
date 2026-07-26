import type { ActionType, Group, LogEntry, LogType, Plant } from '../../types';
import { fmtLong, fmtShort } from '../../utils/date';
import { avatarBg, interval } from '../../domain/species';
import { dueDate, lastOf, relLabel } from '../../domain/schedule';
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
  move: { emoji: '📍', label: 'Zmiana miejsca', bg: '#ffe0d0' },
};

const buildSchedule = (p: Plant): ScheduleItem[] =>
  (['water', 'fert'] as const)
    .map((type): ScheduleItem | null => {
      const iv = interval(p.species, type);
      if (iv == null) return null;
      const due = dueDate(p, type) as string;
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

const buildHistory = (p: Plant, log: LogEntry[]): HistoryItem[] => {
  const entries = log
    .filter((e) => e.id === p.id)
    .sort((a, b) => b.date.localeCompare(a.date) || b.uid - a.uid);
  return entries.map((e, i) => {
    const meta = LOG_META[e.type];
    return {
      key: e.uid,
      emoji: meta.emoji,
      label: meta.label,
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

export const selectProfile = (p: Plant, log: LogEntry[], groups: Group[]): ProfileView => {
  const history = buildHistory(p, log);
  return {
    avatarBg: avatarBg(p.id),
    groups: p.groups.map((name) => ({
      name,
      emoji: groups.find((g) => g.name === name)?.emoji ?? '📁',
    })),
    schedule: buildSchedule(p),
    history,
    histCount: `${history.length} wpisów`,
    potText: potText(p),
    potNote:
      p.potL != null
        ? 'Wpływa na częstotliwość podlewania'
        : 'Dodaj, by lepiej prognozować podlewanie',
  };
};
