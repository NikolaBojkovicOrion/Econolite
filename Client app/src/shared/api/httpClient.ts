import { getAccessToken } from '../../features/auth/authStorage'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5102'

export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    signal,
    headers: getAuthHeaders(),
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

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

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.json() as Promise<TResponse>
}

function getAuthHeaders(): HeadersInit {
  const token = getAccessToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}
