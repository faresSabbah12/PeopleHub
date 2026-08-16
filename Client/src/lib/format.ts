/** Locale-aware formatting helpers built on Intl — no date library needed. */

export const formatRelativeMinutes = (minutes: number, locale: string) => {
  const formatter = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' });

  if (minutes < 60) {
    return formatter.format(-minutes, 'minute');
  }

  if (minutes < 60 * 24) {
    return formatter.format(-Math.round(minutes / 60), 'hour');
  }

  return formatter.format(-Math.round(minutes / (60 * 24)), 'day');
};

export const formatMediumDate = (isoDate: string, locale: string) =>
  new Intl.DateTimeFormat(locale, {
    day: 'numeric',
    month: 'short',
  }).format(new Date(isoDate));

export const formatNumber = (value: number, locale: string) =>
  new Intl.NumberFormat(locale).format(value);
