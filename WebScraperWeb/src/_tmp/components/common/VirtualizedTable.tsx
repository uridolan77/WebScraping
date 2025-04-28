import React, { useState, useCallback, useMemo } from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  TablePagination,
  Typography,
  CircularProgress,
  Alert,
  TextField,
  InputAdornment,
  IconButton,
  Toolbar,
  Tooltip,
  useTheme
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import { useVirtualizer } from '@tanstack/react-virtual';
import { debounce } from '../../utils/helpers';
import { TableColumn } from '../../types';

interface VirtualizedTableProps<T> {
  columns: TableColumn[];
  data: T[];
  totalCount?: number;
  page?: number;
  rowsPerPage?: number;
  onPageChange?: (page: number) => void;
  onRowsPerPageChange?: (rowsPerPage: number) => void;
  onSort?: (column: string, direction: 'asc' | 'desc') => void;
  onSearch?: (term: string) => void;
  onRefresh?: () => void;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchTerm?: string;
  isLoading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  title?: React.ReactNode;
  toolbarActions?: React.ReactNode;
  rowKey?: string;
  searchPlaceholder?: string;
  searchFields?: string[];
  defaultRowsPerPage?: number;
  rowsPerPageOptions?: number[];
  stickyHeader?: boolean;
  maxHeight?: number | string;
  renderRowActions?: (row: T) => React.ReactNode;
  onRowClick?: (row: T) => void;
  overscan?: number;
}

/**
 * A virtualized table component for efficiently rendering large datasets
 */
