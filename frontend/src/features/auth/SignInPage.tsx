import { useState } from 'react';
import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { TextField } from '../../components/form/TextField';
import { Button } from '../../components/ui/Button';
import { useAuth } from '../../state/AuthContext';
import { ApiError } from '../../api/client';
import styles from './SignInPage.module.css';

interface SignInValues {
  email: string;
  username: string;
}

const schema = Yup.object({
  email: Yup.string().trim().email('Podaj poprawny adres e-mail').required('Podaj e-mail'),
  username: Yup.string().trim().min(3, 'Minimum 3 znaki').required('Podaj nazwę użytkownika'),
});

export const SignInPage = () => {
  const { signIn } = useAuth();
  const [error, setError] = useState<string | null>(null);

  return (
    <div className={styles.wrap}>
      <div className={styles.logo}>🌱</div>
      <h1 className={styles.title}>Grow</h1>
      <p className={styles.desc}>
        Zaloguj się, aby zobaczyć swój ogród. Jeśli nie masz jeszcze konta, zostanie utworzone
        automatycznie.
      </p>

      <Formik<SignInValues>
        initialValues={{ email: '', username: '' }}
        validationSchema={schema}
        onSubmit={async (values, helpers) => {
          setError(null);
          try {
            await signIn(values.email.trim(), values.username.trim());
          } catch (cause) {
            setError(
              cause instanceof ApiError ? cause.userMessage : 'Nie udało się zalogować.',
            );
          } finally {
            helpers.setSubmitting(false);
          }
        }}
      >
        {({ isSubmitting }) => (
          <Form>
            <TextField
              name="email"
              label="E-mail"
              requiredMark
              type="email"
              placeholder="ty@example.com"
            />
            <TextField name="username" label="Nazwa użytkownika" requiredMark placeholder="np. Kasia" />
            <div className={styles.actions}>
              <Button type="submit" block disabled={isSubmitting}>
                {isSubmitting ? 'Logowanie…' : 'Zaloguj / utwórz konto'}
              </Button>
            </div>
          </Form>
        )}
      </Formik>

      {error && <p className={styles.error}>{error}</p>}
    </div>
  );
};
