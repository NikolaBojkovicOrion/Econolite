import { getJson } from '../../shared/api/httpClient'
import type { Intersection } from '../../types/traffic'

export function getIntersections(signal?: AbortSignal): Promise<Intersection[]> {
  return getJson<Intersection[]>('/api/intersections', signal)
}
