import type { AuthResponse } from '../types/auth'

const apiBaseUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:8080').replace(/\/+$/, '')
const accessTokenKey = 'mdrn.accessToken'
const refreshTokenKey = 'mdrn.refreshToken'
const rolesKey = 'mdrn.roles'

export function saveAuthSession(session: AuthResponse) {
  localStorage.setItem(accessTokenKey, session.accessToken)
  localStorage.setItem(refreshTokenKey, session.refreshToken)
  localStorage.setItem(rolesKey, JSON.stringify(session.roles))
}

export function clearAuthSession() {
  localStorage.removeItem(accessTokenKey)
  localStorage.removeItem(refreshTokenKey)
  localStorage.removeItem(rolesKey)
}

export function hasAdminRole() {
  const roles = localStorage.getItem(rolesKey)
  if (!roles) {
    return false
  }

  try {
    const parsed: unknown = JSON.parse(roles)
    return Array.isArray(parsed) && parsed.includes('Admin')
  } catch {
    clearAuthSession()
    return false
  }
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const url = `${apiBaseUrl}/${path.replace(/^\/+/, '')}`
  const response = await sendRequest(url, init)
  if (response.status === 401 && !path.includes('/auth/')) {
    const refreshed = await refreshSession()
    if (refreshed) {
      const retry = await sendRequest(url, init)
      return readResponse<T>(retry)
    }
  }

  return readResponse<T>(response)
}

async function sendRequest(url: string, init: RequestInit) {
  const headers = new Headers(init.headers)
  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const accessToken = localStorage.getItem(accessTokenKey)
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  return fetch(url, { ...init, headers })
}

async function refreshSession() {
  const refreshToken = localStorage.getItem(refreshTokenKey)
  if (!refreshToken) {
    clearAuthSession()
    return false
  }

  const response = await fetch(`${apiBaseUrl}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  })
  if (!response.ok) {
    clearAuthSession()
    return false
  }

  saveAuthSession((await response.json()) as AuthResponse)
  return true
}

async function readResponse<T>(response: Response): Promise<T> {
  if (response.status === 204) {
    return undefined as T
  }

  const body: unknown = await response.json().catch(() => null)
  if (!response.ok) {
    if (typeof body === 'object' && body !== null && 'error' in body) {
      throw new Error(String(body.error))
    }
    throw new Error(`Request failed with status ${response.status}.`)
  }

  return body as T
}
