import type { CSSProperties, ReactNode } from 'react';
import styles from './Card.module.css';

interface CardProps {
  children: ReactNode;
  /** Larger offset shadow for prominent cards. */
  raised?: boolean;
  className?: string;
  style?: CSSProperties;
}

/** White, ink-outlined surface with a hard offset shadow. */
export const Card = ({ children, raised = false, className, style }: CardProps) => (
  <div
    className={[styles.card, raised ? styles.raised : '', className]
      .filter(Boolean)
      .join(' ')}
    style={style}
  >
    {children}
  </div>
);
