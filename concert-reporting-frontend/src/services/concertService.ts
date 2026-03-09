const API_BASE_URL = 'https://localhost:7160/api'

import type { Concert } from '../types/concert'

export async function getConcerts(): Promise<Concert[]> {
  const response = await fetch(`${API_BASE_URL}/concerts`)
  if (!response.ok) throw new Error('Failed to fetch concerts')
  return response.json()
}