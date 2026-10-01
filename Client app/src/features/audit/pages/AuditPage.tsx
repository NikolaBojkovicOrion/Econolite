import { useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { getAuditEntries } from '../auditApi'
import type { AuditEntry } from '../../../types/traffic'
import { TrafficConnectionStatus } from '../../trafficEvents/components/TrafficConnectionStatus'
import { useTrafficUpdates } from '../../trafficEvents/useTrafficUpdates'

type LoadState = 'loading' | 'ready' | 'error'

export function AuditPage() {
  const [entries, setEntries] = useState<AuditEntry[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [traceId, setTraceId] = useState<string | null>(null)

  const connectionState = useTrafficUpdates({
    onTrafficEvent: () => undefined,
    onIntersectionStatus: () => undefined,
    onReconnect: () => void loadEntries(),
    onAuditEntry: (entry) => {
      setEntries((currentEntries) => {
        const updatedEntries = [entry, ...currentEntries.filter((current) => current.id !== entry.id)]
        return updatedEntries
          .sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt))
          .slice(0, 100)
      })
      setLoadState('ready')
    },
  })

  async function loadEntries(signal?: AbortSignal): Promise<void> {
    try {
      const loadedEntries = await getAuditEntries(signal)
      setEntries((currentEntries) => {
        const entriesById = new Map(
          [...loadedEntries, ...currentEntries].map((entry) => [entry.id, entry]),
        )
        return [...entriesById.values()]
          .sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt))
          .slice(0, 100)
      })
      setLoadState('ready')
      setErrorMessage(null)
      setTraceId(null)
    } catch (error) {
      if (!(error instanceof DOMException && error.name === 'AbortError')) {
        setErrorMessage(error instanceof Error ? error.message : 'Unable to load audit history.')
        setTraceId(error instanceof ApiError ? error.traceId ?? null : null)
        setLoadState('error')
      }
    }
  }

  useEffect(() => {
    const controller = new AbortController()
    void loadEntries(controller.signal)
    return () => controller.abort()
  }, [])

  return (
    <section className="page operational-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Accountability</p>
          <h1>Audit history</h1>
        </div>
        <p>Recent operator actions recorded against traffic events.</p>
      </header>
      <TrafficConnectionStatus state={connectionState} />
      {loadState === 'loading' && <p role="status">Loading audit history...</p>}
      {loadState === 'error' && (
        <div className="placeholder-note" role="alert">
          <p>{errorMessage}</p>
          {traceId && <small>Trace ID: {traceId}</small>}{' '}
          <button type="button" onClick={() => void loadEntries()}>Retry</button>
        </div>
      )}
      {loadState === 'ready' && entries.length === 0 && (
        <p className="placeholder-note">No audit entries have been recorded.</p>
      )}
      {loadState === 'ready' && entries.length > 0 && (
        <div className="event-table-wrap">
          <table className="event-table">
            <thead>
              <tr>
                <th scope="col">Action</th>
                <th scope="col">Actor</th>
                <th scope="col">Entity</th>
                <th scope="col">Time</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((entry) => (
                <tr key={entry.id}>
                  <td>{entry.action}</td>
                  <td>{entry.userId ?? 'System'}</td>
                  <td>{entry.entityType} #{entry.entityId}</td>
                  <td><time dateTime={entry.createdAt}>{new Date(entry.createdAt).toLocaleString()}</time></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}