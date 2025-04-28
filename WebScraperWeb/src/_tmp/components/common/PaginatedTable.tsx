import React, { useState, useEffect } from 'react';
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  TablePagination,
  Paper,
  Typography,
  CircularProgress,
  Alert,
  TextField,
  InputAdornment,
  IconButton,
  Toolbar,
  Tooltip
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterListIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import { debounce } from '../../utils/helpers';

interface TableColumn {
  id: string;
  label: React.ReactNode;
  minWidth?: number;
  width?: number | string;
  maxWidth?: number;
  align?: 'left' | 'right' | 'center';
  sortable?: boolean;
  render?: (value: any, row: any) => React.ReactNode;
}

interface PaginatedTableProps {
  columns: TableColumn[];
  data?: any[];
  totalCount?: number;
  page?: number;
  rowsPerPage?: number;
  onPageChange?: (page: number) => void;
  onRowsPerPageChange?: (rowsPerPage: number) => void;
  onSort?: (sortBy: string, sortDirection: 'asc' | 'desc') => void;
  onSearch?: (searchTerm: string) => void;
  onRefresh?: () => void;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchTerm?: string;
  isLoading?: boolean;
  error?: string;
  emptyMessage?: string;
  title?: React.ReactNode;
  toolbarActions?: React.ReactNode;
  rowKey?: string;
  searchPlaceholder?: string;
  searchFields?: string[];
  defaultRowsPerPage?: number;
  rowsPerPageOptions?: number[];
  stickyHeader?: boolean;
  maxHeight?: number | string | null;
  renderRowActions?: (row: any) => React.ReactNode;
  onRowClick?: (row: any) => void;
}

/**
 * A reusable paginated table component with sorting and filtering
 */
