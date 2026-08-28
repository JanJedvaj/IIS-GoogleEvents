import { apiClient } from './client';

export interface GraphQLError {
  message: string;
  path?: (string | number)[];
  extensions?: { code?: string };
}

export interface GraphQLResponse<T = unknown> {
  data: T | null;
  errors?: GraphQLError[];
}

export async function runGraphQL<T = unknown>(
  query: string,
  variables?: Record<string, unknown>,
): Promise<GraphQLResponse<T>> {
  const response = await apiClient.post<GraphQLResponse<T>>('/graphql', { query, variables });
  return response.data;
}
