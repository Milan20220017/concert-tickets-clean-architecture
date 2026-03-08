import { createFileRoute, Link, Outlet } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import { getConcerts } from '../services/concertService'
import type { Concert } from '../types/concert'

export const Route = createFileRoute('/concerts')({
  component: ConcertsPage,
})

function formatConcertDate(dateString: string) {
  const date = new Date(dateString)

  return new Intl.DateTimeFormat('sr-RS', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}

function ConcertsPage() {
  const [concerts, setConcerts] = useState<Concert[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadConcerts() {
      try {
        setLoading(true)
        setError('')
        const data = await getConcerts()
        setConcerts(data)
      } catch (err) {
        setError('Failed to load concerts.')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    loadConcerts()
  }, [])

  return (
    <div className="mx-auto max-w-5xl px-6 py-12">
      <h1 className="mb-6 text-3xl font-bold">Concerts</h1>

      {loading && <p>Loading concerts...</p>}
      {error && <p className="text-red-600">{error}</p>}
      {!loading && !error && concerts.length === 0 && <p>No concerts found.</p>}

      {!loading && !error && concerts.length > 0 && (
        <div className="grid gap-5">
          {concerts.map((concert) => (
            <div
              key={concert.id}
              className="rounded-xl border border-gray-300 bg-white p-5 shadow-sm"
            >
              <div className="mb-3 flex items-start justify-between gap-4">
                <div>
                  <h2 className="text-xl font-semibold">{concert.name}</h2>
                  <p className="text-sm text-gray-600">
                    Concert ID: {concert.id}
                  </p>
                </div>

                <span className="rounded-full border px-3 py-1 text-sm">
                  {concert.categoryName}
                </span>
              </div>

              <div className="space-y-1 text-sm text-gray-700">
                <p>
                  <span className="font-medium">Date:</span>{' '}
                  {formatConcertDate(concert.date)}
                </p>
                <p>
                  <span className="font-medium">Location:</span>{' '}
                  {concert.locationName}
                </p>
              </div>

              <div className="mt-4">
                <Link
                  to="/concerts/$concertId"
                  params={{ concertId: concert.id.toString() }}
                  className="inline-block rounded-lg bg-black px-4 py-2 text-white hover:opacity-90"
                >
                  View details
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}

      <Outlet />
    </div>
  )
}