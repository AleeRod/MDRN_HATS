import { Link } from 'react-router-dom'

export function HomePage() {
  return (
    <section className="rounded-3xl bg-stone-900 px-6 py-16 text-white sm:px-12 sm:py-24">
      <p className="mb-4 text-sm font-semibold uppercase tracking-[0.25em] text-amber-300">MDRN HATS</p>
      <h1 className="max-w-2xl text-4xl font-black sm:text-6xl">Tu estilo, desde la cabeza.</h1>
      <p className="mt-5 max-w-xl text-stone-300">La tienda está en preparación. Muy pronto encontrarás aquí la colección.</p>
      <Link className="mt-8 inline-flex rounded-full bg-white px-6 py-3 font-semibold text-stone-900" to="/catalog">
        Explorar catálogo
      </Link>
    </section>
  )
}
