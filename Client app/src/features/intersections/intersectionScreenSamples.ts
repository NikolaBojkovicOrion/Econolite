import type { IntersectionStatus } from '../../types/traffic'

export type DetectorFreshness = 'Fresh' | 'Delayed' | 'Stale'

export interface IntersectionScreenItem {
  id: number
  name: string
  status: IntersectionStatus
  speedMph: number | null
  activeEventCount: number
  lastUpdate: string | null
  freshness: DetectorFreshness
  location: string
  latitude: string
  longitude: string
}

export interface IntersectionScreenEvent {
  id: string
  type: string
  severity: string
  status: string
  detectedAt: string
}

export const sampleIntersections: IntersectionScreenItem[] = [
  {
    id: 101,
    name: 'Harbor Boulevard / Katella Avenue',
    status: 'Healthy',
    speedMph: 38,
    activeEventCount: 0,
    lastUpdate: '2 min ago',
    freshness: 'Fresh',
    location: 'Harbor Boulevard & Katella Avenue',
    latitude: '33.8037',
    longitude: '-117.9180',
  },
  {
    id: 103,
    name: 'Lincoln Avenue / Euclid Street',
    status: 'Healthy',
    speedMph: 24,
    activeEventCount: 1,
    lastUpdate: '8 min ago',
    freshness: 'Delayed',
    location: 'Lincoln Avenue & Euclid Street',
    latitude: '33.8366',
    longitude: '-117.9412',
  },
  {
    id: 102,
    name: 'Main Street / Broadway',
    status: 'Degraded',
    speedMph: null,
    activeEventCount: 2,
    lastUpdate: '37 min ago',
    freshness: 'Stale',
    location: 'Main Street & Broadway',
    latitude: '33.7791',
    longitude: '-117.9137',
  },
  {
    id: 104,
    name: 'Maple Avenue / Seventh Street',
    status: 'Healthy',
    speedMph: 31,
    activeEventCount: 0,
    lastUpdate: '1 min ago',
    freshness: 'Fresh',
    location: 'Maple Avenue & Seventh Street',
    latitude: '33.8170',
    longitude: '-117.9274',
  },
  {
    id: 105,
    name: 'Civic Center Drive / First Street',
    status: 'Offline',
    speedMph: null,
    activeEventCount: 1,
    lastUpdate: null,
    freshness: 'Stale',
    location: 'Civic Center Drive & First Street',
    latitude: '33.8103',
    longitude: '-117.9221',
  },
  {
    id: 106,
    name: 'Garden Grove Boulevard / Brookhurst',
    status: 'Healthy',
    speedMph: 42,
    activeEventCount: 0,
    lastUpdate: '3 min ago',
    freshness: 'Fresh',
    location: 'Garden Grove Boulevard & Brookhurst Street',
    latitude: '33.7745',
    longitude: '-117.9552',
  },
  {
    id: 107,
    name: 'Chapman Avenue / State College Boulevard',
    status: 'Degraded',
    speedMph: 12,
    activeEventCount: 1,
    lastUpdate: '16 min ago',
    freshness: 'Delayed',
    location: 'Chapman Avenue & State College Boulevard',
    latitude: '33.7878',
    longitude: '-117.8901',
  },
  {
    id: 108,
    name: 'Orangewood Avenue / Lewis Street',
    status: 'Healthy',
    speedMph: 35,
    activeEventCount: 0,
    lastUpdate: '4 min ago',
    freshness: 'Fresh',
    location: 'Orangewood Avenue & Lewis Street',
    latitude: '33.8009',
    longitude: '-117.8893',
  },
]

export const sampleIntersectionEvents: IntersectionScreenEvent[] = [
  {
    id: '2048',
    type: 'SlowTraffic',
    severity: 'Medium',
    status: 'Active',
    detectedAt: 'Today, 10:41 AM',
  },
]