import { useEffect, useRef } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import type { GardenState } from '../state/gardenReducer';
import { gardenApi } from '../api/endpoints';
import { gardenKeys } from '../api/queryKeys';
import { fromDTO, toDTO } from '../api/dto';

export interface GardenSync {
  /** Push the whole snapshot to the backend (retried by the query client). */
  push: (state: GardenState) => void;
  /** True while a load or push is in flight. */
  isSyncing: boolean;
}

/**
 * Bridges the reducer state with the backend via React Query:
 * - `useQuery` loads the server snapshot (and refetches on reconnect); when it
 *   arrives, `onServerData` hydrates the reducer.
 * - `useMutation` pushes local edits back up.
 * Failures surface as connection popups through the global QueryClient handlers.
 */
export const useGardenSync = (onServerData: (state: GardenState) => void): GardenSync => {
  const query = useQuery({ queryKey: gardenKeys.all, queryFn: gardenApi.fetch });
  const callback = useRef(onServerData);
  callback.current = onServerData;

  useEffect(() => {
    if (query.data) callback.current(fromDTO(query.data));
  }, [query.data]);

  const mutation = useMutation({
    mutationFn: (state: GardenState) => gardenApi.push(toDTO(state)),
  });

  return {
    push: mutation.mutate,
    isSyncing: query.isFetching || mutation.isPending,
  };
};
