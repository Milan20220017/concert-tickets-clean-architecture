import { createFileRoute, Link } from '@tanstack/react-router'

export const Route = createFileRoute('/admin/')({
  component: AdminDashboardPage,
})

function AdminDashboardPage() {
  const cards = [
    { title: 'Manage categories', to: '/admin/categories' },
    { title: 'Manage locations', to: '/admin/locations' },
    { title: 'Manage concerts', to: '/admin/concerts' },
    { title: 'Manage currencies', to: '/admin/currencies' },
  ] as const

  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <div className="mb-8">
        <h1 className="text-4xl font-bold">Admin dashboard</h1>
        <p className="mt-2 text-gray-600">
          Manage categories, locations, concerts, currencies and ticket prices.
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {cards.map((card) => (
          <Link
            key={card.to}
            to={card.to}
            className="rounded-2xl border border-gray-300 bg-white p-8 shadow-sm transition hover:shadow-md"
          >
            <h2 className="text-2xl font-semibold">{card.title}</h2>
            <p className="mt-2 text-sm text-gray-600">Open section</p>
          </Link>
        ))}
      </div>
    </div>
  )
}