// src/hooks/useApi.ts

import { useError } from '@/contexts/error/useError';
import { useLoading } from '@/contexts/loading/useLoading';
import { apiRequest, type ApiRequestOptions } from '@/lib/api';
import { useEffect, useState, type DependencyList } from 'react';
import { useTranslation } from 'react-i18next';

export function useApi<TResponse, TBody = unknown>(
  endpoint: string,
  options: ApiRequestOptions<TBody>,
  deps: DependencyList,
) {
  const [response, setResponse] = useState<TResponse | null>(null);
  const { startLoading, stopLoading } = useLoading();
  const { showError } = useError();

  const { t } = useTranslation();

  const request = async (
    endpoint: string,
    options: ApiRequestOptions<TBody>,
  ) => {
    try {
      startLoading();

      const result = await apiRequest<TResponse, TBody>(endpoint, options);

      setResponse(result);
      return result;
    } catch (error) {
      const message =
        error instanceof Error ? error.message : t('SOMETHING_WENT_WRONG');

      showError(message);
    } finally {
      stopLoading();
    }
  };

  useEffect(() => {
    (async () => await request(endpoint, options))();
  }, deps);

  return {
    // mutate: request,
    response,
  };
}

export function useApiQuery<TResponse, TBody = unknown>(
  endpoint: string,
  options: ApiRequestOptions<TBody>,
  deps: DependencyList = [],
) {
  return useApi<TResponse, TBody>(endpoint, options, deps).response;
}
