import { type PropsWithChildren, useState } from 'react'
import { login as loginRequest } from './authApi'
import { clearStoredSession, getStoredSession, storeSession } from './authStorage'
import { AuthContext } from './authContext'
import type { AuthSession, LoginRequest } from './authTypes'

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSession | null>(() => getStoredSession())

  async function login(request: LoginRequest): Promise<void> {
    const response = await loginRequest(request)
    const nextSession: AuthSession = {
      accessToken: response.accessToken,
      expiresAt: response.expiresAt,
      user: response.user,
    }

    storeSession(nextSession)
    setSession(nextSession)
  }

  function logout(): void {
    clearStoredSession()
    setSession(null)
  }

  return (
    <AuthContext.Provider value={{ session, user: session?.user ?? null, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
