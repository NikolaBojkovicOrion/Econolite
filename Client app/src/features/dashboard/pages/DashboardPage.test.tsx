import { render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getIntersections } from '../../intersections/intersectionApi'
import { getActiveTrafficEvents } from '../../trafficEvents/trafficEventApi'
import { DashboardPage } from './DashboardPage'

vi.mock('../../intersections/intersectionApi', () => ({
  getIntersections: vi.fn(),
}))

vi.mock('../../trafficEvents/trafficEventApi', () => ({
  getActiveTrafficEvents: vi.fn(),
}))

const mockedGetIntersections = vi.mocked(getIntersections)
const mockedGetActiveTrafficEvents = vi.mocked(getActiveTrafficEvents)

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loads and displays API data', async () => {
    mockedGetIntersections.mockResolvedValue([{
      id: 101,
      name: 'Harbor Boulevard / Katella Avenue',
      latitude: 33.8021,
      longitude: -117.9143,
      status: 'Healthy',
      lastDetectorUpdate: null,
    }])
    mockedGetActiveTrafficEvents.mockResolvedValue([{
      id: 'event-1',
      intersectionId: 101,
      type: 'Congestion',
      severity: 'High',
      status: 'Active',
      detectedAt: '2026-09-29T10:00:00Z',
    }])

    render(<DashboardPage />)

    expect(screen.getByText('Loading current intersection data...')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Harbor Boulevard / Katella Avenue')).toBeInTheDocument())

    expect(screen.getByText('Congestion at #101')).toBeInTheDocument()
    expect(screen.getByText('Intersections monitored')).toBeInTheDocument()
  })

  it('shows a recoverable error when the API fails', async () => {
    mockedGetIntersections.mockRejectedValue(new Error('API unavailable'))
    mockedGetActiveTrafficEvents.mockRejectedValue(new Error('API unavailable'))

    render(<DashboardPage />)

    await waitFor(() => expect(screen.getByText('Unable to load current data.')).toBeInTheDocument())
    expect(screen.getByRole('button', { name: 'Retry' })).toBeInTheDocument()
  })
})
