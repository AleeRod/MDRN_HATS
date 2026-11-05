import { Link } from 'react-router-dom'

export function AdminDashboardPage() {
  return (
    <section>
      <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Administración</p>
      <h1 className="mt-2 text-3xl font-bold">Dashboard</h1>
      <p className="mt-4 text-stone-600">Panel inicial de MDRN HATS.</p>
      <Link className="mt-6 inline-flex rounded-lg bg-stone-900 px-4 py-2 font-semibold text-white" to="/admin/products">
        Administrar productos
      </Link>
    </section>
  )
}
