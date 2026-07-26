import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { TODAY } from '../../config';
import { fmtLong, weekdayLong } from '../../utils/date';
import { Avatar } from '../../components/ui/Avatar';
import { CheckToggle } from '../../components/ui/CheckToggle';
import { plantPath } from '../../routes/paths';
import { selectToday, type TodayRow } from './today.selectors';
import styles from './TodayPage.module.css';

export const TodayPage = () => {
  const { garden, done, toggleToday } = useGarden();
  const navigate = useNavigate();
  const { sections, left, allDone } = selectToday(garden, done);

  return (
    <div className={styles.page}>
      <header className={styles.head}>
        <div>
          <p className={styles.date}>{`${weekdayLong(TODAY)}, ${fmtLong(TODAY)}`}</p>
          <h1 className={styles.greeting}>Dzień dobry 🌱</h1>
        </div>
        <div className={styles.counter}>
          <span className={styles.counterNum}>{left}</span>
          <span className={styles.counterLabel}>DO ZROB.</span>
        </div>
      </header>

      {allDone && (
        <div className={styles.allDone}>
          <div className={styles.confetti}>🎉</div>
          <p className={styles.allDoneTitle}>Wszystko zrobione!</p>
          <p className={styles.allDoneSub}>Rośliny zadowolone. Do zobaczenia jutro.</p>
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
                onToggle={() => toggleToday(row.id, section.type)}
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
  onToggle: () => void;
}

const TaskRow = ({ row, onOpen, onToggle }: TaskRowProps) => (
  <li className={styles.row}>
    <button type="button" className={styles.rowMain} onClick={onOpen}>
      <Avatar emoji={row.emoji} bg={row.avatarBg} size={44} radius={13} />
      <span className={styles.rowText}>
        <span className={styles.rowTitleLine}>
          <span className={row.done ? styles.doneText : undefined}>{row.name}</span>
          {row.overdue && <span className={styles.overdue}>zaległe</span>}
        </span>
        <span className={styles.rowSub}>{row.sub}</span>
      </span>
    </button>
    <CheckToggle checked={row.done} onClick={onToggle} />
  </li>
);
