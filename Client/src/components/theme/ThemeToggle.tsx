import { Monitor, Moon, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { useTranslation } from 'react-i18next';

import { Button } from '@/components/ui/button';

/** Light -> Dark -> System, so the OS-following mode stays reachable. */
const themeCycle = ['light', 'dark', 'system'] as const;

const themeIcons = {
  light: Sun,
  dark: Moon,
  system: Monitor,
};

export function ThemeToggle() {
  const { theme = 'system', setTheme } = useTheme();
  const { t } = useTranslation();

  const currentTheme = (
    themeCycle.includes(theme as (typeof themeCycle)[number]) ? theme : 'system'
  ) as (typeof themeCycle)[number];

  const nextTheme =
    themeCycle[(themeCycle.indexOf(currentTheme) + 1) % themeCycle.length];

  const Icon = themeIcons[currentTheme];

  return (
    <Button
      variant='ghost'
      size='icon'
      onClick={() => setTheme(nextTheme)}
      aria-label={t('THEME_SWITCH_TO', {
        theme: t(`THEME_${nextTheme.toUpperCase()}`),
      })}
      title={t(`THEME_${currentTheme.toUpperCase()}`)}
    >
      <Icon />
    </Button>
  );
}
