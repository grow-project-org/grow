import { useState } from 'react';
import { Form, Formik, useFormikContext } from 'formik';
import * as Yup from 'yup';
import { useNavigate } from 'react-router-dom';
import { useGarden } from '../../state/GardenContext';
import { useToast } from '../../state/ToastContext';
import { ApiError } from '../../api/client';
import { codeTaken, suggestCode } from '../../domain/ids';
import { GROUP_TYPE_META } from '../../domain/groups';
import { ROUTES } from '../../routes/paths';
import { PageHeader } from '../../components/layout/PageHeader';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import { PlusIcon } from '../../components/ui/icons';
import { AddSpeciesSheet } from './AddSpeciesSheet';
import styles from './AddPlantPage.module.css';

interface AddValues {
  specieId: string;
  groupIds: string[];
  code: string;
}

const schema = Yup.object({
  specieId: Yup.string().trim().required('Wybierz gatunek'),
});

const INITIAL: AddValues = { specieId: '', groupIds: [], code: '' };

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
          Wybierz gatunek — kod zostanie zaproponowany automatycznie. Kod jest trwałą etykietą
          doniczki i musi być unikalny w całym ogrodzie.
        </p>
      </div>

      <Formik<AddValues>
        initialValues={INITIAL}
        validationSchema={schema}
        onSubmit={async (values, helpers) => {
          const code = values.code.trim();
          if (!code) {
            helpers.setFieldError('code', 'Podaj kod');
            return;
          }
          if (codeTaken(garden.plants, code)) {
            helpers.setFieldError('code', 'Ten kod jest już zajęty');
            flash('⚠️ Ten kod jest już zajęty');
            return;
          }

          try {
            await garden.addPlant({ specieId: values.specieId, code, groupIds: values.groupIds });
            navigate(ROUTES.plants);
          } catch (error) {
            flash(error instanceof ApiError ? error.userMessage : 'Nie udało się dodać rośliny.');
          }
        }}
      >
        <Form>
          <SpeciesPicker />
          <TextField
            name="code"
            label="Kod"
            requiredMark
            placeholder="np. PAP-01"
            hint="Kod jest trwałą etykietą doniczki — nie da się go później zmienić."
          />
          <GroupPicker />
          <SubmitButton />
        </Form>
      </Formik>
    </div>
  );
};

const SpeciesPicker = () => {
  const { species, plants, addSpecies } = useGarden();
  const { values, setFieldValue, errors, touched } = useFormikContext<AddValues>();
  const [addingSpecies, setAddingSpecies] = useState(false);

  const pick = (id: string, name: string) => {
    void setFieldValue('specieId', id);
    if (!values.code.trim()) void setFieldValue('code', suggestCode(plants, name));
  };

  return (
    <div className={styles.groupBlock}>
      <span className={styles.label}>
        Gatunek <span className={styles.required}>•</span>
      </span>
      <div className={styles.groupChips}>
        {species.map((s) => (
          <button
            key={s.id}
            type="button"
            className={`${styles.groupChip} ${values.specieId === s.id ? styles.groupChipActive : ''}`}
            onClick={() => pick(s.id, s.name)}
          >
            {s.name}
          </button>
        ))}
        <button type="button" className={styles.groupChip} onClick={() => setAddingSpecies(true)}>
          <PlusIcon size={14} /> Nowy gatunek
        </button>
      </div>
      {touched.specieId && errors.specieId && <p className={styles.error}>{errors.specieId}</p>}

      <AddSpeciesSheet
        open={addingSpecies}
        onClose={() => setAddingSpecies(false)}
        existingNames={species.map((s) => s.name)}
        onCreate={async (name, w, f) => {
          await addSpecies({ name, w, f });
          setAddingSpecies(false);
        }}
      />
    </div>
  );
};

const GroupPicker = () => {
  const { groups } = useGarden();
  const { values, setFieldValue } = useFormikContext<AddValues>();

  const toggle = (id: string) => {
    const next = values.groupIds.includes(id)
      ? values.groupIds.filter((g) => g !== id)
      : [...values.groupIds, id];
    void setFieldValue('groupIds', next);
  };

  return (
    <div className={styles.groupBlock}>
      <span className={styles.label}>
        Dodaj do regionów / grup <span className={styles.optional}>opcjonalnie</span>
      </span>
      <div className={styles.groupChips}>
        {groups.map((g) => {
          const on = values.groupIds.includes(g.id);
          return (
            <button
              key={g.id}
              type="button"
              className={`${styles.groupChip} ${on ? styles.groupChipActive : ''}`}
              onClick={() => toggle(g.id)}
            >
              {GROUP_TYPE_META[g.type].emoji} {g.name}
            </button>
          );
        })}
      </div>
    </div>
  );
};

const SubmitButton = () => {
  const { values, isSubmitting } = useFormikContext<AddValues>();

  return (
    <Button
      type="submit"
      block
      disabled={isSubmitting || !values.specieId}
      className={styles.save}
    >
      {isSubmitting ? 'Dodawanie…' : 'Dodaj roślinę'}
    </Button>
  );
};
