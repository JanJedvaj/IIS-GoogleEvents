import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from './useAuth';
import type { Role } from '../types/models';

const RANK: Record<Role, number> = {
  User: 100,
  Admin: 200,
};

interface RequireRoleProps {
  minRole: Role;
}

export function RequireRole({ minRole }: RequireRoleProps) {
  const { role } = useAuth();

  if (role === null || RANK[role] < RANK[minRole]) return <Navigate to="/events" replace />;
  return <Outlet />;
}
