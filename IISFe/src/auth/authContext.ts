import { createContext } from 'react';
import type { LoginRequestDto, RegisterRequestDto, Role } from '../types/models';

export interface AuthContextValue {
  isBootstrapping: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  username: string | null;
  role: Role | null;
  login: (request: LoginRequestDto) => Promise<void>;
  register: (request: RegisterRequestDto) => Promise<void>;
  signOut: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
