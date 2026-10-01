import { useEffect, useState } from 'react'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '../../../shared/api/ApiError'
import type { IntersectionDetailResponse, TrafficEvent } from '../../../types/traffic'
import { getIntersectionDetails } from '../intersectionApi'
import { formatDetectorUpdate } from '../intersectionFormatting'

const eventColumns: ColumnDef<TrafficEvent>[] = [
  {
    accessorKey: 'type',
    header: 'Event',
    cell: ({ row }) => (
      <div className="intersection-name-cell">
        <strong>{row.original.type}</strong>
        <span>Event #{row.original.id.slice(0, 8)}</span>
      </div>
    ),
  },
  {
    accessorKey: 'severity',
    header: 'Severity',
    cell: ({ row }) => <span className={`event-severity severity-${row.original.severity.toLowerCase()}`}>{row.original.severity}</span>,
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <span className="event-status">{row.original.status}</span>,
  },
  {
    accessorKey: 'detectedAt',
    header: 'Detected',
    cell: ({ row }) => formatDetectorUpdate(row.original.detectedAt),
  },
]

const emptyEvents: TrafficEvent[] = []

export function IntersectionDetailPage() {
  const { id } = useParams()
  const intersectionId = Number(id)
  const [detail, setDetail] = useState<IntersectionDetailResponse | null>(null)
  const [status, setStatus] = useState<'loading' | 'ready' | 'error' | 'not-found'>('loading')
  const [error, setError] = useState<{ message: string; traceId?: string } | null>(null)
  const [retryCount, setRetryCount] = useState(0)

  useEffect(() => {
    if (!Number.isInteger(intersectionId) || intersectionId < 1) {
      setStatus('not-found')
      return
    }

    const controller = new AbortController()
    setStatus('loading')
    setError(null)
    setDetail(null)

    void getIntersectionDetails(intersectionId, controller.signal).then(
      (response) => {
        setDetail(response)
        setStatus('ready')
      },
      (requestError: unknown) => {
        if (controller.signal.aborted || (requestError instanceof DOMException && requestError.name === 'AbortError')) {
          return
        }

        if (requestError instanceof ApiError && requestError.status === 404) {
          setStatus('not-found')
          return
        }

        setError({
          message: requestError instanceof Error ? requestError.message : 'Unable to load this intersection.',
          traceId: requestError instanceof ApiError ? requestError.traceId : undefined,
        })
        setStatus('error')
      },
    )

    return () => controller.abort()
  }, [intersectionId, retryCount])

  const intersection = detail?.intersection
  const eventTable = useReactTable({
    data: detail?.activeEvents ?? emptyEvents,
    columns: eventColumns,
    getRowId: (row) => row.id,
    getCoreRowModel: getCoreRowModel(),
  })

  if (status === 'loading') {
    return (
      <main className="page intersection-page">
        <Link className="back-link" to="/intersections">‹ <span>Intersections</span></Link>
        <div className="intersection-state" role="status">Loading intersection details...</div>
      </main>
    )
  }

  if (status === 'error') {
    return (
      <main className="page intersection-page">
        <Link className="back-link" to="/intersections">‹ <span>Intersections</span></Link>
        <section className="intersection-error-state" role="alert">
          <div>
            <span className="eyebrow">Intersection details</span>
            <h1>Unable to load intersection</h1>
            <p>{error?.message ?? 'Unable to load this intersection.'}</p>
            {error?.traceId && <p className="trace-id">Trace ID: {error.traceId}</p>}
          </div>
          <button className="secondary-action" type="button" onClick={() => setRetryCount((count) => count + 1)}>
            Retry
          </button>
        </section>
      </main>
    )
  }

  if (status === 'not-found' || !intersection) {
    return (
      <main className="page intersection-page">
        <Link className="back-link" to="/intersections">‹ <span>Intersections</span></Link>
        <section className="intersection-not-found">
          <span className="eyebrow">Intersection details</span>
          <h1>Intersection not found</h1>
          <p>This intersection may have been removed or the address may be incorrect.</p>
          <Link className="secondary-action" to="/intersections">Back to intersections</Link>
        </section>
      </main>
    )
  }

  return (
    <main className="page intersection-page">
      <Link className="back-link" to="/intersections">‹ <span>Intersections</span></Link>
      <header className="intersection-detail-header">
        <div>
          <span className="eyebrow">Intersection details</span>
          <h1>{intersection.name}</h1>
          <p>Intersection INT-{intersection.id}</p>
        </div>
        <div className="intersection-detail-badges">
          <span className={`status status-${intersection.status.toLowerCase()}`}>{intersection.status}</span>
          <span className={`freshness-badge freshness-${intersection.freshness.toLowerCase()}`}>{intersection.freshness}</span>
        </div>
      </header>

      <div className="intersection-detail-metrics">
        <article className="metric-card">
          <span className="detail-metric-label">Latest speed</span>
          <strong>{intersection.speedMph === null ? 'Unavailable' : `${intersection.speedMph} mph`}</strong>
          <span>Detector reading</span>
        </article>
        <article className="metric-card">
          <span className="detail-metric-label">Active events</span>
          <strong>{intersection.activeEventCount}</strong>
          <span>{intersection.activeEventCount === 1 ? 'Requires operator review' : 'At this intersection'}</span>
        </article>
        <article className="metric-card">
          <span className="detail-metric-label">Last detector update</span>
          <strong className="metric-time">{formatDetectorUpdate(intersection.lastDetectorUpdate)}</strong>
          <span className={`detail-freshness detail-freshness-${intersection.freshness.toLowerCase()}`}>
            {intersection.freshness === 'Fresh' ? 'Current detector data' : `${intersection.freshness} data; verify before acting`}
          </span>
        </article>
      </div>

      <div className="intersection-detail-grid">
        <section className="feature-panel detail-panel" aria-labelledby="location-heading">
          <span className="eyebrow">Location</span>
          <h2 id="location-heading">{intersection.name}</h2>
          <dl className="location-coordinates">
            <div><dt>Latitude</dt><dd>{intersection.latitude.toFixed(4)}</dd></div>
            <div><dt>Longitude</dt><dd>{intersection.longitude.toFixed(4)}</dd></div>
          </dl>
          <p className="panel-note">Coordinates shown as returned by the API.</p>
        </section>

        <section className="feature-panel detail-panel freshness-panel" aria-labelledby="freshness-heading">
          <span className="eyebrow">Detector freshness</span>
          <h2 id="freshness-heading">Latest reading is {intersection.freshness.toLowerCase()}</h2>
          <p>Health remains {intersection.status}; freshness describes the age of the detector data.</p>
          <div className={`freshness-callout freshness-callout-${intersection.freshness.toLowerCase()}`}>
            <span className="freshness-dot" aria-hidden="true" />
            <span>Last update: {formatDetectorUpdate(intersection.lastDetectorUpdate)}</span>
          </div>
        </section>
      </div>

      <section className="intersection-table-panel event-table-panel" aria-labelledby="active-events-heading">
        <div className="intersection-table-heading">
          <div>
            <span className="eyebrow">Active conditions</span>
            <h2 id="active-events-heading">Events at this intersection</h2>
          </div>
        </div>
        {eventTable.getRowModel().rows.length === 0 ? (
          <div className="intersection-empty-state compact-empty-state">
            <h3>No active events</h3>
            <p>There are no current traffic events for this intersection.</p>
          </div>
        ) : (
          <div className="intersection-table-scroll">
            <table className="intersection-table event-table">
              <thead>
                {eventTable.getHeaderGroups().map((headerGroup) => (
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
                {eventTable.getRowModel().rows.map((row) => (
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
      </section>
    </main>
  )
}