import type { ReservationStatusResponse } from '../types/reservationStatus'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface CheckReservationStatusRequest {
  loginCode: string
  email: string
}

export async function checkReservationStatus(
  payload: CheckReservationStatusRequest
): Promise<ReservationStatusResponse> {
  const response = await fetch(`${API_BASE_URL}/reservations/check-status`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed with status ${response.status}`)
  }

  return response.json() as Promise<ReservationStatusResponse>
}