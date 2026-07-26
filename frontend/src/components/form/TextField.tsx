import { useField } from 'formik';
import type { HTMLInputTypeAttribute } from 'react';
import styles from './form.module.css';

interface TextFieldProps {
  name: string;
  label: string;
  placeholder?: string;
  /** Shows a subtle "opcjonalnie" marker; requiredMark shows a green dot. */
  optional?: boolean;
  requiredMark?: boolean;
  hint?: string;
  type?: HTMLInputTypeAttribute;
  inputMode?: 'text' | 'numeric' | 'decimal';
}

/** Formik-bound labelled text input with inline validation error. */
export const TextField = ({
  name,
  label,
  placeholder,
  optional = false,
  requiredMark = false,
  hint,
  type = 'text',
  inputMode = 'text',
}: TextFieldProps) => {
  const [field, meta] = useField(name);
  const showError = meta.touched && !!meta.error;
  const showLabel = !!label || requiredMark || optional;

  return (
    <div className={styles.field}>
      {showLabel && (
        <label className={styles.label} htmlFor={name}>
          {label}{' '}
          {requiredMark && <span className={styles.required}>•</span>}
          {optional && <span className={styles.optional}>opcjonalnie</span>}
        </label>
      )}
      {hint && <p className={styles.hint}>{hint}</p>}
      <input
        id={name}
        className={`${styles.input} ${showError ? styles.inputError : ''}`}
        type={type}
        inputMode={inputMode}
        placeholder={placeholder}
        {...field}
        value={field.value ?? ''}
      />
      {showError && <p className={styles.error}>{meta.error}</p>}
    </div>
  );
};
