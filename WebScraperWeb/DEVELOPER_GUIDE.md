# Developer Guide

This guide provides information on how to use the new features and patterns implemented in the WebScraper Backoffice application.

## Table of Contents

1. [TypeScript Integration](#typescript-integration)
2. [React Query for Data Fetching](#react-query-for-data-fetching)
3. [Performance Optimizations](#performance-optimizations)
4. [Component Structure](#component-structure)
5. [State Management](#state-management)

## TypeScript Integration

### Converting JavaScript Files to TypeScript

Use the provided script to convert JavaScript files to TypeScript:

```bash
node scripts/convertToTypeScript.js
```

### Type Definitions

Common types are defined in `src/types/index.ts`. When creating new components or hooks, import types from this file:

```typescript
import { Scraper, ScraperStatus } from '../types';
```

### Best Practices

- Always define prop types for components:

```typescript
interface MyComponentProps {
  title: string;
  data: Scraper[];
  onAction: (id: string) => void;
}

const MyComponent: React.FC<MyComponentProps> = ({ title, data, onAction }) => {
  // Component implementation
};
```

- Use type inference where possible, but be explicit when necessary:

```typescript
// Good - type is inferred
const [count, setCount] = useState(0);

// Good - explicit type when initial value is null or undefined
const [scraper, setScraper] = useState<Scraper | null>(null);
```

## React Query for Data Fetching

### Basic Usage

```typescript
import { useQuery } from '@tanstack/react-query';
import { getAllScrapers } from '../api/scrapers';

const MyComponent = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ['scrapers'],
    queryFn: getAllScrapers
  });

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div>
      {data.map(scraper => (
        <div key={scraper.id}>{scraper.name}</div>
      ))}
    </div>
  );
};
```

### Using Predefined Query Hooks

```typescript
import { useScrapers, useScraper } from '../hooks';

// Get all scrapers
const { data: scrapers, isLoading } = useScrapers();

// Get a specific scraper
const { data: scraper } = useScraper(id);
```

### Mutations

```typescript
import { useCreateScraper } from '../hooks';

const { mutate, isPending } = useCreateScraper();

const handleSubmit = (data) => {
  mutate(data, {
    onSuccess: () => {
      // Handle success
    },
    onError: (error) => {
      // Handle error
    }
  });
};
```

## Performance Optimizations

### Virtualized Tables

Use the `VirtualizedTable` component for large datasets:

```typescript
import { VirtualizedTable } from '../components/common';

const columns = [
  { id: 'name', label: 'Name', sortable: true },
  { id: 'baseUrl', label: 'Base URL', sortable: true },
  // More columns...
];

return (
  <VirtualizedTable
    columns={columns}
    data={data}
    isLoading={isLoading}
    error={error?.message}
    // Additional props...
  />
);
```

### Component Memoization

Use `React.memo` for pure components:

```typescript
const MyComponent = React.memo(({ data }) => {
  // Component implementation
});
```

Use `useMemo` for expensive computations:

```typescript
const filteredData = useMemo(() => {
  return data.filter(item => item.name.includes(searchTerm));
}, [data, searchTerm]);
```

Use `useCallback` for event handlers:

```typescript
const handleClick = useCallback(() => {
  // Handler implementation
}, [dependencies]);
```

## Component Structure

### Folder Structure

```
src/
├── components/
│   ├── common/       # Reusable components
│   ├── feature1/     # Feature-specific components
│   ├── feature2/     # Feature-specific components
```

### Component Template

```typescript
import React, { useState, useCallback, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { AsyncWrapper } from '../common';

interface MyComponentProps {
  // Props definition
}

const MyComponent: React.FC<MyComponentProps> = ({ /* props */ }) => {
  // State
  const [state, setState] = useState(initialState);

  // Queries
  const { data, isLoading, error } = useQuery({
    // Query configuration
  });

  // Callbacks
  const handleAction = useCallback(() => {
    // Action implementation
  }, [dependencies]);

  // Memoized values
  const processedData = useMemo(() => {
    // Data processing
    return result;
  }, [data]);

  // Render
  return (
    <AsyncWrapper loading={isLoading} error={error}>
      {/* Component content */}
    </AsyncWrapper>
  );
};

export default React.memo(MyComponent);
```

## State Management

### Context API

Use the provided context providers for global state:

```typescript
import { useAppState, useAuth, useTheme } from '../providers';

const MyComponent = () => {
  const { toggleSidebar } = useAppState();
  const { currentUser, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();

  // Component implementation
};
```

### React Query for Server State

Use React Query for all server state:

```typescript
import { useQueryClient } from '@tanstack/react-query';

const queryClient = useQueryClient();

// Invalidate queries to refetch data
queryClient.invalidateQueries({ queryKey: ['scrapers'] });

// Prefetch data
queryClient.prefetchQuery({
  queryKey: ['scraper', id],
  queryFn: () => getScraper(id)
});

// Get cached data
const scrapers = queryClient.getQueryData(['scrapers']);
```

### Local Component State

Use `useState` for component-specific state:

```typescript
const [isOpen, setIsOpen] = useState(false);
const [selectedItem, setSelectedItem] = useState<Item | null>(null);
```
