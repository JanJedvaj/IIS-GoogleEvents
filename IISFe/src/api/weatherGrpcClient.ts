import { createClient, type Client, type Interceptor } from '@connectrpc/connect';
import { createGrpcWebTransport } from '@connectrpc/connect-web';
import { Code, ConnectError } from '@connectrpc/connect';
import { WeatherService } from '../gen/weather_pb';
import { getAccessToken } from '../auth/tokenStore';
import { isTokenExpired } from '../auth/jwt';
import { refreshAccessToken } from './client';

async function currentToken(): Promise<string | null> {
  const token = getAccessToken();
  if (token && isTokenExpired(token)) return refreshAccessToken();
  return token;
}

const authInterceptor: Interceptor = (next) => async (request) => {
  const token = await currentToken();
  if (token) request.header.set('Authorization', `Bearer ${token}`);

  try {
    return await next(request);
  } catch (error) {
    if (!(error instanceof ConnectError) || error.code !== Code.Unauthenticated) throw error;

    const refreshed = await refreshAccessToken();
    if (!refreshed) throw error;

    request.header.set('Authorization', `Bearer ${refreshed}`);
    return next(request);
  }
};

const transport = createGrpcWebTransport({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
  interceptors: [authInterceptor],
});

export const weatherClient: Client<typeof WeatherService> = createClient(
  WeatherService,
  transport,
);
