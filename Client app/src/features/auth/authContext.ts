import { createContext } from 'react'
import type { AuthSession, CurrentUser, LoginRequest } from './authTypes'

export type AuthContextValue = {
  session: AuthSession | null
  user: CurrentUser | null
  login: (request: LoginRequest) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
