import { useState } from 'react';
import { useCareDate } from '../../state/CareDateContext';
import { CareDateSheet } from '../sheet/CareDateSheet';
import { relLabel } from '../../domain/schedule';
import { fmtLong } from '../../utils/date';
import styles from './CareDateBar.module.css';

const BACKDATE_NOTE =
  'Wpis starszy niż ostatni zapisany zabieg trafi do historii, ale nie przesunie terminu — ' +
  'termin liczy się od najnowszego zabiegu.';

export const CareDateChip = () => {
  const { careDate, today, isBackdated, setCareDate } = useCareDate();
  const [open, setOpen] = useState(false);

  return (
    <>
      <button
        type="button"
        className={`${styles.chip} ${isBackdated ? styles.chipBackdated : ''}`}
        onClick={() => setOpen(true)}
        aria-label="Zmień datę zapisywanych zabiegów"
      >
        📅 {isBackdated ? relLabel(careDate, today).text : 'dziś'}
      </button>

      <CareDateSheet
        open={open}
        onClose={() => setOpen(false)}
        value={careDate}
        today={today}
        onChange={setCareDate}
        note={isBackdated ? BACKDATE_NOTE : undefined}
      />
    </>
  );
};

export const CareDateBanner = () => {
  const { careDate, isBackdated, resetToToday } = useCareDate();

  if (!isBackdated) return null;

  return (
    <div className={styles.banner}>
      <span className={styles.bannerIcon}>🕓</span>
      <span className={styles.bannerText}>
        <span className={styles.bannerTitle}>Uzupełniasz {fmtLong(careDate)}</span>
        Odhaczenia zapiszą się z tą datą, nie z dzisiejszą.
      </span>
      <button type="button" className={styles.bannerReset} onClick={resetToToday}>
        Wróć do dziś
      </button>
    </div>
  );
};
