import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '../../../shared/api/ApiError'
import type { IntersectionDetailResponse } from '../../../types/traffic'
import { getIntersectionDetails } from '../intersectionApi'
import { formatDetectorUpdate } from '../intersectionFormatting'
import { IntersectionEventsTable } from '../components/IntersectionEventsTable'

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

      <IntersectionEventsTable events={detail.activeEvents} />
    </main>
  )
}