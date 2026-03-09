const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface TicketPrice {
  id: number
  amount: number
  concertId: number
  regionSeatingId: number
  regionSeating: {
    id: number
    name: string
    capacity: number
    locationId: number
  } | null
  currencyId: number
  currency: {
    id: number
    code: string
  } | null
}

export interface UpsertTicketPricePayload {
  concertId: number
  regionSeatingId: number
  currencyId: number
  amount: number
}

export async function getTicketPricesByConcert(
  concertId: number
): Promise<TicketPrice[]> {
  const response = await fetch(
    `${API_BASE_URL}/ticketprices/by-concert/${concertId}`
  )

  if (!response.ok) {
    throw new Error('Failed to fetch ticket prices')
  }

  return response.json()
}

export async function upsertTicketPrice(
  payload: UpsertTicketPricePayload
): Promise<TicketPrice> {
  const response = await fetch(`${API_BASE_URL}/ticketprices`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Failed to save ticket price')
  }

  return response.json()
}