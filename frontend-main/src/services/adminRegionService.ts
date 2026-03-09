const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Region {
  id: number
  name: string
  capacity: number
  locationId: number
}

export async function getRegionsByLocation(
  locationId: number
): Promise<Region[]> {
  const response = await fetch(`${API_BASE_URL}/locations/${locationId}/regions`)

  if (!response.ok) {
    throw new Error('Failed to fetch regions')
  }

  return response.json()
}

export async function createRegion(
  locationId: number,
  name: string,
  capacity: number
): Promise<Region> {
  const response = await fetch(`${API_BASE_URL}/locations/${locationId}/regions`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ name, capacity }),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Failed to create region')
  }

  return response.json()
}