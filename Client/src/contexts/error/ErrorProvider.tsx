import { useState, type ReactNode } from 'react';
import { ErrorContext } from './ErrorContext';

interface ErrorProviderProps {
  children: ReactNode;
}

export function ErrorProvider({ children }: ErrorProviderProps) {
  const [error, setError] = useState<string | null>(null);

  const showError = (message: string) => {
    setError(message);
  };

  const hideError = () => {
    setError(null);
  };

  return (
    <ErrorContext.Provider
      value={{
        error,
        showError,
        hideError,
      }}
    >
      {children}

      {error && (
        <div>
          <p>{error}</p>
          <button onClick={hideError}>Close</button>
        </div>
      )}
    </ErrorContext.Provider>
  );
}
