import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import {
  generateEventsXml,
  getEventCount,
  searchEvents,
  validateGeneratedXml,
} from '../../api/soapApi';
import { getCapabilities } from '../../api/eventsApi';
import { DataGrid } from '../../shared/DataGrid';
import { useAuth } from '../../auth/useAuth';
import { extractErrorMessage } from '../../shared/errors';
import { formatDate } from '../../shared/dateUtils';
import type { EventSearchResultDto } from '../../types/models';

const schema = z.object({
  searchTerm: z.string().min(1, 'Search term is required'),
});

type FormValues = z.infer<typeof schema>;

export function SoapPage() {
  const { isAdmin } = useAuth();
  const [generateError, setGenerateError] = useState<string | null>(null);
  const [searchError, setSearchError] = useState<string | null>(null);
  const [countError, setCountError] = useState<string | null>(null);
  const [validateError, setValidateError] = useState<string | null>(null);

  const capabilitiesQuery = useQuery({
    queryKey: ['capabilities'],
    queryFn: () => getCapabilities(),
  });
  const source = capabilitiesQuery.data?.data?.source;

  const generateMutation = useMutation({
    mutationFn: () => generateEventsXml(),
    onError: (err: unknown) => setGenerateError(extractErrorMessage(err, 'Failed to generate XML.')),
  });

  const searchMutation = useMutation({
    mutationFn: (term: string) => searchEvents(term),
    onError: (err: unknown) => setSearchError(extractErrorMessage(err, 'Search failed.')),
  });

  const countMutation = useMutation({
    mutationFn: () => getEventCount(),
    onError: (err: unknown) => setCountError(extractErrorMessage(err, 'Failed to get event count.')),
  });

  const validateMutation = useMutation({
    mutationFn: () => validateGeneratedXml(),
    onError: (err: unknown) => setValidateError(extractErrorMessage(err, 'Validation failed.')),
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { searchTerm: '' } });

  const onSearchSubmit = (values: FormValues) => {
    setSearchError(null);
    searchMutation.mutate(values.searchTerm);
  };

  const generateResult = generateMutation.data?.data ?? null;
  const searchResult = searchMutation.data ?? null;
  const validateResult = validateMutation.data ?? null;

  return (
    <Stack spacing={2}>
      <Typography variant="h5">SOAP</Typography>

      {isAdmin && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1">Generate XML</Typography>
            <Typography variant="body2" color="text.secondary">
              Generating the XML file always reads from the local database regardless of the
              active data source{source ? ` (currently ${source}).` : '.'}
            </Typography>
            <Box>
              <Button
                variant="contained"
                onClick={() => {
                  setGenerateError(null);
                  generateMutation.mutate();
                }}
                disabled={generateMutation.isPending}
              >
                Generate XML
              </Button>
            </Box>
            {generateError && <Alert severity="error">{generateError}</Alert>}
            {generateResult && (
              <Alert severity="success">
                Generated {generateResult.eventCount} event
                {generateResult.eventCount === 1 ? '' : 's'} to {generateResult.filePath}.
              </Alert>
            )}
          </Stack>
        </Paper>
      )}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Typography variant="subtitle1">Search events</Typography>
          <Box component="form" onSubmit={handleSubmit(onSearchSubmit)} noValidate>
            <Stack direction="row" spacing={2} sx={{ alignItems: 'flex-start' }}>
              <TextField
                label="Search term"
                size="small"
                error={!!errors.searchTerm}
                helperText={errors.searchTerm?.message}
                {...register('searchTerm')}
              />
              <Button type="submit" variant="contained" disabled={searchMutation.isPending}>
                Search
              </Button>
            </Stack>
          </Box>
          {searchError && <Alert severity="error">{searchError}</Alert>}
          {searchResult && (
            <Stack spacing={1}>
              <Typography variant="body2">Total found: {searchResult.totalFound}</Typography>
              {searchResult.message && <Alert severity="info">{searchResult.message}</Alert>}
              <DataGrid<EventSearchResultDto>
                columns={[
                  { header: 'Summary', render: (event) => event.summary },
                  { header: 'Description', render: (event) => event.description ?? '-' },
                  { header: 'Location', render: (event) => event.location ?? '-' },
                  { header: 'Start', render: (event) => formatDate(event.start, false) },
                  { header: 'End', render: (event) => formatDate(event.end, false) },
                  { header: 'Status', render: (event) => event.status },
                  { header: 'Matched in', render: (event) => event.matchedIn },
                ]}
                data={searchResult.results}
                rowKey={(event) => event.googleEventId}
                loading={searchMutation.isPending}
                emptyMessage="No matching events."
              />
            </Stack>
          )}
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Typography variant="subtitle1">Event count</Typography>
          <Box>
            <Button
              variant="outlined"
              onClick={() => {
                setCountError(null);
                countMutation.mutate();
              }}
              disabled={countMutation.isPending}
            >
              Count
            </Button>
          </Box>
          {countError && <Alert severity="error">{countError}</Alert>}
          {countMutation.data !== undefined && (
            <Typography>Event count: {countMutation.data}</Typography>
          )}
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Typography variant="subtitle1">Validate generated XML</Typography>
          <Box>
            <Button
              variant="outlined"
              onClick={() => {
                setValidateError(null);
                validateMutation.mutate();
              }}
              disabled={validateMutation.isPending}
            >
              Validate
            </Button>
          </Box>
          {validateError && <Alert severity="error">{validateError}</Alert>}
          {validateResult &&
            (validateResult.isValid ? (
              <Alert severity="success">
                {validateResult.message ?? 'The generated XML file is valid.'}
              </Alert>
            ) : (
              <Alert severity="error">
                {validateResult.message && (
                  <Typography variant="subtitle2">{validateResult.message}</Typography>
                )}
                {validateResult.errors.map((message, index) => (
                  <Typography variant="body2" key={index}>
                    {message}
                  </Typography>
                ))}
              </Alert>
            ))}
        </Stack>
      </Paper>
    </Stack>
  );
}
