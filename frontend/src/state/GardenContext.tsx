import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useReducer,
  useRef,
  type ReactNode,
} from 'react';
import type { ActionType, GroupType, LogType } from '../types';
import { gardenReducer, type AddPlantsInput, type GardenState, type LogExtraData } from './gardenReducer';
import { doneKey } from '../domain/schedule';
import { buildSeed } from '../data/seed';
import { loadSnapshot, saveSnapshot } from './persistence';
import { useGardenSync } from '../hooks/useGardenSync';
import { useToast } from './ToastContext';
import type { Plant } from '../types';

/**
 * The public API pages depend on — a small, intention-revealing surface that
 * hides the reducer/dispatch plumbing (Dependency Inversion). Side effects such
 * as toasts are folded into the relevant methods so callers stay declarative.
 */
export interface GardenApi extends GardenState {
  plantById: (id: number) => Plant | undefined;
  /** Mark an action done for a set of plants and optionally show a toast. */
  commitAction: (ids: number[], type: ActionType, message?: string) => void;
  /** Toggle a single plant's "done today" state (check / uncheck). */
  toggleToday: (id: number, type: ActionType) => void;
  /** Record a one-off event (pruning, harvest, a custom note…) for one or more plants. */
  logExtra: (ids: number[], type: LogType, message: string, data?: LogExtraData) => void;
  repot: (id: number, potL: number | null, potCm: number | null, message: string) => void;
  addPlants: (input: AddPlantsInput) => void;
  addSpecies: (name: string, emoji: string, w: number | null, f: number | null) => void;
  toggleGroupMember: (id: number, group: string) => void;
  addGroup: (name: string, type: GroupType, message: string) => void;
  dismissWarning: (group: string) => void;
  /** True while loading from or pushing to the backend. */
  isSyncing: boolean;
}

const GardenContext = createContext<GardenApi | null>(null);

/** Seed on first run, otherwise resume from the last local snapshot. */
const init = (): GardenState =>
  loadSnapshot() ?? { ...buildSeed(), done: {}, dismissed: {} };

export const GardenProvider = ({ children }: { children: ReactNode }) => {
  const [state, dispatch] = useReducer(gardenReducer, undefined, init);
  const { flash } = useToast();

  // Persist every change locally (offline-first).
  useEffect(() => {
    saveSnapshot(state);
  }, [state]);

  // Load the server snapshot on mount; hydrate the reducer when it arrives.
  const { push, isSyncing } = useGardenSync(
    useCallback((remote: GardenState) => dispatch({ kind: 'HYDRATE', state: remote }), []),
  );

  // Debounced push of local edits to the backend once it becomes reachable.
  const pushRef = useRef(push);
  pushRef.current = push;
  const firstRun = useRef(true);
  useEffect(() => {
    if (firstRun.current) {
      firstRun.current = false;
      return;
    }
    const timer = setTimeout(() => pushRef.current(state), 800);
    return () => clearTimeout(timer);
  }, [state]);

  const plantById = useCallback(
    (id: number) => state.garden.find((p) => p.id === id),
    [state.garden],
  );

  const commitAction = useCallback(
    (ids: number[], type: ActionType, message?: string) => {
      if (!ids.length) return;
      dispatch({ kind: 'COMMIT_ACTION', ids, type });
      if (message) flash(message);
    },
    [flash],
  );

  const toggleToday = useCallback(
    (id: number, type: ActionType) => {
      if (state.done[doneKey(id, type)]) {
        dispatch({ kind: 'UNDO_TODAY', id, type });
      } else {
        dispatch({ kind: 'COMMIT_ACTION', ids: [id], type });
      }
    },
    [state.done],
  );

  const logExtra = useCallback(
    (ids: number[], type: LogType, message: string, data?: LogExtraData) => {
      if (!ids.length) return;
      dispatch({ kind: 'LOG_EXTRA', ids, type, data });
      flash(message);
    },
    [flash],
  );

  const repot = useCallback(
    (id: number, potL: number | null, potCm: number | null, message: string) => {
      dispatch({ kind: 'REPOT', id, potL, potCm });
      flash(message);
    },
    [flash],
  );

  const addPlants = useCallback(
    (input: AddPlantsInput) => {
      dispatch({ kind: 'ADD_PLANTS', input });
      const message =
        input.qty > 1
          ? `🌱 Dodano ${input.qty} szt. „${input.species}”`
          : `🌱 Dodano „${input.species}”`;
      flash(message);
    },
    [flash],
  );

  const addSpecies = useCallback(
    (name: string, emoji: string, w: number | null, f: number | null) => {
      dispatch({ kind: 'ADD_SPECIES', name, emoji, w, f });
      flash(`🌱 Dodano gatunek „${name}”`);
    },
    [flash],
  );

  const toggleGroupMember = useCallback((id: number, group: string) => {
    dispatch({ kind: 'TOGGLE_GROUP_MEMBER', id, group });
  }, []);

  const addGroup = useCallback(
    (name: string, type: GroupType, message: string) => {
      dispatch({ kind: 'ADD_GROUP', name, type });
      flash(message);
    },
    [flash],
  );

  const dismissWarning = useCallback((group: string) => {
    dispatch({ kind: 'DISMISS_WARNING', group });
  }, []);

  const value = useMemo<GardenApi>(
    () => ({
      ...state,
      plantById,
      commitAction,
      toggleToday,
      logExtra,
      repot,
      addPlants,
      addSpecies,
      toggleGroupMember,
      addGroup,
      dismissWarning,
      isSyncing,
    }),
    [
      state,
      plantById,
      commitAction,
      toggleToday,
      logExtra,
      repot,
      addPlants,
      addSpecies,
      toggleGroupMember,
      addGroup,
      dismissWarning,
      isSyncing,
    ],
  );

  return <GardenContext.Provider value={value}>{children}</GardenContext.Provider>;
};

export const useGarden = (): GardenApi => {
  const ctx = useContext(GardenContext);
  if (!ctx) throw new Error('useGarden must be used within a GardenProvider');
  return ctx;
};
