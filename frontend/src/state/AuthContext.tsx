import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { usersApi } from '../api/endpoints';
import type { LoginRequest } from '../api/dto';

export type AuthStatus = 'checking' | 'authenticated' | 'anonymous';

export interface AuthApi {
  status: AuthStatus;
  username: string | null;
  /**
   * Signs in an existing account, or creates one on the fly if the email
   * isn't registered yet. The backend has no "does this email exist" check
   * (see `GetUserByEmailQueryHandler`), so login-first-then-register is the
   * only reliable order — TODO(backend): a real "check email" endpoint
   * would let this be a straightforward login-or-register choice instead.
   */
  signIn: (email: string, username: string) => Promise<void>;
}

const AuthContext = createContext<AuthApi | null>(null);

/**
 * `Microsoft.AspNetCore.Identity.Data.LoginRequest.Password` is required by
 * the DTO shape but the backend doesn't verify it yet (`SessionStorage`
 * only checks `IsUserVerified`) — TODO(backend): add real password auth.
 * Until then this placeholder just satisfies the required field.
 */
const UNUSED_PASSWORD = 'not-checked-by-backend-yet';

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [status, setStatus] = useState<AuthStatus>('checking');
  const [username, setUsername] = useState<string | null>(null);

  // Deliberately not a React Query `useQuery` — "not signed in" is an
  // expected state here, not a connection failure, so it must not trip the
  // global error popup wired up in `app/queryClient.ts`.
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

  const signIn = useCallback(
    async (email: string, name: string) => {
      const login: LoginRequest = { email, password: UNUSED_PASSWORD };
      try {
        await usersApi.login(login);
      } catch {
        await usersApi.register({ email, username: name });
        await usersApi.login(login);
      }
      await probe();
    },
    [probe],
  );

  const value = useMemo<AuthApi>(() => ({ status, username, signIn }), [status, username, signIn]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthApi => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
};
