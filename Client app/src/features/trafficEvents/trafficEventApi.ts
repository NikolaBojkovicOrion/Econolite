import { getJson } from '../../shared/api/httpClient'
import type { TrafficEvent } from '../../types/traffic'

export function getActiveTrafficEvents(signal?: AbortSignal): Promise<TrafficEvent[]> {
  return getJson<TrafficEvent[]>('/api/events', signal)
}
