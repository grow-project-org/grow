import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { TODAY } from '../../config';
import { DOW } from '../../utils/date';
import { Avatar } from '../../components/ui/Avatar';
import { plantPath } from '../../routes/paths';
import { selectCalendar } from './calendar.selectors';
import styles from './CalendarPage.module.css';

export const CalendarPage = () => {
  const { garden } = useGarden();
  const navigate = useNavigate();
  const [selected, setSelected] = useState(TODAY);
  const view = selectCalendar(garden, selected);

  return (
    <div className={styles.page}>
      <h1 className={styles.heading}>Kalendarz</h1>

      <div className={styles.calendar}>
        <div className={styles.monthTitle}>{view.title}</div>

        <div className={styles.dow}>
          {DOW.map((d) => (
            <div key={d} className={styles.dowCell}>
              {d}
            </div>
          ))}
        </div>

        <div className={styles.grid}>
          {Array.from({ length: view.leadingBlanks }, (_, i) => (
            <span key={`blank-${i}`} />
          ))}
          {view.cells.map((cell) => {
            const isSelected = cell.iso === selected;
            const cls = [
              styles.cell,
              cell.isToday ? styles.today : '',
              isSelected ? styles.selected : '',
            ]
              .filter(Boolean)
              .join(' ');
            return (
              <button key={cell.iso} type="button" className={cls} onClick={() => setSelected(cell.iso)}>
                <span>{cell.day}</span>
                {cell.hasEvents && (
                  <span className={isSelected ? styles.dotSelected : styles.dot} />
                )}
              </button>
            );
          })}
        </div>
      </div>

      <h2 className={styles.selectedTitle}>{view.selectedTitle}</h2>

      <div className={styles.events}>
        {view.events.map((event) => (
          <button
            key={`${event.id}-${event.action}`}
            type="button"
            className={styles.event}
            onClick={() => navigate(plantPath(event.id))}
          >
            <Avatar emoji={event.emoji} bg={event.bg} size={40} radius={12} fontSize={19} thinBorder />
            <span className={styles.eventText}>
              <span className={styles.eventName}>{event.name}</span>
              <span className={styles.eventMeta}>
                {event.action} · {event.loc}
              </span>
            </span>
          </button>
        ))}

        {view.empty && <div className={styles.empty}>Brak zaplanowanych zabiegów 🌤️</div>}
      </div>
    </div>
  );
};
