import { createFileRoute, Link, Outlet, useNavigate } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  createConcert,
  deleteConcert,
  getConcerts,
  type Concert,
} from '../../services/adminConcertService'
import {
  getCategories,
  type Category,
} from '../../services/adminCategoryService'
import {
  getLocations,
  type Location,
} from '../../services/adminLocationService'

export const Route = createFileRoute('/admin/concerts')({
  component: AdminConcertsPage,
})

function AdminConcertsPage() {
  const navigate = useNavigate()

  const [concerts, setConcerts] = useState<Concert[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [locations, setLocations] = useState<Location[]>([])

  const [name, setName] = useState('')
  const [date, setDate] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [locationId, setLocationId] = useState('')
  const [earlyBirdDiscountUntil, setEarlyBirdDiscountUntil] = useState('')

  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function loadData() {
    try {
      setLoading(true)
      setError('')

      const [concertsData, categoriesData, locationsData] = await Promise.all([
        getConcerts(),
        getCategories(),
        getLocations(),
      ])

      setConcerts(concertsData)
      setCategories(categoriesData)
      setLocations(locationsData)
    } catch (err) {
      setError('Failed to load concerts data.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!name.trim() || !date || !categoryId || !locationId) {
      setMessage('All required fields must be filled.')
      return
    }

    if (
      earlyBirdDiscountUntil &&
      new Date(earlyBirdDiscountUntil) > new Date(date)
    ) {
      setMessage('Early bird discount date must be before concert date.')
      return
    }

    try {
      setSubmitting(true)
      setError('')
      setMessage('')

      const created = await createConcert({
        name: name.trim(),
        date,
        categoryId: Number(categoryId),
        locationId: Number(locationId),
        earlyBirdDiscountUntil: earlyBirdDiscountUntil || undefined,
      })

      setName('')
      setDate('')
      setCategoryId('')
      setLocationId('')
      setEarlyBirdDiscountUntil('')

      await navigate({
        to: '/admin/concerts/$concertId/prices',
        params: { concertId: String(created.id) },
      })
    } catch (err) {
      setMessage(
        err instanceof Error ? err.message : 'Failed to create concert.'
      )
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDelete(id: number) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this concert?'
    )

    if (!confirmed) return

    try {
      setError('')
      setMessage('')

      await deleteConcert(id)
      setMessage('Concert deleted successfully.')
      await loadData()
    } catch (err) {
      setMessage(
        err instanceof Error ? err.message : 'Failed to delete concert.'
      )
    }
  }

  return (
    <div className="mx-auto max-w-7xl px-6 py-12">
      <div className="mb-6">
        <Link to="/admin" className="text-sm underline">
          ← Back to admin
        </Link>
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold">Manage concerts</h1>
        <p className="mt-2 text-gray-600">
          Schedule concerts and continue immediately to ticket price setup.
        </p>
      </div>

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Add concert</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium">
                Concert name
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
                placeholder="Enter concert name"
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Concert date and time
              </label>
              <input
                type="datetime-local"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Early bird discount until
              </label>
              <input
                type="datetime-local"
                value={earlyBirdDiscountUntil}
                onChange={(e) => setEarlyBirdDiscountUntil(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              />
              <p className="mt-1 text-xs text-gray-500">
                Optional. Until this date, 10% discount will be applied.
              </p>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Category
              </label>
              <select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              >
                <option value="">Select category</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Location
              </label>
              <select
                value={locationId}
                onChange={(e) => setLocationId(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              >
                <option value="">Select location</option>
                {locations.map((location) => (
                  <option key={location.id} value={location.id}>
                    {location.name}
                  </option>
                ))}
              </select>
              
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
            >
              {submitting ? 'Saving...' : 'Add concert'}
            </button>
          </form>

          {message && <p className="mt-4 text-sm text-gray-700">{message}</p>}
        </div>

        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Existing concerts</h2>

          {loading && <p>Loading concerts...</p>}
          {error && <p className="text-red-600">{error}</p>}

          {!loading && !error && concerts.length === 0 && (
            <p className="text-gray-600">No concerts found.</p>
          )}

          {!loading && !error && concerts.length > 0 && (
            <div className="space-y-3">
              {concerts.map((concert) => (
                <div key={concert.id} className="rounded-lg border p-4">
                  <div className="mb-3">
                    <p className="font-medium">{concert.name}</p>
                    <p className="text-sm text-gray-500">
                      {concert.categoryName} • {concert.locationName}
                    </p>
                    <p className="text-sm text-gray-500">
                      Concert date: {new Date(concert.date).toLocaleString()}
                    </p>

                    {concert.earlyBirdDiscountUntil && (
                      <p className="text-sm text-gray-500">
                        Early bird until:{' '}
                        {new Date(
                          concert.earlyBirdDiscountUntil
                        ).toLocaleString()}
                      </p>
                    )}

                    <p className="text-sm text-gray-400">ID: {concert.id}</p>
                  </div>
                      
                  <div className="flex flex-wrap gap-3">
                    <Link
                      to="/admin/concerts/$concertId/prices"
                      params={{ concertId: String(concert.id) }}
                      className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:opacity-90"
                    >
                      Manage ticket prices
                    </Link>

                    <button
                      type="button"
                      onClick={() => handleDelete(concert.id)}
                      className="rounded-lg bg-red-600 px-4 py-2 text-white hover:opacity-90"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
      <Outlet />
    </div>
  )
}