import { createFileRoute } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  getConcertSales,
  getLocationSales,
} from '../services/reportingServices'
import type {
  ConcertSalesReport,
  LocationSalesReport,
} from '../types/reporting'

export const Route = createFileRoute('/reporting')({
  component: ReportingPage,
})

function ReportingPage() {
  const [concertSales, setConcertSales] = useState<ConcertSalesReport[]>([])
  const [locationSales, setLocationSales] = useState<LocationSalesReport[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadData() {
      try {
        setLoading(true)
        setError('')

        const [concertData, locationData] = await Promise.all([
          getConcertSales(),
          getLocationSales(),
        ])

        setConcertSales(concertData)
        setLocationSales(locationData)
      } catch (err) {
        setError('Failed to load reporting data.')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    loadData()
  }, [])

  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <h1 className="mb-8 text-3xl font-bold">Reporting</h1>

      {loading && <p>Loading reporting data...</p>}
      {error && <p className="text-red-600">{error}</p>}

      {!loading && !error && (
        <div className="space-y-8">
          <div className="rounded-2xl border border-gray-300 bg-white p-8 shadow-sm">
            <h2 className="mb-6 text-2xl font-bold">Sales by concert</h2>

            {concertSales.length === 0 ? (
              <p className="text-gray-600">No concert sales data available.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full border-collapse">
                  <thead>
                    <tr className="border-b text-left">
                      <th className="px-4 py-3">Concert ID</th>
                      <th className="px-4 py-3">Created tickets</th>
                      <th className="px-4 py-3">Cancelled tickets</th>
                      <th className="px-4 py-3">Net tickets sold</th>
                    </tr>
                  </thead>
                  <tbody>
                    {concertSales.map((item) => (
                      <tr key={item.concertId} className="border-b">
                        <td className="px-4 py-3">{item.concertId}</td>
                        <td className="px-4 py-3">{item.createdTickets}</td>
                        <td className="px-4 py-3">{item.cancelledTickets}</td>
                        <td className="px-4 py-3 font-semibold">
                          {item.netTicketsSold}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <div className="rounded-2xl border border-gray-300 bg-white p-8 shadow-sm">
            <h2 className="mb-6 text-2xl font-bold">Sales by location</h2>

            {locationSales.length === 0 ? (
              <p className="text-gray-600">No location sales data available.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full border-collapse">
                  <thead>
                    <tr className="border-b text-left">
                      <th className="px-4 py-3">Location ID</th>
                      <th className="px-4 py-3">Created tickets</th>
                      <th className="px-4 py-3">Cancelled tickets</th>
                      <th className="px-4 py-3">Net tickets sold</th>
                    </tr>
                  </thead>
                  <tbody>
                    {locationSales.map((item) => (
                      <tr key={item.locationId} className="border-b">
                        <td className="px-4 py-3">{item.locationId}</td>
                        <td className="px-4 py-3">{item.createdTickets}</td>
                        <td className="px-4 py-3">{item.cancelledTickets}</td>
                        <td className="px-4 py-3 font-semibold">
                          {item.netTicketsSold}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}