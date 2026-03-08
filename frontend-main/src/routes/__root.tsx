import { createRootRoute, Link, Outlet } from '@tanstack/react-router'

export const Route = createRootRoute({
  component: RootLayout,
})

function RootLayout() {
  return (
    <div className="min-h-screen bg-white text-black">
      <header className="border-b">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <Link to="/" className="text-2xl font-bold">
            Concert Tickets
          </Link>

          <nav className="flex gap-4">
            <Link to="/">Home</Link>
            <Link to="/concerts">Concerts</Link>
            <Link to="/reservation-check">Check Reservation</Link>
          </nav>
        </div>
      </header>

      <main>
        <Outlet />
      </main>
    </div>
  )
}