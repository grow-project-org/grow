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
import { HarvestSheet } from '../../components/sheet/HarvestSheet';
import { CustomEventSheet } from '../../components/sheet/CustomEventSheet';
import { selectProfile } from './profile.selectors';
import styles from './PlantProfilePage.module.css';

type OpenSheet = 'actions' | 'harvest' | 'custom' | null;

export const PlantProfilePage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { plantById, groups, log, species, commitAction, logExtra } = useGarden();
  const { flash } = useToast();
  const [sheet, setSheet] = useState<OpenSheet>(null);

  const plant = plantById(Number(id));
  if (!plant) return <Navigate to={ROUTES.plants} replace />;

  const view = selectProfile(species, plant, log, groups);
  const displayName = plant.species ?? 'Roślina bez gatunku';

  const onFertilise = () => {
    if (interval(species, plant.species, 'fert') == null) {
      flash('Ten gatunek nie ma ustawionego nawożenia');
      return;
    }
    commitAction([plant.id], 'fert', `🌱 Nawożono ${plant.code}`);
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
        <h1 className={styles.name}>{displayName}</h1>
        <p className={styles.meta}>{view.region}</p>

        <div className={styles.codeBtn}>{plant.code}</div>

        <div className={styles.tags}>
          {view.groups.map((g) => (
            <span key={g.name} className={styles.tag}>
              {g.emoji} {g.name}
            </span>
          ))}
        </div>

        <div className={styles.quick}>
          <button type="button" className={`${styles.quickBtn} ${styles.quickWater}`} onClick={() => commitAction([plant.id], 'water', `💧 Podlano ${plant.code}`)}>
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
        title={displayName}
        actions={EXTRA_ACTIONS.map((a) => ({
          emoji: a.emoji,
          label: a.label,
          onClick: () => {
            if (a.kind === 'harvest') {
              setSheet('harvest');
              return;
            }
            if (a.kind === 'custom') {
              setSheet('custom');
              return;
            }
            logExtra([plant.id], a.kind, `${a.emoji} ${a.label} · ${plant.code}`);
            setSheet(null);
          },
        }))}
      />

      <HarvestSheet
        open={sheet === 'harvest'}
        onClose={() => setSheet(null)}
        onSave={(qty, weight) => {
          logExtra([plant.id], 'harvest', `🧺 Zbiór · ${plant.code}`, { qty, weight });
          setSheet(null);
        }}
      />

      <CustomEventSheet
        open={sheet === 'custom'}
        onClose={() => setSheet(null)}
        onSave={(note) => {
          logExtra([plant.id], 'custom', `📝 ${note} · ${plant.code}`, { note });
          setSheet(null);
        }}
      />
    </div>
  );
};
