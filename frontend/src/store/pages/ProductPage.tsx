import { useParams } from 'react-router-dom'

export function ProductPage() {
  const { id } = useParams()
  return (
    <section>
      <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Tienda</p>
      <h1 className="mt-2 text-3xl font-bold">Detalle de producto</h1>
      <p className="mt-4 text-stone-600">Producto: {id}</p>
    </section>
  )
}
