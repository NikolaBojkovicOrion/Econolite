import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type OnChangeFn,
  type PaginationState,
} from '@tanstack/react-table'
import type { IntersectionPageResponse, IntersectionSummary } from '../../../types/traffic'
import { intersectionColumns } from './intersectionColumns'

interface IntersectionLoadError {
  message: string
  traceId?: string
}

interface IntersectionDirectoryTableProps {
  result: IntersectionPageResponse | null
  loading: boolean
  error: IntersectionLoadError | null
  pagination: PaginationState
  hasActiveFilters: boolean
  onPaginationChange: OnChangeFn<PaginationState>
  onRetry: () => void
  onClearFilters: () => void
}

const emptyIntersections: IntersectionSummary[] = []

export function IntersectionDirectoryTable({
  result,
  loading,
  error,
  pagination,
  hasActiveFilters,
  onPaginationChange,
  onRetry,
  onClearFilters,
}: IntersectionDirectoryTableProps) {
  const table = useReactTable({
    data: result?.items ?? emptyIntersections,
    columns: intersectionColumns,
    getRowId: (row) => String(row.id),
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    pageCount: result ? Math.ceil(result.totalCount / pagination.pageSize) : 0,
    onPaginationChange,
    state: { pagination },
  })

  const totalCount = result?.totalCount ?? 0
  const pageIndex = table.getState().pagination.pageIndex
  const pageSize = table.getState().pagination.pageSize
  const firstVisible = totalCount === 0 ? 0 : pageIndex * pageSize + 1
  const lastVisible = Math.min(pageIndex * pageSize + table.getRowModel().rows.length, totalCount)

  return (
    <section className="intersection-table-panel" aria-labelledby="intersection-directory-title">
      <div className="intersection-table-heading">
        <div>
          <span className="eyebrow">Intersection directory</span>
          <h2 id="intersection-directory-title">{totalCount} intersections</h2>
        </div>
        <span className="result-range">Showing {firstVisible}-{lastVisible} of {totalCount}</span>
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
          <button className="secondary-action" type="button" onClick={onRetry}>Retry</button>
        </div>
      ) : result?.totalCount === 0 ? (
        <div className="intersection-empty-state">
          {hasActiveFilters ? (
            <>
              <h3>No intersections match these filters</h3>
              <p>Adjust your search or clear the filters to see the full list.</p>
              <button className="secondary-action" type="button" onClick={onClearFilters}>Clear filters</button>
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
  )
}