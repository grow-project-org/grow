import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { BottomSheet } from './BottomSheet';
import { TextField } from '../form/TextField';
import { Button } from '../ui/Button';
import styles from './sheetForm.module.css';

interface CustomEventSheetProps {
  open: boolean;
  onClose: () => void;
  onSave: (note: string) => void;
}

interface CustomEventValues {
  note: string;
}

const schema = Yup.object({
  note: Yup.string().trim().required('Opisz zdarzenie'),
});

/** Log a one-off event outside the fixed list, with a free-text description. */
export const CustomEventSheet = ({ open, onClose, onSave }: CustomEventSheetProps) => (
  <BottomSheet open={open} onClose={onClose}>
    <h2 className={styles.title}>Własne zdarzenie</h2>
    <p className={styles.desc}>Krótki opis trafi do historii z dzisiejszą datą.</p>

    <Formik<CustomEventValues>
      initialValues={{ note: '' }}
      validationSchema={schema}
      onSubmit={(values) => onSave(values.note.trim())}
    >
      <Form>
        <TextField name="note" label="Opis" placeholder="np. Zauważone mączniaki" />
        <div className={styles.actions}>
          <Button variant="neutral" onClick={onClose}>
            Anuluj
          </Button>
          <Button type="submit" block>
            Zapisz
          </Button>
        </div>
      </Form>
    </Formik>
  </BottomSheet>
);
