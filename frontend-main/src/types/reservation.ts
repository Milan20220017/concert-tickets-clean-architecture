export interface CreateReservationItemRequest {
  regionSeatingId: number
  quantity: number
}

export interface CreateReservationRequest {
  concertId: number
  currencyId: number
  email: string
  usedPromoCodeId?: number
  items: CreateReservationItemRequest[]
}