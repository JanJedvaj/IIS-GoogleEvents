import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Alert, Button, Stack, TextField, Typography } from '@mui/material';
import { deleteEvent, searchEvents } from '../../api/eventsApi';
import { DataGrid } from '../../shared/DataGrid';
import { useAuth } from '../../auth/useAuth';
import { extractErrorMessage } from '../../shared/errors';
import { formatDate } from '../../shared/dateUtils';
import type { CalendarEventDto } from '../../types/models';

export function EventsPage() {
  const { isAdmin } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [query, setQuery] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [actionError, setActionError] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['events', searchTerm],
    queryFn: () => searchEvents(searchTerm || undefined),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteEvent(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
    onError: (err: unknown) => setActionError(extractErrorMessage(err, 'Failed to delete event.')),
  });

  const events = data?.data ?? [];

  const handleDelete = (event: CalendarEventDto) => {
    if (!window.confirm(`Delete "${event.summary}"?`)) return;
    setActionError(null);
    deleteMutation.mutate(event.googleEventId);
  };

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Events</Typography>
      {actionError && (
        <Alert severity="error" onClose={() => setActionError(null)}>
          {actionError}
        </Alert>
      )}
      {data && !data.success && (
        <Alert severity="error">{data.message ?? 'Failed to load events.'}</Alert>
      )}
      <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
        <TextField
          label="Search"
          size="small"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter') setSearchTerm(query);
          }}
        />
        <Button variant="outlined" onClick={() => setSearchTerm(query)}>
          Search
        </Button>
        {isAdmin && (
          <Button variant="contained" sx={{ ml: 'auto' }} onClick={() => navigate('/events/new')}>
            Create event
          </Button>
        )}
      </Stack>
      <DataGrid<CalendarEventDto>
        columns={[
          { header: 'Summary', render: (event) => event.summary },
          { header: 'Location', render: (event) => event.location ?? '-' },
          { header: 'Start', render: (event) => formatDate(event.start, event.isAllDay) },
          { header: 'End', render: (event) => formatDate(event.end, event.isAllDay) },
          { header: 'Status', render: (event) => event.status },
        ]}
        data={events}
        rowKey={(event) => event.googleEventId}
        loading={isLoading}
        emptyMessage="No events found."
        actions={[
          {
            label: 'Edit',
            visible: () => isAdmin,
            onClick: (event) =>
              navigate(`/events/${encodeURIComponent(event.googleEventId)}/edit`),
          },
          {
            label: 'Delete',
            color: 'error',
            visible: () => isAdmin,
            onClick: handleDelete,
          },
        ]}
      />
    </Stack>
  );
}
