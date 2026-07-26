import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconButton } from '../ui/IconButton';
import { ChevronLeftIcon } from '../ui/icons';
import styles from './PageHeader.module.css';

interface PageHeaderProps {
  title: string;
  /** Where the back button goes; defaults to browser back. */
  onBack?: () => void;
  right?: ReactNode;
}

/** Back button + screen title, used by the stacked (detail) screens. */
export const PageHeader = ({ title, onBack, right }: PageHeaderProps) => {
  const navigate = useNavigate();
  const back = onBack ?? (() => navigate(-1));
  return (
    <header className={styles.header}>
      <IconButton soft aria-label="Wróć" onClick={back}>
        <ChevronLeftIcon />
      </IconButton>
      <h1 className={styles.title}>{title}</h1>
      {right}
    </header>
  );
};
