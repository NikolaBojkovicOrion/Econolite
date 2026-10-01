import { useEffect, useState } from 'react'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
  type PaginationState,
} from '@tanstack/react-table'
import { Link } from 'react-router-dom'
import { ApiError } from '../../../shared/api/ApiError'
import type { IntersectionPageResponse, IntersectionSummary } from '../../../types/traffic'
import { getIntersectionPage } from '../intersectionApi'
import { formatDetectorUpdate } from '../intersectionFormatting'

const intersectionColumns: ColumnDef<IntersectionSummary>[] = [
  {
    accessorKey: 'name',
    header: 'Intersection',
    cell: ({ row }) => (
      <div className="intersection-name-cell">
        <Link to={`/intersections/${row.original.id}`}>{row.original.name}</Link>
        <span>INT-{row.original.id}</span>
      </div>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Health',
    cell: ({ row }) => (
      <span className={`status status-${row.original.status.toLowerCase()}`}>
        {row.original.status}
      </span>
    ),
  },
  {
    accessorKey: 'speedMph',
    header: 'Speed',
    cell: ({ getValue }) => {
      const speed = getValue<number | null>()
      return speed === null ? <span className="muted-value">Unavailable</span> : `${speed} mph`
    },
  },
  {
    accessorKey: 'activeEventCount',
    header: 'Active events',
  },
  {
    accessorKey: 'lastUpdate',
    header: 'Last update',
    cell: ({ row }) => formatDetectorUpdate(row.original.lastDetectorUpdate),
  },
  {
    accessorKey: 'freshness',
    header: 'Freshness',
    cell: ({ row }) => (
      <span className={`freshness-badge freshness-${row.original.freshness.toLowerCase()}`}>
        {row.original.freshness}
      </span>
    ),
  },
]

const freshnessStates = ['Fresh', 'Delayed', 'Stale'] as const
const emptyIntersections: IntersectionSummary[] = []
const searchDebounceMilliseconds = 300

export function IntersectionsPage() {
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [health, setHealth] = useState('All')
  const [freshness, setFreshness] = useState('All')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 5 })
  const [result, setResult] = useState<IntersectionPageResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<{ message: string; traceId?: string } | null>(null)
  const [retryCount, setRetryCount] = useState(0)
  const normalizedSearch = search.trim()

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedSearch(normalizedSearch)
      setPagination((current) => current.pageIndex === 0
        ? current
        : { ...current, pageIndex: 0 })
    }, searchDebounceMilliseconds)

    return () => window.clearTimeout(timeoutId)
  }, [normalizedSearch])

  useEffect(() => {
    if (normalizedSearch !== debouncedSearch) return

    const controller = new AbortController()
    setLoading(true)
    setError(null)

    void getIntersectionPage({
      pageNumber: pagination.pageIndex + 1,
      pageSize: pagination.pageSize,
      name: debouncedSearch || undefined,
      status: health === 'All' ? undefined : health,
      freshness: freshness === 'All' ? undefined : freshness,
    }, controller.signal).then(
      (page) => {
        setResult(page)
        setLoading(false)
      },
      (requestError: unknown) => {
        if (controller.signal.aborted || (requestError instanceof DOMException && requestError.name === 'AbortError')) {
          return
        }

        setError({
          message: requestError instanceof Error ? requestError.message : 'Unable to load intersections.',
          traceId: requestError instanceof ApiError ? requestError.traceId : undefined,
        })
        setLoading(false)
      },
    )

    return () => controller.abort()
  }, [debouncedSearch, freshness, health, normalizedSearch, pagination.pageIndex, pagination.pageSize, retryCount])

  const table = useReactTable({
    data: result?.items ?? emptyIntersections,
    columns: intersectionColumns,
    getRowId: (row) => String(row.id),
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    pageCount: result ? Math.ceil(result.totalCount / pagination.pageSize) : 0,
    onPaginationChange: setPagination,
    state: { pagination },
  })

  const filteredCount = result?.totalCount ?? 0
  const pageIndex = table.getState().pagination.pageIndex
  const pageSize = table.getState().pagination.pageSize
  const firstVisible = filteredCount === 0 ? 0 : pageIndex * pageSize + 1
  const lastVisible = Math.min(pageIndex * pageSize + table.getRowModel().rows.length, filteredCount)

  function clearFilters(): void {
    setSearch('')
    setHealth('All')
    setFreshness('All')
    setPagination((current) => ({ ...current, pageIndex: 0 }))
  }

  return (
    <main className="page intersection-page">
      <header className="intersection-page-header">
        <div>
          <span className="eyebrow">Network status</span>
          <h1>Intersections</h1>
          <p>Monitor intersection health and the freshness of detector data.</p>
        </div>
      </header>

      <div className="intersection-metrics" aria-label="Intersection summary">
        <article className="metric-card">
          <strong>{result?.summary.totalCount ?? '—'}</strong>
          <span>Intersections monitored</span>
        </article>
        <article className="metric-card">
          <strong>{result?.summary.healthyCount ?? '—'}</strong>
          <span>Healthy intersections</span>
        </article>
        <article className="metric-card">
          <strong>{result?.summary.delayedOrStaleCount ?? '—'}</strong>
          <span>Delayed or stale detectors</span>
        </article>
      </div>

      <section className="intersection-toolbar" aria-label="Filter intersections">
        <label className="intersection-search">
          <span className="sr-only">Search intersection name</span>
          <input
            type="search"
            value={search}
            placeholder="Search intersection name"
            onChange={(event) => setSearch(event.target.value)}
          />
        </label>
        <label className="intersection-filter">
          <span className="sr-only">Filter by health</span>
          <select value={health} onChange={(event) => {
            setHealth(event.target.value)
            setPagination((current) => ({ ...current, pageIndex: 0 }))
          }}>
            <option value="All">All health statuses</option>
            <option value="Healthy">Healthy</option>
            <option value="Degraded">Degraded</option>
            <option value="Offline">Offline</option>
          </select>
        </label>
        <label className="intersection-filter">
          <span className="sr-only">Filter by detector freshness</span>
          <select value={freshness} onChange={(event) => {
            setFreshness(event.target.value)
            setPagination((current) => ({ ...current, pageIndex: 0 }))
          }}>
            <option value="All">All freshness states</option>
            {freshnessStates.map((state) => <option key={state} value={state}>{state}</option>)}
          </select>
        </label>
          {(search || health !== 'All' || freshness !== 'All') && (
          <button className="clear-filters" type="button" onClick={clearFilters}>Clear filters</button>
        )}
      </section>

      <section className="intersection-table-panel" aria-labelledby="intersection-directory-title">
        <div className="intersection-table-heading">
          <div>
            <span className="eyebrow">Intersection directory</span>
            <h2 id="intersection-directory-title">{filteredCount} intersections</h2>
          </div>
          <span className="result-range">
            Showing {firstVisible}-{lastVisible} of {filteredCount}
          </span>
        </div>

        {loading ? (
          <div className="intersection-state" role="status">Loading intersections...</div>
        ) : error ? (
          <div className="intersection-error-state" role="alert">
            <div>
              <h3>Unable to load intersections</h3>
              <p>{error.message}</p>
              {error.traceId && <p className="trace-id">Trace ID: {error.traceId}</p>}
            </div>
            <button className="secondary-action" type="button" onClick={() => setRetryCount((count) => count + 1)}>
              Retry
            </button>
          </div>
        ) : result?.totalCount === 0 ? (
          <div className="intersection-empty-state">
            {normalizedSearch || health !== 'All' || freshness !== 'All' ? (
              <>
                <h3>No intersections match these filters</h3>
                <p>Adjust your search or clear the filters to see the full list.</p>
                <button className="secondary-action" type="button" onClick={clearFilters}>Clear filters</button>
              </>
            ) : (
              <>
                <h3>No intersections configured</h3>
                <p>There are no intersections available to monitor.</p>
              </>
            )}
          </div>
        ) : (
          <div className="intersection-table-scroll">
            <table className="intersection-table">
              <thead>
                {table.getHeaderGroups().map((headerGroup) => (
                  <tr key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <th key={header.id} scope="col">
                        {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                      </th>
                    ))}
                  </tr>
                ))}
              </thead>
              <tbody>
                {table.getRowModel().rows.map((row) => (
                  <tr key={row.id}>
                    {row.getVisibleCells().map((cell) => (
                      <td key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div className="intersection-pagination">
          <label>
            <span>Rows per page</span>
            <select value={pageSize} onChange={(event) => table.setPageSize(Number(event.target.value))}>
              {[5, 10, 20].map((size) => <option key={size} value={size}>{size}</option>)}
            </select>
          </label>
          <span className="page-count">Page {pageIndex + 1} of {Math.max(table.getPageCount(), 1)}</span>
          <div className="page-controls">
            <button type="button" aria-label="Previous page" onClick={() => table.previousPage()} disabled={!table.getCanPreviousPage()}>
              <span aria-hidden="true">‹</span>
            </button>
            <button type="button" aria-label="Next page" onClick={() => table.nextPage()} disabled={!table.getCanNextPage()}>
              <span aria-hidden="true">›</span>
            </button>
          </div>
        </div>
      </section>
    </main>
  )
}