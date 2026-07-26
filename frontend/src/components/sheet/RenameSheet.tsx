import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { BottomSheet } from './BottomSheet';
import { TextField } from '../form/TextField';
import { Button } from '../ui/Button';
import styles from './sheetForm.module.css';

interface RenameSheetProps {
  open: boolean;
  onClose: () => void;
  initial: string;
  onSave: (label: string) => void;
}

interface RenameValues {
  label: string;
}

const schema = Yup.object({
  label: Yup.string().trim().required('Podaj etykietę lub ID'),
});

/** Rename / relabel a single plant instance. */
export const RenameSheet = ({ open, onClose, initial, onSave }: RenameSheetProps) => (
  <BottomSheet open={open} onClose={onClose}>
    <h2 className={styles.title}>Identyfikator rośliny</h2>
    <p className={styles.desc}>
      Nadaj własną etykietę (np. „Balkon-róg”) albo zostaw wygenerowany kod.
    </p>
    <Formik<RenameValues>
      initialValues={{ label: initial }}
      enableReinitialize
      validationSchema={schema}
      onSubmit={(values) => onSave(values.label.trim())}
    >
      <Form>
        <TextField name="label" label="Etykieta / ID" placeholder="Etykieta / ID" />
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
