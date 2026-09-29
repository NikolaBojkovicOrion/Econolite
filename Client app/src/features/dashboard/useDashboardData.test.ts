import { describe, expect, it } from 'vitest'
import { dashboardReducer, type DashboardState } from './useDashboardData'

const initialState: DashboardState = {
  status: 'loading',
  intersections: [],
  events: [],
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

    const nextState = dashboardReducer(loadedState, { type: 'failed' })

    expect(nextState.status).toBe('error')
    expect(nextState.intersections).toHaveLength(1)
  })
})
