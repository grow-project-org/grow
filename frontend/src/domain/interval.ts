const TIME_SPAN = /^(-)?(?:(\d+)\.)?(\d{2}):(\d{2}):(\d{2})/;

export const parseIntervalDays = (timeSpan: string | undefined): number | null => {
  if (!timeSpan) return null;

  const match = TIME_SPAN.exec(timeSpan);
  if (!match) return null;

  const [, negative, days, hours, minutes, seconds] = match;
  const totalSeconds =
    Number(days ?? 0) * 86400 + Number(hours) * 3600 + Number(minutes) * 60 + Number(seconds);
  const value = Math.round(totalSeconds / 86400);

  return negative ? -value : value;
};

export const formatIntervalDays = (days: number): string =>
  `${Math.max(0, Math.round(days))}.00:00:00`;
