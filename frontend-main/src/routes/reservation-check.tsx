import { createFileRoute } from '@tanstack/react-router'
import { useState } from 'react'
import { checkReservationStatus } from '../services/reservationStatusService'
import { cancelReservation } from '../services/cancelReservationService'
import type { ReservationStatusResponse } from '../types/reservationStatus'

export const Route = createFileRoute('/reservation-check')({
  component: ReservationCheckPage,
})

function ReservationCheckPage() {
  const [loginCode, setLoginCode] = useState('')
  const [email, setEmail] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [cancelMessage, setCancelMessage] = useState('')
  const [result, setResult] = useState<ReservationStatusResponse | null>(null)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    try {
      setLoading(true)
      setError('')
      setCancelMessage('')
      setResult(null)

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

  async function handleCancel() {
    try {
      setCancelMessage('')
      setError('')

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
    <div className="mx-auto max-w-3xl px-6 py-12">
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
            <div className="mt-4 space-y-3 rounded-lg bg-green-50 p-4 text-green-800">
              <p>
                <span className="font-medium">Reservation ID:</span>{' '}
                {result.reservationId ?? '-'}
              </p>
              <p>
                <span className="font-medium">Total price:</span>{' '}
                {result.totalPrice ?? '-'}
              </p>
              <p>
                <span className="font-medium">Request status:</span>{' '}
                {result.status}
              </p>
              <p>
                <span className="font-medium">Reservation status:</span>{' '}
                {result.reservationStatus ?? '-'}
              </p>
              {result.generatedPromoCode && (
                    <div className="mt-4 rounded-lg bg-blue-50 p-4 text-blue-800">
                      <p className="font-medium">Your promo code for the next reservation:</p>
                      <p className="mt-2 text-lg font-bold tracking-widest">
                        {result.generatedPromoCode}
                      </p>
                      <p className="mt-1 text-sm">
                        This code gives a discount on the next reservation and can be used by you or someone else.
                      </p>
                    </div>
                  )}
              {result.reservationStatus !== 'Cancelled' && (
                <button
                  type="button"
                  onClick={handleCancel}
                  className="mt-3 rounded-lg bg-red-600 px-5 py-3 text-white hover:opacity-90"
                >
                  Cancel reservation
                </button>
              )}

              {cancelMessage && (
                <p className="text-sm text-green-700">{cancelMessage}</p>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  )
}