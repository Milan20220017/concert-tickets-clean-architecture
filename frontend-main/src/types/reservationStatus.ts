export interface ReservationStatusItem {
  regionSeatingId: number
  regionName: string
  quantity: number
}

export interface ReservationPriceBreakdownItem {
  regionSeatingId: number
  regionName: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export interface ReservationStatusResponse {
  loginCode: string
  email: string
  status: string
  errorMessage: string | null
  reservationId: number | null
  totalPrice: number | null
  reservationStatus: string | null
  generatedPromoCode: string | null
  items: ReservationStatusItem[]

  subtotalBeforeDiscounts: number | null
  earlyBirdDiscountAmount: number | null
  promoDiscountAmount: number | null
  finalTotalPrice: number | null
  priceBreakdown: ReservationPriceBreakdownItem[]
}