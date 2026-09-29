export interface ProblemDetailsPayload {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
}

export class ApiError extends Error {
  readonly status: number
  readonly traceId?: string
  readonly problem: ProblemDetailsPayload

  constructor(status: number, problem: ProblemDetailsPayload) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.traceId = problem.traceId
    this.problem = problem
  }
}
