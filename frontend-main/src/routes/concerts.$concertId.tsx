import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  getConcertById,
  getRegionsAvailabilityByConcert,
} from '../services/concertService'
import { validatePromoCode } from '../services/promoCodeService'
import {
  createReservation,
  calculateReservation,
} from '../services/reservationService'
import { getCurrencies } from '../services/currencyService'
import type { Currency } from '../services/currencyService'
import { getTicketPricesByConcert } from '../services/ticketPriceService'
import type { Concert } from '../types/concert'
import type { TicketPrice } from '../types/ticketPrice'
import type { RegionAvailability } from '../types/regionAvailability'

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

function formatMoney(value: number | null | undefined) {
  if (value === null || value === undefined) return '-'

  return new Intl.NumberFormat('sr-RS', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}

function ConcertDetailsPage() {
  const { concertId } = Route.useParams()

  const [concert, setConcert] = useState<Concert | null>(null)
  const [regions, setRegions] = useState<RegionAvailability[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [ticketPrices, setTicketPrices] = useState<TicketPrice[]>([])

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [promoValidationMessage, setPromoValidationMessage] = useState('')
  const [promoValidationSuccess, setPromoValidationSuccess] = useState<boolean | null>(null)
  const [validatingPromo, setValidatingPromo] = useState(false)

  const [email, setEmail] = useState('')
  const [currencyId, setCurrencyId] = useState('')
  const [promoCode, setPromoCode] = useState('')
  const [submitMessage, setSubmitMessage] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [createdLoginCode, setCreatedLoginCode] = useState('')

  const [calculation, setCalculation] = useState<any>(null)
  const [calculating, setCalculating] = useState(false)

  const [reservationItems, setReservationItems] = useState([
    { regionSeatingId: '', quantity: '1' },
  ])

  useEffect(() => {
    async function loadData() {
      try {
        setLoading(true)
        setError('')
        setSubmitMessage('')

        const numericConcertId = Number(concertId)

        const concertData = await getConcertById(numericConcertId)
        setConcert(concertData)

        const regionData = await getRegionsAvailabilityByConcert(numericConcertId)
        setRegions(regionData)

        const currencyData = await getCurrencies()
        setCurrencies(currencyData)

        if (currencyData.length > 0) {
          setCurrencyId(String(currencyData[0].id))
        }

        const pricesData = await getTicketPricesByConcert(numericConcertId)
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

  function addReservationItem() {
    setReservationItems((prev) => [
      ...prev,
      { regionSeatingId: '', quantity: '1' },
    ])
  }

  function removeReservationItem(index: number) {
    setReservationItems((prev) => prev.filter((_, i) => i !== index))
  }

  function updateReservationItem(
    index: number,
    field: 'regionSeatingId' | 'quantity',
    value: string
  ) {
    setReservationItems((prev) =>
      prev.map((item, i) =>
        i === index ? { ...item, [field]: value } : item
      )
    )
  }

  async function handleValidatePromoCode() {
    if (!promoCode.trim()) {
      setPromoValidationSuccess(false)
      setPromoValidationMessage('Enter a promo code first.')
      return
    }

    try {
      setValidatingPromo(true)
      setPromoValidationMessage('')
      setPromoValidationSuccess(null)

      const result = await validatePromoCode(promoCode.trim())

      setPromoValidationSuccess(result.isValid)
      setPromoValidationMessage(
        result.isValid && result.discountPercent
          ? `${result.message} Discount: ${result.discountPercent}%.`
          : result.message
      )
    } catch (err) {
      setPromoValidationSuccess(false)
      setPromoValidationMessage(
        err instanceof Error ? err.message : 'Failed to validate promo code.'
      )
    } finally {
      setValidatingPromo(false)
    }
  }

  async function handleCalculatePrice() {
    if (!currencyId) {
      setSubmitMessage('Please select a currency.')
      return
    }

    const hasEmptyRegion = reservationItems.some(
      (item) => !item.regionSeatingId.trim()
    )
    if (hasEmptyRegion) {
      setSubmitMessage('Please select a region for each row.')
      return
    }

    const hasInvalidQuantity = reservationItems.some(
      (item) => !item.quantity.trim() || Number(item.quantity) <= 0
    )
    if (hasInvalidQuantity) {
      setSubmitMessage('Each quantity must be greater than 0.')
      return
    }

    const preparedItems = reservationItems.map((item) => ({
      regionSeatingId: Number(item.regionSeatingId),
      quantity: Number(item.quantity),
    }))

    const regionIds = preparedItems.map((item) => item.regionSeatingId)
    const hasDuplicates = new Set(regionIds).size !== regionIds.length

    if (hasDuplicates) {
      setSubmitMessage('You cannot select the same region more than once.')
      return
    }

    try {
      setCalculating(true)
      setSubmitMessage('')
      setCalculation(null)

      const result = await calculateReservation({
        concertId: Number(concertId),
        currencyId: Number(currencyId),
        promoCode: promoCode.trim() || undefined,
        items: preparedItems,
      })

      setCalculation(result)
    } catch (err) {
      setSubmitMessage(
        err instanceof Error ? err.message : 'Calculation failed.'
      )
    } finally {
      setCalculating(false)
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!concert) return

    if (!email.trim()) {
      setSubmitMessage('Email is required.')
      return
    }

    if (!currencyId) {
      setSubmitMessage('Please select a currency.')
      return
    }

    const hasEmptyRegion = reservationItems.some(
      (item) => !item.regionSeatingId.trim()
    )
    if (hasEmptyRegion) {
      setSubmitMessage('Please select a region for each row.')
      return
    }

    const hasInvalidQuantity = reservationItems.some(
      (item) => !item.quantity.trim() || Number(item.quantity) <= 0
    )
    if (hasInvalidQuantity) {
      setSubmitMessage('Each quantity must be greater than 0.')
      return
    }

    const preparedItems = reservationItems.map((item) => ({
      regionSeatingId: Number(item.regionSeatingId),
      quantity: Number(item.quantity),
    }))

    const regionIds = preparedItems.map((item) => item.regionSeatingId)
    const hasDuplicates = new Set(regionIds).size !== regionIds.length

    if (hasDuplicates) {
      setSubmitMessage('You cannot select the same region more than once.')
      return
    }

    for (const item of preparedItems) {
      const region = regions.find((r) => r.id === item.regionSeatingId)

      if (!region) {
        setSubmitMessage('One of the selected regions does not exist.')
        return
      }

      if (item.quantity > region.availableSeats) {
        setSubmitMessage(
          `Only ${region.availableSeats} seats are available for ${region.name}.`
        )
        return
      }
    }

    try {
      setSubmitting(true)
      setSubmitMessage('')

      const result = await createReservation({
        concertId: concert.id,
        currencyId: Number(currencyId),
        email,
        promoCode: promoCode.trim() || undefined,
        items: preparedItems,
      })

      setCreatedLoginCode(result.loginCode)
      setSubmitMessage(
        'Reservation request has been sent. Save your reservation code.'
      )
    } catch (err) {
      setSubmitMessage(
        err instanceof Error ? err.message : 'Failed to submit reservation request.'
      )
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
                <p className="mb-4 text-sm text-gray-600 md:col-span-2">
                  Base prices are shown in EUR. Final price will be converted to
                  the selected currency during reservation.
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
                      {currency.code}
                    </option>
                  ))}
                </select>
              </div>

              <div className="space-y-4">
                <label className="block text-sm font-medium">
                  Tickets by region
                </label>

                {reservationItems.map((item, index) => {
                  const selectedRegionId = Number(item.regionSeatingId)
                  const price = item.regionSeatingId
                    ? getPriceForRegion(selectedRegionId, Number(currencyId))
                    : null

                  return (
                    <div
                      key={index}
                      className="grid gap-3 rounded-lg border p-4 md:grid-cols-[1fr_120px_auto]"
                    >
                      <select
                        value={item.regionSeatingId}
                        onChange={(e) =>
                          updateReservationItem(
                            index,
                            'regionSeatingId',
                            e.target.value
                          )
                        }
                        className="w-full rounded-lg border px-3 py-2"
                      >
                        <option value="">Select region</option>
                        {regions.map((region) => (
                          <option
                            key={region.id}
                            value={region.id}
                            disabled={region.availableSeats <= 0}
                          >
                            {region.name} (available: {region.availableSeats} / {region.capacity})
                          </option>
                        ))}
                      </select>

                      <input
                        type="number"
                        min="1"
                        value={item.quantity}
                        onChange={(e) =>
                          updateReservationItem(index, 'quantity', e.target.value)
                        }
                        className="w-full rounded-lg border px-3 py-2"
                      />

                      <button
                        type="button"
                        onClick={() => removeReservationItem(index)}
                        className="rounded-lg bg-red-600 px-4 py-2 text-white hover:opacity-90"
                        disabled={reservationItems.length === 1}
                      >
                        Remove
                      </button>

                      {price && (
                        <p className="text-sm text-gray-600 md:col-span-3">
                          Price: {price.amount} {price.currency?.code ?? ''}
                        </p>
                      )}
                    </div>
                  )
                })}

                <button
                  type="button"
                  onClick={addReservationItem}
                  className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:opacity-90"
                >
                  Add another region
                </button>
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium">
                  Promo code (optional)
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    value={promoCode}
                    onChange={(e) => setPromoCode(e.target.value.toUpperCase())}
                    className="w-full rounded-lg border px-3 py-2"
                    placeholder="Enter promo code"
                  />

                  <button
                    type="button"
                    onClick={handleValidatePromoCode}
                    disabled={validatingPromo}
                    className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:opacity-90 disabled:opacity-50"
                  >
                    {validatingPromo ? 'Checking...' : 'Check'}
                  </button>
                </div>

                {promoValidationMessage && (
                  <p
                    className={`mt-2 text-sm ${
                      promoValidationSuccess ? 'text-green-600' : 'text-red-600'
                    }`}
                  >
                    {promoValidationMessage}
                  </p>
                )}
              </div>

              <button
                type="button"
                onClick={handleCalculatePrice}
                className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:opacity-90"
              >
                {calculating ? 'Calculating...' : 'Calculate price'}
              </button>

              {calculation && (
                <div className="rounded-lg border bg-gray-50 p-4">
                  <h3 className="mb-3 font-semibold">Price breakdown</h3>

                  <div className="space-y-2">
                    {calculation.breakdown?.map((item: any, index: number) => (
                      <div
                        key={index}
                        className="flex items-center justify-between border-b pb-2"
                      >
                        <div>
                          <p className="font-medium">{item.regionName}</p>
                          <p className="text-sm text-gray-500">
                            {item.quantity} × {formatMoney(item.unitPrice)}
                          </p>
                        </div>

                        <p className="font-medium">
                          {formatMoney(item.lineTotal)}
                        </p>
                      </div>
                    ))}
                  </div>

                  <div className="mt-4 space-y-1 text-sm">
                    <p>
                      <span className="font-medium">Subtotal:</span>{' '}
                      {formatMoney(calculation.subtotal)}
                    </p>
                    <p>
                      <span className="font-medium">Early bird discount:</span>{' '}
                      -{formatMoney(calculation.earlyBirdDiscount)}
                    </p>
                    <p>
                      <span className="font-medium">Promo discount:</span>{' '}
                      -{formatMoney(calculation.promoDiscount)}
                    </p>
                    <p className="pt-2 text-base font-bold">
                      Final price: {formatMoney(calculation.finalTotal)}
                    </p>
                  </div>
                </div>
              )}

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
                  Save this code and use it later with your email to check
                  reservation status.
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