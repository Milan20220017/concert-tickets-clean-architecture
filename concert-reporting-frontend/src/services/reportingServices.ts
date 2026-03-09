import type { ConcertSalesReport, LocationSalesReport } from '../types/reporting'

const API_BASE_URL = 'https://localhost:7132/api'
// ako ti reporting backend radi na drugom portu, ovdje stavi taj port

export async function getConcertSales(): Promise<ConcertSalesReport[]> {
  const response = await fetch(`${API_BASE_URL}/reporting/concert-sales`)

  if (!response.ok) {
    throw new Error('Failed to fetch concert sales')
  }

  return response.json()
}

export async function getLocationSales(): Promise<LocationSalesReport[]> {
  const response = await fetch(`${API_BASE_URL}/reporting/location-sales`)

  if (!response.ok) {
    throw new Error('Failed to fetch location sales')
  }

  return response.json()
}