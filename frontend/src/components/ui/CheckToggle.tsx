import type { CSSProperties } from 'react';
import { CheckIcon } from './icons';
import styles from './CheckToggle.module.css';

interface CheckToggleProps {
  checked: boolean;
  onClick: () => void;
  disabled?: boolean;
  size?: number;
  radius?: number;
  label?: string;
}

export const CheckToggle = ({
  checked,
  onClick,
  disabled = false,
  size = 38,
  radius = 12,
  label = 'Odhacz',
}: CheckToggleProps) => {
  const style: CSSProperties = { width: size, height: size, borderRadius: radius };
  return (
    <button
      type="button"
      className={styles.toggle}
      style={style}
      onClick={onClick}
      disabled={disabled}
      aria-pressed={checked}
      aria-label={label}
    >
      {checked && (
        <span className={styles.fill} style={{ borderRadius: radius - 3 }}>
          <CheckIcon size={Math.round(size * 0.52)} />
        </span>
      )}
    </button>
  );
};
