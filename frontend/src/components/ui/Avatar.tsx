import type { CSSProperties } from 'react';
import styles from './Avatar.module.css';

interface AvatarProps {
  emoji: string;
  /** Data-driven background colour (per-plant), applied inline. */
  bg?: string;
  size?: number;
  radius?: number;
  /** Emoji font-size; defaults to roughly half the box. */
  fontSize?: number;
  thinBorder?: boolean;
}

export const Avatar = ({
  emoji,
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
      {emoji}
    </span>
  );
};
