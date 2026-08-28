import type { ReactNode } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';

export interface DataGridColumn<T> {
  header: string;
  render: (row: T) => ReactNode;
}

export interface DataGridAction<T> {
  label: string;
  onClick: (row: T) => void;
  visible?: (row: T) => boolean;
  color?: 'primary' | 'error';
}

interface DataGridProps<T> {
  columns: DataGridColumn<T>[];
  data: T[];
  rowKey: (row: T) => string;
  actions?: DataGridAction<T>[];
  loading?: boolean;
  emptyMessage?: string;
}

export function DataGrid<T>({
  columns,
  data,
  rowKey,
  actions = [],
  loading = false,
  emptyMessage = 'No results.',
}: DataGridProps<T>) {
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (data.length === 0) {
    return (
      <Box sx={{ py: 4 }}>
        <Typography color="text.secondary">{emptyMessage}</Typography>
      </Box>
    );
  }

  const hasVisibleActions = data.some((row) =>
    actions.some((action) => action.visible?.(row) ?? true),
  );

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          {columns.map((column) => (
            <TableCell key={column.header}>{column.header}</TableCell>
          ))}
          {hasVisibleActions && <TableCell align="right">Actions</TableCell>}
        </TableRow>
      </TableHead>
      <TableBody>
        {data.map((row) => {
          const visibleActions = actions.filter((action) => action.visible?.(row) ?? true);
          return (
            <TableRow key={rowKey(row)}>
              {columns.map((column) => (
                <TableCell key={column.header}>{column.render(row)}</TableCell>
              ))}
              {hasVisibleActions && (
                <TableCell align="right">
                  <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                    {visibleActions.map((action) => (
                      <Button
                        key={action.label}
                        size="small"
                        color={action.color ?? 'primary'}
                        onClick={() => action.onClick(row)}
                      >
                        {action.label}
                      </Button>
                    ))}
                  </Stack>
                </TableCell>
              )}
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );
}
