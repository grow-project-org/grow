import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { BottomSheet } from '../sheet/BottomSheet';
import { TextField } from '../form/TextField';
import { Button } from '../ui/Button';
import { useAuth } from '../../state/AuthContext';
import { useToast } from '../../state/ToastContext';
import sheet from '../sheet/sheetForm.module.css';

interface AccountSheetProps {
  open: boolean;
  onClose: () => void;
}

interface AccountValues {
  email: string;
  username: string;
}

/**
 * Minimal sign-in surface. The backend doesn't check passwords yet (see
 * `AuthContext`'s note), so this only collects an email + display name and
 * figures out register-vs-login itself.
 */
export const AccountSheet = ({ open, onClose }: AccountSheetProps) => {
  const { status, username, signIn } = useAuth();
  const { flash } = useToast();

  const schema = Yup.object({
    email: Yup.string().trim().email('Podaj poprawny adres e-mail').required('Podaj e-mail'),
    username: Yup.string().trim().required('Podaj nazwę użytkownika'),
  });

  return (
    <BottomSheet open={open} onClose={onClose}>
      <h2 className={sheet.title}>Konto</h2>

      {status === 'authenticated' ? (
        <>
          <p className={sheet.desc}>
            Zalogowano jako <strong>{username}</strong>. Zmiany zapisują się też na serwerze.
          </p>
          {/* TODO(backend): no /logout route is mapped (UsersEndpoints.Logout
             exists but MapUsersEndpoints never registers it) — nothing to
             call here yet. */}
          <div className={sheet.actions}>
            <Button variant="neutral" onClick={onClose} block>
              Zamknij
            </Button>
          </div>
        </>
      ) : (
        <>
          <p className={sheet.desc}>
            Zaloguj się, aby zapisywać zmiany też na serwerze. Aplikacja działa również bez logowania —
            dane zostają wtedy tylko na tym urządzeniu.
          </p>
          <Formik<AccountValues>
            initialValues={{ email: '', username: '' }}
            validationSchema={schema}
            onSubmit={async (values, helpers) => {
              try {
                await signIn(values.email.trim(), values.username.trim());
                helpers.resetForm();
                onClose();
              } catch {
                flash('Nie udało się zalogować. Spróbuj ponownie.');
              } finally {
                helpers.setSubmitting(false);
              }
            }}
          >
            <Form>
              <TextField name="email" label="E-mail" requiredMark type="email" placeholder="ty@example.com" />
              <TextField name="username" label="Nazwa użytkownika" requiredMark placeholder="np. Kasia" />
              <div className={sheet.actions}>
                <Button variant="neutral" onClick={onClose}>
                  Anuluj
                </Button>
                <Button type="submit" block>
                  Zaloguj / utwórz konto
                </Button>
              </div>
            </Form>
          </Formik>
        </>
      )}
    </BottomSheet>
  );
};
