import { useEffect, useMemo, useState, type ReactNode } from 'react';
import * as authApi from '../api/authApi';
import { refreshAccessToken } from '../api/client';
import { getAccessToken, setAccessToken, subscribeToAccessToken } from './tokenStore';
import { parseJwt } from './jwt';
import { AuthContext, type AuthContextValue } from './authContext';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(getAccessToken());
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  useEffect(() => subscribeToAccessToken(setToken), []);

  useEffect(() => {
    refreshAccessToken().finally(() => setIsBootstrapping(false));
  }, []);

  const decoded = useMemo(() => (token ? parseJwt(token) : null), [token]);

  const value = useMemo<AuthContextValue>(
    () => ({
      isBootstrapping,
      isAuthenticated: token !== null,
      isAdmin: decoded?.role === 'Admin',
      username: decoded?.unique_name ?? null,
      role: decoded?.role ?? null,
      login: async (request) => {
        const response = await authApi.login(request);
        setAccessToken(response.data?.accessToken ?? null);
      },
      register: async (request) => {
        const response = await authApi.register(request);
        setAccessToken(response.data?.accessToken ?? null);
      },
      signOut: async () => {
        setAccessToken(null);
        await authApi.signOut().catch(() => undefined);
      },
    }),
    [token, decoded, isBootstrapping],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
