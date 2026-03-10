const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface ReservationItemPayload {
  regionSeatingId: number
  quantity: number
}

export interface CreateReservationPayload {
  concertId: number
  currencyId: number
  email: string
  promoCode?: string
  items: ReservationItemPayload[]
}

export interface CreateReservationResponse {
  message: string
  loginCode: string
}

export interface CalculateReservationPayload {
  concertId: number
  currencyId: number
  promoCode?: string
  items: ReservationItemPayload[]
}

export interface CalculateReservationResponse {
  subtotal: number
  earlyBirdDiscount: number
  promoDiscount: number
  finalTotal: number
  breakdown: {
    regionName: string
    quantity: number
    unitPrice: number
    lineTotal: number
  }[]
}

async function getErrorMessage(
  response: Response,
  fallback: string
): Promise<string> {
  try {
    const text = await response.text()
    if (!text) return fallback

    try {
      const data = JSON.parse(text)
      if (data?.error) return data.error
      if (data?.message) return data.message
      return fallback
    } catch {
      return text
    }
  } catch {
    return fallback
  }
}

export async function createReservation(
  payload: CreateReservationPayload
): Promise<CreateReservationResponse> {
  const response = await fetch(`${API_BASE_URL}/reservations`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response, 'Failed to create reservation.')
    )
  }

  return response.json()
}

export async function calculateReservation(
  payload: CalculateReservationPayload
): Promise<CalculateReservationResponse> {
  const response = await fetch(`${API_BASE_URL}/reservations/calculate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response, 'Failed to calculate price.')
    )
  }

  return response.json()
}