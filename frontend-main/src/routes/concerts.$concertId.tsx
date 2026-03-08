import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import { getConcertById } from '../services/concertService'
import { getRegionsByLocationId } from '../services/locationService'
import { createReservation } from '../services/reservationService'
import type { Concert } from '../types/concert'
import type { Region } from '../types/region'

export const Route = createFileRoute('/concerts/$concertId')({
  component: ConcertDetailsPage,
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

function ConcertDetailsPage() {
  const { concertId } = Route.useParams()

  const [concert, setConcert] = useState<Concert | null>(null)
  const [regions, setRegions] = useState<Region[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [email, setEmail] = useState('')
  const [currencyId, setCurrencyId] = useState('1')
  const [regionSeatingId, setRegionSeatingId] = useState('')
  const [quantity, setQuantity] = useState('1')
  const [usedPromoCodeId, setUsedPromoCodeId] = useState('')
  const [submitMessage, setSubmitMessage] = useState('')
  const [submitting, setSubmitting] = useState(false)
const [createdLoginCode, setCreatedLoginCode] = useState('')
  useEffect(() => {
    async function loadData() {
      try {
        setLoading(true)
        setError('')
        setSubmitMessage('')

        const concertData = await getConcertById(Number(concertId))
        setConcert(concertData)

        const regionData = await getRegionsByLocationId(concertData.locationId)
        setRegions(regionData)
      } catch (err) {
        setError('Failed to load concert details.')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    loadData()
  }, [concertId])

async function handleSubmit(e: React.FormEvent) {
  e.preventDefault()

  if (!concert) return

  const region = regions.find(r => r.id === Number(regionSeatingId))

  if (region && Number(quantity) > region.capacity) {
    setSubmitMessage(`Maximum tickets for ${region.name} is ${region.capacity}`)
    return
  }

  try {
    setSubmitting(true)
    setSubmitMessage("")

const result = await createReservation({
  concertId: concert.id,
  currencyId: Number(currencyId),
  email,
  usedPromoCodeId: usedPromoCodeId ? Number(usedPromoCodeId) : undefined,
  items: [
    {
      regionSeatingId: Number(regionSeatingId),
      quantity: Number(quantity),
    },
  ],
})

setCreatedLoginCode(result.loginCode)
setSubmitMessage('Reservation request has been sent. Save your reservation code.')
  } catch (err) {
    setSubmitMessage("Failed to submit reservation request.")
    console.error(err)
  } finally {
    setSubmitting(false)
  }
}

  return (
    <div className="mx-auto max-w-4xl px-6 py-12">
      <div className="mb-6">
        <Link to="/concerts" className="text-sm underline">
          ← Back to concerts
        </Link>
      </div>

      {loading && <p>Loading concert details...</p>}
      {error && <p className="text-red-600">{error}</p>}

      {!loading && !error && concert && (
        <div className="space-y-8">
          <div className="rounded-2xl border border-gray-300 bg-white p-8 shadow-sm">
            <h1 className="mb-2 text-4xl font-bold">{concert.name}</h1>
            <p className="mb-8 text-gray-600">Concert ID: {concert.id}</p>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="rounded-xl border p-4">
                <p className="mb-1 text-sm text-gray-500">Date</p>
                <p className="text-lg font-medium">
                  {formatConcertDate(concert.date)}
                </p>
              </div>

              <div className="rounded-xl border p-4">
                <p className="mb-1 text-sm text-gray-500">Category</p>
                <p className="text-lg font-medium">{concert.categoryName}</p>
              </div>

              <div className="rounded-xl border p-4 md:col-span-2">
                <p className="mb-1 text-sm text-gray-500">Location</p>
                <p className="text-lg font-medium">{concert.locationName}</p>
              </div>
            </div>
          </div>

          <div className="rounded-2xl border border-gray-300 bg-white p-8 shadow-sm">
            <h2 className="mb-6 text-2xl font-bold">Reserve tickets</h2>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="mb-1 block text-sm font-medium">Email</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  className="w-full rounded-lg border px-3 py-2"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium">Currency ID</label>
                <input
                  type="number"
                  value={currencyId}
                  onChange={(e) => setCurrencyId(e.target.value)}
                  required
                  min="1"
                  className="w-full rounded-lg border px-3 py-2"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium">Region</label>
                <select
                  value={regionSeatingId}
                  onChange={(e) => setRegionSeatingId(e.target.value)}
                  required
                  className="w-full rounded-lg border px-3 py-2"
                >
                  <option value="">Select a region</option>
                  {regions.map((region) => (
                    <option key={region.id} value={region.id}>
                    {region.name} (capacity: {region.capacity})
                  </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium">Quantity</label>
                <input
                  type="number"
                  value={quantity}
                  onChange={(e) => setQuantity(e.target.value)}
                  required
                  min="1"
                  className="w-full rounded-lg border px-3 py-2"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium">
                  Promo code ID (optional)
                </label>
                <input
                  type="number"
                  value={usedPromoCodeId}
                  onChange={(e) => setUsedPromoCodeId(e.target.value)}
                  className="w-full rounded-lg border px-3 py-2"
                />
              </div>
                    
              <button
                type="submit"
                disabled={submitting}
                className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
              >
                {submitting ? 'Submitting...' : 'Send reservation request'}
              </button>
            </form>
                {createdLoginCode && (
  <div className="mt-4 rounded-lg border border-green-300 bg-green-50 p-4">
    <p className="font-medium">Your reservation code:</p>
    <p className="mt-1 text-2xl font-bold tracking-wide">{createdLoginCode}</p>
    <p className="mt-2 text-sm text-gray-700">
      Save this code and use it later with your email to check reservation status.
    </p>
  </div>
)}
            {submitMessage && (
              <p className="mt-4 text-sm text-gray-700">{submitMessage}</p>
            )}
          </div>
        </div>
      )}
    </div>
  )
}