import { createContext, useCallback, useContext, useMemo, type ReactNode } from 'react';
import { useIsMutating, useQuery, useQueryClient } from '@tanstack/react-query';
import { plantGroupsApi, plantsApi, speciesApi } from '../api/resources';
import { plantGroupsKeys, plantsKeys, speciesKeys } from '../api/queryKeys';
import { toGroup, toPlant, toSpecies } from '../domain/mappers';
import { ACTION_API_TYPE, ACTION_ROUTE_NAME } from '../domain/actions';
import { formatIntervalDays } from '../domain/interval';
import { GROUP_TYPE_TO_API } from '../domain/groups';
import type { ActionType, Group, GroupType, Plant, Species } from '../types';
import { today as todayIso } from '../utils/date';
import { useAuth } from './AuthContext';
import { useToast } from './ToastContext';

export interface AddPlantInput {
  specieId: string;
  code: string;
  groupIds: string[];
}

export interface AddSpeciesInput {
  name: string;
  w: number | null;
  f: number | null;
}

export interface GardenApi {
  species: Species[];
  plants: Plant[];
  groups: Group[];
  today: string;
  isLoading: boolean;
  isSyncing: boolean;
  plantById: (id: string) => Plant | undefined;
  commitAction: (
    plantIds: string[],
    type: ActionType,
    message?: string,
    executedOn?: string,
  ) => Promise<void>;
  addPlant: (input: AddPlantInput) => Promise<void>;
  addSpecies: (input: AddSpeciesInput) => Promise<void>;
  addGroup: (name: string, type: GroupType) => Promise<void>;
  setGroupMembership: (plantId: string, groupId: string, member: boolean) => Promise<void>;
}

const GardenContext = createContext<GardenApi | null>(null);

export const GardenProvider = ({ children }: { children: ReactNode }) => {
  const { status } = useAuth();
  const { flash } = useToast();
  const queryClient = useQueryClient();
  const enabled = status === 'authenticated';

  const speciesQuery = useQuery({
    queryKey: speciesKeys.all,
    queryFn: () => speciesApi.list(),
    enabled,
  });

  const plantsQuery = useQuery({
    queryKey: plantsKeys.all,
    queryFn: () => plantsApi.list(),
    enabled,
  });

  const groupsQuery = useQuery({
    queryKey: plantGroupsKeys.all,
    queryFn: () => plantGroupsApi.list(),
    enabled,
  });

  const pendingMutations = useIsMutating();

  const species = useMemo(() => (speciesQuery.data ?? []).map(toSpecies), [speciesQuery.data]);
  const plants = useMemo(() => (plantsQuery.data ?? []).map(toPlant), [plantsQuery.data]);
  const groups = useMemo(() => (groupsQuery.data ?? []).map(toGroup), [groupsQuery.data]);
  const today = useMemo(() => todayIso(), []);

  const invalidate = useCallback(
    (keys: readonly (readonly string[])[]) =>
      Promise.all(keys.map((queryKey) => queryClient.invalidateQueries({ queryKey }))),
    [queryClient],
  );

  const plantById = useCallback((id: string) => plants.find((p) => p.id === id), [plants]);

  const commitAction = useCallback(
    async (plantIds: string[], type: ActionType, message?: string, executedOn?: string) => {
      if (!plantIds.length) return;

      const executedAt = `${executedOn ?? today}T00:00:00.000Z`;
      await Promise.all(
        plantIds.map((id) => plantsApi.addEvent(id, ACTION_API_TYPE[type], executedAt)),
      );

      await invalidate([plantsKeys.all]);
      if (message) flash(message);
    },
    [today, invalidate, flash],
  );

  const addPlant = useCallback(
    async ({ specieId, code, groupIds }: AddPlantInput) => {
      const created = await plantsApi.create(code, specieId);
      await Promise.all(groupIds.map((groupId) => plantsApi.addToGroup(created.createdPlantId, groupId)));

      await invalidate([plantsKeys.all, plantGroupsKeys.all]);
      flash(`🌱 Dodano „${code}”`);
    },
    [invalidate, flash],
  );

  const addSpecies = useCallback(
    async ({ name, w, f }: AddSpeciesInput) => {
      const created = await speciesApi.create(name);

      if (w != null) {
        await speciesApi.updateInterval(created.specieId, ACTION_ROUTE_NAME.water, formatIntervalDays(w));
      }
      if (f != null) {
        await speciesApi.updateInterval(created.specieId, ACTION_ROUTE_NAME.fert, formatIntervalDays(f));
      }

      await invalidate([speciesKeys.all]);
      flash(`🌱 Dodano gatunek „${name}”`);
    },
    [invalidate, flash],
  );

  const addGroup = useCallback(
    async (name: string, type: GroupType) => {
      await plantGroupsApi.create(name, GROUP_TYPE_TO_API[type]);

      await invalidate([plantGroupsKeys.all]);
      flash(`➕ Dodano „${name}”`);
    },
    [invalidate, flash],
  );

  const setGroupMembership = useCallback(
    async (plantId: string, groupId: string, member: boolean) => {
      await (member
        ? plantsApi.addToGroup(plantId, groupId)
        : plantsApi.removeFromGroup(plantId, groupId));

      await invalidate([plantsKeys.all, plantGroupsKeys.all]);
    },
    [invalidate],
  );

  const value = useMemo<GardenApi>(
    () => ({
      species,
      plants,
      groups,
      today,
      isLoading: speciesQuery.isLoading || plantsQuery.isLoading || groupsQuery.isLoading,
      isSyncing:
        pendingMutations > 0 ||
        speciesQuery.isFetching ||
        plantsQuery.isFetching ||
        groupsQuery.isFetching,
      plantById,
      commitAction,
      addPlant,
      addSpecies,
      addGroup,
      setGroupMembership,
    }),
    [
      species,
      plants,
      groups,
      today,
      speciesQuery.isLoading,
      speciesQuery.isFetching,
      plantsQuery.isLoading,
      plantsQuery.isFetching,
      groupsQuery.isLoading,
      groupsQuery.isFetching,
      pendingMutations,
      plantById,
      commitAction,
      addPlant,
      addSpecies,
      addGroup,
      setGroupMembership,
    ],
  );

  return <GardenContext.Provider value={value}>{children}</GardenContext.Provider>;
};

export const useGarden = (): GardenApi => {
  const ctx = useContext(GardenContext);
  if (!ctx) throw new Error('useGarden must be used within a GardenProvider');
  return ctx;
};
