const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface ValidatePromoCodeResponse {
  isValid: boolean
  message: string
  code?: string
  discountPercent?: number
}

export async function validatePromoCode(code: string): Promise<ValidatePromoCodeResponse> {
  const response = await fetch(`${API_BASE_URL}/promocodes/validate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ code }),
  })

  if (!response.ok) {
    throw new Error('Failed to validate promo code.')
  }

  return response.json()
}