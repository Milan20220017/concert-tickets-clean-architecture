const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Location {
  id: number
  name: string
  address: string
}

export async function getLocations(): Promise<Location[]> {
  const response = await fetch(`${API_BASE_URL}/locations`)

  if (!response.ok) {
    throw new Error('Failed to fetch locations')
  }

  return response.json()
}

export async function createLocation(
  name: string,
  address: string
): Promise<Location> {
  const response = await fetch(`${API_BASE_URL}/locations`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ name, address }),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Failed to create location')
  }

  return response.json()
}

export async function deleteLocation(id: number): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/locations/${id}`, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error('Failed to delete location')
  }
}