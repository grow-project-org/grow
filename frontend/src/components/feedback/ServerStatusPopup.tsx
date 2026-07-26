import { useSyncExternalStore } from 'react';
import { dismissNotice, getNotices, subscribeNotices } from '../../state/notifications';
import { queryClient } from '../../app/queryClient';
import { gardenKeys } from '../../api/queryKeys';
import styles from './ServerStatusPopup.module.css';

/** Connection-error popup driven by the notifications store. */
export const ServerStatusPopup = () => {
  const notices = useSyncExternalStore(subscribeNotices, getNotices, getNotices);
  if (!notices.length) return null;

  const latest = notices[notices.length - 1];
  const retry = () => queryClient.refetchQueries({ queryKey: gardenKeys.all });

  return (
    <div className={styles.wrap}>
      <div className={styles.popup} role="alert">
        <span className={styles.icon}>📡</span>
        <div className={styles.body}>
          <div className={styles.title}>{latest.title}</div>
          <div className={styles.message}>{latest.message}</div>
          <div className={styles.note}>Zmiany są zapisywane lokalnie i zsynchronizują się po odzyskaniu połączenia.</div>
          <div className={styles.actions}>
            <button type="button" className={styles.retry} onClick={retry}>
              Spróbuj ponownie
            </button>
            <button type="button" className={styles.dismiss} onClick={() => dismissNotice(latest.id)}>
              Ukryj
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
