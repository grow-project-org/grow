import type { Group, Plant, Species } from '../../types';
import { BottomSheet } from '../../components/sheet/BottomSheet';
import { CheckIcon } from '../../components/ui/icons';
import { Button } from '../../components/ui/Button';
import { speciesName } from '../../domain/species';
import { groupsOf } from '../../domain/regions';
import styles from './PlantPickerSheet.module.css';

interface PlantPickerSheetProps {
  open: boolean;
  onClose: () => void;
  group: Group | undefined;
  plants: readonly Plant[];
  species: readonly Species[];
  groups: readonly Group[];
  onToggle: (plantId: string, member: boolean) => void;
}

export const PlantPickerSheet = ({
  open,
  onClose,
  group,
  plants,
  species,
  groups,
  onToggle,
}: PlantPickerSheetProps) => {
  if (!group) return null;

  const memberSub = (plant: Plant): string => {
    const names = groupsOf(plant, groups).map((g) => g.name);
    return `${speciesName(species, plant.specieId)} · ${names.length ? names.join(', ') : 'bez grupy'}`;
  };

  return (
    <BottomSheet open={open} onClose={onClose}>
      <div className={styles.head}>
        <h2 className={styles.title}>Rośliny w grupie</h2>
        <span className={styles.count}>{group.plantIds.length} w grupie</span>
      </div>
      <p className={styles.desc}>Grupa „{group.name}”. Zielone są w grupie.</p>

      <div className={styles.rows}>
        {plants.map((plant) => {
          const inGroup = group.plantIds.includes(plant.id);

          return (
            <button
              key={plant.id}
              type="button"
              className={`${styles.row} ${inGroup ? styles.rowActive : ''}`}
              onClick={() => onToggle(plant.id, !inGroup)}
            >
              <span className={styles.box}>
                {inGroup && (
                  <span className={styles.boxFill}>
                    <CheckIcon size={15} />
                  </span>
                )}
              </span>
              <span className={styles.text}>
                <span className={styles.name}>{plant.code}</span>
                <span className={styles.sub}>{memberSub(plant)}</span>
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
