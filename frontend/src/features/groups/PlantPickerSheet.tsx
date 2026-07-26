import type { Plant } from '../../types';
import { BottomSheet } from '../../components/sheet/BottomSheet';
import { CheckIcon } from '../../components/ui/icons';
import { Button } from '../../components/ui/Button';
import styles from './PlantPickerSheet.module.css';

interface PlantPickerSheetProps {
  open: boolean;
  onClose: () => void;
  groupName: string;
  garden: Plant[];
  onToggle: (id: number) => void;
}

const memberSub = (p: Plant): string => {
  const species = p.species ?? 'gatunek?';
  const groups = p.groups.length ? p.groups.join(', ') : 'bez grupy';
  return `${species} · ${groups}`;
};

/** Add / remove any plant from a group, regardless of its current groups. */
export const PlantPickerSheet = ({
  open,
  onClose,
  groupName,
  garden,
  onToggle,
}: PlantPickerSheetProps) => {
  const inGroupCount = garden.filter((p) => p.groups.includes(groupName)).length;

  return (
    <BottomSheet open={open} onClose={onClose}>
      <div className={styles.head}>
        <h2 className={styles.title}>Rośliny w grupie</h2>
        <span className={styles.count}>{inGroupCount} w grupie</span>
      </div>
      <p className={styles.desc}>Grupa „{groupName}”. Zielone są w grupie.</p>

      <div className={styles.rows}>
        {garden.map((p) => {
          const inGroup = p.groups.includes(groupName);
          return (
            <button
              key={p.id}
              type="button"
              className={`${styles.row} ${inGroup ? styles.rowActive : ''}`}
              onClick={() => onToggle(p.id)}
            >
              <span className={styles.box}>
                {inGroup && (
                  <span className={styles.boxFill}>
                    <CheckIcon size={15} />
                  </span>
                )}
              </span>
              <span className={styles.text}>
                <span className={styles.name}>{p.code}</span>
                <span className={styles.sub}>{memberSub(p)}</span>
              </span>
            </button>
          );
        })}
      </div>

      <Button block onClick={onClose} className={styles.done}>
        Gotowe
      </Button>
    </BottomSheet>
  );
};
