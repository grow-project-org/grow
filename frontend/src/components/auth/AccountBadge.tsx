import { useState } from 'react';
import { useGarden } from '../../state/GardenContext';
import { AccountSheet } from './AccountSheet';
import styles from './AccountBadge.module.css';

export const AccountBadge = () => {
  const { isSyncing } = useGarden();
  const [open, setOpen] = useState(false);

  return (
    <>
      <button
        type="button"
        className={styles.badge}
        aria-label="Konto"
        onClick={() => setOpen(true)}
      >
        {isSyncing ? '⏳' : '🟢'}
      </button>
      <AccountSheet open={open} onClose={() => setOpen(false)} />
    </>
  );
};
