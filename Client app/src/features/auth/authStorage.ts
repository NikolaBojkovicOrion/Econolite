import type { AuthSession } from './authTypes'

const storageKey = 'econolite.auth.session'

export function getStoredSession(): AuthSession | null {
  const value = sessionStorage.getItem(storageKey)
  if (!value) {
    return null
  }

  try {
    return JSON.parse(value) as AuthSession
  } catch {
    sessionStorage.removeItem(storageKey)
    return null
  }
}

export function storeSession(session: AuthSession): void {
  sessionStorage.setItem(storageKey, JSON.stringify(session))
}

export function clearStoredSession(): void {
  sessionStorage.removeItem(storageKey)
}

export function getAccessToken(): string | null {
  return getStoredSession()?.accessToken ?? null
}
