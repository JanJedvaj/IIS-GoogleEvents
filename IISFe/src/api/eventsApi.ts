import { apiClient } from './client';
import type {
  CalendarEventDto,
  CreateCalendarEventDto,
  StandardResponse,
  UpdateCalendarEventDto,
} from '../types/models';

export async function searchEvents(query?: string): Promise<StandardResponse<CalendarEventDto[]>> {
  const response = await apiClient.get<StandardResponse<CalendarEventDto[]>>('/api/events', {
    params: query ? { query } : undefined,
  });
  return response.data;
}

export async function getEvent(id: string): Promise<StandardResponse<CalendarEventDto>> {
  const response = await apiClient.get<StandardResponse<CalendarEventDto>>(
    `/api/events/${encodeURIComponent(id)}`,
  );
  return response.data;
}

export async function createEvent(
  request: CreateCalendarEventDto,
): Promise<StandardResponse<CalendarEventDto>> {
  const response = await apiClient.post<StandardResponse<CalendarEventDto>>(
    '/api/events',
    request,
  );
  return response.data;
}

export async function updateEvent(
  id: string,
  request: UpdateCalendarEventDto,
): Promise<StandardResponse<CalendarEventDto>> {
  const response = await apiClient.put<StandardResponse<CalendarEventDto>>(
    `/api/events/${encodeURIComponent(id)}`,
    request,
  );
  return response.data;
}

export async function deleteEvent(id: string): Promise<StandardResponse<boolean>> {
  const response = await apiClient.delete<StandardResponse<boolean>>(
    `/api/events/${encodeURIComponent(id)}`,
  );
  return response.data;
}
