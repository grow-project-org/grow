import { useEffect, useMemo, useRef, type Dispatch } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useAuth } from '../state/AuthContext';
import type { GardenAction, GardenState } from '../state/gardenReducer';
import { plantGroupsApi, plantsApi, speciesApi } from '../api/endpoints';
import { speciesKeys } from '../api/queryKeys';
import {
  ACTION_TYPE_ROUTE_NAME,
  ACTION_TYPE_TO_API,
  formatIntervalDays,
  GROUP_TYPE_TO_API,
  parseIntervalDays,
} from '../api/dto';
import { DEFAULT_EMOJI } from '../domain/species';
import { prefixFromName } from '../domain/ids';
import type { ActionType, Species } from '../types';

/**
 * Reactively mirrors local creates/events to the backend wherever it has a
 * matching endpoint (see api/endpoints.ts for exactly what that covers).
 * Local state (reducer + localStorage) stays the single source of truth —
 * this only ever *adds* a `remoteId`/`syncedToServer` marker once a push
 * succeeds, it never blocks, reverts or waits on a local edit. Driven by
 * diffing state against what's already been pushed (tracked in refs) rather
 * than by hooking each dispatch call, so a plant created before its species
 * finished syncing (or an app reload with unsynced items) still catches up
 * automatically once its dependency gets a `remoteId`.
 *
 * Anything with no backend counterpart at all (repot, prune, harvest,
 * custom notes, undoing an event, reading plants/groups back) is left
 * entirely alone — see the `TODO(backend)` comments at those call sites in
 * `state/GardenContext.tsx`.
 */
export interface BackendSync {
  /** True while any backend push/pull triggered by this hook is in flight. */
  isSyncing: boolean;
}

