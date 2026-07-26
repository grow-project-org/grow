import type { ButtonHTMLAttributes, ReactNode } from 'react';
import styles from './Button.module.css';

type Variant = 'primary' | 'neutral';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  variant?: Variant;
  block?: boolean;
}

/** Chunky sticker button. `primary` is the green call-to-action. */
export const Button = ({
  children,
  variant = 'primary',
  block = false,
  type = 'button',
  className,
  ...rest
}: ButtonProps) => (
  <button
    type={type}
    className={[styles.btn, styles[variant], block ? styles.block : '', className]
      .filter(Boolean)
      .join(' ')}
    {...rest}
  >
    {children}
  </button>
);
