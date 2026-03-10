import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  createCurrency,
  deleteCurrency,
  getCurrencies,
  type Currency,
} from '../../services/adminCurrencyService'

export const Route = createFileRoute('/admin/currencies')({
  component: AdminCurrenciesPage,
})

function AdminCurrenciesPage() {
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [code, setCode] = useState('')
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function loadCurrencies() {
    try {
      setLoading(true)
      setError('')
      const data = await getCurrencies()
      setCurrencies(data)
    } catch (err) {
      setError('Failed to load currencies.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadCurrencies()
  }, [])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!code.trim()) {
      setMessage('Currency code is required.')
      return
    }

    try {
      setSubmitting(true)
      setError('')
      setMessage('')

      await createCurrency(code.trim().toUpperCase())
      setCode('')
      setMessage('Currency created successfully.')
      await loadCurrencies()
    } catch (err) {
      setMessage(
        err instanceof Error ? err.message : 'Failed to create currency.'
      )
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDelete(id: number) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this currency?'
    )

    if (!confirmed) return

    try {
      setError('')
      setMessage('')

      await deleteCurrency(id)
      setMessage('Currency deleted successfully.')
      await loadCurrencies()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to delete currency.'
      )
    }
  }

  return (
    <div className="mx-auto max-w-5xl px-6 py-12">
      <div className="mb-6">
        <Link to="/admin" className="text-sm underline">
          ← Back to admin
        </Link>
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold">Manage currencies</h1>
        <p className="mt-2 text-gray-600">Add allowed payment currencies.</p>
      </div>

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Add currency</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium">
                Currency code
              </label>
              <input
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value.toUpperCase())}
                className="w-full rounded-lg border px-3 py-2"
                placeholder="Enter code, e.g. EUR"
              />
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
            >
              {submitting ? 'Saving...' : 'Add currency'}
            </button>
          </form>

          {message && <p className="mt-4 text-sm text-gray-700">{message}</p>}
          {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
        </div>

        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Existing currencies</h2>

          {loading && <p>Loading currencies...</p>}
          {!loading && error && <p className="text-red-600">{error}</p>}

          {!loading && !error && currencies.length === 0 && (
            <p className="text-gray-600">No currencies found.</p>
          )}

          {!loading && currencies.length > 0 && (
            <div className="space-y-3">
              {currencies.map((currency) => (
                <div key={currency.id} className="rounded-lg border p-4">
                  <div className="mb-3">
                    <p className="font-medium">{currency.code}</p>
                    <p className="text-sm text-gray-400">ID: {currency.id}</p>
                  </div>

                  <button
                    type="button"
                    onClick={() => handleDelete(currency.id)}
                    className="rounded-lg bg-red-600 px-4 py-2 text-white hover:opacity-90"
                  >
                    Delete
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}