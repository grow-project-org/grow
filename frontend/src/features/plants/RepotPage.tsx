import { Form, Formik, useFormikContext } from 'formik';
import * as Yup from 'yup';
import { Navigate, useNavigate, useParams } from 'react-router-dom';
import type { Plant } from '../../types';
import { useGarden } from '../../state/GardenContext';
import { parseDecimal, isValidOptionalDecimal } from '../../utils/number';
import { ROUTES } from '../../routes/paths';
import { PageHeader } from '../../components/layout/PageHeader';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import { Avatar } from '../../components/ui/Avatar';
import { avatarBg } from '../../domain/species';
import styles from './RepotPage.module.css';

interface RepotValues {
  potL: string;
  potCm: string;
}

const schema = Yup.object({
  potL: Yup.string().test('l', 'Podaj poprawną objętość', isValidOptionalDecimal),
  potCm: Yup.string().test('cm', 'Podaj poprawną średnicę', isValidOptionalDecimal),
});

export const RepotPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { plantById, repot } = useGarden();

  const plant = plantById(Number(id));
  if (!plant) return <Navigate to={ROUTES.plants} replace />;

  const potLabel =
    plant.potL != null ? `${plant.potL} l` : plant.potCm != null ? `Ø ${plant.potCm} cm` : 'nie podano';

  return (
    <div className={styles.page}>
      <PageHeader title="Przesadzanie" />

      <div className={styles.summary}>
        <Avatar emoji={plant.emoji} bg={avatarBg(plant.id)} size={56} radius={16} fontSize={28} />
        <div>
          <div className={styles.summaryName}>{plant.species ?? 'Roślina bez gatunku'} · {plant.code}</div>
          <div className={styles.summaryPot}>Obecnie: {potLabel}</div>
        </div>
      </div>

      <h2 className={styles.sectionLabel}>Nowy pojemnik</h2>
      <p className={styles.sectionHint}>Oba pola opcjonalne — podaj tyle, ile wiesz.</p>

      <Formik<RepotValues>
        initialValues={{ potL: '', potCm: '' }}
        validationSchema={schema}
        onSubmit={(values) => {
          repot(plant.id, parseDecimal(values.potL), parseDecimal(values.potCm), `🪴 Przesadzono ${plant.code}`);
          navigate(-1);
        }}
      >
        <Form>
          <div className={styles.row}>
            <div className={styles.col}>
              <TextField name="potL" label="Objętość (litry)" placeholder="np. 10" inputMode="decimal" />
            </div>
            <div className={styles.col}>
              <TextField name="potCm" label="Średnica (cm)" placeholder="np. 26" inputMode="decimal" />
            </div>
          </div>

          <RepotHint plant={plant} />

          <Button type="submit" block className={styles.save}>
            Zapisz przesadzenie
          </Button>
        </Form>
      </Formik>
    </div>
  );
};

const RepotHint = ({ plant }: { plant: Plant }) => {
  const { values } = useFormikContext<RepotValues>();
  const next = parseDecimal(values.potL);

  let hint =
    'Większy pojemnik zatrzymuje więcej wody → zwykle rzadsze podlewanie. Zaktualizujemy prognozy po zapisie.';
  if (next != null && plant.potL != null) {
    if (next > plant.potL) {
      hint = `Z ${plant.potL} l na ${next} l — więcej ziemi, woda wolniej wysycha. Odstępy podlewania mogą się wydłużyć.`;
    } else if (next < plant.potL) {
      hint = 'Mniejszy pojemnik szybciej wysycha — podlewanie może być częstsze.';
    }
  }

  return (
    <div className={styles.tip}>
      <span className={styles.tipIcon}>💡</span>
      <p className={styles.tipText}>{hint}</p>
    </div>
  );
};
