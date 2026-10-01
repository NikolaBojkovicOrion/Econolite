import { getJson, patchJson, postJson } from '../../shared/api/httpClient'
import type { TrafficEvent } from '../../types/traffic'

export interface CreateTrafficEventRequest {
  type: string
  severity: string
  detectedAt: string
  sourceSystem: string
  externalEventId: string
}

export function getActiveTrafficEvents(signal?: AbortSignal, limit?: number): Promise<TrafficEvent[]> {
  const query = limit === undefined ? '' : `?limit=${limit}`
  return getJson<TrafficEvent[]>(`/api/events${query}`, signal)
}

export function createTrafficEvent(
  intersectionId: number,
  request: CreateTrafficEventRequest,
): Promise<TrafficEvent> {
  return postJson<CreateTrafficEventRequest, TrafficEvent>(
    `/api/intersections/${intersectionId}/events`,
    request,
  )
}

export function acknowledgeTrafficEvent(eventId: string): Promise<TrafficEvent> {
  return patchJson<TrafficEvent>(`/api/events/${eventId}/acknowledge`)
}

export function resolveTrafficEvent(eventId: string): Promise<TrafficEvent> {
  return patchJson<TrafficEvent>(`/api/events/${eventId}/resolve`)
}
