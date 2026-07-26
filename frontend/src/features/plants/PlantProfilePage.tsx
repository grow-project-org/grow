import { useState } from 'react';
import { Navigate, useNavigate, useParams } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { useToast } from '../../state/ToastContext';
import { interval } from '../../domain/species';
import { EXTRA_ACTIONS } from '../../domain/extraActions';
import { ROUTES, repotPath } from '../../routes/paths';
import { IconButton } from '../../components/ui/IconButton';
import { ChevronLeftIcon, MoreVerticalIcon } from '../../components/ui/icons';
import { ActionGridSheet } from '../../components/sheet/ActionGridSheet';
import { RenameSheet } from '../../components/sheet/RenameSheet';
import { selectProfile } from './profile.selectors';
import styles from './PlantProfilePage.module.css';

type OpenSheet = 'actions' | 'rename' | null;

export const PlantProfilePage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { plantById, groups, log, commitAction, logExtra, rename } = useGarden();
  const { flash } = useToast();
  const [sheet, setSheet] = useState<OpenSheet>(null);

  const plant = plantById(Number(id));
  if (!plant) return <Navigate to={ROUTES.plants} replace />;

  const view = selectProfile(plant, log, groups);

  const onFertilise = () => {
    if (interval(plant.species, 'fert') == null) {
      flash('Ten gatunek nie ma ustawionego nawożenia');
      return;
    }
    commitAction([plant.id], 'fert', `🌱 Nawożono ${plant.name}`);
  };

  return (
    <div>
      <div className={styles.hero} style={{ background: view.avatarBg }}>
        <IconButton soft aria-label="Wróć" className={styles.heroBtnLeft} onClick={() => navigate(-1)}>
          <ChevronLeftIcon />
        </IconButton>
        <IconButton soft aria-label="Więcej" className={styles.heroBtnRight} onClick={() => setSheet('actions')}>
          <MoreVerticalIcon />
        </IconButton>
        <span className={styles.heroEmoji}>{plant.emoji}</span>
        <span className={styles.photoTag}>📷 dodaj zdjęcie</span>
      </div>

      <div className={styles.body}>
        <h1 className={styles.name}>{plant.name}</h1>
        <p className={styles.meta}>
          {(plant.species ?? 'gatunek nieznany')} · {(plant.loc ?? 'brak lokalizacji')}
        </p>

        <button type="button" className={styles.codeBtn} onClick={() => setSheet('rename')}>
          {plant.code} <span className={styles.codeEdit}>✏️</span>
        </button>

        <div className={styles.tags}>
          {view.groups.map((g) => (
            <span key={g.name} className={styles.tag}>
              {g.emoji} {g.name}
            </span>
          ))}
        </div>

        <div className={styles.quick}>
          <button type="button" className={`${styles.quickBtn} ${styles.quickWater}`} onClick={() => commitAction([plant.id], 'water', `💧 Podlano ${plant.name}`)}>
            <span className={styles.quickEmoji}>💧</span>Podlej
          </button>
          <button type="button" className={`${styles.quickBtn} ${styles.quickFert}`} onClick={onFertilise}>
            <span className={styles.quickEmoji}>🌱</span>Nawóź
          </button>
          <button type="button" className={`${styles.quickBtn} ${styles.quickRepot}`} onClick={() => navigate(repotPath(plant.id))}>
            <span className={styles.quickEmoji}>🪴</span>Przesadź
          </button>
          <button type="button" className={`${styles.quickBtn} ${styles.quickMore}`} onClick={() => setSheet('actions')}>
            <span className={styles.quickEmoji}>⋯</span>
          </button>
        </div>

        <h2 className={styles.sectionLabel}>Harmonogram</h2>
        <div className={styles.schedule}>
          {view.schedule.map((s) => (
            <div key={s.type} className={styles.scheduleRow}>
              <span className={styles.scheduleIcon} style={{ background: s.bg }}>{s.emoji}</span>
              <div className={styles.scheduleText}>
                <div className={styles.scheduleTitle}>{s.label}</div>
                <div className={styles.scheduleDetail}>{s.detail}</div>
              </div>
              <span className={styles.schedulePill} style={{ background: s.pill, color: s.ink }}>
                {s.rel}
              </span>
            </div>
          ))}
        </div>

        <h2 className={styles.sectionLabel}>Pojemnik</h2>
        <div className={styles.pot}>
          <span className={styles.potEmoji}>🪴</span>
          <div className={styles.potText}>
            <div className={styles.potTitle}>{view.potText}</div>
            <div className={styles.potNote}>{view.potNote}</div>
          </div>
          <button type="button" className={styles.potBtn} onClick={() => navigate(repotPath(plant.id))}>
            Przesadź
          </button>
        </div>

        <div className={styles.historyHead}>
          <span className={styles.sectionLabel}>Historia</span>
          <span className={styles.histCount}>{view.histCount}</span>
        </div>
        <div className={styles.history}>
          {view.history.map((h) => (
            <div key={h.key} className={styles.histRow}>
              <div className={styles.histRail}>
                <span className={styles.histDot} style={{ background: h.bg }}>{h.emoji}</span>
                {h.showLine && <span className={styles.histLine} />}
              </div>
              <div className={styles.histBody}>
                <div className={styles.histLabel}>{h.label}</div>
                <div className={styles.histDate}>{h.date}</div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <ActionGridSheet
        open={sheet === 'actions'}
        onClose={() => setSheet(null)}
        kicker={`Roślina · ${plant.code}`}
        title={plant.name}
        actions={EXTRA_ACTIONS.map((a) => ({
          emoji: a.emoji,
          label: a.label,
          onClick: () => {
            logExtra(plant.id, a.kind, `${a.emoji} ${a.label} · ${plant.name}`);
            setSheet(null);
          },
        }))}
      />

      <RenameSheet
        open={sheet === 'rename'}
        onClose={() => setSheet(null)}
        initial={plant.code}
        onSave={(label) => {
          rename(plant.id, label, `✏️ Etykieta: ${label}`);
          setSheet(null);
        }}
      />
    </div>
  );
};
