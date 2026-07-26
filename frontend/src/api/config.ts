/**
 * API configuration. Points at the local Grow.WebApi backend by default;
 * override with `VITE_API_URL` for other environments.
 */
const env = import.meta.env as { VITE_API_URL?: string };

export const API_BASE_URL = env.VITE_API_URL ?? 'https://localhost:7122';

/** Abort a request after this many milliseconds. */
export const API_TIMEOUT_MS = 8000;
