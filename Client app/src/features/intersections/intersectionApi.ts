import { getJson } from '../../shared/api/httpClient'
import type {
  Intersection,
  IntersectionDetailResponse,
  IntersectionPageResponse,
} from '../../types/traffic'

export interface IntersectionPageRequest {
  pageNumber: number
  pageSize: number
  name?: string
  status?: string
  freshness?: string
}

export function getIntersectionPage(
  request: IntersectionPageRequest,
  signal?: AbortSignal,
): Promise<IntersectionPageResponse> {
  const query = new URLSearchParams({
    pageNumber: String(request.pageNumber),
    pageSize: String(request.pageSize),
  })

  if (request.name) query.set('name', request.name)
  if (request.status) query.set('status', request.status)
  if (request.freshness) query.set('freshness', request.freshness)

  return getJson<IntersectionPageResponse>(`/api/intersections?${query.toString()}`, signal)
}

export function getIntersections(signal?: AbortSignal, pageSize = 100): Promise<Intersection[]> {
  return getIntersectionPage({ pageNumber: 1, pageSize }, signal)
    .then((response) => response.items)
}

export function getIntersectionDetails(
  id: number,
  signal?: AbortSignal,
): Promise<IntersectionDetailResponse> {
  return getJson<IntersectionDetailResponse>(`/api/intersections/${id}`, signal)
}
