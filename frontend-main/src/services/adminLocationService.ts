const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Location {
  id: number
  name: string
  address: string
}

async function getErrorMessage(
  response: Response,
  fallback: string
): Promise<string> {
  try {
    const data = await response.json()
    if (data?.error) return data.error
    if (data?.message) return data.message
    return fallback
  } catch {
    try {
      const text = await response.text()
      return text || fallback
    } catch {
      return fallback
    }
  }
}

export async function getLocations(): Promise<Location[]> {
  const response = await fetch(`${API_BASE_URL}/locations`)

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, 'Failed to fetch locations'))
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
    throw new Error(await getErrorMessage(response, 'Failed to create location'))
  }

  return response.json()
}

export async function deleteLocation(id: number): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/locations/${id}`, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, 'Failed to delete location'))
  }
}