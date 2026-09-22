import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { fmtLong, weekdayLong } from '../../utils/date';
import { Avatar } from '../../components/ui/Avatar';
import { CheckToggle } from '../../components/ui/CheckToggle';
import { ACTION_META } from '../../domain/actions';
import { plantPath } from '../../routes/paths';
import { selectToday, type TodayRow } from './today.selectors';
import type { ActionType } from '../../types';
import styles from './TodayPage.module.css';

export const TodayPage = () => {
  const { species, groups, plants, today, isLoading, commitAction } = useGarden();
  const navigate = useNavigate();
  const { summary, sections, left, allDone } = selectToday(species, groups, plants, today);

  const check = (id: string, type: ActionType) =>
    void commitAction([id], type, `${ACTION_META[type].doneLabel} 1 roślinę`);

  return (
    <div className={styles.page}>
      <header className={styles.head}>
        <div>
          <p className={styles.date}>{`${weekdayLong(today)}, ${fmtLong(today)}`}</p>
          <h1 className={styles.greeting}>Dzień dobry 🌱</h1>
        </div>
        <div className={styles.counter}>
          <span className={styles.counterNum}>{left}</span>
          <span className={styles.counterLabel}>DO ZROB.</span>
        </div>
      </header>

      <div className={styles.summary}>
        <div className={`${styles.summaryStat} ${styles.summaryWater}`}>
          <span className={styles.summaryValue}>{summary.water}</span>
          <span className={styles.summaryLabel}>💧 do podlania</span>
        </div>
        <div className={`${styles.summaryStat} ${styles.summaryFert}`}>
          <span className={styles.summaryValue}>{summary.fert}</span>
          <span className={styles.summaryLabel}>🌱 do nawożenia</span>
        </div>
        <div className={`${styles.summaryStat} ${styles.summaryOverdue}`}>
          <span className={styles.summaryValue}>{summary.overdue}</span>
          <span className={styles.summaryLabel}>⚠️ zaległe</span>
        </div>
      </div>

      {isLoading && <p className={styles.state}>Wczytywanie ogrodu…</p>}

      {!isLoading && allDone && !sections.length && (
        <div className={styles.allDone}>
          <div className={styles.confetti}>🎉</div>
          <p className={styles.allDoneTitle}>Nic na dziś!</p>
          <p className={styles.allDoneSub}>
            {plants.length
              ? 'Rośliny zadowolone. Do zobaczenia jutro.'
              : 'Dodaj pierwszą roślinę, żeby zobaczyć tu harmonogram.'}
          </p>
        </div>
      )}

      {sections.map((section) => (
        <section key={section.type} className={styles.section}>
          <div className={styles.sectionHead}>
            <span className={styles.sectionEmoji}>{section.emoji}</span>
            <span className={styles.sectionTitle}>{section.title}</span>
            <span className={styles.sectionCount}>{section.rows.length}</span>
          </div>
          <ul className={styles.rows}>
            {section.rows.map((row) => (
              <TaskRow
                key={row.id}
                row={row}
                onOpen={() => navigate(plantPath(row.id))}
                onCheck={() => check(row.id, section.type)}
              />
            ))}
          </ul>
        </section>
      ))}
    </div>
  );
};

interface TaskRowProps {
  row: TodayRow;
  onOpen: () => void;
  onCheck: () => void;
}

const TaskRow = ({ row, onOpen, onCheck }: TaskRowProps) => (
  <li className={styles.row}>
    <button type="button" className={styles.rowMain} onClick={onOpen}>
      <Avatar label={row.initial} bg={row.avatarBg} size={44} radius={13} />
      <span className={styles.rowText}>
        <span className={styles.rowTitleLine}>
          <span className={row.done ? styles.doneText : undefined}>{row.name}</span>
          {row.overdue && <span className={styles.overdue}>zaległe</span>}
        </span>
        <span className={styles.rowSub}>{row.sub}</span>
      </span>
    </button>
    <CheckToggle checked={row.done} disabled={row.done} onClick={onCheck} />
  </li>
);
