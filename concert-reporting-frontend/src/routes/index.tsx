import { createFileRoute, Link } from '@tanstack/react-router'

export const Route = createFileRoute('/')({
  component: HomePage,
})

function HomePage() {
  return (
    <div className="p-8">
      <h1 className="mb-4 text-3xl font-bold">Reporting frontend</h1>

      <Link
        to="/reporting"
        className="rounded-lg bg-black px-4 py-2 text-white"
      >
        Open reporting
      </Link>
    </div>
  )
}