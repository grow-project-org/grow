/**
 * A tiny external store for server-connection notices, consumed by
 * {@link ServerStatusPopup} through `useSyncExternalStore`. Kept outside React
 * so the QueryClient's global error handlers can push into it directly.
 */
export interface ServerNotice {
  id: number;
  title: string;
  message: string;
}

type Listener = () => void;

let notices: ServerNotice[] = [];
const listeners = new Set<Listener>();
let seq = 1;

const emit = (): void => {
  for (const listener of listeners) listener();
};

/** Push a server-error notice (de-duplicated by message). */
export const notifyServerError = (
  message: string,
  title = 'Brak połączenia z serwerem',
): void => {
  notices = [...notices.filter((n) => n.message !== message), { id: seq++, title, message }];
  emit();
};

export const dismissNotice = (id: number): void => {
  notices = notices.filter((n) => n.id !== id);
  emit();
};

/** Clear everything — called on a successful request (connection restored). */
export const clearNotices = (): void => {
  if (!notices.length) return;
  notices = [];
  emit();
};

export const subscribeNotices = (listener: Listener): (() => void) => {
  listeners.add(listener);
  return () => listeners.delete(listener);
};

export const getNotices = (): ServerNotice[] => notices;
