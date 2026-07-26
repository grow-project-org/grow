import type { CSSProperties, ReactNode } from 'react';
import styles from './Pill.module.css';

interface PillProps {
  children: ReactNode;
  bg?: string;
  ink?: string;
  bordered?: boolean;
}

/** Small rounded status label. Colours are data-driven, so applied inline. */
export const Pill = ({ children, bg, ink, bordered = true }: PillProps) => {
  const style: CSSProperties = {
    background: bg,
    color: ink,
    border: bordered ? '1.5px solid var(--color-ink)' : 'none',
  };
  return (
    <span className={styles.pill} style={style}>
      {children}
    </span>
  );
};
