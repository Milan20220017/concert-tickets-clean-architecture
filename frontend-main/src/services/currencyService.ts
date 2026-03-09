const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Currency {
  id: number
  code: string
  name: string
}

export async function getCurrencies(): Promise<Currency[]> {
  const response = await fetch(`${API_BASE_URL}/currencies`)

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed with status ${response.status}`)
  }

  return response.json() as Promise<Currency[]>
}