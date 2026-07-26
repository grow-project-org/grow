import { useToast } from '../../state/ToastContext';
import styles from './Toast.module.css';

export const Toast = () => {
  const { message } = useToast();
  if (!message) return null;
  return <div className={styles.toast}>{message}</div>;
};
