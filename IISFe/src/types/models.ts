export type ResultStatus =
  | 'Ok'
  | 'Created'
  | 'BadRequest'
  | 'NotFound'
  | 'Unauthorized'
  | 'Forbidden'
  | 'Conflict'
  | 'InternalError';

export interface StandardResponse<T> {
  success: boolean;
  data: T | null;
  status: ResultStatus;
  message: string | null;
  errors: string[] | null;
}

export type Role = 'User' | 'Admin';

export interface AuthResponseDto {
  accessToken: string;
  username: string;
  role: Role;
}

export interface LoginRequestDto {
  username: string;
  password: string;
}

export interface RegisterRequestDto {
  username: string;
  password: string;
}

export type EventStatus = 'confirmed' | 'tentative' | 'cancelled';

export interface CalendarEventDto {
  googleEventId: string;
  summary: string;
  description: string | null;
  location: string | null;
  start: string;
  end: string;
  isAllDay: boolean;
  status: EventStatus;
  htmlLink: string;
  created: string;
  updated: string;
}

export interface CreateCalendarEventDto {
  summary: string;
  description?: string | null;
  location?: string | null;
  start: string;
  end: string;
  isAllDay: boolean;
}

export interface UpdateCalendarEventDto {
  summary?: string | null;
  description?: string | null;
  location?: string | null;
  start?: string | null;
  end?: string | null;
  isAllDay?: boolean | null;
}

export type DataSource = 'Local' | 'External';

export interface CalendarCapabilitiesDto {
  source: DataSource;
  softDeletes: boolean;
}

export interface ImportResultDto {
  importedCount: number;
  xmlErrors: string[];
  jsonErrors: string[];
  businessErrors: string[];
}