function VirtualizedTable<T extends Record<string, any>>({
  columns,
  data,
  totalCount,
  page = 0,
  rowsPerPage = 50,
  onPageChange,
  onRowsPerPageChange,
  onSort,
  onSearch,
  onRefresh,
  sortBy,
  sortDirection = 'asc',
  searchTerm = '',
  isLoading = false,
  error = null,
  emptyMessage = 'No data available',
  title,
  toolbarActions,
  rowKey = 'id',
  searchPlaceholder = 'Search...',
  searchFields = [],
  defaultRowsPerPage = 50,
  rowsPerPageOptions = [25, 50, 100, 250],
  stickyHeader = true,
  maxHeight = 600,
  renderRowActions,
  onRowClick,
  overscan = 5
}: VirtualizedTableProps<T>) {
  const theme = useTheme();
  const [localSearchTerm, setLocalSearchTerm] = useState(searchTerm);
  const [tableHeight, setTableHeight] = useState<number>(
    typeof maxHeight === 'number' ? maxHeight : 600
  );

  // Reference to the table container for virtualization
  const parentRef = React.useRef<HTMLDivElement>(null);

  // Update local search term when prop changes
  React.useEffect(() => {
    setLocalSearchTerm(searchTerm);
  }, [searchTerm]);

  // Measure table height on mount and window resize
  React.useEffect(() => {
    const updateTableHeight = () => {
      if (parentRef.current) {
        const containerHeight = parentRef.current.clientHeight;
        setTableHeight(containerHeight);
      }
    };

    updateTableHeight();
    window.addEventListener('resize', updateTableHeight);
    
    return () => {
      window.removeEventListener('resize', updateTableHeight);
    };
  }, []);

  // Create virtualizer
  const rowVirtualizer = useVirtualizer({
    count: data.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 53, // Approximate row height
    overscan
  });

  // Debounced search handler
  const debouncedSearch = useMemo(
    () => debounce((value: string) => {
      if (onSearch) {
        onSearch(value);
      }
    }, 300),
    [onSearch]
  );

  // Handle search input change
  const handleSearchChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setLocalSearchTerm(value);
    debouncedSearch(value);
  }, [debouncedSearch]);

  // Handle clear search
  const handleClearSearch = useCallback(() => {
    setLocalSearchTerm('');
    if (onSearch) {
      onSearch('');
    }
  }, [onSearch]);

  // Handle page change
  const handlePageChange = useCallback((_: unknown, newPage: number) => {
    if (onPageChange) {
      onPageChange(newPage);
    }
  }, [onPageChange]);

  // Handle rows per page change
  const handleRowsPerPageChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    if (onRowsPerPageChange) {
      onRowsPerPageChange(parseInt(event.target.value, 10));
    }
  }, [onRowsPerPageChange]);

  // Handle sort
  const handleSort = useCallback((column: TableColumn) => {
    if (onSort && column.sortable !== false) {
      const isAsc = sortBy === column.id && sortDirection === 'asc';
      onSort(column.id, isAsc ? 'desc' : 'asc');
    }
  }, [onSort, sortBy, sortDirection]);

  // Handle refresh
  const handleRefresh = useCallback(() => {
    if (onRefresh) {
      onRefresh();
    }
  }, [onRefresh]);

  // Render table header
  const renderTableHeader = useCallback(() => (
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
  ), [columns, sortBy, sortDirection, onSort, handleSort, renderRowActions]);

  // Render toolbar
  const renderToolbar = useCallback(() => (
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
  ), [
    title, 
    onSearch, 
    localSearchTerm, 
    searchPlaceholder, 
    handleSearchChange, 
    handleClearSearch, 
    onRefresh, 
    handleRefresh, 
    isLoading, 
    toolbarActions
  ]);

  // Render loading state
  const renderLoading = useCallback(() => (
    <TableRow>
      <TableCell colSpan={columns.length + (renderRowActions ? 1 : 0)} align="center" sx={{ py: 3 }}>
        <CircularProgress size={40} />
        <Typography variant="body2" sx={{ mt: 2 }}>
          Loading data...
        </Typography>
      </TableCell>
    </TableRow>
  ), [columns.length, renderRowActions]);

  // Render error state
  const renderError = useCallback(() => (
    <TableRow>
      <TableCell colSpan={columns.length + (renderRowActions ? 1 : 0)} align="center" sx={{ py: 3 }}>
        <Alert severity="error">{error}</Alert>
      </TableCell>
    </TableRow>
  ), [columns.length, renderRowActions, error]);

  // Render empty state
  const renderEmpty = useCallback(() => (
    <TableRow>
      <TableCell colSpan={columns.length + (renderRowActions ? 1 : 0)} align="center" sx={{ py: 3 }}>
        <Typography variant="body1">{emptyMessage}</Typography>
      </TableCell>
    </TableRow>
  ), [columns.length, renderRowActions, emptyMessage]);

  // Calculate total height of virtualized rows
  const totalHeight = rowVirtualizer.getTotalSize();

  // Get virtualized rows
  const virtualRows = rowVirtualizer.getVirtualItems();

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      {(title || onSearch || toolbarActions) && renderToolbar()}
      
      <TableContainer 
        ref={parentRef} 
        sx={{ 
          maxHeight: maxHeight,
          height: tableHeight,
          overflow: 'auto'
        }}
      >
        <Table stickyHeader={stickyHeader} aria-label={typeof title === 'string' ? title : 'data-table'}>
          {renderTableHeader()}
          
          <TableBody>
            {isLoading && data.length === 0 ? (
              renderLoading()
            ) : error ? (
              renderError()
            ) : data.length === 0 ? (
              renderEmpty()
            ) : (
              <>
                {/* Spacer row to account for virtualized content above */}
                {virtualRows.length > 0 && (
                  <TableRow>
                    <TableCell 
                      style={{ 
                        height: virtualRows[0].start, 
                        padding: 0,
                        border: 'none'
                      }} 
                      colSpan={columns.length + (renderRowActions ? 1 : 0)} 
                    />
                  </TableRow>
                )}
                
                {/* Virtualized rows */}
                {virtualRows.map((virtualRow) => {
                  const row = data[virtualRow.index];
                  const rowId = row[rowKey];
                  
                  return (
                    <TableRow
                      key={rowId}
                      hover
                      onClick={onRowClick ? () => onRowClick(row) : undefined}
                      sx={{ 
                        cursor: onRowClick ? 'pointer' : 'default',
                        '&:nth-of-type(odd)': {
                          backgroundColor: theme.palette.action.hover,
                        }
                      }}
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
                })}
                
                {/* Spacer row to account for virtualized content below */}
                {virtualRows.length > 0 && (
                  <TableRow>
                    <TableCell 
                      style={{ 
                        height: totalHeight - virtualRows[virtualRows.length - 1].end,
                        padding: 0,
                        border: 'none'
                      }} 
                      colSpan={columns.length + (renderRowActions ? 1 : 0)} 
                    />
                  </TableRow>
                )}
              </>
            )}
          </TableBody>
        </Table>
      </TableContainer>
      
      <TablePagination
        rowsPerPageOptions={rowsPerPageOptions}
        component="div"
        count={totalCount || data.length}
        rowsPerPage={rowsPerPage || defaultRowsPerPage}
        page={page}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
      />
    </Paper>
  );
}

export default React.memo(VirtualizedTable) as typeof VirtualizedTable;
