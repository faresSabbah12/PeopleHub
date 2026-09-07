import { useState, type ReactNode } from 'react';
import { PageLoader } from '@/components/common/Loading';
import { LoadingContext } from './LoadingContext';

interface LoadingProviderProps {
  children: ReactNode;
}

export function LoadingProvider({ children }: LoadingProviderProps) {
  const [isLoading, setIsLoading] = useState(false);

  const startLoading = () => {
    setIsLoading(true);
  };

  const stopLoading = () => {
    setIsLoading(false);
  };

  return (
    <LoadingContext.Provider
      value={{
        startLoading,
        stopLoading,
      }}
    >
      {children}
      {isLoading && <PageLoader />}
    </LoadingContext.Provider>
  );
}
