import { LayoutGrid, type LucideIcon } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { EmptyState } from '@/components/common/EmptyState';
import { PageHeader } from '@/components/common/PageHeader';

interface ModulePlaceholderPageProps {
  title: string;
  icon?: LucideIcon;
}

/** Shell-consistent page for modules that have not been built yet. */
export function ModulePlaceholderPage({
  title,
  icon = LayoutGrid,
}: ModulePlaceholderPageProps) {
  const { t } = useTranslation();

  return (
    <div className='mx-auto flex w-full max-w-[100rem] flex-col gap-5'>
      <PageHeader title={title} description={t('MODULE_PENDING_DESCRIPTION')} />

      <EmptyState
        icon={icon}
        title={t('MODULE_PENDING_TITLE', { module: title })}
        description={t('MODULE_PENDING_HINT')}
      />
    </div>
  );
}
