export interface CurrentUser {
  id: string
  email: string
  roles: string[]
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  expiresAt: string
  user: CurrentUser
}

export interface AuthSession {
  accessToken: string
  expiresAt: string
  user: CurrentUser
}
