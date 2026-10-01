export type DashboardStatus = 'loading' | 'ready' | 'error'

export type IntersectionStatus = 'Healthy' | 'Degraded' | 'Offline'

export interface Intersection {
  id: number
  name: string
  latitude: number
  longitude: number
  status: IntersectionStatus | string
  lastDetectorUpdate: string | null
  freshness?: DetectorFreshness
}

export type DetectorFreshness = 'Fresh' | 'Delayed' | 'Stale'

export interface IntersectionSummary extends Intersection {
  speedMph: number | null
  freshness: DetectorFreshness
  activeEventCount: number
}

export interface IntersectionSummaryCounts {
  totalCount: number
  healthyCount: number
  delayedOrStaleCount: number
}

export interface IntersectionPageResponse {
  items: IntersectionSummary[]
  pageNumber: number
  pageSize: number
  totalCount: number
  summary: IntersectionSummaryCounts
}

export interface IntersectionDetailResponse {
  intersection: IntersectionSummary
  activeEvents: TrafficEvent[]
}

export interface TrafficEvent {
  id: string
  intersectionId: number
  type: string
  severity: string
  status: string
  detectedAt: string
}

export interface AuditEntry {
  id: string
  userId: string | null
  action: string
  entityType: string
  entityId: string
  createdAt: string
}
