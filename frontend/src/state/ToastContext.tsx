import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';

interface ToastContextValue {
  message: string;
  /** Show a transient toast; auto-clears after a short delay. */
  flash: (message: string) => void;
  clear: () => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const TOAST_MS = 2100;

export const ToastProvider = ({ children }: { children: ReactNode }) => {
  const [message, setMessage] = useState('');
  const timer = useRef<ReturnType<typeof setTimeout>>();

  const clear = useCallback(() => {
    if (timer.current) clearTimeout(timer.current);
    setMessage('');
  }, []);

  const flash = useCallback((next: string) => {
    if (timer.current) clearTimeout(timer.current);
    setMessage(next);
    timer.current = setTimeout(() => setMessage(''), TOAST_MS);
  }, []);

  const value = useMemo<ToastContextValue>(
    () => ({ message, flash, clear }),
    [message, flash, clear],
  );

  return <ToastContext.Provider value={value}>{children}</ToastContext.Provider>;
};

export const useToast = (): ToastContextValue => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within a ToastProvider');
  return ctx;
};
