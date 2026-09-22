import type { CSSProperties } from 'react';
import styles from './Avatar.module.css';

interface AvatarProps {
  label: string;
  bg?: string;
  size?: number;
  radius?: number;
  fontSize?: number;
  thinBorder?: boolean;
}

export const Avatar = ({
  label,
  bg = 'var(--color-green-tint)',
  size = 44,
  radius = 13,
  fontSize,
  thinBorder = false,
}: AvatarProps) => {
  const style: CSSProperties = {
    width: size,
    height: size,
    borderRadius: radius,
    background: bg,
    fontSize: fontSize ?? Math.round(size * 0.5),
  };
  return (
    <span className={thinBorder ? styles.avatarThin : styles.avatar} style={style}>
      {label}
    </span>
  );
};
