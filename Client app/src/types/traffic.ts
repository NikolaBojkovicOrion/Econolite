export type DashboardStatus = 'loading' | 'ready' | 'error'

export type IntersectionStatus = 'Healthy' | 'Degraded' | 'Offline'

export interface Intersection {
  id: number
  name: string
  latitude: number
  longitude: number
  status: IntersectionStatus | string
  lastDetectorUpdate: string | null
}

export interface TrafficEvent {
  id: string
  intersectionId: number
  type: string
  severity: string
  status: string
  detectedAt: string
}
