import { getAccessToken } from '../../features/auth/authStorage'
import { ApiError, type ProblemDetailsPayload } from './ApiError'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5102'

export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    signal,
    headers: getAuthHeaders(),
  })

  await throwForProblemResponse(response)

  return response.json() as Promise<T>
}

export async function postJson<TRequest, TResponse>(path: string, body: TRequest): Promise<TResponse> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: {
      ...getAuthHeaders(),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  })

  await throwForProblemResponse(response)

  return response.json() as Promise<TResponse>
}

export async function patchJson<TResponse>(path: string): Promise<TResponse> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'PATCH',
    headers: getAuthHeaders(),
  })

  await throwForProblemResponse(response)

  return response.json() as Promise<TResponse>
}

function getAuthHeaders(): HeadersInit {
  const token = getAccessToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function throwForProblemResponse(response: Response): Promise<void> {
  if (response.ok) {
    return
  }

  const problem = await response.json().catch((): ProblemDetailsPayload => ({}))
  throw new ApiError(response.status, problem as ProblemDetailsPayload)
}
