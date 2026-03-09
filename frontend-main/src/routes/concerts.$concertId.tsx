import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
// import { getConcertById } from '../services/concertService'
import { getRegionsByLocationId } from '../services/locationService'
import { createReservation } from '../services/reservationService'
import { getCurrencies } from '../services/currencyService'
import type { Currency } from '../services/currencyService'
import { getTicketPricesByConcert } from '../services/ticketPriceService'
import type { Concert } from '../types/concert'
import type { Region } from '../types/region'
import type { TicketPrice } from '../types/ticketPrice'
import type { RegionAvailability } from '#/types/regionAvailability'
import {
  getConcertById,
  getRegionsAvailabilityByConcert,
} from '../services/concertService'
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
  const [regions, setRegions] = useState<RegionAvailability[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [ticketPrices, setTicketPrices] = useState<TicketPrice[]>([])

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [email, setEmail] = useState('')
  const [currencyId, setCurrencyId] = useState('')
  const [regionSeatingId, setRegionSeatingId] = useState('')
  const [quantity, setQuantity] = useState('1')
  const [promoCode, setPromoCode] = useState('')
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

      const regionData = await getRegionsAvailabilityByConcert(Number(concertId))
      setRegions(regionData)

        const currencyData = await getCurrencies()
        setCurrencies(currencyData)

        if (currencyData.length > 0) {
          setCurrencyId(String(currencyData[0].id))
        }

        const pricesData = await getTicketPricesByConcert(Number(concertId))
        setTicketPrices(pricesData)
      } catch (err) {
        setError('Failed to load concert details.')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    loadData()
  }, [concertId])

  function getPriceForRegion(regionId: number, selectedCurrencyId: number) {
    return ticketPrices.find(
      (price) =>
        price.regionSeatingId === regionId &&
        price.currencyId === selectedCurrencyId
    )
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!concert) return

    const region = regions.find((r) => r.id === Number(regionSeatingId))

   if (region && Number(quantity) > region.availableSeats) {
  setSubmitMessage(
    `Only ${region.availableSeats} seats are available for ${region.name}.`
  )
  return
}

    try {
      setSubmitting(true)
      setSubmitMessage('')

      const result = await createReservation({
        concertId: concert.id,
        currencyId: Number(currencyId),
        email,
        promoCode: promoCode.trim() || undefined,
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
      setSubmitMessage('Failed to submit reservation request.')
      console.error(err)
    } finally {
      setSubmitting(false)
    }
  }

const visiblePrices = ticketPrices.filter(
  (price) => price.currency?.code?.toUpperCase() === 'EUR'
)

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
            <h2 className="mb-6 text-2xl font-bold">Ticket prices</h2>
 
            {visiblePrices.length === 0 ? (
              <p className="text-gray-600">
                No ticket prices available for selected currency.
              </p>
            ) : (
              <div className="grid gap-4 md:grid-cols-2">
                {visiblePrices.map((price) => (
                  <div key={price.id} className="rounded-xl border p-4">
                    <p className="mb-1 text-sm text-gray-500">
                      {price.regionSeating?.name ?? `Region ${price.regionSeatingId}`}
                    </p>
                    
                    <p className="text-lg font-semibold">
                      {price.amount} {price.currency?.code ?? ''}
                    </p>
     
                  </div>
                  
                ))}
                                                <p className="mb-4 text-sm text-gray-600">
        Base prices are shown in EUR. Final price will be converted to the selected currency during reservation.
      </p>
              </div>
            )}
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
                <label className="mb-1 block text-sm font-medium">Currency</label>
                <select
                  value={currencyId}
                  onChange={(e) => setCurrencyId(e.target.value)}
                  required
                  className="w-full rounded-lg border px-3 py-2"
                >
                  <option value="">Select currency</option>
                  {currencies.map((currency) => (
                    <option key={currency.id} value={currency.id}>
                      {currency.code} - {currency.name}
                    </option>
                  ))}
                </select>
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
                 {regions.map((region) => {
  const price = getPriceForRegion(region.id, Number(currencyId))

  return (
    <option key={region.id} value={region.id} disabled={region.availableSeats <= 0}>
      {region.name}
      {price ? ` - ${price.amount} ${price.currency?.code ?? ''}` : ''}
      {` (available: ${region.availableSeats} / ${region.capacity})`}
    </option>
  )
})}
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
                  Promo code (optional)
                </label>
                <input
                  type="text"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value.toUpperCase())}
                  className="w-full rounded-lg border px-3 py-2"
                  placeholder="Enter promo code"
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

                <p className="mt-1 text-2xl font-bold tracking-wide">
                  {createdLoginCode}
                </p>

                <p className="mt-2 text-sm text-gray-700">
                  Save this code and use it later with your email to check reservation status.
                </p>

                <Link
                  to="/reservation-check"
                  className="mt-3 inline-block rounded-lg bg-black px-4 py-2 text-white hover:opacity-90"
                >
                  Check reservation status
                </Link>
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