import { Navigate, useNavigate, useParams } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { useCareDate } from '../../state/CareDateContext';
import { useToast } from '../../state/ToastContext';
import { interval } from '../../domain/species';
import { ACTION_META } from '../../domain/actions';
import { isDoneToday } from '../../domain/schedule';
import { ROUTES } from '../../routes/paths';
import { fmtLong } from '../../utils/date';
import { IconButton } from '../../components/ui/IconButton';
import { ChevronLeftIcon } from '../../components/ui/icons';
import { CareDateBanner, CareDateChip } from '../../components/ui/CareDateBar';
import { selectProfile } from './profile.selectors';
import type { ActionType } from '../../types';
import styles from './PlantProfilePage.module.css';

export const PlantProfilePage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { plantById, groups, species, today, isLoading, commitAction } = useGarden();
  const { careDate, isBackdated } = useCareDate();
  const { flash } = useToast();

  const plant = id ? plantById(id) : undefined;

  if (!plant) {
    if (isLoading) return <p className={styles.state}>Wczytywanie…</p>;
    return <Navigate to={ROUTES.plants} replace />;
  }

  const view = selectProfile(species, plant, groups, today);

  const run = (type: ActionType) => {
    if (interval(species, plant.specieId, type) == null) {
      flash(`Ten gatunek nie ma ustawionego interwału: ${ACTION_META[type].label.toLowerCase()}`);
      return;
    }
    if (!isBackdated && isDoneToday(plant, type, today)) {
      flash('Już odhaczone dzisiaj');
      return;
    }

    const when = isBackdated ? ` (${fmtLong(careDate)})` : '';
    void commitAction(
      [plant.id],
      type,
      `${ACTION_META[type].emoji} ${ACTION_META[type].doneLabel} ${plant.code}${when}`,
      careDate,
    );
  };

  return (
    <div>
      <div className={styles.hero} style={{ background: view.avatarBg }}>
        <IconButton soft aria-label="Wróć" className={styles.heroBtnLeft} onClick={() => navigate(-1)}>
          <ChevronLeftIcon />
        </IconButton>
        <span className={styles.heroEmoji}>{view.initial}</span>
      </div>

      <div className={styles.body}>
        <h1 className={styles.name}>{view.name}</h1>
        <p className={styles.meta}>{view.region}</p>

        <div className={styles.codeBtn}>{plant.code}</div>

        <div className={styles.tags}>
          {view.groups.map((g) => (
            <span key={g.id} className={styles.tag}>
              {g.emoji} {g.name}
            </span>
          ))}
        </div>

        <div className={styles.careDateRow}>
          <span className={styles.careDateLabel}>Data zabiegu</span>
          <CareDateChip />
        </div>

        <CareDateBanner />

        <div className={styles.quick}>
          <button
            type="button"
            className={`${styles.quickBtn} ${styles.quickWater}`}
            onClick={() => run('water')}
          >
            <span className={styles.quickEmoji}>💧</span>Podlej
          </button>
          <button
            type="button"
            className={`${styles.quickBtn} ${styles.quickFert}`}
            onClick={() => run('fert')}
          >
            <span className={styles.quickEmoji}>🌱</span>Nawóź
          </button>
        </div>

        <h2 className={styles.sectionLabel}>Harmonogram</h2>
        <div className={styles.schedule}>
          {view.schedule.map((s) => (
            <div key={s.type} className={styles.scheduleRow}>
              <span className={styles.scheduleIcon} style={{ background: s.bg }}>
                {s.emoji}
              </span>
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

        <p className={styles.note}>
          Przypisanie do grup i regionów zmieniasz na ekranie <strong>Grupy</strong>.
        </p>
      </div>
    </div>
  );
};
