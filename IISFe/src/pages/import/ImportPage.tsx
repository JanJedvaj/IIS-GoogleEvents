import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Button, Link, Paper, Stack, Typography } from '@mui/material';
import { importEvents } from '../../api/importApi';
import { getCapabilities } from '../../api/eventsApi';
import { extractErrorMessage } from '../../shared/errors';
import type { ImportResultDto, StandardResponse } from '../../types/models';

export function ImportPage() {
  const [xmlFile, setXmlFile] = useState<File | null>(null);
  const [jsonFile, setJsonFile] = useState<File | null>(null);

  const capabilitiesQuery = useQuery({
    queryKey: ['capabilities'],
    queryFn: () => getCapabilities(),
  });

  const importMutation = useMutation({
    mutationFn: () => importEvents(xmlFile as File, jsonFile as File),
  });

  const source = capabilitiesQuery.data?.data?.source;
  const result = importMutation.data?.data ?? null;

  const failedResponse = importMutation.error instanceof AxiosError
    ? (importMutation.error.response?.data as StandardResponse<ImportResultDto> | undefined)
    : undefined;
  const errorResult = failedResponse?.data ?? null;
  const generalError = importMutation.isError && !errorResult
    ? extractErrorMessage(importMutation.error, 'Import failed.')
    : null;

  const canUpload = xmlFile !== null && jsonFile !== null && !importMutation.isPending;

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Import</Typography>
      <Typography variant="body2" color="text.secondary">
        Import always writes to the local database regardless of the active data source
        {source ? ` (currently ${source}).` : '.'}
      </Typography>
      {source === 'External' && (
        <Alert severity="info">
          The active data source is External, so the events list reads from Google Calendar.
          Imported records are stored locally and will not appear there until the source is set
          back to Local.
        </Alert>
      )}
      <Stack direction="row" spacing={2} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
        <Button variant="outlined" component="label">
          {xmlFile ? xmlFile.name : 'Choose XML file'}
          <input
            type="file"
            accept=".xml"
            hidden
            onChange={(event) => setXmlFile(event.target.files?.[0] ?? null)}
          />
        </Button>
        <Button variant="outlined" component="label">
          {jsonFile ? jsonFile.name : 'Choose JSON file'}
          <input
            type="file"
            accept=".json"
            hidden
            onChange={(event) => setJsonFile(event.target.files?.[0] ?? null)}
          />
        </Button>
        <Button variant="contained" disabled={!canUpload} onClick={() => importMutation.mutate()}>
          Upload
        </Button>
      </Stack>
      {generalError && <Alert severity="error">{generalError}</Alert>}
      {errorResult && (
        <Stack spacing={2}>
          {errorResult.xmlErrors.length > 0 && (
            <Alert severity="error">
              <Typography variant="subtitle2">XML errors</Typography>
              {errorResult.xmlErrors.map((message, index) => (
                <Typography variant="body2" key={index}>
                  {message}
                </Typography>
              ))}
            </Alert>
          )}
          {errorResult.jsonErrors.length > 0 && (
            <Alert severity="error">
              <Typography variant="subtitle2">JSON errors</Typography>
              {errorResult.jsonErrors.map((message, index) => (
                <Typography variant="body2" key={index}>
                  {message}
                </Typography>
              ))}
            </Alert>
          )}
          {errorResult.businessErrors.length > 0 && (
            <Alert severity="error">
              <Typography variant="subtitle2">Business rules</Typography>
              {errorResult.businessErrors.map((message, index) => (
                <Typography variant="body2" key={index}>
                  {message}
                </Typography>
              ))}
            </Alert>
          )}
        </Stack>
      )}
      {result && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography>
            Imported {result.importedCount} event{result.importedCount === 1 ? '' : 's'}.{' '}
            <Link component={RouterLink} to="/events">
              View events
            </Link>
          </Typography>
        </Paper>
      )}
    </Stack>
  );
}
