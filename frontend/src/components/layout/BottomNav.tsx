import { NavLink } from 'react-router-dom';
import styles from './BottomNav.module.css';
import { ROUTES } from '../../routes/paths';

interface NavItem {
  to: string;
  emoji: string;
  label: string;
  /** Also mark active for sub-routes under this prefix (e.g. plant profile). */
  matchPrefix?: string;
}

const ITEMS: readonly NavItem[] = [
  { to: ROUTES.today, emoji: '🏠', label: 'Dziś' },
  { to: ROUTES.plants, emoji: '🌿', label: 'Rośliny', matchPrefix: '/plants' },
  { to: ROUTES.add, emoji: '➕', label: 'Dodaj' },
  { to: ROUTES.calendar, emoji: '📅', label: 'Kalendarz' },
  { to: ROUTES.groups, emoji: '🗂️', label: 'Grupy' },
];

export const BottomNav = () => (
  <nav className={styles.nav}>
    {ITEMS.map((item) => (
      <NavLink
        key={item.to}
        to={item.to}
        end={item.to === ROUTES.today}
        className={({ isActive }) =>
          isActive ? `${styles.item} ${styles.active}` : styles.item
        }
      >
        <span className={styles.emoji}>{item.emoji}</span>
        <span className={styles.label}>{item.label}</span>
      </NavLink>
    ))}
  </nav>
);