const PaginatedTable: React.FC<PaginatedTableProps> = ({
  columns,
  data,
  totalCount,
  page,
  rowsPerPage,
  onPageChange,
  onRowsPerPageChange,
  onSort,
  onSearch,
  onRefresh,
  sortBy,
  sortDirection,
  searchTerm,
  isLoading,
  error,
  emptyMessage,
  title,
  toolbarActions,
  rowKey = 'id',
  searchPlaceholder = 'Search...',
  searchFields = [],
  defaultRowsPerPage = 10,
  rowsPerPageOptions = [5, 10, 25, 50],
  stickyHeader = true,
  maxHeight = null,
  renderRowActions,
  onRowClick
}) => {
  const [localSearchTerm, setLocalSearchTerm] = useState(searchTerm || '');

  // Update local search term when prop changes
  useEffect(() => {
    setLocalSearchTerm(searchTerm || '');
  }, [searchTerm]);

  // Debounced search handler
  const debouncedSearch = React.useMemo(
    () => debounce((value: string) => {
      if (onSearch) {
        onSearch(value);
      }
    }, 300),
    [onSearch]
  );

  // Handle search input change
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setLocalSearchTerm(value);
    debouncedSearch(value);
  };

  // Handle clear search
  const handleClearSearch = () => {
    setLocalSearchTerm('');
    if (onSearch) {
      onSearch('');
    }
  };

  // Handle page change
  const handlePageChange = (event: React.MouseEvent<HTMLButtonElement> | null, newPage: number) => {
    if (onPageChange) {
      onPageChange(newPage);
    }
  };

  // Handle rows per page change
  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (onRowsPerPageChange) {
      onRowsPerPageChange(parseInt(event.target.value, 10));
    }
  };

  // Handle sort
  const handleSort = (column: TableColumn) => {
    if (onSort && column.sortable !== false) {
      const isAsc = sortBy === column.id && sortDirection === 'asc';
      onSort(column.id, isAsc ? 'desc' : 'asc');
    }
  };

  // Handle refresh
  const handleRefresh = () => {
    if (onRefresh) {
      onRefresh();
    }
  };

  // Render table header
  const renderTableHeader = () => (
    <TableHead>
      <TableRow>
        {columns.map((column) => (
          <TableCell
            key={column.id}
            align={column.align || 'left'}
            style={{
              minWidth: column.minWidth,
              width: column.width,
              maxWidth: column.maxWidth
            }}
            sortDirection={sortBy === column.id ? sortDirection : false}
          >
            {column.sortable !== false && onSort ? (
              <TableSortLabel
                active={sortBy === column.id}
                direction={sortBy === column.id ? sortDirection : 'asc'}
                onClick={() => handleSort(column)}
              >
                {column.label}
              </TableSortLabel>
            ) : (
              column.label
            )}
          </TableCell>
        ))}
        {renderRowActions && <TableCell align="right">Actions</TableCell>}
      </TableRow>
    </TableHead>
  );

  // Render table body
  const renderTableBody = () => {
    if (isLoading) {
      return (
        <TableRow>
          <TableCell colSpan={columns.length + (renderRowActions ? 1 : 0)} align="center" sx={{ py: 3 }}>
            <CircularProgress size={40} />
            <Typography variant="body2" sx={{ mt: 2 }}>
              Loading data...
            </Typography>
          </TableCell>
        </TableRow>
      );
    }

    if (error) {
      return (
        <TableRow>
          <TableCell colSpan={columns.length + (renderRowActions ? 1 : 0)} align="center" sx={{ py: 3 }}>
            <Alert severity="error">{error}</Alert>
          </TableCell>
        </TableRow>
      );
    }

    if (!data || data.length === 0) {
      return (
        <TableRow>
          <TableCell colSpan={columns.length + (renderRowActions ? 1 : 0)} align="center" sx={{ py: 3 }}>
            <Typography variant="body1">{emptyMessage || 'No data available'}</Typography>
          </TableCell>
        </TableRow>
      );
    }

    return data.map((row) => {
      const rowId = row[rowKey];
      return (
        <TableRow
          hover
          key={rowId}
          onClick={onRowClick ? () => onRowClick(row) : undefined}
          sx={{ cursor: onRowClick ? 'pointer' : 'default' }}
        >
          {columns.map((column) => {
            const value = row[column.id];
            return (
              <TableCell key={`${rowId}-${column.id}`} align={column.align || 'left'}>
                {column.render ? column.render(value, row) : value}
              </TableCell>
            );
          })}
          {renderRowActions && (
            <TableCell align="right">
              {renderRowActions(row)}
            </TableCell>
          )}
        </TableRow>
      );
    });
  };

  // Render toolbar
  const renderToolbar = () => (
    <Toolbar
      sx={{
        pl: { sm: 2 },
        pr: { xs: 1, sm: 1 },
        display: 'flex',
        justifyContent: 'space-between',
        flexWrap: 'wrap',
        gap: 2
      }}
    >
      <Box sx={{ flex: '1 1 100%', display: 'flex', alignItems: 'center' }}>
        {title && (
          <Typography
            sx={{ flex: '0 0 auto', mr: 2 }}
            variant="h6"
            id="tableTitle"
            component="div"
          >
            {title}
          </Typography>
        )}

        {onSearch && (
          <TextField
            variant="outlined"
            size="small"
            placeholder={searchPlaceholder}
            value={localSearchTerm}
            onChange={handleSearchChange}
            sx={{ flex: '1 1 auto', maxWidth: 300 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" />
                </InputAdornment>
              ),
              endAdornment: localSearchTerm ? (
                <InputAdornment position="end">
                  <IconButton
                    size="small"
                    aria-label="clear search"
                    onClick={handleClearSearch}
                    edge="end"
                  >
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ) : null
            }}
          />
        )}
      </Box>

      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
        {onRefresh && (
          <Tooltip title="Refresh">
            <IconButton onClick={handleRefresh} disabled={isLoading}>
              {isLoading ? <CircularProgress size={24} /> : <RefreshIcon />}
            </IconButton>
          </Tooltip>
        )}

        {toolbarActions}
      </Box>
    </Toolbar>
  );

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      {(title || onSearch || toolbarActions) && renderToolbar()}

      <TableContainer sx={{ maxHeight: maxHeight }}>
        <Table stickyHeader={stickyHeader} aria-label={typeof title === 'string' ? title : 'data-table'}>
          {renderTableHeader()}
          <TableBody>
            {renderTableBody()}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        rowsPerPageOptions={rowsPerPageOptions}
        component="div"
        count={totalCount || (data ? data.length : 0)}
        rowsPerPage={rowsPerPage || defaultRowsPerPage}
        page={page || 0}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
      />
    </Paper>
  );
};



export default PaginatedTable;
