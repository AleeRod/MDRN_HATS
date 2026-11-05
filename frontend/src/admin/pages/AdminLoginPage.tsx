import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiRequest, saveAuthSession } from '../../shared/api/client'
import type { AuthResponse } from '../../shared/types/auth'

export function AdminLoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      const session = await apiRequest<AuthResponse>('api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      })
      if (!session.roles.includes('Admin')) {
        throw new Error('Esta cuenta no tiene acceso de administrador.')
      }
      saveAuthSession(session)
      navigate('/admin', { replace: true })
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'No se pudo iniciar sesión.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="mx-auto max-w-md rounded-2xl border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Administración</p>
      <h1 className="mt-2 text-3xl font-bold">Iniciar sesión</h1>
      <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
        <label className="block text-sm font-medium">
          Correo electrónico
          <input
            className="mt-1 w-full rounded-lg border border-stone-300 px-3 py-2"
            autoComplete="username"
            type="email"
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </label>
        <label className="block text-sm font-medium">
          Contraseña
          <input
            className="mt-1 w-full rounded-lg border border-stone-300 px-3 py-2"
            autoComplete="current-password"
            type="password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </label>
        {error && <p className="text-sm text-red-700" role="alert">{error}</p>}
        <button
          className="w-full rounded-lg bg-stone-900 px-4 py-3 font-semibold text-white disabled:opacity-60"
          disabled={submitting}
          type="submit"
        >
          {submitting ? 'Ingresando…' : 'Ingresar'}
        </button>
      </form>
    </section>
  )
}
