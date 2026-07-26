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
import type { ActionType, GroupType, LogType, Plant } from '../types';
import { gardenReducer, type AddPlantsInput, type GardenState } from './gardenReducer';
import { doneKey } from '../domain/schedule';
import { buildSeed } from '../data/seed';
import { loadSnapshot, saveSnapshot } from './persistence';
import { useGardenSync } from '../hooks/useGardenSync';
import { useToast } from './ToastContext';

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
  /** Record a non-scheduled event (pruning, harvest, …) in a plant's history. */
  logExtra: (id: number, type: LogType, message: string) => void;
  repot: (id: number, potL: number | null, potCm: number | null, message: string) => void;
  rename: (id: number, code: string, message: string) => void;
  addPlants: (input: AddPlantsInput) => void;
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
    (id: number, type: LogType, message: string) => {
      dispatch({ kind: 'LOG_EXTRA', id, type });
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

  const rename = useCallback(
    (id: number, code: string, message: string) => {
      dispatch({ kind: 'RENAME', id, code });
      flash(message);
    },
    [flash],
  );

  const addPlants = useCallback(
    (input: AddPlantsInput) => {
      dispatch({ kind: 'ADD_PLANTS', input });
      const message =
        input.qty > 1
          ? `🌱 Dodano ${input.qty} szt. „${input.name}”`
          : `🌱 Dodano „${input.name}”`;
      flash(message);
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
      rename,
      addPlants,
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
      rename,
      addPlants,
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
