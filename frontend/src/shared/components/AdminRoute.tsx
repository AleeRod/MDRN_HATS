import { Navigate, Outlet } from 'react-router-dom'
import { hasAdminRole } from '../api/client'

export function AdminRoute() {
  const accessToken = localStorage.getItem('mdrn.accessToken')
  return accessToken && hasAdminRole()
    ? <Outlet />
    : <Navigate to="/admin/login" replace />
}
