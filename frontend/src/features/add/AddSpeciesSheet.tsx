import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { BottomSheet } from '../../components/sheet/BottomSheet';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import { isValidOptionalInt, parseOptionalInt } from '../../utils/number';
import sheet from '../../components/sheet/sheetForm.module.css';

interface AddSpeciesSheetProps {
  open: boolean;
  onClose: () => void;
  existingNames: readonly string[];
  onCreate: (name: string, w: number | null, f: number | null) => Promise<void>;
}

interface AddSpeciesValues {
  name: string;
  w: string;
  f: string;
}

export const AddSpeciesSheet = ({ open, onClose, existingNames, onCreate }: AddSpeciesSheetProps) => {
  const schema = Yup.object({
    name: Yup.string()
      .trim()
      .required('Podaj nazwę gatunku')
      .test('unique', 'Taki gatunek już istnieje', (value) =>
        !value || !existingNames.some((n) => n.toLowerCase() === value.trim().toLowerCase()),
      ),
    w: Yup.string().test('w', 'Podaj liczbę dni', isValidOptionalInt),
    f: Yup.string().test('f', 'Podaj liczbę dni', isValidOptionalInt),
  });

  return (
    <BottomSheet open={open} onClose={onClose}>
      <h2 className={sheet.title}>Nowy gatunek</h2>
      <p className={sheet.desc}>
        Prywatny, tylko dla Ciebie. Interwały możesz zostawić puste, jeśli dana czynność nie dotyczy
        tego gatunku.
      </p>

      <Formik<AddSpeciesValues>
        initialValues={{ name: '', w: '', f: '' }}
        validationSchema={schema}
        onSubmit={async (values, helpers) => {
          await onCreate(values.name.trim(), parseOptionalInt(values.w), parseOptionalInt(values.f));
          helpers.resetForm();
        }}
      >
        {({ isSubmitting }) => (
          <Form>
            <TextField name="name" label="Nazwa" requiredMark placeholder="np. Papryka Jalapeño" />
            <TextField name="w" label="Podlewanie co (dni)" optional placeholder="np. 3" inputMode="numeric" />
            <TextField name="f" label="Nawożenie co (dni)" optional placeholder="np. 14" inputMode="numeric" />
            <div className={sheet.actions}>
              <Button variant="neutral" onClick={onClose}>
                Anuluj
              </Button>
              <Button type="submit" block disabled={isSubmitting}>
                {isSubmitting ? 'Dodawanie…' : 'Dodaj gatunek'}
              </Button>
            </div>
          </Form>
        )}
      </Formik>
    </BottomSheet>
  );
};
