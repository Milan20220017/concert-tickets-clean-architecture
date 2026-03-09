import { apiGet } from '../lib/api'
import type { Concert } from '../types/concert'
import type { RegionAvailability } from '../types/regionAvailability'

export async function getConcerts(): Promise<Concert[]> {
  return apiGet<Concert[]>('/concerts')
}

export async function getConcertById(concertId: number): Promise<Concert> {
  return apiGet<Concert>(`/concerts/${concertId}`)
}

export async function getRegionsAvailabilityByConcert(
  concertId: number
): Promise<RegionAvailability[]> {
  return apiGet<RegionAvailability[]>(`/concerts/${concertId}/regions-availability`)
}