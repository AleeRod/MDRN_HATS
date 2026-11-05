import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AdminRoute } from '../shared/components/AdminRoute'
import { SiteLayout } from '../shared/components/SiteLayout'
import { AdminDashboardPage } from '../admin/pages/AdminDashboardPage'
import { AdminLoginPage } from '../admin/pages/AdminLoginPage'
import { AdminProductsPage } from '../admin/pages/AdminProductsPage'
import { CartPage } from '../store/pages/CartPage'
import { CatalogPage } from '../store/pages/CatalogPage'
import { HomePage } from '../store/pages/HomePage'
import { ProductPage } from '../store/pages/ProductPage'

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<SiteLayout />}>
          <Route index element={<HomePage />} />
          <Route path="catalog" element={<CatalogPage />} />
          <Route path="product/:id" element={<ProductPage />} />
          <Route path="cart" element={<CartPage />} />
          <Route path="admin/login" element={<AdminLoginPage />} />
          <Route element={<AdminRoute />}>
            <Route path="admin" element={<AdminDashboardPage />} />
            <Route path="admin/products" element={<AdminProductsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
