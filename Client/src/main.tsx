import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

import './i18n';
import './index.css';
import { ThemeProvider } from './components/theme/ThemeProvider';
import { LoadingProvider } from './contexts/loading/LoadingProvider';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <LoadingProvider>
      <ThemeProvider
        attribute='class'
        defaultTheme='system'
        enableSystem
        disableTransitionOnChange
      >
        <App />
      </ThemeProvider>
    </LoadingProvider>
  </React.StrictMode>,
);
