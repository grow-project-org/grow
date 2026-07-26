import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { BottomSheet } from './BottomSheet';
import { TextField } from '../form/TextField';
import { Button } from '../ui/Button';
import { parseOptionalInt, isValidOptionalInt } from '../../utils/number';
import styles from './sheetForm.module.css';

interface HarvestSheetProps {
  open: boolean;
  onClose: () => void;
  onSave: (qty: number | null, weight: number | null) => void;
}

interface HarvestValues {
  qty: string;
  weight: string;
}

const schema = Yup.object({
  qty: Yup.string().test('qty', 'Podaj liczbę sztuk', isValidOptionalInt),
  weight: Yup.string().test('weight', 'Podaj wagę w gramach', isValidOptionalInt),
});

/** Log a harvest event — quantity and weight are both optional, never required. */
export const HarvestSheet = ({ open, onClose, onSave }: HarvestSheetProps) => (
  <BottomSheet open={open} onClose={onClose}>
    <h2 className={styles.title}>Zbiór plonów</h2>
    <p className={styles.desc}>Ilość i waga są opcjonalne — pomiń, jeśli nie chce ci się liczyć ani ważyć.</p>

    <Formik<HarvestValues>
      initialValues={{ qty: '', weight: '' }}
      validationSchema={schema}
      onSubmit={(values) => onSave(parseOptionalInt(values.qty), parseOptionalInt(values.weight))}
    >
      <Form>
        <TextField name="qty" label="Ilość (szt.)" optional placeholder="np. 12" inputMode="numeric" />
        <TextField name="weight" label="Waga (g)" optional placeholder="np. 350" inputMode="numeric" />
        <div className={styles.actions}>
          <Button variant="neutral" onClick={onClose}>
            Anuluj
          </Button>
          <Button type="submit" block>
            Zapisz zbiór
          </Button>
        </div>
      </Form>
    </Formik>
  </BottomSheet>
);
