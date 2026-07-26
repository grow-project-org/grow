import { useState } from 'react';
import { Form, Formik, useFormikContext } from 'formik';
import * as Yup from 'yup';
import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { useToast } from '../../state/ToastContext';
import { MAX_BULK_ADD } from '../../config';
import { clamp, parseDecimal } from '../../utils/number';
import { codeTaken } from '../../domain/ids';
import { ROUTES } from '../../routes/paths';
import { PageHeader } from '../../components/layout/PageHeader';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import { PlusIcon } from '../../components/ui/icons';
import { AddSpeciesSheet } from './AddSpeciesSheet';
import styles from './AddPlantPage.module.css';

interface AddValues {
  species: string;
  qty: number;
  potL: string;
  groups: string[];
  code: string;
}

const schema = Yup.object({
  species: Yup.string().trim().required('Wybierz gatunek'),
  qty: Yup.number().required().integer().min(1).max(MAX_BULK_ADD),
  potL: Yup.string().test('potL', 'Podaj poprawną objętość', (raw) => {
    if (!raw || !raw.trim()) return true;
    const n = Number(raw.trim().replace(',', '.'));
    return Number.isFinite(n) && n > 0;
  }),
});

const QTY_CHIPS = [1, 5, 10, 20] as const;

const INITIAL: AddValues = { species: '', qty: 1, potL: '', groups: [], code: '' };

export const AddPlantPage = () => {
  const navigate = useNavigate();
  const garden = useGarden();
  const { flash } = useToast();

  return (
    <div className={styles.page}>
      <PageHeader title="Nowa roślina" />

      <div className={styles.tip}>
        <span className={styles.tipIcon}>🌿</span>
        <p className={styles.tipText}>
          Wybierz gatunek — resztę uzupełnisz kiedykolwiek. Apka działa też, gdy nie pamiętasz
          historii.
        </p>
      </div>

      <Formik<AddValues>
        initialValues={INITIAL}
        validationSchema={schema}
        onSubmit={(values, helpers) => {
          const qty = clamp(values.qty, 1, MAX_BULK_ADD);
          const code = qty === 1 ? values.code.trim() : '';
          if (code && codeTaken(garden.garden, code)) {
            helpers.setFieldError('code', 'Ten kod jest już zajęty');
            flash('⚠️ Ten kod jest już zajęty');
            return;
          }
          garden.addPlants({
            species: values.species,
            qty,
            potL: parseDecimal(values.potL),
            groups: values.groups,
            code: code || null,
          });
          navigate(ROUTES.plants);
        }}
      >
        <Form>
          <SpeciesPicker />

          <QuantityField />

          {/* A manual code is a permanent physical pot label — only meaningful for a single plant. */}
          <CodeFieldIfSingle />

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

const SpeciesPicker = () => {
  const { species, addSpecies } = useGarden();
  const { values, setFieldValue, errors, touched } = useFormikContext<AddValues>();
  const [addingSpecies, setAddingSpecies] = useState(false);

  return (
    <div className={styles.groupBlock}>
      <span className={styles.label}>
        Gatunek <span className={styles.required}>•</span>
      </span>
      <div className={styles.groupChips}>
        {species.map((s) => (
          <button
            key={s.name}
            type="button"
            className={`${styles.groupChip} ${values.species === s.name ? styles.groupChipActive : ''}`}
            onClick={() => setFieldValue('species', s.name)}
          >
            {s.emoji} {s.name}
          </button>
        ))}
        <button type="button" className={styles.groupChip} onClick={() => setAddingSpecies(true)}>
          <PlusIcon size={14} /> Nowy gatunek
        </button>
      </div>
      {touched.species && errors.species && <p className={styles.error}>{errors.species}</p>}

      <AddSpeciesSheet
        open={addingSpecies}
        onClose={() => setAddingSpecies(false)}
        existingNames={species.map((s) => s.name)}
        onCreate={(name, emoji, w, f) => {
          addSpecies(name, emoji, w, f);
          setFieldValue('species', name);
          setAddingSpecies(false);
        }}
      />
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
        Każda sztuka to osobna instancja z własnym, trwałym kodem (np. PAP-01) i historią —
        pojemniki i grupy ustawisz później indywidualnie.
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

const CodeFieldIfSingle = () => {
  const { values } = useFormikContext<AddValues>();
  if (values.qty !== 1) return null;
  return (
    <TextField
      name="code"
      label="Kod (opcjonalnie)"
      optional
      placeholder="np. PJ03 — zostaw puste, by wygenerować"
      hint="Kod jest trwałą etykietą doniczki — nie da się go później zmienić."
    />
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
        Dodaj do regionów / grup <span className={styles.optional}>opcjonalnie</span>
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
    <Button type="submit" block disabled={!values.species.trim()} className={styles.save}>
      Dodaj roślinę
    </Button>
  );
};
