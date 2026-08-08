import { SidebarProvider } from '@/components/ui/sidebar';

import { AppHeader } from './AppHeader';
import { AppSidebar } from './AppSidebar';

export function AppLayout() {
  return (
    <SidebarProvider>
      <AppSidebar />

      <div className='flex min-h-screen flex-1 flex-col'>
        <AppHeader />

        <main className='flex-1 bg-background p-6'>Main Content</main>
      </div>
    </SidebarProvider>
  );
}
