import { Form, Formik, useFormikContext } from 'formik';
import * as Yup from 'yup';
import type { GroupType } from '../../types';
import { BottomSheet } from '../../components/sheet/BottomSheet';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import sheet from '../../components/sheet/sheetForm.module.css';
import styles from './AddGroupSheet.module.css';

interface AddGroupSheetProps {
  open: boolean;
  onClose: () => void;
  onCreate: (name: string, type: GroupType) => void;
}

interface AddGroupValues {
  name: string;
  type: GroupType;
}

interface TypeOption {
  id: GroupType;
  emoji: string;
  label: string;
  desc: string;
}

const TYPE_OPTIONS: readonly TypeOption[] = [
  { id: 'work', emoji: '⚡', label: 'Robocza', desc: 'Ten sam rytm — akcja od razu na wszystkie' },
  { id: 'region', emoji: '📍', label: 'Region', desc: 'Obszar ogrodu — statystyki i podgląd' },
  { id: 'adhoc', emoji: '📌', label: 'Doraźna', desc: 'Tymczasowe zadanie na teraz' },
];

const schema = Yup.object({
  name: Yup.string().trim().required('Podaj nazwę grupy'),
});

export const AddGroupSheet = ({ open, onClose, onCreate }: AddGroupSheetProps) => (
  <BottomSheet open={open} onClose={onClose}>
    <h2 className={sheet.title}>Nowa grupa</h2>
    <p className={sheet.desc}>Wybierz typ — decyduje o zachowaniu grupy.</p>

    <Formik<AddGroupValues>
      initialValues={{ name: '', type: 'work' }}
      validationSchema={schema}
      onSubmit={(values) => onCreate(values.name.trim(), values.type)}
    >
      <Form>
        <TypePicker />
        <TextField name="name" label="" placeholder="Nazwa grupy" />
        <div className={sheet.actions}>
          <Button variant="neutral" onClick={onClose}>
            Anuluj
          </Button>
          <Button type="submit" block>
            Dodaj grupę
          </Button>
        </div>
      </Form>
    </Formik>
  </BottomSheet>
);

const TypePicker = () => {
  const { values, setFieldValue } = useFormikContext<AddGroupValues>();
  return (
    <div className={styles.types}>
      {TYPE_OPTIONS.map((opt) => (
        <button
          key={opt.id}
          type="button"
          className={`${styles.type} ${values.type === opt.id ? styles.typeActive : ''}`}
          onClick={() => setFieldValue('type', opt.id)}
        >
          <span className={styles.typeEmoji}>{opt.emoji}</span>
          <span className={styles.typeText}>
            <span className={styles.typeLabel}>{opt.label}</span>
            <span className={styles.typeDesc}>{opt.desc}</span>
          </span>
        </button>
      ))}
    </div>
  );
};
