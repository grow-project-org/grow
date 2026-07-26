import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { Avatar } from '../../components/ui/Avatar';
import { ChevronRightIcon } from '../../components/ui/icons';
import { plantPath } from '../../routes/paths';
import {
  selectPlants,
  type PlantsFilter,
  type PlantVariety,
} from './plants.selectors';
import styles from './PlantsPage.module.css';

interface StatDef {
  key: PlantsFilter;
  emoji: string;
  label: string;
  value: number;
  bg: string;
}

export const PlantsPage = () => {
  const { garden, done } = useGarden();
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [filter, setFilter] = useState<PlantsFilter>('all');
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});

  const view = selectPlants(garden, done, query, filter);

  const stats: StatDef[] = [
    { key: 'all', emoji: '🌿', label: 'wszystkich', value: view.total, bg: 'var(--color-card)' },
    { key: 'water', emoji: '💧', label: 'do podlania', value: view.dueWater, bg: 'var(--color-water-tint)' },
    { key: 'fert', emoji: '🌱', label: 'do nawożenia', value: view.dueFert, bg: 'var(--color-fert-tint)' },
  ];

  const toggleFilter = (key: PlantsFilter) =>
    setFilter((cur) => (cur === key ? 'all' : key));

  const toggleExpand = (name: string) =>
    setExpanded((cur) => ({ ...cur, [name]: !cur[name] }));

  return (
    <div className={styles.page}>
      <h1 className={styles.heading}>Rośliny</h1>

      <div className={styles.stats}>
        {stats.map((stat) => (
          <button
            key={stat.key}
            type="button"
            className={`${styles.stat} ${filter === stat.key ? styles.statActive : ''}`}
            style={{ background: stat.bg }}
            onClick={() => toggleFilter(stat.key)}
          >
            <span className={styles.statValue}>{stat.value}</span>
            <span className={styles.statLabel}>
              {stat.emoji} {stat.label}
            </span>
          </button>
        ))}
      </div>

      <div className={styles.search}>
        <span className={styles.searchIcon}>🔍</span>
        <input
          className={styles.searchInput}
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Szukaj odmiany lub ID (np. PAP-05)…"
        />
      </div>

      <p className={styles.filterLabel}>{view.filterLabel}</p>

      <div className={styles.list}>
        {view.varieties.map((variety) => (
          <VarietyCard
            key={variety.name}
            variety={variety}
            expanded={!!expanded[variety.name]}
            onToggle={() => toggleExpand(variety.name)}
            onOpen={(id) => navigate(plantPath(id))}
          />
        ))}

        {view.empty && (
          <div className={styles.emptyState}>
            <div className={styles.emptyEmoji}>🔍</div>
            <p className={styles.emptyText}>Brak wyników</p>
          </div>
        )}
      </div>
    </div>
  );
};

interface VarietyCardProps {
  variety: PlantVariety;
  expanded: boolean;
  onToggle: () => void;
  onOpen: (id: number) => void;
}

const VarietyCard = ({ variety, expanded, onToggle, onOpen }: VarietyCardProps) => (
  <div className={styles.card}>
    <button type="button" className={styles.cardHead} onClick={onToggle}>
      <Avatar emoji={variety.emoji} bg="#eaf5e4" size={50} radius={15} fontSize={25} />
      <span className={styles.cardText}>
        <span className={styles.cardName}>{variety.name}</span>
        <span className={styles.cardSub}>{variety.sub}</span>
      </span>
      <span className={styles.badges}>
        {variety.dueW > 0 && (
          <span className={styles.badgeWater}>💧 {variety.dueW}</span>
        )}
        {variety.dueF > 0 && (
          <span className={styles.badgeFert}>🌱 {variety.dueF}</span>
        )}
      </span>
    </button>

    {expanded && (
      <ul className={styles.instances}>
        {variety.instances.map((inst) => (
          <li key={inst.id}>
            <button type="button" className={styles.instance} onClick={() => onOpen(inst.id)}>
              <Avatar emoji={inst.emoji} bg={inst.avatarBg} size={38} radius={11} fontSize={18} thinBorder />
              <span className={styles.instanceText}>
                <span className={styles.instanceCode}>{inst.code}</span>
                <span className={styles.instanceLoc}>{inst.loc}</span>
              </span>
              <span
                className={styles.instanceNext}
                style={{ background: inst.next.bg, color: inst.next.ink }}
              >
                {inst.next.label}
              </span>
              <ChevronRightIcon size={16} className={styles.chevron} />
            </button>
          </li>
        ))}
      </ul>
    )}
  </div>
);
