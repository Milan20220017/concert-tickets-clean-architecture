const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Concert {
  id: number
  name: string
  date: string
  categoryId: number
  categoryName: string
  locationId: number
  locationName: string
  earlyBirdDiscountUntil?: string | null
  isPublished: boolean
}

export interface CreateConcertPayload {
  name: string
  date: string
  categoryId: number
  locationId: number
  earlyBirdDiscountUntil?: string
}

export async function getConcerts(): Promise<Concert[]> {
  const response = await fetch(`${API_BASE_URL}/concerts/admin/all`)

  if (!response.ok) {
    throw new Error('Failed to fetch concerts')
  }

  return response.json()
}

export async function createConcert(
  payload: CreateConcertPayload
): Promise<Concert> {
  const response = await fetch(`${API_BASE_URL}/concerts`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Failed to create concert')
  }

  return response.json()
}

export async function deleteConcert(id: number): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/concerts/${id}`, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error('Failed to delete concert')
  }
}