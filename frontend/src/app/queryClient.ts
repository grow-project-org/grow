import { QueryCache, QueryClient, MutationCache } from '@tanstack/react-query';
import { ApiError } from '../api/http';
import { clearNotices, notifyServerError } from '../state/notifications';

const describe = (error: unknown): string =>
  error instanceof ApiError ? error.userMessage : 'Nie udało się połączyć z serwerem.';

/**
 * A single QueryClient with global handlers: any query/mutation failure raises
 * a connection popup, and any success clears it (connection restored).
 * `networkMode: 'always'` makes requests actually fire against the (dead) API
 * so the offline path is exercised rather than paused.
 */
export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => notifyServerError(describe(error)),
    onSuccess: () => clearNotices(),
  }),
  mutationCache: new MutationCache({
    onError: (error) => notifyServerError(describe(error)),
    onSuccess: () => clearNotices(),
  }),
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      refetchOnReconnect: true,
      staleTime: 30_000,
      networkMode: 'always',
    },
    mutations: {
      retry: 2,
      networkMode: 'always',
    },
  },
});
