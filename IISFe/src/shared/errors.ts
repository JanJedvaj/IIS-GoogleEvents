import { AxiosError } from 'axios';
import type { StandardResponse } from '../types/models';

export function extractErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as StandardResponse<unknown> | undefined;
    if (data?.errors && data.errors.length > 0) return data.errors.join(' ');
    if (data?.message) return data.message;
  }
  if (error instanceof Error && error.message) return error.message;
  return fallback;
}
