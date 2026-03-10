import { createFileRoute, Link, Outlet, useNavigate } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  createLocation,
  deleteLocation,
  getLocations,
  type Location,
} from '../../services/adminLocationService'

export const Route = createFileRoute('/admin/locations')({
  component: AdminLocationsPage,
})

function AdminLocationsPage() {
  const navigate = useNavigate()

  const [locations, setLocations] = useState<Location[]>([])
  const [name, setName] = useState('')
  const [address, setAddress] = useState('')

  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function loadLocations() {
    try {
      setLoading(true)
      setError('')
      const data = await getLocations()
      setLocations(data)
    } catch (err) {
      setError('Failed to load locations.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadLocations()
  }, [])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!name.trim() || !address.trim()) {
      setMessage('Name and address are required.')
      return
    }

    try {
      setSubmitting(true)
      setError('')
      setMessage('')

      const created = await createLocation(name.trim(), address.trim())

      setName('')
      setAddress('')

      await navigate({
        to: '/admin/locations/$locationId/regions',
        params: { locationId: String(created.id) },
      })
    } catch (err) {
      setMessage(
        err instanceof Error ? err.message : 'Failed to create location.'
      )
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDelete(id: number) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this location?'
    )

    if (!confirmed) return

    try {
      setError('')
      setMessage('')

      await deleteLocation(id)
      setMessage('Location deleted successfully.')
      await loadLocations()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to delete location.'
      )
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <div className="mb-6">
        <Link to="/admin" className="text-sm underline">
          ← Back to admin
        </Link>
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold">Manage locations</h1>
        <p className="mt-2 text-gray-600">
          Add new locations and define seating regions.
        </p>
      </div>

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Add location</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium">
                Location name
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
                placeholder="Enter location name"
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Address
              </label>
              <input
                type="text"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
                placeholder="Enter address"
              />
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
            >
              {submitting ? 'Saving...' : 'Add location'}
            </button>
          </form>

          {message && <p className="mt-4 text-sm text-gray-700">{message}</p>}
          {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
        </div>

        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Existing locations</h2>

          {loading && <p>Loading locations...</p>}
          {!loading && error && <p className="text-red-600">{error}</p>}

          {!loading && !error && locations.length === 0 && (
            <p className="text-gray-600">No locations found.</p>
          )}

          {!loading && locations.length > 0 && (
            <div className="space-y-3">
              {locations.map((location) => (
                <div key={location.id} className="rounded-lg border p-4">
                  <div className="mb-3">
                    <p className="font-medium">{location.name}</p>
                    <p className="text-sm text-gray-500">{location.address}</p>
                    <p className="text-sm text-gray-400">ID: {location.id}</p>
                  </div>

                  <div className="flex flex-wrap gap-3">
                    <Link
                      to="/admin/locations/$locationId/regions"
                      params={{ locationId: String(location.id) }}
                      className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:opacity-90"
                    >
                      Manage regions
                    </Link>

                    <button
                      type="button"
                      onClick={() => handleDelete(location.id)}
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