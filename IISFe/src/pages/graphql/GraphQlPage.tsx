import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import { runGraphQL } from '../../api/graphqlApi';
import { useAuth } from '../../auth/useAuth';

interface Preset {
  label: string;
  query: string;
  adminOnly: boolean;
}

const PRESETS: Preset[] = [
  {
    label: 'List events',
    query: '{ events { googleEventId summary start end status } }',
    adminOnly: false,
  },
  {
    label: 'Get event',
    query:
      '{ event(id: "REPLACE_ME") { googleEventId summary description location start end isAllDay status htmlLink } }',
    adminOnly: false,
  },
  {
    label: 'Capabilities',
    query: '{ capabilities { source softDeletes } }',
    adminOnly: false,
  },
  {
    label: 'Create event',
    query:
      'mutation { createEvent(input: { summary: "New event", description: null, location: null, start: "2026-09-01T10:00:00Z", end: "2026-09-01T11:00:00Z", isAllDay: false }) { googleEventId summary } }',
    adminOnly: true,
  },
  {
    label: 'Update event',
    query:
      'mutation { updateEvent(id: "REPLACE_ME", input: { summary: "Updated summary", description: null, location: null, start: null, end: null, isAllDay: null }) { googleEventId summary } }',
    adminOnly: true,
  },
  {
    label: 'Delete event',
    query: 'mutation { deleteEvent(id: "REPLACE_ME") }',
    adminOnly: true,
  },
];

export function GraphQlPage() {
  const { isAdmin } = useAuth();
  const [queryText, setQueryText] = useState(PRESETS[0].query);

  const runMutation = useMutation({
    mutationFn: (query: string) => runGraphQL(query),
  });

  const result = runMutation.data;

  return (
    <Stack spacing={2}>
      <Typography variant="h5">GraphQL</Typography>
      <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
        {PRESETS.filter((preset) => !preset.adminOnly || isAdmin).map((preset) => (
          <Button
            key={preset.label}
            variant="outlined"
            size="small"
            onClick={() => setQueryText(preset.query)}
          >
            {preset.label}
          </Button>
        ))}
      </Stack>
      <TextField
        label="Query"
        multiline
        minRows={8}
        value={queryText}
        onChange={(event) => setQueryText(event.target.value)}
        slotProps={{ input: { sx: { fontFamily: 'monospace', fontSize: 14 } } }}
      />
      <Box>
        <Button
          variant="contained"
          onClick={() => runMutation.mutate(queryText)}
          disabled={runMutation.isPending}
        >
          Run
        </Button>
      </Box>
      {runMutation.isError && (
        <Alert severity="error">Request failed. Check that the backend is running.</Alert>
      )}
      {result?.errors && result.errors.length > 0 && (
        <Alert severity="error">
          <Box component="pre" sx={{ m: 0, whiteSpace: 'pre-wrap' }}>
            {result.errors
              .map((error) => `${error.extensions?.code ?? 'ERROR'}: ${error.message}`)
              .join('\n')}
          </Box>
        </Alert>
      )}
      {result && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Box
            component="pre"
            sx={{ m: 0, whiteSpace: 'pre-wrap', fontFamily: 'monospace', fontSize: 13 }}
          >
            {JSON.stringify(result.data, null, 2)}
          </Box>
        </Paper>
      )}
    </Stack>
  );
}
