import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { DOW } from '../../utils/date';
import { Avatar } from '../../components/ui/Avatar';
import { plantPath } from '../../routes/paths';
import { selectCalendar } from './calendar.selectors';
import styles from './CalendarPage.module.css';

export const CalendarPage = () => {
  const { species, groups, plants, today } = useGarden();
  const navigate = useNavigate();
  const [selected, setSelected] = useState(today);
  const view = selectCalendar(species, groups, plants, selected, today);

  return (
    <div className={styles.page}>
      <h1 className={styles.heading}>Kalendarz</h1>

      <div className={styles.calendar}>
        <div className={styles.monthBar}>
          <button type="button" className={styles.monthNav} onClick={() => setSelected(view.prevMonth)} aria-label="Poprzedni miesiąc">
            ‹
          </button>
          <div className={styles.monthTitle}>{view.title}</div>
          <button type="button" className={styles.monthNav} onClick={() => setSelected(view.nextMonth)} aria-label="Następny miesiąc">
            ›
          </button>
        </div>

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
            <Avatar label={event.initial} bg={event.bg} size={40} radius={12} fontSize={17} thinBorder />
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
