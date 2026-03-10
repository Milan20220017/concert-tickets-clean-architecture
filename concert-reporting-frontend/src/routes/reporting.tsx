import { createFileRoute } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  getConcertSales,
  getLocationSales,
} from '../services/reportingServices'
import { getConcerts } from '../services/concertService'
import { getLocations } from '../services/locationService'
import type {
  ConcertSalesReport,
  LocationSalesReport,
} from '../types/reporting'
import type { Concert } from '../types/concert'
import type { Location } from '../types/location'

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js'

import { Bar } from 'react-chartjs-2'

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
)

export const Route = createFileRoute('/reporting')({
  component: ReportingPage,
})

function ReportingPage() {
  const [concertSales, setConcertSales] = useState<ConcertSalesReport[]>([])
  const [locationSales, setLocationSales] = useState<LocationSalesReport[]>([])
  const [concerts, setConcerts] = useState<Concert[]>([])
  const [locations, setLocations] = useState<Location[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadData()
  }, [])

  async function loadData() {
    try {
      setLoading(true)
      setError('')

      const [concertSalesData, locationSalesData, concertsData, locationsData] =
        await Promise.all([
          getConcertSales(),
          getLocationSales(),
          getConcerts(),
          getLocations(),
        ])

      setConcertSales(concertSalesData)
      setLocationSales(locationSalesData)
      setConcerts(concertsData)
      setLocations(locationsData)
    } catch (err) {
      setError('Failed to load reporting data.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  function getConcertName(concertId: number) {
    return concerts.find((c) => c.id === concertId)?.name ?? `Concert ${concertId}`
  }

  function getLocationName(locationId: number) {
    return locations.find((l) => l.id === locationId)?.name ?? `Location ${locationId}`
  }

  const totalCreated = concertSales.reduce((sum, item) => sum + item.createdTickets, 0)

  const totalCancelled = concertSales.reduce((sum, item) => sum + item.cancelledTickets, 0)

  const totalNetSold = concertSales.reduce((sum, item) => sum + item.netTicketsSold, 0)

  const sortedConcerts = [...concertSales].sort(
    (a, b) => b.netTicketsSold - a.netTicketsSold
  )

  const sortedLocations = [...locationSales].sort(
    (a, b) => b.netTicketsSold - a.netTicketsSold
  )

  const concertChartData = {
    labels: sortedConcerts.map((item) => getConcertName(item.concertId)),
    datasets: [
      {
        label: 'Tickets sold',
        data: sortedConcerts.map((item) => item.netTicketsSold),
        backgroundColor: '#2563eb',
      },
    ],
  }

  const locationChartData = {
    labels: sortedLocations.map((item) => getLocationName(item.locationId)),
    datasets: [
      {
        label: 'Tickets sold',
        data: sortedLocations.map((item) => item.netTicketsSold),
        backgroundColor: '#16a34a',
      },
    ],
  }

  return (
    <div className="min-h-screen bg-slate-100">
      <div className="mx-auto max-w-7xl px-6 py-10">

        <div className="flex items-center justify-between mb-10">
          <div>
            <h1 className="text-4xl font-bold text-slate-900">
              Reporting dashboard
            </h1>
            <p className="text-slate-600 mt-2">
              Overview of concert ticket activity.
            </p>
          </div>

          <button
            onClick={loadData}
            className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            Refresh data
          </button>
        </div>

        {loading && <p>Loading reporting data...</p>}
        {error && <p className="text-red-600">{error}</p>}

        {!loading && !error && (
          <>
            <div className="grid gap-6 md:grid-cols-3 mb-10">

              <div className="rounded-xl bg-white p-6 shadow">
                <p className="text-sm text-gray-500">Total created tickets</p>
                <p className="text-3xl font-bold mt-2">{totalCreated}</p>
              </div>

              <div className="rounded-xl bg-white p-6 shadow">
                <p className="text-sm text-gray-500">Total cancelled</p>
                <p className="text-3xl font-bold text-red-600 mt-2">
                  {totalCancelled}
                </p>
              </div>

              <div className="rounded-xl bg-white p-6 shadow">
                <p className="text-sm text-gray-500">Net sold</p>
                <p className="text-3xl font-bold text-green-600 mt-2">
                  {totalNetSold}
                </p>
              </div>

            </div>

            <div className="grid gap-8 lg:grid-cols-2 mb-10">

              <div className="rounded-xl bg-white p-6 shadow">
                <h2 className="font-semibold text-lg mb-4">
                  Concert sales
                </h2>

                <div className="h-80">
                  <Bar
                    data={concertChartData}
                    options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: { display: false },
                        tooltip: {
                          callbacks: {
                            label: (ctx) => `Tickets sold: ${ctx.raw}`,
                          },
                        },
                      },
                    }}
                  />
                </div>
              </div>

              <div className="rounded-xl bg-white p-6 shadow">
                <h2 className="font-semibold text-lg mb-4">
                  Location sales
                </h2>

                <div className="h-80">
                  <Bar
                    data={locationChartData}
                    options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: { display: false },
                        tooltip: {
                          callbacks: {
                            label: (ctx) => `Tickets sold: ${ctx.raw}`,
                          },
                        },
                      },
                    }}
                  />
                </div>
              </div>

            </div>

            <div className="rounded-xl bg-white p-6 shadow mb-10">
              <h2 className="font-semibold text-lg mb-4">
                Sales by concert
              </h2>

              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-gray-600">
                    <th className="py-2">Concert</th>
                    <th>Created</th>
                    <th>Cancelled</th>
                    <th>Net sold</th>
                  </tr>
                </thead>

                <tbody>
                  {sortedConcerts.map((item) => (
                    <tr key={item.concertId} className="border-b">
                      <td className="py-2">
                        {getConcertName(item.concertId)}
                      </td>
                      <td>{item.createdTickets}</td>
                      <td>{item.cancelledTickets}</td>
                      <td className="font-semibold">
                        {item.netTicketsSold}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="rounded-xl bg-white p-6 shadow">
              <h2 className="font-semibold text-lg mb-4">
                Sales by location
              </h2>

              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-gray-600">
                    <th className="py-2">Location</th>
                    <th>Created</th>
                    <th>Cancelled</th>
                    <th>Net sold</th>
                  </tr>
                </thead>

                <tbody>
                  {sortedLocations.map((item) => (
                    <tr key={item.locationId} className="border-b">
                      <td className="py-2">
                        {getLocationName(item.locationId)}
                      </td>
                      <td>{item.createdTickets}</td>
                      <td>{item.cancelledTickets}</td>
                      <td className="font-semibold">
                        {item.netTicketsSold}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

          </>
        )}
      </div>
    </div>
  )
}