export const useBackendSync = (state: GardenState, dispatch: Dispatch<GardenAction>): BackendSync => {
  const { status } = useAuth();
  const authenticated = status === 'authenticated';

  const speciesQuery = useQuery({
    queryKey: speciesKeys.all,
    queryFn: speciesApi.list,
    enabled: authenticated,
  });

  // Pull: species the backend already knows about but this device doesn't.
  useEffect(() => {
    if (!speciesQuery.data) return;
    for (const dto of speciesQuery.data) {
      if (state.species.some((s) => s.name === dto.name)) continue;
      const species: Species = {
        name: dto.name,
        emoji: DEFAULT_EMOJI,
        prefix: prefixFromName(dto.name),
        w: dto.intervals.Watering != null ? parseIntervalDays(dto.intervals.Watering) : null,
        f: dto.intervals.Fertilizing != null ? parseIntervalDays(dto.intervals.Fertilizing) : null,
        remoteId: dto.id,
      };
      dispatch({ kind: 'ADD_SPECIES_FROM_SERVER', species });
    }
  }, [speciesQuery.data, state.species, dispatch]);

  const createSpecies = useMutation({ mutationFn: speciesApi.create });
  const updateInterval = useMutation({
    mutationFn: (args: { specieId: string; type: ActionType; days: number }) =>
      speciesApi.updateInterval(args.specieId, ACTION_TYPE_ROUTE_NAME[args.type], {
        interval: formatIntervalDays(args.days),
      }),
  });

  // Push: local species without a remoteId yet.
  const pendingSpecies = useRef(new Set<string>());
  useEffect(() => {
    if (!authenticated) return;
    for (const s of state.species) {
      if (s.remoteId || pendingSpecies.current.has(s.name)) continue;
      pendingSpecies.current.add(s.name);
      createSpecies
        .mutateAsync({ name: s.name })
        .then(async ({ specieId }) => {
          if (s.w != null) await updateInterval.mutateAsync({ specieId, type: 'water', days: s.w });
          if (s.f != null) await updateInterval.mutateAsync({ specieId, type: 'fert', days: s.f });
          dispatch({ kind: 'SET_SPECIES_REMOTE_ID', name: s.name, remoteId: specieId });
        })
        .catch(() => {
          // Surfaced through the mutation cache's global handler already
          // (see app/queryClient.ts) — nothing more to do than retry later.
        })
        .finally(() => pendingSpecies.current.delete(s.name));
    }
    // createSpecies/updateInterval are stable react-query mutation objects;
    // omitted to avoid re-running this effect on every render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authenticated, state.species, dispatch]);

  // Push: local plants without a remoteId yet, once their species has one.
  const createPlant = useMutation({ mutationFn: plantsApi.create });
  const pendingPlants = useRef(new Set<number>());
  useEffect(() => {
    if (!authenticated) return;
    for (const p of state.garden) {
      if (p.remoteId || pendingPlants.current.has(p.id)) continue;
      const specie = state.species.find((s) => s.name === p.species);
      if (!specie?.remoteId) continue; // waits for the species push above
      pendingPlants.current.add(p.id);
      createPlant
        .mutateAsync({ customId: p.code, specieId: specie.remoteId })
        .then(({ createdPlantId }) => {
          dispatch({ kind: 'SET_PLANT_REMOTE_ID', id: p.id, remoteId: createdPlantId });
        })
        .catch(() => {})
        .finally(() => pendingPlants.current.delete(p.id));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authenticated, state.garden, state.species, dispatch]);

  // Push: water/fert log entries not yet posted as a plant event.
  const addEvent = useMutation({
    mutationFn: (args: { plantId: string; type: ActionType; executedAt: string }) =>
      plantsApi.addEvent(args.plantId, { type: ACTION_TYPE_TO_API[args.type], executedAt: args.executedAt }),
  });
  const pendingEvents = useRef(new Set<number>());
  useEffect(() => {
    if (!authenticated) return;
    for (const entry of state.log) {
      if (entry.type !== 'water' && entry.type !== 'fert') continue; // TODO(backend): no other event types exist server-side
      if (entry.syncedToServer || pendingEvents.current.has(entry.uid)) continue;
      const plant = state.garden.find((p) => p.id === entry.id);
      if (!plant?.remoteId) continue;
      pendingEvents.current.add(entry.uid);
      addEvent
        .mutateAsync({
          plantId: plant.remoteId,
          type: entry.type,
          executedAt: new Date(`${entry.date}T00:00:00.000Z`).toISOString(),
        })
        .then(() => dispatch({ kind: 'MARK_LOG_ENTRY_SYNCED', uid: entry.uid }))
        .catch(() => {})
        .finally(() => pendingEvents.current.delete(entry.uid));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authenticated, state.log, state.garden, dispatch]);

  // Push: local groups without a remoteId yet.
  const createGroup = useMutation({ mutationFn: plantGroupsApi.create });
  const pendingGroups = useRef(new Set<string>());
  useEffect(() => {
    if (!authenticated) return;
    for (const g of state.groups) {
      if (g.remoteId || pendingGroups.current.has(g.name)) continue;
      pendingGroups.current.add(g.name);
      createGroup
        .mutateAsync({ name: g.name, type: GROUP_TYPE_TO_API[g.type] })
        .then(({ createdPlantGroupId }) => {
          dispatch({ kind: 'SET_GROUP_REMOTE_ID', name: g.name, remoteId: createdPlantGroupId });
        })
        .catch(() => {})
        .finally(() => pendingGroups.current.delete(g.name));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authenticated, state.groups, dispatch]);

  // Push: plant↔group membership diffed against what we last successfully
  // synced (there's no `remoteId`-style field for a membership row itself).
  const addToGroup = useMutation({
    mutationFn: (args: { plantId: string; groupId: string }) =>
      plantsApi.addToGroup(args.plantId, args.groupId),
  });
  const removeFromGroup = useMutation({
    mutationFn: (args: { plantId: string; groupId: string }) =>
      plantsApi.removeFromGroup(args.plantId, args.groupId),
  });
  const lastSyncedMembership = useRef(new Map<string, boolean>());
  const pendingMembership = useRef(new Set<string>());
  useEffect(() => {
    if (!authenticated) return;
    for (const p of state.garden) {
      if (!p.remoteId) continue;
      for (const g of state.groups) {
        if (!g.remoteId) continue;
        const key = `${p.id}:${g.name}`;
        const isMember = p.groups.includes(g.name);
        const lastSynced = lastSyncedMembership.current.get(key) ?? false;
        if (isMember === lastSynced || pendingMembership.current.has(key)) continue;
        pendingMembership.current.add(key);
        const plantId = p.remoteId;
        const groupId = g.remoteId;
        const call = isMember
          ? addToGroup.mutateAsync({ plantId, groupId })
          : removeFromGroup.mutateAsync({ plantId, groupId });
        call
          .then(() => lastSyncedMembership.current.set(key, isMember))
          .catch(() => {})
          .finally(() => pendingMembership.current.delete(key));
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authenticated, state.garden, state.groups]);

  const isSyncing = useMemo(
    () =>
      speciesQuery.isFetching ||
      createSpecies.isPending ||
      updateInterval.isPending ||
      createPlant.isPending ||
      addEvent.isPending ||
      createGroup.isPending ||
      addToGroup.isPending ||
      removeFromGroup.isPending,
    [
      speciesQuery.isFetching,
      createSpecies.isPending,
      updateInterval.isPending,
      createPlant.isPending,
      addEvent.isPending,
      createGroup.isPending,
      addToGroup.isPending,
      removeFromGroup.isPending,
    ],
  );

  return { isSyncing };
};
