import { createContext } from 'react';

interface ErrorContextType {
  error: string | null;
  showError: (message: string) => void;
  hideError: () => void;
}

export const ErrorContext = createContext<ErrorContextType | null>(null);
