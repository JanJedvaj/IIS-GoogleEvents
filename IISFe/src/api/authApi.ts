import { apiClient } from './client';
import type {
  AuthResponseDto,
  LoginRequestDto,
  RegisterRequestDto,
  StandardResponse,
} from '../types/models';

export async function login(
  request: LoginRequestDto,
): Promise<StandardResponse<AuthResponseDto>> {
  const response = await apiClient.post<StandardResponse<AuthResponseDto>>(
    '/api/auth/login',
    request,
  );
  return response.data;
}

export async function register(
  request: RegisterRequestDto,
): Promise<StandardResponse<AuthResponseDto>> {
  const response = await apiClient.post<StandardResponse<AuthResponseDto>>(
    '/api/auth/register',
    request,
  );
  return response.data;
}

export async function refresh(): Promise<StandardResponse<AuthResponseDto>> {
  const response = await apiClient.post<StandardResponse<AuthResponseDto>>('/api/auth/refresh');
  return response.data;
}

export async function signOut(): Promise<StandardResponse<boolean>> {
  const response = await apiClient.post<StandardResponse<boolean>>('/api/auth/signout');
  return response.data;
}
