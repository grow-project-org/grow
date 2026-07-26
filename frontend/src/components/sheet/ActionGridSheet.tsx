import { BottomSheet } from './BottomSheet';
import { PlusIcon } from '../ui/icons';
import styles from './ActionGridSheet.module.css';

export interface SheetAction {
  emoji: string;
  label: string;
  onClick: () => void;
}

interface ActionGridSheetProps {
  open: boolean;
  onClose: () => void;
  kicker: string;
  title: string;
  actions: SheetAction[];
  /** Optional "add / remove plants" entry point (group sheets). */
  onAddPlants?: () => void;
}

/** Bottom sheet showing a titled grid of one-tap actions. */
export const ActionGridSheet = ({
  open,
  onClose,
  kicker,
  title,
  actions,
  onAddPlants,
}: ActionGridSheetProps) => (
  <BottomSheet open={open} onClose={onClose}>
    <p className={styles.kicker}>{kicker}</p>
    <h2 className={styles.title}>{title}</h2>

    {onAddPlants && (
      <button type="button" className={styles.addPlants} onClick={onAddPlants}>
        <PlusIcon size={18} />
        Dodaj / usuń rośliny
      </button>
    )}

    <div className={styles.grid}>
      {actions.map((action) => (
        <button
          key={action.label}
          type="button"
          className={styles.action}
          onClick={action.onClick}
        >
          <span className={styles.actionEmoji}>{action.emoji}</span>
          {action.label}
        </button>
      ))}
    </div>
  </BottomSheet>
);
