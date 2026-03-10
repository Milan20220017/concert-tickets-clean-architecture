import { createFileRoute } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import { checkReservationStatus } from '../services/reservationStatusService'
import { cancelReservation } from '../services/cancelReservationService'
import { updateReservation } from '../services/updateReservationService'
import type { ReservationStatusResponse } from '../types/reservationStatus'

export const Route = createFileRoute('/reservation-check')({
  component: ReservationCheckPage,
})

function formatMoney(value: number | null | undefined) {
  if (value === null || value === undefined) return '-'
  return new Intl.NumberFormat('sr-RS', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}

function ReservationCheckPage() {
  const [loginCode, setLoginCode] = useState('')
  const [email, setEmail] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [cancelMessage, setCancelMessage] = useState('')
  const [updateMessage, setUpdateMessage] = useState('')
  const [updating, setUpdating] = useState(false)
  const [result, setResult] = useState<ReservationStatusResponse | null>(null)

  const [editedItems, setEditedItems] = useState<
    { regionSeatingId: number; regionName: string; quantity: string }[]
  >([])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    try {
      setLoading(true)
      setError('')
      setCancelMessage('')
      setUpdateMessage('')
      setResult(null)
      setEditedItems([])

      const data = await checkReservationStatus({
        loginCode,
        email,
      })

      setResult(data)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Failed to check reservation status.'
      )
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (result?.items?.length) {
      setEditedItems(
        result.items.map((item) => ({
          regionSeatingId: item.regionSeatingId,
          regionName: item.regionName,
          quantity: String(item.quantity),
        }))
      )
    } else {
      setEditedItems([])
    }
  }, [result])

  async function handleCancel() {
    try {
      setCancelMessage('')
      setError('')
      setUpdateMessage('')

      const response = await cancelReservation({
        loginCode,
        email,
      })

      setCancelMessage(response.message)

      const refreshed = await checkReservationStatus({
        loginCode,
        email,
      })

      setResult(refreshed)
    } catch (err) {
      setCancelMessage(
        err instanceof Error ? err.message : 'Failed to cancel reservation.'
      )
    }
  }

  async function handleUpdateReservation() {
    if (!editedItems.length) {
      setUpdateMessage('No reservation items available for update.')
      return
    }

    const invalidItem = editedItems.find(
      (item) => !item.quantity.trim() || Number(item.quantity) <= 0
    )

    if (invalidItem) {
      setUpdateMessage('Each ticket quantity must be greater than 0.')
      return
    }

    try {
      setUpdating(true)
      setError('')
      setCancelMessage('')
      setUpdateMessage('')

      const response = await updateReservation({
        loginCode,
        email,
        items: editedItems.map((item) => ({
          regionSeatingId: item.regionSeatingId,
          quantity: Number(item.quantity),
        })),
      })

      setUpdateMessage(response.message)

      const refreshed = await checkReservationStatus({
        loginCode,
        email,
      })

      setResult(refreshed)
    } catch (err) {
      setUpdateMessage(
        err instanceof Error ? err.message : 'Failed to update reservation.'
      )
    } finally {
      setUpdating(false)
    }
  }

  function handleQuantityChange(regionSeatingId: number, value: string) {
    setEditedItems((prev) =>
      prev.map((item) =>
        item.regionSeatingId === regionSeatingId
          ? { ...item, quantity: value }
          : item
      )
    )
  }

  function renderStatusBadge(status: string) {
    if (status === 'Pending') {
      return (
        <span className="rounded-full bg-yellow-100 px-3 py-1 text-sm font-medium text-yellow-800">
          Pending
        </span>
      )
    }

    if (status === 'Rejected') {
      return (
        <span className="rounded-full bg-red-100 px-3 py-1 text-sm font-medium text-red-700">
          Rejected
        </span>
      )
    }

    if (status === 'Accepted') {
      return (
        <span className="rounded-full bg-green-100 px-3 py-1 text-sm font-medium text-green-700">
          Accepted
        </span>
      )
    }

    if (status === 'Cancelled') {
      return (
        <span className="rounded-full bg-red-100 px-3 py-1 text-sm font-medium text-red-700">
          Cancelled
        </span>
      )
    }

    return (
      <span className="rounded-full bg-gray-100 px-3 py-1 text-sm font-medium text-gray-700">
        {status}
      </span>
    )
  }

  return (
    <div className="mx-auto max-w-4xl px-6 py-12">
      <h1 className="mb-6 text-3xl font-bold">Check reservation status</h1>

      <form onSubmit={handleSubmit} className="space-y-4 rounded-xl border p-6">
        <div>
          <label className="mb-1 block text-sm font-medium">
            Reservation code
          </label>
          <input
            type="text"
            value={loginCode}
            onChange={(e) => setLoginCode(e.target.value.toUpperCase())}
            required
            className="w-full rounded-lg border px-3 py-2"
            placeholder="Enter your reservation code"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">Email</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className="w-full rounded-lg border px-3 py-2"
            placeholder="Enter your email"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
        >
          {loading ? 'Checking...' : 'Check status'}
        </button>
      </form>

      {error && <p className="mt-4 text-red-600">{error}</p>}

      {result && (
        <div className="mt-6 rounded-xl border p-6">
          <div className="mb-4 flex items-center justify-between gap-4">
            <h2 className="text-2xl font-semibold">Reservation request</h2>
            {renderStatusBadge(result.status)}
          </div>

          <div className="space-y-2">
            <p>
              <span className="font-medium">Code:</span> {result.loginCode}
            </p>
            <p>
              <span className="font-medium">Email:</span> {result.email}
            </p>
          </div>

          {result.status === 'Pending' && (
            <div className="mt-4 rounded-lg bg-yellow-50 p-4 text-yellow-800">
              Your reservation request is still being processed.
            </div>
          )}

          {result.status === 'Rejected' && (
            <div className="mt-4 rounded-lg bg-red-50 p-4 text-red-700">
              <p className="font-medium">Reservation request was rejected.</p>
              {result.errorMessage && (
                <p className="mt-1">{result.errorMessage}</p>
              )}
            </div>
          )}

          {result.status === 'Accepted' && (
            <div className="mt-4 space-y-4 rounded-lg bg-green-50 p-4 text-green-800">
              <div className="grid gap-3 md:grid-cols-2">
                <p>
                  <span className="font-medium">Reservation ID:</span>{' '}
                  {result.reservationId ?? '-'}
                </p>
                <p>
                  <span className="font-medium">Reservation status:</span>{' '}
                  {result.reservationStatus ?? '-'}
                </p>
                <p>
                  <span className="font-medium">Request status:</span>{' '}
                  {result.status}
                </p>
                <p>
                  <span className="font-medium">Stored total price:</span>{' '}
                  {formatMoney(result.totalPrice)}
                </p>
              </div>

              {result.generatedPromoCode && (
                <div className="rounded-lg bg-blue-50 p-4 text-blue-800">
                  <p className="font-medium">
                    Your promo code for the next reservation:
                  </p>
                  <p className="mt-2 text-lg font-bold tracking-widest">
                    {result.generatedPromoCode}
                  </p>
                  <p className="mt-1 text-sm">
                    This code gives a discount on the next reservation and can
                    be used by you or someone else.
                  </p>
                </div>
              )}

              {result.priceBreakdown && result.priceBreakdown.length > 0 && (
                <div className="rounded-lg border border-green-200 bg-white p-4 text-gray-800">
                  <h3 className="mb-4 text-lg font-semibold">
                    Detailed price breakdown
                  </h3>

                  <div className="overflow-x-auto">
                    <table className="w-full border-collapse text-sm">
                      <thead>
                        <tr className="border-b bg-gray-50 text-left">
                          <th className="px-3 py-2">Region</th>
                          <th className="px-3 py-2">Quantity</th>
                          <th className="px-3 py-2">Unit price</th>
                          <th className="px-3 py-2">Formula</th>
                          <th className="px-3 py-2">Line total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {result.priceBreakdown.map((item) => (
                          <tr key={item.regionSeatingId} className="border-b">
                            <td className="px-3 py-2 font-medium">
                              {item.regionName}
                            </td>
                            <td className="px-3 py-2">{item.quantity}</td>
                            <td className="px-3 py-2">
                              {formatMoney(item.unitPrice)}
                            </td>
                            <td className="px-3 py-2 text-gray-500">
                              {item.quantity} × {formatMoney(item.unitPrice)}
                            </td>
                            <td className="px-3 py-2 font-medium">
                              {formatMoney(item.lineTotal)}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div className="mt-5 space-y-2 rounded-lg bg-gray-50 p-4 text-sm">
                    <p>
                      <span className="font-medium">
                        Subtotal before discounts:
                      </span>{' '}
                      {formatMoney(result.subtotalBeforeDiscounts)}
                    </p>

                    <p>
                      <span className="font-medium">
                        Early bird discount:
                      </span>{' '}
                      -{formatMoney(result.earlyBirdDiscountAmount ?? 0)}
                    </p>

                    <p>
                      <span className="font-medium">Promo discount:</span> -
                      {formatMoney(result.promoDiscountAmount ?? 0)}
                    </p>

                    <p className="border-t pt-2 text-base font-bold">
                      Final total price:{' '}
                      {formatMoney(result.finalTotalPrice ?? result.totalPrice)}
                    </p>
                  </div>
                </div>
              )}

              {result.reservationStatus !== 'Cancelled' &&
                editedItems.length > 0 && (
                  <div className="rounded-lg border border-green-200 bg-white p-4 text-gray-800">
                    <h3 className="mb-3 text-lg font-semibold">
                      Update ticket quantities
                    </h3>

                    <div className="space-y-3">
                      {editedItems.map((item) => (
                        <div
                          key={item.regionSeatingId}
                          className="flex items-center justify-between gap-4"
                        >
                          <div>
                            <p className="font-medium">{item.regionName}</p>
                          </div>

                          <input
                            type="number"
                            min="1"
                            value={item.quantity}
                            onChange={(e) =>
                              handleQuantityChange(
                                item.regionSeatingId,
                                e.target.value
                              )
                            }
                            className="w-24 rounded-lg border px-3 py-2"
                          />
                        </div>
                      ))}
                    </div>

                    <div className="mt-4 flex flex-wrap gap-3">
                      <button
                        type="button"
                        onClick={handleUpdateReservation}
                        disabled={updating}
                        className="rounded-lg bg-blue-600 px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
                      >
                        {updating ? 'Updating...' : 'Update reservation'}
                      </button>

                      <button
                        type="button"
                        onClick={handleCancel}
                        className="rounded-lg bg-red-600 px-5 py-3 text-white hover:opacity-90"
                      >
                        Cancel reservation
                      </button>
                    </div>

                    {updateMessage && (
                      <p className="mt-3 text-sm text-blue-700">
                        {updateMessage}
                      </p>
                    )}

                    {cancelMessage && (
                      <p className="mt-3 text-sm text-green-700">
                        {cancelMessage}
                      </p>
                    )}
                  </div>
                )}

              {result.reservationStatus !== 'Cancelled' &&
                editedItems.length === 0 && (
                  <button
                    type="button"
                    onClick={handleCancel}
                    className="mt-3 rounded-lg bg-red-600 px-5 py-3 text-white hover:opacity-90"
                  >
                    Cancel reservation
                  </button>
                )}

              {result.reservationStatus === 'Cancelled' && cancelMessage && (
                <p className="text-sm text-green-700">{cancelMessage}</p>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  )
}