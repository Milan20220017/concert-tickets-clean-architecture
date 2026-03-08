import { createFileRoute, Link } from '@tanstack/react-router'

export const Route = createFileRoute('/')({
  component: HomePage,
})

function HomePage() {
  return (
    <div className="mx-auto max-w-5xl px-6 py-12">
      <h1 className="mb-4 text-4xl font-bold">Concert Tickets</h1>

      <p className="mb-8 text-lg">
        Welcome to the main application for browsing concerts and checking reservations.
      </p>

      <div className="flex gap-4">
        <Link to="/concerts">View Concerts</Link>
      </div>
    </div>
  )
}