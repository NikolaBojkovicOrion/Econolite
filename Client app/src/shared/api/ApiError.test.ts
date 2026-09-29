import { ApiError } from './ApiError'

describe('ApiError', () => {
  it('exposes ProblemDetails detail and trace ID', () => {
    const error = new ApiError(503, {
      title: 'Service unavailable',
      detail: 'The database is unavailable.',
      traceId: 'trace-123',
    })

    expect(error.status).toBe(503)
    expect(error.message).toBe('The database is unavailable.')
    expect(error.traceId).toBe('trace-123')
  })
})
