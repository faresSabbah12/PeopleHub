import { createContext } from 'react';

interface LoadingContextType {
  startLoading: () => void;
  stopLoading: () => void;
}

export const LoadingContext = createContext<LoadingContextType | null>(null);
