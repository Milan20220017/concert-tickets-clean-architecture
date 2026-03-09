const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Currency {
  id: number
  code: string
}

export async function getCurrencies(): Promise<Currency[]> {
  const response = await fetch(`${API_BASE_URL}/currencies`)

  if (!response.ok) {
    throw new Error('Failed to fetch currencies')
  }

  return response.json()
}

export async function createCurrency(code: string): Promise<Currency> {
  const response = await fetch(`${API_BASE_URL}/currencies`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ code }),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Failed to create currency')
  }

  return response.json()
}