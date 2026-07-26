import type { MouseEvent, ReactNode } from 'react';
import { createPortal } from 'react-dom';
import styles from './BottomSheet.module.css';

interface BottomSheetProps {
  open: boolean;
  onClose: () => void;
  children: ReactNode;
}

/** A modal sheet anchored to the bottom of the device, rendered through a
 *  portal into `#sheet-root` so it overlays the whole screen (including the
 *  bottom nav) regardless of which page opened it. */
export const BottomSheet = ({ open, onClose, children }: BottomSheetProps) => {
  if (!open) return null;

  const target = document.getElementById('sheet-root') ?? document.body;
  const stop = (e: MouseEvent) => e.stopPropagation();

  return createPortal(
    <div className={styles.backdrop} onClick={onClose}>
      <div className={styles.sheet} onClick={stop} role="dialog" aria-modal="true">
        <div className={styles.grabber} />
        {children}
      </div>
    </div>,
    target,
  );
};
