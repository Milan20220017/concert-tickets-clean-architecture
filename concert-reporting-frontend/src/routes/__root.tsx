import { createRootRoute, Outlet } from '@tanstack/react-router'

export const Route = createRootRoute({
  component: RootComponent,
  notFoundComponent: () => <div className="p-8">Page not found</div>,
})

function RootComponent() {
  return <Outlet />
}