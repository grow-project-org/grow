import { BottomSheet } from './BottomSheet';
import { Button } from '../ui/Button';
import { daysAdd, fmtLong, weekdayLong } from '../../utils/date';
import sheet from './sheetForm.module.css';
import styles from './CareDateSheet.module.css';

interface CareDateSheetProps {
  open: boolean;
  onClose: () => void;
  value: string;
  today: string;
  onChange: (date: string) => void;
  title?: string;
  desc?: string;
  note?: string;
}

const QUICK_OFFSETS = [0, -1, -2, -3] as const;

const quickLabel = (offset: number): string => {
  if (offset === 0) return 'Dziś';
  if (offset === -1) return 'Wczoraj';
  return `${Math.abs(offset)} dni temu`;
};

export const CareDateSheet = ({
  open,
  onClose,
  value,
  today,
  onChange,
  title = 'Data zabiegu',
  desc = 'Domyślnie dzisiaj. Wybierz wcześniejszy dzień, jeśli uzupełniasz zaległe wpisy.',
  note,
}: CareDateSheetProps) => (
  <BottomSheet open={open} onClose={onClose}>
    <h2 className={sheet.title}>{title}</h2>
    <p className={sheet.desc}>{desc}</p>

    <div className={styles.quick}>
      {QUICK_OFFSETS.map((offset) => {
        const date = daysAdd(today, offset);
        return (
          <button
            key={offset}
            type="button"
            className={`${styles.option} ${value === date ? styles.optionActive : ''}`}
            onClick={() => onChange(date)}
          >
            <span className={styles.optionLabel}>{quickLabel(offset)}</span>
            <span className={styles.optionDate}>
              {weekdayLong(date)}, {fmtLong(date)}
            </span>
          </button>
        );
      })}
    </div>

    <label className={styles.customLabel} htmlFor="care-date">
      Inna data
    </label>
    <input
      id="care-date"
      className={styles.input}
      type="date"
      value={value}
      max={today}
      onChange={(e) => e.target.value && onChange(e.target.value)}
    />

    {note && <p className={styles.note}>{note}</p>}

    <div className={sheet.actions}>
      <Button block onClick={onClose}>
        Gotowe
      </Button>
    </div>
  </BottomSheet>
);
