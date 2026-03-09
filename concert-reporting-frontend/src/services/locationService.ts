const API_BASE_URL = 'https://localhost:7160/api'

import type { Location } from '../types/location'

export async function getLocations(): Promise<Location[]> {
  const response = await fetch(`${API_BASE_URL}/locations`)
  if (!response.ok) throw new Error('Failed to fetch locations')
  return response.json()
}