export interface ReservationStatusResponse {
    loginCode: string
    email: string
    status: string
    errorMessage: string | null
    reservationId: number | null
    totalPrice: number | null   
    reservationStatus: string | null
    generatedPromoCode: string | null
}