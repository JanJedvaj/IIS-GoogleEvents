import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import { getWeatherByCity } from '../../api/weatherApi';
import { DataGrid, type DataGridColumn } from '../../shared/DataGrid';
import { extractErrorMessage } from '../../shared/errors';
import type { CityWeatherDto } from '../../types/models';

const schema = z.object({
  cityName: z.string().min(1, 'City name is required'),
});

type FormValues = z.infer<typeof schema>;

const columns: DataGridColumn<CityWeatherDto>[] = [
  { header: 'City', render: (row) => row.cityName },
  { header: 'Temperature', render: (row) => row.temperature },
  { header: 'Humidity', render: (row) => row.humidity },
  { header: 'Pressure', render: (row) => row.pressure },
  { header: 'Wind direction', render: (row) => row.windDirection },
  { header: 'Wind speed', render: (row) => row.windSpeed },
  { header: 'Condition', render: (row) => row.condition },
];

export function WeatherPage() {
  const [searchError, setSearchError] = useState<string | null>(null);
  const [submittedCity, setSubmittedCity] = useState<string | null>(null);

  const weatherMutation = useMutation({
    mutationFn: (cityName: string) => getWeatherByCity(cityName),
    onError: (err: unknown) => setSearchError(extractErrorMessage(err, 'Failed to fetch weather.')),
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { cityName: '' } });

  const onSubmit = (values: FormValues) => {
    setSearchError(null);
    setSubmittedCity(values.cityName);
    weatherMutation.mutate(values.cityName);
  };

  const readings = weatherMutation.data ?? null;
  const results = readings?.results ?? [];

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Weather</Typography>
      <Typography variant="body2" color="text.secondary">
        Live DHMZ readings, fetched over gRPC-Web from the backend&apos;s gRPC service. A partial
        city name matches every station whose name contains it.
      </Typography>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
            <Stack direction="row" spacing={2} sx={{ alignItems: 'flex-start' }}>
              <TextField
                label="City name"
                size="small"
                error={!!errors.cityName}
                helperText={errors.cityName?.message}
                {...register('cityName')}
              />
              <Button type="submit" variant="contained" disabled={weatherMutation.isPending}>
                Search
              </Button>
            </Stack>
          </Box>

          {searchError && <Alert severity="error">{searchError}</Alert>}

          {results.length > 1 && (
            <Alert severity="info">
              {results.length} stations matched &quot;{submittedCity}&quot;.
            </Alert>
          )}

          {readings?.lastUpdated && (
            <Typography variant="body2" color="text.secondary">
              DHMZ feed last updated: {readings.lastUpdated}
            </Typography>
          )}

          {weatherMutation.isSuccess || weatherMutation.isPending ? (
            <DataGrid
              columns={columns}
              data={results}
              rowKey={(row) => row.cityName}
              loading={weatherMutation.isPending}
              emptyMessage={`No stations matched "${submittedCity}".`}
            />
          ) : null}
        </Stack>
      </Paper>
    </Stack>
  );
}
