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
          <span className={`status status-${intersection.status.toLowerCase()}`}>
            {intersection.status}
          </span>
        </li>
      ))}
    </ul>
  )
}
