import { Form, Formik, useFormikContext } from 'formik';
import * as Yup from 'yup';
import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { MAX_BULK_ADD } from '../../config';
import { clamp, parseDecimal } from '../../utils/number';
import { ROUTES } from '../../routes/paths';
import { PageHeader } from '../../components/layout/PageHeader';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import styles from './AddPlantPage.module.css';

interface AddValues {
  name: string;
  species: string;
  qty: number;
  loc: string;
  potL: string;
  groups: string[];
}

const schema = Yup.object({
  name: Yup.string().trim().required('Podaj nazwę'),
  qty: Yup.number().required().integer().min(1).max(MAX_BULK_ADD),
  potL: Yup.string().test('potL', 'Podaj poprawną objętość', (raw) => {
    if (!raw || !raw.trim()) return true;
    const n = Number(raw.trim().replace(',', '.'));
    return Number.isFinite(n) && n > 0;
  }),
});

const QTY_CHIPS = [1, 5, 10, 20] as const;

const INITIAL: AddValues = { name: '', species: '', qty: 1, loc: '', potL: '', groups: [] };

export const AddPlantPage = () => {
  const navigate = useNavigate();
  const { addPlants } = useGarden();

  return (
    <div className={styles.page}>
      <PageHeader title="Nowa roślina" />

      <div className={styles.tip}>
        <span className={styles.tipIcon}>🌿</span>
        <p className={styles.tipText}>
          Wystarczy nazwa. Resztę uzupełnisz kiedykolwiek — apka działa też, gdy nie pamiętasz
          historii.
        </p>
      </div>

      <Formik<AddValues>
        initialValues={INITIAL}
        validationSchema={schema}
        onSubmit={(values) => {
          addPlants({
            name: values.name.trim(),
            species: values.species.trim() || null,
            qty: clamp(values.qty, 1, MAX_BULK_ADD),
            loc: values.loc.trim() || null,
            potL: parseDecimal(values.potL),
            groups: values.groups,
          });
          navigate(ROUTES.plants);
        }}
      >
        <Form>
          <TextField name="name" label="Nazwa" requiredMark placeholder="np. Monstera z salonu" />
          <TextField name="species" label="Gatunek" optional placeholder="np. Pomidor, Bazylia…" />

          <QuantityField />
          <TextField name="loc" label="Lokalizacja" optional placeholder="np. Balkon, Parapet…" />
          <TextField
            name="potL"
            label="Objętość doniczki w litrach"
            optional
            placeholder="np. 5"
            inputMode="decimal"
          />

          <GroupPicker />

          <SubmitButton />
        </Form>
      </Formik>
    </div>
  );
};

const QuantityField = () => {
  const { values, setFieldValue } = useFormikContext<AddValues>();
  const setQty = (n: number) => setFieldValue('qty', clamp(n, 1, MAX_BULK_ADD));

  return (
    <div className={styles.qtyBlock}>
      <span className={styles.label}>Ile sztuk?</span>
      <p className={styles.hint}>
        Każda sztuka to osobna instancja z własnym ID (np. PAP-01) i historią — pojemniki i daty
        ustawisz później indywidualnie.
      </p>
      <div className={styles.stepper}>
        <button type="button" className={styles.stepBtn} onClick={() => setQty(values.qty - 1)}>
          −
        </button>
        <input
          className={styles.stepInput}
          inputMode="numeric"
          value={values.qty}
          onChange={(e) => {
            const digits = e.target.value.replace(/[^0-9]/g, '').slice(0, 3);
            setFieldValue('qty', digits ? Number(digits) : 1);
          }}
        />
        <button type="button" className={styles.stepBtn} onClick={() => setQty(values.qty + 1)}>
          +
        </button>
      </div>
      <div className={styles.chips}>
        {QTY_CHIPS.map((v) => (
          <button
            key={v}
            type="button"
            className={`${styles.chip} ${values.qty === v ? styles.chipActive : ''}`}
            onClick={() => setFieldValue('qty', v)}
          >
            {v}
          </button>
        ))}
      </div>
    </div>
  );
};

const GroupPicker = () => {
  const { groups } = useGarden();
  const { values, setFieldValue } = useFormikContext<AddValues>();

  const toggle = (name: string) => {
    const next = values.groups.includes(name)
      ? values.groups.filter((g) => g !== name)
      : [...values.groups, name];
    setFieldValue('groups', next);
  };

  return (
    <div className={styles.groupBlock}>
      <span className={styles.label}>
        Dodaj do grup <span className={styles.optional}>opcjonalnie</span>
      </span>
      <div className={styles.groupChips}>
        {groups.map((g) => {
          const on = values.groups.includes(g.name);
          return (
            <button
              key={g.name}
              type="button"
              className={`${styles.groupChip} ${on ? styles.groupChipActive : ''}`}
              onClick={() => toggle(g.name)}
            >
              {g.emoji} {g.name}
            </button>
          );
        })}
      </div>
    </div>
  );
};

const SubmitButton = () => {
  const { values } = useFormikContext<AddValues>();
  return (
    <Button type="submit" block disabled={!values.name.trim()} className={styles.save}>
      Dodaj roślinę
    </Button>
  );
};
