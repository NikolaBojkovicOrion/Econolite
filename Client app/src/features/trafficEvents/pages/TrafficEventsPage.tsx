import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import type { TrafficEvent } from '../../../types/traffic'
import {
  acknowledgeTrafficEvent,
  getActiveTrafficEvents,
  resolveTrafficEvent,
} from '../trafficEventApi'

type LoadState = 'loading' | 'ready' | 'error'

export function TrafficEventsPage() {
  const { user } = useAuth()
  const [events, setEvents] = useState<TrafficEvent[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [loadError, setLoadError] = useState<string | null>(null)
  const [traceId, setTraceId] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [pendingEventId, setPendingEventId] = useState<string | null>(null)
  const canResolveCritical = user?.roles.some((role) => role === 'Supervisor' || role === 'Admin') ?? false

  async function loadEvents(signal?: AbortSignal): Promise<void> {
    try {
      const loadedEvents = await getActiveTrafficEvents(signal)
      setEvents(loadedEvents)
      setLoadState('ready')
      setLoadError(null)
      setTraceId(null)
    } catch (error) {
      if (!(error instanceof DOMException && error.name === 'AbortError')) {
        const apiError = error instanceof ApiError ? error : null
        setLoadError(error instanceof Error ? error.message : 'Unable to load traffic events.')
        setTraceId(apiError?.traceId ?? null)
        setLoadState('error')
      }
    }
  }

  useEffect(() => {
    const controller = new AbortController()
    void loadEvents(controller.signal)
    return () => controller.abort()
  }, [])

  async function acknowledge(eventId: string): Promise<void> {
    setPendingEventId(eventId)
    setActionError(null)
    try {
      await acknowledgeTrafficEvent(eventId)
      await loadEvents()
    } catch (error) {
      setActionError(error instanceof Error ? error.message : 'Unable to acknowledge this event.')
    } finally {
      setPendingEventId(null)
    }
  }

  async function resolve(event: TrafficEvent): Promise<void> {
    if (!window.confirm(`Resolve ${event.type} at intersection #${event.intersectionId}?`)) {
      return
    }

    setPendingEventId(event.id)
    setActionError(null)
    try {
      await resolveTrafficEvent(event.id)
      await loadEvents()
    } catch (error) {
      setActionError(error instanceof Error ? error.message : 'Unable to resolve this event.')
    } finally {
      setPendingEventId(null)
    }
  }

  return (
    <section className="page operational-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Operator workflow</p>
          <h1>Traffic events</h1>
        </div>
        <p>Review active and acknowledged incidents, then record the operator response.</p>
      </header>
      {actionError && <p className="placeholder-note form-error" role="alert">{actionError}</p>}
      {loadState === 'loading' && <p role="status">Loading traffic events...</p>}
      {loadState === 'error' && (
        <div className="placeholder-note" role="alert">
          <p>{loadError}</p>
          {traceId && <small>Trace ID: {traceId}</small>}{' '}
          <button type="button" onClick={() => void loadEvents()}>Retry</button>
        </div>
      )}
      {loadState === 'ready' && events.length === 0 && (
        <p className="placeholder-note">There are no unresolved traffic events.</p>
      )}
      {loadState === 'ready' && events.length > 0 && (
        <div className="event-table-wrap">
          <table className="event-table">
            <thead>
              <tr>
                <th scope="col">Intersection</th>
                <th scope="col">Condition</th>
                <th scope="col">Severity</th>
                <th scope="col">Status</th>
                <th scope="col">Detected</th>
                <th scope="col"><span className="visually-hidden">Actions</span></th>
              </tr>
            </thead>
            <tbody>
              {events.map((event) => {
                const isPending = pendingEventId === event.id
                const canResolve = event.severity.toLowerCase() !== 'critical' || canResolveCritical

                return (
                  <tr key={event.id}>
                    <td>#{event.intersectionId}</td>
                    <td>{event.type}</td>
                    <td><span className={`status status-${event.severity.toLowerCase()}`}>{event.severity}</span></td>
                    <td><span className={`status status-${event.status.toLowerCase()}`}>{event.status}</span></td>
                    <td><time dateTime={event.detectedAt}>{new Date(event.detectedAt).toLocaleString()}</time></td>
                    <td>
                      <div className="event-actions">
                        {event.status === 'Active' && (
                          <button type="button" disabled={isPending} onClick={() => void acknowledge(event.id)}>
                            Acknowledge
                          </button>
                        )}
                        {canResolve && (event.status === 'Active' || event.status === 'Acknowledged') && (
                          <button type="button" disabled={isPending} onClick={() => void resolve(event)}>
                            Resolve
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}