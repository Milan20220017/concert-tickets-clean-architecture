import { apiGet } from '../lib/api'
import type { Concert } from '../types/concert'

export async function getConcerts(): Promise<Concert[]> {
  return apiGet<Concert[]>('/concerts')
}

export async function getConcertById(concertId: number): Promise<Concert> {
  return apiGet<Concert>(`/concerts/${concertId}`)
}