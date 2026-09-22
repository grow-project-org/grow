import createClient from 'openapi-fetch';
import type { paths } from './schema';

const env = import.meta.env as { VITE_API_URL?: string };

export const API_BASE_URL = env.VITE_API_URL ?? 'https://localhost:7122';

const TIMEOUT_MS = 8000;
const CSRF_HEADER = 'X-CSRF-TOKEN';
const SAFE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS', 'TRACE']);

export class ApiError extends Error {
  readonly status: number;
  readonly userMessage: string;

  constructor(status: number, userMessage: string) {
    super(`HTTP ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.userMessage = userMessage;
  }
}

const MESSAGES: Record<number, string> = {
  0: 'Nie można połączyć się z serwerem.',
  400: 'Serwer odrzucił żądanie jako nieprawidłowe.',
  401: 'Sesja wygasła. Zaloguj się ponownie.',
  403: 'Brak uprawnień do tej operacji.',
  404: 'Nie znaleziono zasobu.',
  429: 'Zbyt wiele żądań. Spróbuj ponownie za chwilę.',
};

const TIMEOUT_MESSAGE = 'Przekroczono czas oczekiwania na serwer.';
const FALLBACK_MESSAGE = 'Serwer odpowiedział błędem.';

type UnauthorizedListener = () => void;

const unauthorizedListeners = new Set<UnauthorizedListener>();

export const subscribeUnauthorized = (listener: UnauthorizedListener): (() => void) => {
  unauthorizedListeners.add(listener);
  return () => {
    unauthorizedListeners.delete(listener);
  };
};

let csrfToken: string | null = null;
let csrfRequest: Promise<string | null> | null = null;

const readCsrfToken = async (): Promise<string | null> => {
  if (csrfToken) return csrfToken;

  csrfRequest ??= fetch(`${API_BASE_URL}/api/users/csrf`, { credentials: 'include' })
    .then((response) => (response.ok ? (response.json() as Promise<{ token?: string }>) : null))
    .then((body) => {
      csrfToken = body?.token ?? null;
      return csrfToken;
    })
    .catch(() => null);

  const token = await csrfRequest;
  csrfRequest = null;
  return token;
};

export const resetCsrfToken = (): void => {
  csrfToken = null;
};

const fetchWithTimeout: typeof fetch = async (input, init) => {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);
  try {
    return await fetch(input, { ...init, signal: controller.signal });
  } finally {
    clearTimeout(timer);
  }
};

export const api = createClient<paths>({
  baseUrl: API_BASE_URL,
  credentials: 'include',
  fetch: fetchWithTimeout,
});

api.use({
  async onRequest({ request }) {
    if (SAFE_METHODS.has(request.method)) return request;

    const token = await readCsrfToken();
    if (token) request.headers.set(CSRF_HEADER, token);
    return request;
  },
  async onResponse({ response }) {
    if (response.status === 401) {
      resetCsrfToken();
      for (const listener of unauthorizedListeners) listener();
    }
    return response;
  },
});

interface ClientResult<T> {
  data?: T;
  error?: unknown;
  response: Response;
}

const isAbort = (error: unknown): boolean =>
  error instanceof DOMException && error.name === 'AbortError';

export const unwrap = async <T>(call: Promise<ClientResult<T>>): Promise<T> => {
  let result: ClientResult<T>;

  try {
    result = await call;
  } catch (error) {
    throw new ApiError(0, isAbort(error) ? TIMEOUT_MESSAGE : MESSAGES[0]);
  }

  if (!result.response.ok) {
    throw new ApiError(result.response.status, MESSAGES[result.response.status] ?? FALLBACK_MESSAGE);
  }

  return result.data as T;
};
