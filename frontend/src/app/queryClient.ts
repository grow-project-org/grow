import { QueryCache, QueryClient, MutationCache } from '@tanstack/react-query';
import { ApiError } from '../api/client';
import { clearNotices, notifyServerError } from '../state/notifications';

const describe = (error: unknown): string =>
  error instanceof ApiError ? error.userMessage : 'Nie udało się połączyć z serwerem.';

const isExpected = (error: unknown): boolean =>
  error instanceof ApiError && error.status === 401;

export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (!isExpected(error)) notifyServerError(describe(error));
    },
    onSuccess: () => clearNotices(),
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      if (!isExpected(error)) notifyServerError(describe(error));
    },
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
      retry: 0,
      networkMode: 'always',
    },
  },
});
