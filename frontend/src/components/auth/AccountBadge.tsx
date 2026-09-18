import { useState } from 'react';
import { useAuth } from '../../state/AuthContext';
import { AccountSheet } from './AccountSheet';
import styles from './AccountBadge.module.css';

const ICON: Record<'checking' | 'authenticated' | 'anonymous', string> = {
  checking: '⏳',
  authenticated: '🟢',
  anonymous: '👤',
};

/** Always-visible account indicator/trigger, floating over every page. */
export const AccountBadge = () => {
  const { status } = useAuth();
  const [open, setOpen] = useState(false);

  return (
    <>
      <button
        type="button"
        className={styles.badge}
        aria-label="Konto"
        onClick={() => setOpen(true)}
      >
        {ICON[status]}
      </button>
      <AccountSheet open={open} onClose={() => setOpen(false)} />
    </>
  );
};
