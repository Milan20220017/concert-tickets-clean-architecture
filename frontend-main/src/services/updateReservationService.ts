const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface UpdateReservationRequest {
  loginCode: string
  email: string
  items: {
    regionSeatingId: number
    quantity: number
  }[]
}

export async function updateReservation(payload: UpdateReservationRequest) {
  const response = await fetch(`${API_BASE_URL}/reservations/update-by-code`, {
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

  return response.json() as Promise<{
    message: string
    reservationId: number
    totalPrice: number
    status: string
  }>
}