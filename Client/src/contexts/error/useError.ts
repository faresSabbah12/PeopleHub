import { useContext } from 'react';
import { ErrorContext } from './errorContext';

export function useError() {
  const context = useContext(ErrorContext);

  if (!context) {
    throw new Error('useError must be used inside ErrorProvider');
  }

  return context;
}
