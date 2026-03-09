export interface TicketPrice {
  id: number
  amount: number
  concertId: number
  regionSeatingId: number
  regionSeating: {
    id: number
    name: string
    capacity: number
    locationId: number
  } | null
  currencyId: number
  currency: {
    id: number
    code: string
  } | null
}