import { useEffect, useReducer } from 'react'
import { getIntersections } from '../intersections/intersectionApi'
import { getActiveTrafficEvents } from '../trafficEvents/trafficEventApi'
import type { DashboardState } from './dashboardTypes'
import type { Intersection, TrafficEvent } from '../../types/traffic'

const overviewItemLimit = 5

type DashboardAction =
  | { type: 'loading' }
  | { type: 'loaded'; intersections: Intersection[]; events: TrafficEvent[] }
  | { type: 'event-received'; event: TrafficEvent }
  | { type: 'failed'; message: string; traceId: string | null }

const initialDashboardState: DashboardState = {
  status: 'loading',
  intersections: [],
  events: [],
  errorMessage: null,
  traceId: null,
}

export function dashboardReducer(state: DashboardState, action: DashboardAction): DashboardState {
  switch (action.type) {
    case 'loading':
      return { ...state, status: 'loading' }
    case 'loaded':
      return { status: 'ready', intersections: action.intersections, events: action.events, errorMessage: null, traceId: null }
    case 'event-received': {
      const events = state.events.filter((event) => event.id !== action.event.id)
      if (action.event.status !== 'Resolved') {
        events.unshift(action.event)
      }

      return { ...state, events }
    }
    case 'failed':
      return { ...state, status: 'error', errorMessage: action.message, traceId: action.traceId }
    default:
      return state
  }
}

export function useDashboardData(): {
  dashboard: DashboardState
  retry: () => void
  receiveEvent: (event: TrafficEvent) => void
} {
  const [dashboard, dispatch] = useReducer(dashboardReducer, initialDashboardState)

  async function loadDashboard(signal?: AbortSignal): Promise<void> {
    try {
      const [intersections, events] = await Promise.all([
        getIntersections(signal, overviewItemLimit),
        getActiveTrafficEvents(signal, overviewItemLimit),
      ])

      dispatch({ type: 'loaded', intersections, events })
    } catch (error) {
      if (!(error instanceof DOMException && error.name === 'AbortError')) {
        const typedError = error as { message?: string; traceId?: string }
        dispatch({
          type: 'failed',
          message: typedError.message ?? 'Unable to load current data.',
          traceId: typedError.traceId ?? null,
        })
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

  function receiveEvent(event: TrafficEvent): void {
    dispatch({ type: 'event-received', event })
  }

  return { dashboard, retry, receiveEvent }
}
