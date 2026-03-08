import { createFileRoute } from "@tanstack/react-router"
import { useEffect, useState } from "react"

export const Route = createFileRoute("/reservation/$loginCode")({
  component: ReservationPage,
})

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

function ReservationPage() {
  const { loginCode } = Route.useParams()

  const [reservation, setReservation] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")

useEffect(() => {
  let interval: any

  async function loadReservation() {
    try {
      const res = await fetch(
        `${API_BASE_URL}/reservations/by-code/${loginCode}`
      )

      if (res.status === 404) {
        setError("Reservation is still processing...")
        return
      }

      const data = await res.json()
      setReservation(data)
      setError("")

      clearInterval(interval)
    } catch (err) {
      setError("Failed to load reservation.")
    } finally {
      setLoading(false)
    }
  }

  loadReservation()

  interval = setInterval(loadReservation, 3000)

  return () => clearInterval(interval)
}, [loginCode])

  if (loading) return <p>Loading reservation...</p>

  if (error) return <p>{error}</p>

  return (
    <div className="mx-auto max-w-3xl p-8">
      <h1 className="text-3xl font-bold mb-6">Reservation</h1>

      <div className="border rounded-lg p-6 space-y-2">
        <p><b>Code:</b> {reservation.loginCode}</p>
        <p><b>Email:</b> {reservation.email}</p>
        <p><b>Status:</b> {reservation.status}</p>
        <p><b>Total price:</b> {reservation.totalPrice}</p>
      </div>
    </div>
  )
}