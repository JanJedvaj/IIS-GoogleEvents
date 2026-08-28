import { apiClient } from './client';
import type { ImportResultDto, StandardResponse } from '../types/models';

export async function importEvents(
  xmlFile: File,
  jsonFile: File,
): Promise<StandardResponse<ImportResultDto>> {
  const formData = new FormData();
  formData.append('xmlFile', xmlFile);
  formData.append('jsonFile', jsonFile);
  const response = await apiClient.post<StandardResponse<ImportResultDto>>(
    '/api/import',
    formData,
  );
  return response.data;
}
