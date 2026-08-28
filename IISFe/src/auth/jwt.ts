import type { Role } from '../types/models';

export interface DecodedAccessToken {
  sub: string;
  userId: string;
  unique_name: string;
  role: Role;
  exp: number;
}

export function parseJwt(token: string): DecodedAccessToken | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json) as DecodedAccessToken;
  } catch {
    return null;
  }
}

const EARLY_EXPIRY_BUFFER_SECONDS = 30;

export function isTokenExpired(token: string): boolean {
  const decoded = parseJwt(token);
  if (!decoded) return true;
  return (decoded.exp - EARLY_EXPIRY_BUFFER_SECONDS) * 1000 < Date.now();
}
