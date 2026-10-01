import type { TrafficEvent } from '../../../types/traffic'

type ActiveEventListProps = {
  events: TrafficEvent[]
}

export function ActiveEventList({ events }: ActiveEventListProps) {
  if (events.length === 0) {
    return <p>No active traffic events.</p>
  }

  return (
    <ul className="data-list">
      {events.map((event) => (
        <li key={event.id}>
          <span className="event-summary">
            <span>{event.type} at #{event.intersectionId}</span>
            <time dateTime={event.detectedAt}>
              Detected {new Date(event.detectedAt).toLocaleString()}
            </time>
          </span>
          <span className="event-summary-badges">
            <span className={`status status-${event.severity.toLowerCase()}`}>{event.severity}</span>
            <span className={`status status-${event.status.toLowerCase()}`}>{event.status}</span>
          </span>
        </li>
      ))}
    </ul>
  )
}
