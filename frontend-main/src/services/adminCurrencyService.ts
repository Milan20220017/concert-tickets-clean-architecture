const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export interface Currency {
  id: number
  code: string
}

async function getErrorMessage(
  response: Response,
  fallback: string
): Promise<string> {
  try {
    const text = await response.text()

    if (!text) return fallback

    try {
      const data = JSON.parse(text)

      if (data?.error) return data.error
      if (data?.message) return data.message

      return fallback
    } catch {
      return text
    }
  } catch {
    return fallback
  }
}

export async function getCurrencies(): Promise<Currency[]> {
  const response = await fetch(`${API_BASE_URL}/currencies`)

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response, "Failed to fetch currencies")
    )
  }

  return await response.json()
}

export async function deleteCurrency(id: number): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/currencies/${id}`, {
    method: "DELETE",
  })

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response, "Failed to delete currency")
    )
  }
}

export async function createCurrency(code: string): Promise<Currency> {
  const response = await fetch(`${API_BASE_URL}/currencies`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ code }),
  })

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response, "Failed to create currency")
    )
  }

  return await response.json()
}