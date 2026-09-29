import type { DashboardStatus, Intersection, TrafficEvent } from '../../types/traffic'

export interface DashboardState {
  status: DashboardStatus
  intersections: Intersection[]
  events: TrafficEvent[]
  errorMessage: string | null
  traceId: string | null
}
