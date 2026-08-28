import { useEffect, useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { createEvent, getEvent, updateEvent } from '../../api/eventsApi';
import { extractErrorMessage } from '../../shared/errors';
import { fromDatetimeLocalValue, toDatetimeLocalValue } from '../../shared/dateUtils';

const schema = z.object({
  summary: z.string().min(1, 'Summary is required'),
  description: z.string(),
  location: z.string(),
  start: z.string().min(1, 'Start is required'),
  end: z.string().min(1, 'End is required'),
  isAllDay: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

const emptyDefaults: FormValues = {
  summary: '',
  description: '',
  location: '',
  start: '',
  end: '',
  isAllDay: false,
};

export function EventFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = id !== undefined;
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const { data: existing, isLoading } = useQuery({
    queryKey: ['events', id],
    queryFn: () => getEvent(id!),
    enabled: isEdit,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: emptyDefaults });

  useEffect(() => {
    const event = existing?.data;
    if (!event) return;
    reset({
      summary: event.summary,
      description: event.description ?? '',
      location: event.location ?? '',
      start: toDatetimeLocalValue(event.start),
      end: toDatetimeLocalValue(event.end),
      isAllDay: event.isAllDay,
    });
  }, [existing, reset]);

  const mutation = useMutation({
    mutationFn: (values: FormValues) => {
      const payload = {
        summary: values.summary,
        description: values.description || null,
        location: values.location || null,
        start: fromDatetimeLocalValue(values.start),
        end: fromDatetimeLocalValue(values.end),
        isAllDay: values.isAllDay,
      };
      return isEdit ? updateEvent(id!, payload) : createEvent(payload);
    },
    onSuccess: (response) => {
      if (!response.success) {
        setError(response.errors?.join(' ') ?? response.message ?? 'Save failed.');
        return;
      }
      queryClient.invalidateQueries({ queryKey: ['events'] });
      navigate('/events');
    },
    onError: (err: unknown) => setError(extractErrorMessage(err, 'Save failed.')),
  });

  const onSubmit = (values: FormValues) => {
    setError(null);
    mutation.mutate(values);
  };

  if (isEdit && isLoading) return null;

  return (
    <Paper sx={{ p: 3, maxWidth: 480 }}>
      <Typography variant="h5" sx={{ mb: 2 }}>
        {isEdit ? 'Edit event' : 'Create event'}
      </Typography>
      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Stack spacing={2}>
          {error && <Alert severity="error">{error}</Alert>}
          <TextField
            label="Summary"
            autoFocus
            error={!!errors.summary}
            helperText={errors.summary?.message}
            {...register('summary')}
          />
          <TextField label="Description" multiline minRows={2} {...register('description')} />
          <TextField label="Location" {...register('location')} />
          <TextField
            label="Start"
            type="datetime-local"
            slotProps={{ inputLabel: { shrink: true } }}
            error={!!errors.start}
            helperText={errors.start?.message}
            {...register('start')}
          />
          <TextField
            label="End"
            type="datetime-local"
            slotProps={{ inputLabel: { shrink: true } }}
            error={!!errors.end}
            helperText={errors.end?.message}
            {...register('end')}
          />
          <FormControlLabel control={<Checkbox {...register('isAllDay')} />} label="All day" />
          <Stack direction="row" spacing={2}>
            <Button type="submit" variant="contained" disabled={isSubmitting || mutation.isPending}>
              Save
            </Button>
            <Button variant="text" onClick={() => navigate('/events')}>
              Cancel
            </Button>
          </Stack>
        </Stack>
      </Box>
    </Paper>
  );
}
