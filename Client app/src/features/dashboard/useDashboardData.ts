import { useEffect, useReducer } from 'react'
import { getIntersections } from '../intersections/intersectionApi'
import { getActiveTrafficEvents } from '../trafficEvents/trafficEventApi'
import type { DashboardStatus, Intersection, TrafficEvent } from '../../types/traffic'

export interface DashboardState {
  status: DashboardStatus
  intersections: Intersection[]
  events: TrafficEvent[]
}

type DashboardAction =
  | { type: 'loading' }
  | { type: 'loaded'; intersections: Intersection[]; events: TrafficEvent[] }
  | { type: 'failed' }

const initialDashboardState: DashboardState = {
  status: 'loading',
  intersections: [],
  events: [],
}

function dashboardReducer(state: DashboardState, action: DashboardAction): DashboardState {
  switch (action.type) {
    case 'loading':
      return { ...state, status: 'loading' }
    case 'loaded':
      return { status: 'ready', intersections: action.intersections, events: action.events }
    case 'failed':
      return { ...state, status: 'error' }
    default:
      return state
  }
}

export function useDashboardData(): { dashboard: DashboardState; retry: () => void } {
  const [dashboard, dispatch] = useReducer(dashboardReducer, initialDashboardState)

  async function loadDashboard(signal?: AbortSignal): Promise<void> {
    try {
      const [intersections, events] = await Promise.all([
        getIntersections(signal),
        getActiveTrafficEvents(signal),
      ])

      dispatch({ type: 'loaded', intersections, events })
    } catch (error) {
      if (!(error instanceof DOMException && error.name === 'AbortError')) {
        dispatch({ type: 'failed' })
      }
    }
  }

  useEffect(() => {
    const controller = new AbortController()
    void loadDashboard(controller.signal)

    return () => controller.abort()
  }, [])

  function retry(): void {
    dispatch({ type: 'loading' })
    void loadDashboard()
  }

  return { dashboard, retry }
}
