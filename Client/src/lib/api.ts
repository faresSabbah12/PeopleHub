// const API_BASE = 'https://localhost:7059/api';

// export async function apiGet<T>(endpoint: string): Promise<T> {
//   const response = await fetch(`${API_BASE}${endpoint}`);

//   if (!response.ok) {
//     throw new Error(`Request failed: ${response.status}`);
//   }

//   return response.json();
// }

const API_BASE = 'https://localhost:7059/api';

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface ApiRequestOptions<TBody = unknown> {
  method: HttpMethod;
  params?: Record<string, string | number | boolean | null | undefined>;
  body?: TBody;
  headers?: HeadersInit;
}

export async function apiRequest<TResponse, TBody = unknown>(
  endpoint: string,
  options: ApiRequestOptions<TBody>,
): Promise<TResponse> {
  const { method, params, body, headers } = options;

  const url = new URL(`${API_BASE}${endpoint}`);

  // Add query param
  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        url.searchParams.append(key, String(value));
      }
    });
  }

  const response = await fetch(url.toString(), {
    method,

    headers: {
      'Content-Type': 'application/json',
      ...headers,
    },

    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  // DELETE sometimes returns 204 No Content
  if (response.status === 204) {
    return undefined as TResponse;
  }

  return response.json() as Promise<TResponse>;
}
