export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAtUtc: string
  email: string
  roles: string[]
}
