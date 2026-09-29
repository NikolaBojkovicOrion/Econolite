import { postJson } from '../../shared/api/httpClient'
import type { LoginRequest, LoginResponse } from './authTypes'

export function login(request: LoginRequest): Promise<LoginResponse> {
  return postJson<LoginRequest, LoginResponse>('/api/auth/login', request)
}
