import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { usersApi } from '../api/resources';
import { ApiError, resetCsrfToken, subscribeUnauthorized } from '../api/client';

export type AuthStatus = 'checking' | 'authenticated' | 'anonymous';

export interface AuthApi {
  status: AuthStatus;
  username: string | null;
  signIn: (email: string, username: string) => Promise<void>;
}

const AuthContext = createContext<AuthApi | null>(null);

const UNUSED_PASSWORD = 'not-checked-by-backend-yet';

const isServerResponse = (error: unknown): boolean =>
  error instanceof ApiError && error.status !== 0;

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [status, setStatus] = useState<AuthStatus>('checking');
  const [username, setUsername] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const probe = useCallback(async () => {
    try {
      const me = await usersApi.me();
      setUsername(me.username);
      setStatus('authenticated');
    } catch {
      setUsername(null);
      setStatus('anonymous');
    }
  }, []);

  useEffect(() => {
    void probe();
  }, [probe]);

  useEffect(
    () =>
      subscribeUnauthorized(() => {
        setUsername(null);
        setStatus('anonymous');
        queryClient.clear();
      }),
    [queryClient],
  );

  const signIn = useCallback(
    async (email: string, name: string) => {
      resetCsrfToken();

      try {
        await usersApi.login(email, UNUSED_PASSWORD);
      } catch (error) {
        if (!isServerResponse(error)) throw error;
        await usersApi.register(email, name);
        await usersApi.login(email, UNUSED_PASSWORD);
      }

      await probe();
      await queryClient.invalidateQueries();
    },
    [probe, queryClient],
  );

  const value = useMemo<AuthApi>(() => ({ status, username, signIn }), [status, username, signIn]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthApi => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
};
