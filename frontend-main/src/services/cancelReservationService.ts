const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface CancelReservationRequest {
  loginCode: string
  email: string
}

export async function cancelReservation(
  payload: CancelReservationRequest
) {
  const response = await fetch(`${API_BASE_URL}/reservations/cancel-by-code`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed with status ${response.status}`)
  }

  return response.json()
}