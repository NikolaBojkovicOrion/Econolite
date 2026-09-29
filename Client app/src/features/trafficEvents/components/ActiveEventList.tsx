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
          <span>{event.type} at #{event.intersectionId}</span>
          <span className="status status-degraded">{event.severity}</span>
        </li>
      ))}
    </ul>
  )
}
