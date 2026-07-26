import type { ReactNode } from 'react';
import styles from './PhoneFrame.module.css';

interface PhoneFrameProps {
  children: ReactNode;
  footer?: ReactNode;
  /** Rendered above the frame, inside the device (toasts, etc). */
  overlay?: ReactNode;
}

/** The centered device mock: status bar, scrollable content and a fixed footer.
 *  `#sheet-root` is the portal target used by {@link BottomSheet}. */
export const PhoneFrame = ({ children, footer, overlay }: PhoneFrameProps) => (
  <div className={styles.page}>
    <div className={styles.device}>
      <div className={styles.statusbar}>
        <span>9:41</span>
        <span className={styles.signal}>●●● ▮</span>
      </div>

      <main className={`${styles.scroll} hide-scroll`}>{children}</main>

      {footer}

      {overlay}
      <div id="sheet-root" className={styles.sheetRoot} />
    </div>
  </div>
);
