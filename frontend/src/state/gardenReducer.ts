import type {
  ActionType,
  Group,
  GroupType,
  LogEntry,
  LogType,
  Plant,
} from '../types';
import { TODAY } from '../config';
import { doneKey, type DoneMap } from '../domain/schedule';
import {
  countWithPrefix,
  makeCode,
  nextPlantId,
  nextUid,
  speciesPrefix,
} from '../domain/ids';
import { emojiForSpecies } from '../domain/species';

export interface GardenState {
  garden: Plant[];
  groups: Group[];
  log: LogEntry[];
  /** Actions checked off "today" (visual state), keyed by `id:type`. */
  done: DoneMap;
  /** Group names whose mixed-schedule warning has been dismissed. */
  dismissed: Record<string, boolean>;
}

export interface AddPlantsInput {
  name: string;
  species: string | null;
  qty: number;
  loc: string | null;
  potL: number | null;
  groups: string[];
}

/** Discriminated union of every state transition. */
export type GardenAction =
  | { kind: 'COMMIT_ACTION'; ids: number[]; type: ActionType }
  | { kind: 'UNDO_TODAY'; id: number; type: ActionType }
  | { kind: 'LOG_EXTRA'; id: number; type: LogType }
  | { kind: 'REPOT'; id: number; potL: number | null; potCm: number | null }
  | { kind: 'RENAME'; id: number; code: string }
  | { kind: 'ADD_PLANTS'; input: AddPlantsInput }
  | { kind: 'TOGGLE_GROUP_MEMBER'; id: number; group: string }
  | { kind: 'ADD_GROUP'; name: string; type: GroupType }
  | { kind: 'DISMISS_WARNING'; group: string }
  | { kind: 'HYDRATE'; state: GardenState };

const GROUP_TYPE_EMOJI: Record<GroupType, string> = {
  work: '⚡',
  region: '📍',
  adhoc: '📌',
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

const addPlants = (state: GardenState, input: AddPlantsInput): GardenState => {
  const garden = [...state.garden];
  const log = [...state.log];
  let id = nextPlantId(garden);
  let uid = nextUid(log);
  const prefix = speciesPrefix(input.species);
  let count = countWithPrefix(garden, prefix);
  const emoji = emojiForSpecies(input.species);

  for (let i = 0; i < input.qty; i++) {
    count++;
    garden.push({
      id,
      code: makeCode(prefix, count),
      name: input.name,
      species: input.species,
      emoji,
      loc: input.loc,
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

    case 'UNDO_TODAY': {
      const done = { ...state.done };
      delete done[doneKey(action.id, action.type)];
      return { ...state, done };
    }

    case 'LOG_EXTRA': {
      const uid = nextUid(state.log);
      return {
        ...state,
        log: [...state.log, { uid, id: action.id, type: action.type, date: TODAY }],
      };
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
        log: [...state.log, { uid, id: action.id, type: 'repot', date: TODAY }],
      };
    }

    case 'RENAME':
      return {
        ...state,
        garden: state.garden.map((p) =>
          p.id === action.id ? { ...p, code: action.code } : p,
        ),
      };

    case 'ADD_PLANTS':
      return addPlants(state, action.input);

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
