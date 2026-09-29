import { dashboardReducer } from './useDashboardData'
import type { DashboardState } from './dashboardTypes'

const initialState: DashboardState = {
  status: 'loading',
  intersections: [],
  events: [],
  errorMessage: null,
  traceId: null,
}

describe('dashboardReducer', () => {
  it('moves loaded data into the ready state', () => {
    const nextState = dashboardReducer(initialState, {
      type: 'loaded',
      intersections: [{
        id: 101,
        name: 'Harbor Boulevard / Katella Avenue',
        latitude: 33.8021,
        longitude: -117.9143,
        status: 'Healthy',
        lastDetectorUpdate: null,
      }],
      events: [],
    })

    expect(nextState.status).toBe('ready')
    expect(nextState.intersections).toHaveLength(1)
  })

  it('keeps loaded data while showing an error state', () => {
    const loadedState: DashboardState = {
      ...initialState,
      status: 'ready',
      intersections: [{
        id: 101,
        name: 'Harbor Boulevard / Katella Avenue',
        latitude: 33.8021,
        longitude: -117.9143,
        status: 'Healthy',
        lastDetectorUpdate: null,
      }],
    }

    const nextState = dashboardReducer(loadedState, {
      type: 'failed',
      message: 'API unavailable',
      traceId: 'trace-123',
    })

    expect(nextState.status).toBe('error')
    expect(nextState.intersections).toHaveLength(1)
  })
})
