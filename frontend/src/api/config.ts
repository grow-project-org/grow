/**
 * API configuration. The base URL intentionally points at a host that does not
 * exist yet — every request fails, which exercises the offline handling
 * (localStorage fallback + reconnect sync + error popups). Point
 * `VITE_API_URL` at a real backend once one is available.
 */
const env = import.meta.env as { VITE_API_URL?: string };

export const API_BASE_URL = env.VITE_API_URL ?? 'https://api.hodowla-roslin.invalid';

/** Abort a request after this many milliseconds. */
export const API_TIMEOUT_MS = 8000;
