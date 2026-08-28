import axios, { type InternalAxiosRequestConfig } from 'axios';
import { getAccessToken, setAccessToken } from '../auth/tokenStore';
import { isTokenExpired } from '../auth/jwt';
import type { AuthResponseDto, StandardResponse } from '../types/models';

const AUTH_PATH_PREFIX = '/api/auth/';
const RETRY_HEADER = 'X-Auth-Retry';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  withCredentials: true,
});

const refreshClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  withCredentials: true,
});

let pendingRefresh: Promise<string | null> | null = null;

export async function refreshAccessToken(): Promise<string | null> {
  if (!pendingRefresh) {
    pendingRefresh = refreshClient
      .post<StandardResponse<AuthResponseDto>>('/api/auth/refresh')
      .then((response) => {
        const token = response.data.data?.accessToken ?? null;
        setAccessToken(token);
        return token;
      })
      .catch(() => {
        setAccessToken(null);
        return null;
      })
      .finally(() => {
        pendingRefresh = null;
      });
  }
  return pendingRefresh;
}

function isAuthRequest(config: InternalAxiosRequestConfig): boolean {
  return (config.url ?? '').includes(AUTH_PATH_PREFIX);
}

apiClient.interceptors.request.use(async (config) => {
  if (isAuthRequest(config)) return config;

  let token = getAccessToken();
  if (token && isTokenExpired(token)) {
    token = await refreshAccessToken();
  }
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const config = error.config as InternalAxiosRequestConfig | undefined;

    if (
      error.response?.status !== 401 ||
      !config ||
      isAuthRequest(config) ||
      config.headers?.get?.(RETRY_HEADER)
    ) {
      return Promise.reject(error);
    }

    const token = await refreshAccessToken();
    if (!token) {
      return Promise.reject(error);
    }

    config.headers.set(RETRY_HEADER, 'true');
    config.headers.set('Authorization', `Bearer ${token}`);
    return apiClient.request(config);
  },
);
