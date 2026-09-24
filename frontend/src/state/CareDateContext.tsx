import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { diffDays, today as todayIso } from '../utils/date';

export interface CareDateApi {
  careDate: string;
  today: string;
  isBackdated: boolean;
  setCareDate: (date: string) => void;
  resetToToday: () => void;
}

const CareDateContext = createContext<CareDateApi | null>(null);

const clampToPast = (date: string, today: string): string =>
  diffDays(date, today) > 0 ? today : date;

export const CareDateProvider = ({ children }: { children: ReactNode }) => {
  const today = useMemo(() => todayIso(), []);
  const [careDate, setCareDateRaw] = useState(today);

  const setCareDate = useCallback(
    (date: string) => setCareDateRaw(clampToPast(date, today)),
    [today],
  );

  const resetToToday = useCallback(() => setCareDateRaw(today), [today]);

  const value = useMemo<CareDateApi>(
    () => ({
      careDate,
      today,
      isBackdated: careDate !== today,
      setCareDate,
      resetToToday,
    }),
    [careDate, today, setCareDate, resetToToday],
  );

  return <CareDateContext.Provider value={value}>{children}</CareDateContext.Provider>;
};

export const useCareDate = (): CareDateApi => {
  const ctx = useContext(CareDateContext);
  if (!ctx) throw new Error('useCareDate must be used within a CareDateProvider');
  return ctx;
};
