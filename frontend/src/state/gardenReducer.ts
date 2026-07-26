import type {
  ActionType,
  Group,
  GroupType,
  LogEntry,
  LogType,
  Plant,
  Species,
} from '../types';
import { TODAY } from '../config';
import { doneKey, type DoneMap } from '../domain/schedule';
import {
  countWithPrefix,
  makeCode,
  nextPlantId,
  nextUid,
  prefixFromName,
} from '../domain/ids';
import { emojiForSpecies } from '../domain/species';

export interface GardenState {
  garden: Plant[];
  groups: Group[];
  log: LogEntry[];
  species: Species[];
  /** Actions checked off "today" (visual state), keyed by `id:type`. */
  done: DoneMap;
  /** Group names whose mixed-schedule warning has been dismissed. */
  dismissed: Record<string, boolean>;
}

export interface AddPlantsInput {
  species: string;
  qty: number;
  potL: number | null;
  groups: string[];
  /** Manual code — only honoured when qty is exactly 1; otherwise auto-generated. */
  code?: string | null;
}

/** Optional structured data carried by a one-off history entry. */
export interface LogExtraData {
  potL?: number | null;
  potCm?: number | null;
  qty?: number | null;
  weight?: number | null;
  note?: string;
}

/** Discriminated union of every state transition. */
export type GardenAction =
  | { kind: 'COMMIT_ACTION'; ids: number[]; type: ActionType }
  | { kind: 'UNDO_TODAY'; id: number; type: ActionType }
  | { kind: 'LOG_EXTRA'; ids: number[]; type: LogType; data?: LogExtraData }
  | { kind: 'REPOT'; id: number; potL: number | null; potCm: number | null }
  | { kind: 'ADD_PLANTS'; input: AddPlantsInput }
  | { kind: 'ADD_SPECIES'; name: string; emoji: string; w: number | null; f: number | null }
  | { kind: 'TOGGLE_GROUP_MEMBER'; id: number; group: string }
  | { kind: 'ADD_GROUP'; name: string; type: GroupType }
  | { kind: 'DISMISS_WARNING'; group: string }
  | { kind: 'HYDRATE'; state: GardenState };

const GROUP_TYPE_EMOJI: Record<GroupType, string> = {
  work: '⚡',
  region: '📍',
  adhoc: '📌',
};

/**
 * Undoing today's action reverts to the previous last-done date, not just a
 * flag. Call with the log *after* removing today's entry — the most recent
 * remaining entry of that type is what the plant's date reverts to.
 */
const previousDate = (log: readonly LogEntry[], id: number, type: ActionType): string | null => {
  const entries = log
    .filter((e) => e.id === id && e.type === type)
    .sort((a, b) => b.date.localeCompare(a.date) || b.uid - a.uid);
  return entries[0]?.date ?? null;
};

const commitAction = (state: GardenState, ids: number[], type: ActionType): GardenState => {
  const field = type === 'water' ? 'lastWater' : 'lastFert';
  const garden = state.garden.map((p) =>
    ids.includes(p.id) ? { ...p, [field]: TODAY } : p,
  );
  const done: DoneMap = { ...state.done };
  ids.forEach((id) => {
    done[doneKey(id, type)] = true;
  });
  let uid = nextUid(state.log);
  const log = [...state.log];
  ids.forEach((id) => log.push({ uid: uid++, id, type, date: TODAY }));
  return { ...state, garden, done, log };
};

const undoToday = (state: GardenState, id: number, type: ActionType): GardenState => {
  const done = { ...state.done };
  delete done[doneKey(id, type)];

  const field = type === 'water' ? 'lastWater' : 'lastFert';
  const log = state.log.filter((e) => !(e.id === id && e.type === type && e.date === TODAY));
  const restored = previousDate(log, id, type);

  return {
    ...state,
    done,
    log,
    garden: state.garden.map((p) => (p.id === id ? { ...p, [field]: restored } : p)),
  };
};

const addSpecies = (
  state: GardenState,
  name: string,
  emoji: string,
  w: number | null,
  f: number | null,
): GardenState => ({
  ...state,
  species: [...state.species, { name, emoji, prefix: prefixFromName(name), w, f }],
});

const addPlants = (state: GardenState, input: AddPlantsInput): GardenState => {
  const garden = [...state.garden];
  const log = [...state.log];
  let id = nextPlantId(garden);
  let uid = nextUid(log);
  const sp = state.species.find((s) => s.name === input.species);
  const prefix = sp?.prefix ?? prefixFromName(input.species);
  let count = countWithPrefix(garden, prefix);
  const emoji = emojiForSpecies(state.species, input.species);
  const manualCode = input.qty === 1 ? input.code?.trim() || null : null;

  for (let i = 0; i < input.qty; i++) {
    count++;
    const code = manualCode ?? makeCode(prefix, count);
    garden.push({
      id,
      code,
      species: input.species,
      emoji,
      potL: input.potL,
      potCm: null,
      groups: [...input.groups],
      lastWater: TODAY,
      lastFert: null,
    });
    log.push({ uid: uid++, id, type: 'add', date: TODAY });
    id++;
  }
  return { ...state, garden, log };
};

export const gardenReducer = (state: GardenState, action: GardenAction): GardenState => {
  switch (action.kind) {
    case 'COMMIT_ACTION':
      return commitAction(state, action.ids, action.type);

    case 'UNDO_TODAY':
      return undoToday(state, action.id, action.type);

    case 'LOG_EXTRA': {
      let uid = nextUid(state.log);
      const entries: LogEntry[] = action.ids.map((id) => ({
        uid: uid++,
        id,
        type: action.type,
        date: TODAY,
        ...action.data,
      }));
      return { ...state, log: [...state.log, ...entries] };
    }

    case 'REPOT': {
      const uid = nextUid(state.log);
      return {
        ...state,
        garden: state.garden.map((p) =>
          p.id === action.id
            ? { ...p, potL: action.potL ?? p.potL, potCm: action.potCm ?? p.potCm }
            : p,
        ),
        log: [
          ...state.log,
          { uid, id: action.id, type: 'repot', date: TODAY, potL: action.potL, potCm: action.potCm },
        ],
      };
    }

    case 'ADD_PLANTS':
      return addPlants(state, action.input);

    case 'ADD_SPECIES':
      return addSpecies(state, action.name, action.emoji, action.w, action.f);

    case 'TOGGLE_GROUP_MEMBER':
      return {
        ...state,
        garden: state.garden.map((p) => {
          if (p.id !== action.id) return p;
          const inGroup = p.groups.includes(action.group);
          return {
            ...p,
            groups: inGroup
              ? p.groups.filter((g) => g !== action.group)
              : [...p.groups, action.group],
          };
        }),
      };

    case 'ADD_GROUP':
      return {
        ...state,
        groups: [
          ...state.groups,
          { name: action.name, emoji: GROUP_TYPE_EMOJI[action.type], type: action.type },
        ],
      };

    case 'DISMISS_WARNING':
      return {
        ...state,
        dismissed: { ...state.dismissed, [action.group]: true },
      };

    case 'HYDRATE':
      return action.state;

    default:
      return state;
  }
};
