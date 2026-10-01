import type { Intersection } from '../../../types/traffic'

type IntersectionStatusListProps = {
  intersections: Intersection[]
}

export function IntersectionStatusList({ intersections }: IntersectionStatusListProps) {
  return (
    <ul className="data-list">
      {intersections.map((intersection) => (
        <li key={intersection.id}>
          <span>{intersection.name}</span>
          <span className="intersection-status-group">
            <span className={`status status-${intersection.status.toLowerCase()}`}>
              {intersection.status}
            </span>
            {intersection.freshness && (
              <small className={`freshness freshness-${intersection.freshness.toLowerCase()}`}>
                Data {intersection.freshness.toLowerCase()}
              </small>
            )}
          </span>
        </li>
      ))}
    </ul>
  )
}
