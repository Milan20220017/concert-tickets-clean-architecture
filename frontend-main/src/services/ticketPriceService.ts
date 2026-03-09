import { apiGet } from '../lib/api'
import type { TicketPrice } from '../types/ticketPrice'

export async function getTicketPricesByConcert(
  concertId: number
): Promise<TicketPrice[]> {
  return apiGet<TicketPrice[]>(`/ticketprices/by-concert/${concertId}`)
}