import type { ButtonHTMLAttributes, CSSProperties, ReactNode } from 'react';
import styles from './IconButton.module.css';

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  size?: number;
  /** Use the soft (taupe) shadow instead of the hard ink shadow. */
  soft?: boolean;
}

/** Square, ink-outlined button used for back / menu / stepper controls. */
export const IconButton = ({
  children,
  size = 40,
  soft = false,
  style,
  className,
  ...rest
}: IconButtonProps) => {
  const merged: CSSProperties = { width: size, height: size, ...style };
  return (
    <button
      type="button"
      className={[soft ? styles.soft : styles.hard, className].filter(Boolean).join(' ')}
      style={merged}
      {...rest}
    >
      {children}
    </button>
  );
};
