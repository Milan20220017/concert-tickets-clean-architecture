import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  createRegion,
  getRegionsByLocation,
  type Region,
} from '../../services/adminRegionService'
import {
  getLocations,
  type Location,
} from '../../services/adminLocationService'

export const Route = createFileRoute('/admin/locations/$locationId/regions')({
  component: AdminLocationRegionsPage,
})

function AdminLocationRegionsPage() {
  const { locationId } = Route.useParams()

  const [regions, setRegions] = useState<Region[]>([])
  const [location, setLocation] = useState<Location | null>(null)

  const [name, setName] = useState('')
  const [capacity, setCapacity] = useState('')

  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function loadData() {
    try {
      setLoading(true)
      setError('')

      const numericLocationId = Number(locationId)

      const [regionsData, locationsData] = await Promise.all([
        getRegionsByLocation(numericLocationId),
        getLocations(),
      ])

      setRegions(regionsData)
      setLocation(
        locationsData.find((l) => l.id === numericLocationId) ?? null
      )
    } catch (err) {
      setError('Failed to load regions.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [locationId])

  const hasAtLeastOneRegion = regions.length > 0

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!name.trim() || !capacity.trim()) {
      setMessage('Name and capacity are required.')
      return
    }

    const numericCapacity = Number(capacity)

    if (!Number.isFinite(numericCapacity) || numericCapacity <= 0) {
      setMessage('Capacity must be a positive number.')
      return
    }

    try {
      setSubmitting(true)
      setMessage('')
      setError('')

      await createRegion(Number(locationId), name.trim(), numericCapacity)

      setName('')
      setCapacity('')
      setMessage('Region created successfully.')

      await loadData()
    } catch (err) {
      setMessage(err instanceof Error ? err.message : 'Failed to create region.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <div className="mb-6 flex flex-wrap gap-4">
        <Link to="/admin" className="text-sm underline">
          ← Back to admin
        </Link>

        {hasAtLeastOneRegion && (
          <Link to="/admin/locations" className="text-sm underline">
            ← Back to locations
          </Link>
        )}
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold">Manage regions</h1>
        <p className="mt-2 text-gray-600">
          {location
            ? `Location: ${location.name}`
            : `Location ID: ${locationId}`}
        </p>
      </div>

      {!loading && !hasAtLeastOneRegion && (
        <div className="mb-8 rounded-2xl border border-amber-300 bg-amber-50 p-5 text-amber-900">
          <p className="font-semibold">
            This location does not have seating regions yet.
          </p>
          <p className="mt-1 text-sm">
            Add at least one region before returning.
          </p>
        </div>
      )}

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Add region</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium">
                Region name
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">
                Capacity
              </label>
              <input
                type="number"
                value={capacity}
                onChange={(e) => setCapacity(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              />
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
            >
              {submitting ? 'Saving...' : 'Add region'}
            </button>
          </form>

          {message && <p className="mt-4 text-sm text-gray-700">{message}</p>}

          {hasAtLeastOneRegion && (
            <div className="mt-6">
              <Link
                to="/admin/locations"
                className="inline-block rounded-lg bg-green-600 px-5 py-3 text-white hover:opacity-90"
              >
                Done, return to locations
              </Link>
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Existing regions</h2>

          {loading && <p>Loading regions...</p>}
          {error && <p className="text-red-600">{error}</p>}

          {!loading && !error && regions.length === 0 && (
            <p className="text-gray-600">No regions yet.</p>
          )}

          {!loading && !error && regions.length > 0 && (
            <div className="space-y-3">
              {regions.map((region) => (
                <div key={region.id} className="rounded-lg border p-4">
                  <p className="font-medium">{region.name}</p>
                  <p className="text-sm text-gray-500">
                    Capacity: {region.capacity}
                  </p>
                  <p className="text-sm text-gray-400">ID: {region.id}</p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}