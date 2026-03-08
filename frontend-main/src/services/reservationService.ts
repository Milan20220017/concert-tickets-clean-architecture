import type { CreateReservationRequest } from "../types/reservation"

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export async function createReservation(payload: CreateReservationRequest) {
  const response = await fetch(`${API_BASE_URL}/reservations`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed with status ${response.status}`)
  }

  return response.json() as Promise<{
    message: string
    loginCode: string
  }>
}