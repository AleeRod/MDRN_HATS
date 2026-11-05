import { Link, Outlet } from 'react-router-dom'

export function SiteLayout() {
  return (
    <div className="min-h-screen bg-stone-50 text-stone-950">
      <header className="border-b border-stone-200 bg-white">
        <nav className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4 sm:px-6">
          <Link className="text-xl font-black tracking-[0.2em]" to="/">
            MDRN HATS
          </Link>
          <div className="flex items-center gap-4 text-sm font-medium sm:gap-6">
            <Link to="/catalog">Catálogo</Link>
            <Link to="/cart">Carrito</Link>
            <Link to="/admin/login">Admin</Link>
          </div>
        </nav>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
        <Outlet />
      </main>
      <footer className="border-t border-stone-200 px-4 py-6 text-center text-sm text-stone-500">
        MDRN HATS
      </footer>
    </div>
  )
}
