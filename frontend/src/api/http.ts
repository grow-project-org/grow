import { API_BASE_URL, API_TIMEOUT_MS } from './config';

/** Normalised transport/HTTP error carrying a user-facing message. */
export class ApiError extends Error {
  readonly status: number;
  readonly userMessage: string;

  constructor(status: number, message: string, userMessage: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.userMessage = userMessage;
  }
}

interface RequestInitJson extends Omit<RequestInit, 'body'> {
  body?: unknown;
}

/**
 * Thin typed fetch wrapper: JSON in/out, timeout via AbortController and a
 * single error shape ({@link ApiError}) for the query client to surface.
 */
export const request = async <T>(path: string, init: RequestInitJson = {}): Promise<T> => {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), API_TIMEOUT_MS);

  try {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      signal: controller.signal,
      headers: { 'Content-Type': 'application/json', ...init.headers },
      body: init.body === undefined ? undefined : JSON.stringify(init.body),
    });

    if (!response.ok) {
      throw new ApiError(response.status, `HTTP ${response.status}`, 'Serwer odpowiedział błędem.');
    }
    if (response.status === 204) return undefined as T;
    return (await response.json()) as T;
  } catch (error) {
    if (error instanceof ApiError) throw error;
    const aborted = error instanceof DOMException && error.name === 'AbortError';
    throw new ApiError(
      0,
      aborted ? 'timeout' : String(error),
      aborted
        ? 'Przekroczono czas oczekiwania na serwer.'
        : 'Nie można połączyć się z serwerem.',
    );
  } finally {
    clearTimeout(timer);
  }
};
