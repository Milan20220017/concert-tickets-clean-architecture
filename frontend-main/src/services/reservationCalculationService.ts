const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export async function calculateReservation(payload: any) {
  const response = await fetch(`${API_BASE_URL}/reservations/calculate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error('Failed to calculate price.')
  }

  return response.json()
}