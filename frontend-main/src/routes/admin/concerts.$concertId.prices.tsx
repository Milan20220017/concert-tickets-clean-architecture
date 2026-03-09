import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useMemo, useState } from 'react'
import {
  getConcerts,
  type Concert,
} from '../../services/adminConcertService'
import {
  getCurrencies,
  type Currency,
} from '../../services/adminCurrencyService'
import {
  getRegionsByLocation,
  type Region,
} from '../../services/adminRegionService'
import {
  getTicketPricesByConcert,
  upsertTicketPrice,
  type TicketPrice,
} from '../../services/adminTicketPriceService'

export const Route = createFileRoute('/admin/concerts/$concertId/prices')({
  component: AdminConcertPricesPage,
})

function AdminConcertPricesPage() {
  const { concertId } = Route.useParams()

  const [concert, setConcert] = useState<Concert | null>(null)
  const [regions, setRegions] = useState<Region[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [ticketPrices, setTicketPrices] = useState<TicketPrice[]>([])

  const [regionSeatingId, setRegionSeatingId] = useState('')
  const [currencyId, setCurrencyId] = useState('')
  const [amount, setAmount] = useState('')

  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function loadData() {
    try {
      setLoading(true)
      setError('')

      const numericConcertId = Number(concertId)

      const concertsData = await getConcerts()
      const selectedConcert =
        concertsData.find((c) => c.id === numericConcertId) ?? null

      setConcert(selectedConcert)

      if (!selectedConcert) {
        setError('Concert not found.')
        return
      }

      const [regionsData, currenciesData, ticketPricesData] = await Promise.all([
        getRegionsByLocation(selectedConcert.locationId),
        getCurrencies(),
        getTicketPricesByConcert(numericConcertId),
      ])

      setRegions(regionsData)
      setCurrencies(currenciesData)
      setTicketPrices(ticketPricesData)

      if (regionsData.length > 0 && !regionSeatingId) {
        setRegionSeatingId(String(regionsData[0].id))
      }

      if (currenciesData.length > 0 && !currencyId) {
        setCurrencyId(String(currenciesData[0].id))
      }
    } catch (err) {
      setError('Failed to load ticket prices data.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [concertId])

  const hasAtLeastOnePrice = ticketPrices.length > 0

  const selectedExistingPrice = useMemo(() => {
    if (!regionSeatingId || !currencyId) return null

    return (
      ticketPrices.find(
        (price) =>
          price.regionSeatingId === Number(regionSeatingId) &&
          price.currencyId === Number(currencyId)
      ) ?? null
    )
  }, [ticketPrices, regionSeatingId, currencyId])

  useEffect(() => {
    if (selectedExistingPrice) {
      setAmount(String(selectedExistingPrice.amount))
    } else {
      setAmount('')
    }
  }, [selectedExistingPrice])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!regionSeatingId || !currencyId || !amount.trim()) {
      setMessage('Region, currency and amount are required.')
      return
    }

    const numericAmount = Number(amount)

    if (!Number.isFinite(numericAmount) || numericAmount <= 0) {
      setMessage('Amount must be a positive number.')
      return
    }

    try {
      setSubmitting(true)
      setError('')
      setMessage('')

      await upsertTicketPrice({
        concertId: Number(concertId),
        regionSeatingId: Number(regionSeatingId),
        currencyId: Number(currencyId),
        amount: numericAmount,
      })

      setMessage('Ticket price saved successfully.')
      await loadData()
    } catch (err) {
      setMessage(
        err instanceof Error ? err.message : 'Failed to save ticket price.'
      )
    } finally {
      setSubmitting(false)
    }
  }

  function getRegionName(regionId: number) {
    return regions.find((r) => r.id === regionId)?.name ?? `Region ${regionId}`
  }

  function getCurrencyCode(currencyIdValue: number) {
    return (
      currencies.find((c) => c.id === currencyIdValue)?.code ??
      `Currency ${currencyIdValue}`
    )
  }

  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <div className="mb-6 flex flex-wrap gap-4">
        <Link to="/admin" className="text-sm underline">
          ← Back to admin
        </Link>

        {hasAtLeastOnePrice && (
          <Link to="/admin/concerts" className="text-sm underline">
            ← Back to concerts
          </Link>
        )}
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold">Manage ticket prices</h1>
        <p className="mt-2 text-gray-600">
          {concert
            ? `${concert.name} • ${concert.locationName}`
            : `Concert ID: ${concertId}`}
        </p>
      </div>

      {!loading && !hasAtLeastOnePrice && (
        <div className="mb-8 rounded-2xl border border-amber-300 bg-amber-50 p-5 text-amber-900">
          <p className="font-semibold">
            This concert does not have ticket prices yet.
          </p>
          <p className="mt-1 text-sm">
            Define at least one ticket price before returning to the concerts
            list. A concert without prices cannot be used properly for
            reservations.
          </p>
        </div>
      )}

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">
            Add or update ticket price
          </h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium">Region</label>
              <select
                value={regionSeatingId}
                onChange={(e) => setRegionSeatingId(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              >
                <option value="">Select region</option>
                {regions.map((region) => (
                  <option key={region.id} value={region.id}>
                    {region.name} (capacity: {region.capacity})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">Currency</label>
              <select
                value={currencyId}
                onChange={(e) => setCurrencyId(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
              >
                <option value="">Select currency</option>
                {currencies.map((currency) => (
                  <option key={currency.id} value={currency.id}>
                    {currency.code}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium">Amount</label>
              <input
                type="number"
                min="0.01"
                step="0.01"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
                placeholder="Enter ticket price"
              />
              <p className="mt-1 text-xs text-gray-500">
                If a price already exists for selected region and currency, this
                form will update it.
              </p>
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
            >
              {submitting ? 'Saving...' : 'Save ticket price'}
            </button>
          </form>

          {message && <p className="mt-4 text-sm text-gray-700">{message}</p>}

          {hasAtLeastOnePrice && (
            <div className="mt-6">
              <Link
                to="/admin/concerts"
                className="inline-block rounded-lg bg-green-600 px-5 py-3 text-white hover:opacity-90"
              >
                Done, return to concerts
              </Link>
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Existing ticket prices</h2>

          {loading && <p>Loading ticket prices...</p>}
          {error && <p className="text-red-600">{error}</p>}

          {!loading && !error && ticketPrices.length === 0 && (
            <p className="text-gray-600">
              No ticket prices found for this concert.
            </p>
          )}

          {!loading && !error && ticketPrices.length > 0 && (
            <div className="space-y-3">
              {ticketPrices.map((price) => (
                <div key={price.id} className="rounded-lg border p-4">
                  <p className="font-medium">
                    {price.regionSeating?.name ??
                      getRegionName(price.regionSeatingId)}
                  </p>
                  <p className="text-sm text-gray-500">
                    {price.amount}{' '}
                    {price.currency?.code ?? getCurrencyCode(price.currencyId)}
                  </p>
                  <p className="text-sm text-gray-400">ID: {price.id}</p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}