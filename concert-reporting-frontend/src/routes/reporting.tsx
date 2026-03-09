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

    loadData()
  }, [])

  function getConcertName(concertId: number) {
    return concerts.find((c) => c.id === concertId)?.name ?? `Concert ${concertId}`
  }

  function getLocationName(locationId: number) {
    return locations.find((l) => l.id === locationId)?.name ?? `Location ${locationId}`
  }

  const totalCreated = concertSales.reduce(
    (sum, item) => sum + item.createdTickets,
    0
  )

  const totalCancelled = concertSales.reduce(
    (sum, item) => sum + item.cancelledTickets,
    0
  )

  const totalNetSold = concertSales.reduce(
    (sum, item) => sum + item.netTicketsSold,
    0
  )
const sortedConcerts = [...concertSales].sort(
  (a, b) => b.netTicketsSold - a.netTicketsSold
)
const concertChartData = {
  
  labels: sortedConcerts.map((item) => getConcertName(item.concertId)),
  datasets: [
    {
      label: 'Net tickets sold',
      data: sortedConcerts.map((item) => item.netTicketsSold),
      
    },
    
  ],
  
}

  const locationChartData = {
    labels: locationSales.map((item) => getLocationName(item.locationId)),
    datasets: [
      {
        
        label: 'Net tickets sold',
        data: locationSales.map((item) => item.netTicketsSold),
      },
    ],
  }

  return (
    <div className="min-h-screen bg-slate-100">
      <div className="mx-auto max-w-7xl px-6 py-10">
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-slate-900">
            Reporting dashboard
          </h1>
          <p className="mt-2 text-slate-600">
            Overview of concert and location ticket activity.
          </p>
        </div>

        {loading && <p className="text-slate-700">Loading reporting data...</p>}
        {error && <p className="text-red-600">{error}</p>}

        {!loading && !error && (
          <div className="space-y-8">
            <div className="grid gap-4 md:grid-cols-3">
              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <p className="text-sm text-slate-500">Total created tickets</p>
                <p className="mt-2 text-3xl font-bold text-slate-900">
                  {totalCreated}
                </p>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <p className="text-sm text-slate-500">
                  Total cancelled tickets
                </p>
                <p className="mt-2 text-3xl font-bold text-red-600">
                  {totalCancelled}
                </p>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <p className="text-sm text-slate-500">Total net sold</p>
                <p className="mt-2 text-3xl font-bold text-green-600">
                  {totalNetSold}
                </p>
              </div>
              <button
                            onClick={() => window.location.reload()}
                            className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
                          >
                            Refresh data
                          </button>
            </div>

            <div className="grid gap-6 lg:grid-cols-2">
              
              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <h2 className="mb-4 text-xl font-semibold text-slate-900">
                  Concert sales chart
                </h2>
                <div className="h-80">
                  <Bar
                    data={concertChartData}
                    options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        tooltip: {
                          callbacks: {
                            label: function(context) {
                              return `Tickets sold: ${context.raw}`
                            }
                          }
                        }
                      }
                    }}
                  />
                </div>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <h2 className="mb-4 text-xl font-semibold text-slate-900">
                  Location sales chart
                </h2>
                <div className="h-80">
                  <Bar
                    data={locationChartData}
                    options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        tooltip: {
                          callbacks: {
                            label: function(context) {
                              return `Tickets sold: ${context.raw}`
                            }
                          }
                        }
                      }
                    }}
                  />
                </div>
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
              <h2 className="mb-4 text-xl font-semibold text-slate-900">
                Sales by concert
              </h2>

              <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 bg-slate-50 text-left text-slate-700">
                      <th className="px-4 py-3">Concert</th>
                      <th className="px-4 py-3">Created</th>
                      <th className="px-4 py-3">Cancelled</th>
                      <th className="px-4 py-3">Net sold</th>
                    </tr>
                  </thead>
                  <tbody>
                    {concertSales.map((item) => (
                      <tr
                        key={item.concertId}
                        className="border-b border-slate-100 text-slate-800"
                      >
                        <td className="px-4 py-3">
                          {getConcertName(item.concertId)}
                        </td>
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
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
              <h2 className="mb-4 text-xl font-semibold text-slate-900">
                Sales by location
              </h2>

              <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 bg-slate-50 text-left text-slate-700">
                      <th className="px-4 py-3">Location</th>
                      <th className="px-4 py-3">Created</th>
                      <th className="px-4 py-3">Cancelled</th>
                      <th className="px-4 py-3">Net sold</th>
                    </tr>
                  </thead>
                  <tbody>
                    {locationSales.map((item) => (
                      <tr
                        key={item.locationId}
                        className="border-b border-slate-100 text-slate-800"
                      >
                        <td className="px-4 py-3">
                          {getLocationName(item.locationId)}
                        </td>
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
            </div>
          </div>
        )}
      </div>
    </div>
  )
}