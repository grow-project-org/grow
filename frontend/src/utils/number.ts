/** Parse a user-typed decimal that may use a comma separator. */
export const parseDecimal = (raw: string): number | null => {
  const trimmed = raw.trim();
  if (!trimmed) return null;
  const n = Number(trimmed.replace(',', '.'));
  return Number.isFinite(n) ? n : null;
};

/** Valid when empty, or a positive number (comma or dot separator). */
export const isValidOptionalDecimal = (raw?: string): boolean => {
  if (!raw || !raw.trim()) return true;
  const n = Number(raw.trim().replace(',', '.'));
  return Number.isFinite(n) && n > 0;
};

/** Clamp a value into an inclusive range. */
export const clamp = (value: number, min: number, max: number): number =>
  Math.min(max, Math.max(min, value));
