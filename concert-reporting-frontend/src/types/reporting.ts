export interface ConcertSalesReport {
  concertId: number
  createdTickets: number
  cancelledTickets: number
  netTicketsSold: number
}

export interface LocationSalesReport {
  locationId: number
  createdTickets: number
  cancelledTickets: number
  netTicketsSold: number
}